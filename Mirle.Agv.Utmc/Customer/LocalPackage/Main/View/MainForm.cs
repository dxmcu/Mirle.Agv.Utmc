using Mirle.Agv.INX.Controller;
using Mirle.Agv.INX.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mirle.Agv.INX.View
{
    public partial class MainForm : Form
    {
        private MainFlowHandler mainFlow;
        private DrawMapData drawMapData;
        private LocalData localData = LocalData.Instance;
        private UcVehicleImage agv;
        private ComputeFunction computeFunction = ComputeFunction.Instance;

        #region UserControl.
        private LabelNameAndValue loginLevelMessage;
        private LabelAndTextBox login_Account;
        private LabelAndTextBox login_Password;

        private LabelNameAndValue soc;
        private LabelNameAndValue charge;
        private LabelNameAndValue loading;
        private LabelNameAndValue cstID;

        private LabelNameAndValue lastAddress;
        private LabelNameAndValue sectionID;
        private LabelNameAndValue sectionDistance;
        private LabelNameAndValue real;

        private LabelNameAndValue moveControlCommand;
        private LabelNameAndValue moveControlStatus;
        private LabelNameAndValue loadUnloadCommand;
        private LabelNameAndValue loadUnloadStatus;

        private LabelNameAndValue stopSingal;
        private LabelNameAndValue lowSpeedSingal;
        #endregion

        #region Form.
        private MoveControlForm moveControlForm;
        private MIPCViewForm mipcViewForm;
        private ProFaceForm proFaceForm;
        #endregion

        private EnumLoginLevel lastLoginLvel = EnumLoginLevel.User;
        private EnumAutoState lastAutoState = EnumAutoState.Manual;

        private List<string> changeColorAddresList = new List<string>();
        private List<string> addressList = new List<string>();
        private object lockAddressListChange = new object();

        public Panel GetPanel
        {
            get
            {
                return panel_Map;
            }
        }

        public Size GetMapSize
        {
            get
            {
                return pB_Map.Size;
            }
        }

        public Bitmap GetMapImage
        {
            get
            {
                return drawMapData.ObjectAndSection;
            }
        }

        public Dictionary<string, AddressPicture> GetAllAddressPicture
        {

            get
            {
                return drawMapData.AllAddressPicture;
            }
        }

        public MainForm(MainFlowHandler mainFlow)
        {
            this.mainFlow = mainFlow;
            InitializeComponent();
            pB_Map.SizeMode = PictureBoxSizeMode.AutoSize;
            panel_Map.AutoScroll = true;
            InitialMap();
            InitialUserControl();
            InitialForm();
        }

        #region Map.
        private void InitialMap()
        {
            drawMapData = new DrawMapData();

            SetPicureSize();
            DrawObject();
            DrawSection();
            DrawAddress();
            DrawAGV();
        }

        public void SetPicureSize()
        {
            try
            {
                foreach (MapAddress address in localData.TheMapInfo.AllAddress.Values)
                {
                    drawMapData.UpdateMaxAndMin(address.AGVPosition.Position);
                }

                drawMapData.SetMapBorderLength(localData.MapConfig.MapBorderLength, localData.MapConfig.MapScale);
            }
            catch (Exception ex)
            {
                mainFlow.WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        private void DrawSection(string sectionID, EnumSectionAction type)
        {
            if (type == EnumSectionAction.None)
                return;

            if (drawMapData.SectionData.ContainsKey(sectionID) && drawMapData.SectionData[sectionID].Type != type)
            {
                drawMapData.Graphics_ObjectAndSection.DrawLine(drawMapData.AllPen[type], drawMapData.SectionData[sectionID].X1, drawMapData.SectionData[sectionID].Y1,
                                                                                drawMapData.SectionData[sectionID].X2, drawMapData.SectionData[sectionID].Y2);
                drawMapData.SectionData[sectionID].Type = type;

                pB_Map.Image = drawMapData.ObjectAndSection;
            }
        }

        private void DrawObject()
        {
            try
            {
                drawMapData.ObjectAndSection = new Bitmap(drawMapData.MapSize.Width, drawMapData.MapSize.Height);
                drawMapData.Graphics_ObjectAndSection = Graphics.FromImage(drawMapData.ObjectAndSection);
                drawMapData.Graphics_ObjectAndSection.Clear(Color.White);

                Point[] pointArray;
                int centerX;
                int centerY;

                for (int i = 0; i < localData.TheMapInfo.ObjectDataList.Count; i++)
                {
                    pointArray = new Point[localData.TheMapInfo.ObjectDataList[i].PositionList.Count];
                    centerX = 0;
                    centerY = 0;

                    for (int j = 0; j < localData.TheMapInfo.ObjectDataList[i].PositionList.Count; j++)
                    {
                        pointArray[j] = new Point((int)drawMapData.TransferX(localData.TheMapInfo.ObjectDataList[i].PositionList[j].X),
                                                  (int)drawMapData.TransferY(localData.TheMapInfo.ObjectDataList[i].PositionList[j].Y));

                        centerX += pointArray[j].X;
                        centerY += pointArray[j].Y;
                    }

                    centerX /= localData.TheMapInfo.ObjectDataList[i].PositionList.Count;
                    centerY /= localData.TheMapInfo.ObjectDataList[i].PositionList.Count;

                    drawMapData.Graphics_ObjectAndSection.DrawPolygon(Pens.Black, pointArray);

                    int size = 12;
                    Font font = new Font("微軟正黑體", size);

                    SizeF s = drawMapData.Graphics_ObjectAndSection.MeasureString(localData.TheMapInfo.ObjectDataList[i].Name, font);

                    float x = centerX - s.Width / 2;
                    float y = centerY - s.Height / 2;

                    drawMapData.Graphics_ObjectAndSection.DrawString(localData.TheMapInfo.ObjectDataList[i].Name, font, Brushes.Black, new PointF(x, y));
                }

            }
            catch (Exception ex)
            {
                mainFlow.WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        public void DrawSection()
        {
            try
            {
                foreach (MapSection section in localData.TheMapInfo.AllSection.Values)
                {
                    drawMapData.SetSectionData(section);
                    DrawSection(section.Id, EnumSectionAction.Idle);
                }

                pB_Map.Size = drawMapData.MapSize;
                pB_Map.Image = drawMapData.ObjectAndSection;
            }
            catch (Exception ex)
            {
                mainFlow.WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        private void DrawAddressID(MapAddress address)
        {
            float y = drawMapData.TransferY(address.AGVPosition.Position.Y) + localData.MapConfig.AddressWidth / 2 + 1;

            int size = 8;

            Font font = new Font("微軟正黑體", size);

            SizeF s = drawMapData.Graphics_ObjectAndSection.MeasureString(address.Id, font);

            float x = drawMapData.TransferX(address.AGVPosition.Position.X) - s.Width / 2;

            drawMapData.Graphics_ObjectAndSection.FillRectangle(new SolidBrush(Color.White), x, y, s.Width, s.Height);

            drawMapData.Graphics_ObjectAndSection.DrawString(address.Id, font, Brushes.Black, new PointF(x, y));
        }

        public void DrawAddress()
        {
            try
            {
                AddressPicture addressPicture;

                foreach (MapAddress address in localData.TheMapInfo.AllAddress.Values)
                {
                    addressPicture = new AddressPicture(address, localData.MapConfig.AddressWidth, localData.MapConfig.AddressLineWidth,
                                   drawMapData.TransferX(address.AGVPosition.Position.X), drawMapData.TransferY(address.AGVPosition.Position.Y));

                    DrawAddressID(address);
                    drawMapData.AllAddressPicture.Add(address.Id, addressPicture);
                }

                foreach (MapAddress address in localData.TheMapInfo.AllAddress.Values)
                {
                    pB_Map.Controls.Add(drawMapData.AllAddressPicture[address.Id]);
                    drawMapData.AllAddressPicture[address.Id].PB_Address.MouseDoubleClick += AddressPictureClickEvent;
                }
            }
            catch (Exception ex)
            {
                mainFlow.WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        public void DrawAGV()
        {
            agv = new UcVehicleImage(drawMapData);
            pB_Map.Controls.Add(agv);
        }

        private MoveCommandData lastMoveCommand = null;
        private int lastSectionLineIndex = 0;
        private int lastGetReserveIndex = 0;

        private void InitialMoveCommandSectionColor(MoveCommandData temp)
        {
            for (int i = 0; i < temp.ReserveList.Count; i++)
                DrawSection(temp.ReserveList[i].SectionID, EnumSectionAction.NotGetReserve);
        }

        private void RefreshMoveCommandSectionColor()
        {
            while (lastGetReserveIndex < lastMoveCommand.ReserveList.Count && lastMoveCommand.ReserveList[lastGetReserveIndex].GetReserve)
            {
                DrawSection(lastMoveCommand.ReserveList[lastGetReserveIndex].SectionID, EnumSectionAction.GetReserve);
                lastGetReserveIndex++;
            }

            for (; lastSectionLineIndex < lastMoveCommand.IndexOflisSectionLine; lastSectionLineIndex++)
                DrawSection(lastMoveCommand.SectionLineList[lastSectionLineIndex].Section.Id, EnumSectionAction.Idle);
        }

        private void ClearMoveCommandSectionColor()
        {
            for (int i = 0; i < lastMoveCommand.ReserveList.Count; i++)
                DrawSection(lastMoveCommand.ReserveList[i].SectionID, EnumSectionAction.Idle);
        }

        private void SetSectionColor()
        {
            MoveCommandData temp = localData.MoveControlData.MoveCommand;

            if (lastMoveCommand != temp)
            {
                if (temp != null)
                    InitialMoveCommandSectionColor(temp);
                else
                    ClearMoveCommandSectionColor();

                lastMoveCommand = temp;
            }
            else
            {
                if (temp != null)
                    RefreshMoveCommandSectionColor();
            }
        }
        #endregion

        private void InitialUserControl()
        {
            loginLevelMessage = new LabelNameAndValue("LoginLevel", true);
            loginLevelMessage.Location = new Point(10, 40);
            this.Controls.Add(loginLevelMessage);

            login_Account = new LabelAndTextBox("帳號");
            login_Account.Location = new Point(300, 35);
            this.Controls.Add(login_Account);

            login_Password = new LabelAndTextBox("密碼", true);
            login_Password.Location = new Point(550, 35);
            this.Controls.Add(login_Password);

            #region 台車資訊.
            soc = new LabelNameAndValue("SOC");
            soc.Location = new Point(1216, 180);
            this.Controls.Add(soc);

            charge = new LabelNameAndValue("Charge");
            charge.Location = new Point(1216, 220);
            this.Controls.Add(charge);

            loading = new LabelNameAndValue("Loading");
            loading.Location = new Point(1216, 260);
            this.Controls.Add(loading);

            cstID = new LabelNameAndValue("CstID");
            cstID.Location = new Point(1216, 300);
            this.Controls.Add(cstID);
            #endregion

            #region 位置資訊.
            lastAddress = new LabelNameAndValue("Address");
            lastAddress.Location = new Point(1446, 180);
            this.Controls.Add(lastAddress);

            sectionID = new LabelNameAndValue("Section");
            sectionID.Location = new Point(1446, 220);
            this.Controls.Add(sectionID);

            sectionDistance = new LabelNameAndValue("Distance");
            sectionDistance.Location = new Point(1446, 260);
            this.Controls.Add(sectionDistance);

            real = new LabelNameAndValue("Real");
            real.Location = new Point(1446, 300);
            this.Controls.Add(real);
            #endregion

            #region 移動/取放 資訊.
            moveControlCommand = new LabelNameAndValue("移動命令");
            moveControlCommand.Location = new Point(1216, 360);
            this.Controls.Add(moveControlCommand);

            moveControlStatus = new LabelNameAndValue("移動狀態");
            moveControlStatus.Location = new Point(1446, 360);
            this.Controls.Add(moveControlStatus);

            loadUnloadCommand = new LabelNameAndValue("取放命令");
            loadUnloadCommand.Location = new Point(1216, 400);
            this.Controls.Add(loadUnloadCommand);

            loadUnloadStatus = new LabelNameAndValue("取放狀態");
            loadUnloadStatus.Location = new Point(1446, 400);
            this.Controls.Add(loadUnloadStatus);
            #endregion

            #region 停止/降速 資訊.
            stopSingal = new LabelNameAndValue("停車訊號", true);
            stopSingal.Location = new Point(1216, 450);
            this.Controls.Add(stopSingal);

            lowSpeedSingal = new LabelNameAndValue("降速訊號", true);
            lowSpeedSingal.Location = new Point(1216, 490);
            this.Controls.Add(lowSpeedSingal);
            #endregion
        }

        #region Form相關.
        private void InitialForm()
        {
            moveControlForm = new MoveControlForm(mainFlow.MoveControl);
            mipcViewForm = new MIPCViewForm(mainFlow.MipcControl);
            proFaceForm = new ProFaceForm(mainFlow, this);
            proFaceForm.Location = new Point(0, 0);
        }

        private void MoveControl_Click(object sender, EventArgs e)
        {
            try
            {
                moveControlForm.ShowForm();
            }
            catch { }
        }

        private void MIPC_Click(object sender, EventArgs e)
        {
            try
            {
                mipcViewForm.ShowForm();
            }
            catch { }
        }

        private void 人機畫面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                proFaceForm.ShowForm();
            }
            catch { }
        }
        #endregion


        private void timer_Tick(object sender, EventArgs e)
        {
            LoginLevelAction();
            AutoManaulAction();
            agv.UpdateLocate(localData.Real);
            button_SetPosition.Enabled = (localData.AutoManual == EnumAutoState.Manual && localData.MoveControlData.MoveCommand == null);


            #region label更新.
            loginLevelMessage.SetValueAndColor(localData.LoginLevel.ToString());
            soc.SetValueAndColor(String.Concat(localData.BatteryInfo.SOC.ToString("0.0"), " / ", localData.BatteryInfo.V.ToString("0.0"), "V"));
            charge.SetValueAndColor("");
            loading.SetValueAndColor("");
            cstID.SetValueAndColor("");

            lastAddress.SetValueAndColor(localData.Location.LastAddress);
            sectionID.SetValueAndColor(localData.Location.NowSection);
            sectionDistance.SetValueAndColor(localData.Location.DistanceFormSectionHead.ToString("0.0"));
            real.SetValueAndColor(String.Concat("( ", computeFunction.GetMapAGVPositionString(localData.Real, "0"), " )"));

            moveControlCommand.SetValueAndColor(localData.MoveControlData.CommnadID);

            bool moveControlReadyAndNotErrorBit = localData.MoveControlData.Ready && !localData.MoveControlData.ErrorBit;

            moveControlStatus.SetValueAndColor((moveControlReadyAndNotErrorBit ? "Ready" : "NotReady"), (moveControlReadyAndNotErrorBit ? 100 : 300));
            loadUnloadCommand.SetValueAndColor(localData.LoadUnloadData.CommnadID);
            loadUnloadStatus.SetValueAndColor(localData.LoadUnloadData.CommandStatus.ToString(), (int)localData.LoadUnloadData.CommandStatus);

            stopSingal.SetValueAndColor("");
            lowSpeedSingal.SetValueAndColor("");
            #endregion

            if (mainFlow.MoveControl.LocateControl.Status == EnumControlStatus.Ready)
            {
                label_LocateStatus.Text = "定位OK";
                label_LocateStatus.ForeColor = Color.Green;
            }
            else
            {
                label_LocateStatus.Text = "定位NG";
                label_LocateStatus.ForeColor = Color.Red;
            }

            tbxNowAlarm.Text = mainFlow.AlarmHandler.NowAlarm;
            tbxNowAlarmHistory.Text = mainFlow.AlarmHandler.AlarmHistory;

            pB_Alarm.BackColor = (mainFlow.AlarmHandler.HasAlarm ? Color.Red : Color.Transparent);
            pB_Warn.BackColor = (mainFlow.AlarmHandler.HasWarn ? Color.Yellow : Color.Transparent);

            SetSectionColor();
        }

        #region 登入/登出.
        private void button_Login_Click(object sender, EventArgs e)
        {
            mainFlow.UserLoginout.Login(login_Account.ValueString, login_Password.ValueString);
            login_Password.ValueString = "";
        }

        private void button_LogOut_Click(object sender, EventArgs e)
        {
            mainFlow.UserLoginout.Logout();
        }

        private void LoginLevelAction()
        {


            lastLoginLvel = localData.LoginLevel;
        }
        #endregion



        #region AutoManualAction.
        private void AutoManaulAction()
        {
            EnumAutoState newState = localData.AutoManual;

            if (newState != lastAutoState)
            {
                switch (newState)
                {
                    case EnumAutoState.Auto:
                        panel_Map.Size = new Size(1200, 650);
                        button_AutoManual.Enabled = true;
                        button_AutoManual.BackColor = Color.Green;
                        button_AutoManual.Text = EnumAutoState.Auto.ToString();
                        break;
                    case EnumAutoState.Manual:
                        panel_Map.Size = new Size(1070, 650);
                        button_AutoManual.Enabled = true;
                        button_AutoManual.BackColor = Color.Red;
                        button_AutoManual.Text = EnumAutoState.Manual.ToString();
                        break;
                    case EnumAutoState.PreAuto:
                        button_AutoManual.BackColor = Color.DarkRed;
                        button_AutoManual.Text = EnumAutoState.PreAuto.ToString();
                        button_AutoManual.Enabled = false;
                        break;
                    default:
                        button_AutoManual.Enabled = false;
                        break;
                }
            }

            lastAutoState = newState;
        }


        private void button_Alarm_Click(object sender, EventArgs e)
        {
            mainFlow.ResetAlarm();
        }
        #endregion

        #region address add/clear/send相關.
        private void AddressPictureClickEvent(object sender, MouseEventArgs e)
        {
            System.Windows.Forms.Control control = ((System.Windows.Forms.Control)sender).Parent;
            AddressPicture addressPicture = (AddressPicture)control;

            try
            {
                if (localData.AutoManual == EnumAutoState.Manual)
                {
                    lock (lockAddressListChange)
                    {
                        drawMapData.AllAddressPicture[addressPicture.AddressID].ChangeAddressBackColor();
                        MovingAddresList.Items.Add(addressPicture.AddressID);
                        if (!changeColorAddresList.Contains(addressPicture.AddressID))
                            changeColorAddresList.Add(addressPicture.AddressID);

                        addressList.Add(addressPicture.AddressID);
                    }

                    moveControlForm.SetAddress(addressPicture.AddressID);
                }
            }
            catch { }
        }

        private void button_SendToMoveControlDebug_Click(object sender, EventArgs e)
        {
            try
            {
                if (localData.AutoManual == EnumAutoState.Manual)
                {
                    moveControlForm.RecieveAddresListFromMainForm(addressList);
                    moveControlForm.ShowForm();
                }
            }
            catch { }
        }

        private void button_ClearAddressList_Click(object sender, EventArgs e)
        {
            try
            {
                lock (lockAddressListChange)
                {
                    for (int i = 0; i < changeColorAddresList.Count; i++)
                        drawMapData.AllAddressPicture[changeColorAddresList[i]].ResetBackColor();

                    changeColorAddresList = new List<string>();
                    MovingAddresList.Items.Clear();
                    addressList = new List<string>();
                }
            }
            catch { }
        }
        #endregion

        private void 關閉ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            moveControlForm.Close();
            mipcViewForm.Close();

            mainFlow.CloseMainFlowHandler();

            try
            {
                Application.Exit();
                Environment.Exit(Environment.ExitCode);
                this.Close();
            }
            catch { }
        }

        private void button_SetPosition_Click(object sender, EventArgs e)
        {
            try
            {
                if (localData.AutoManual == EnumAutoState.Manual)
                {
                    if (addressList.Count > 0)
                    {
                        string setPositionAddressID = addressList[addressList.Count - 1];
                        mainFlow.MoveControl.LocateControl.SetSLAMPositionByAddressID(setPositionAddressID);
                        button_ClearAddressList_Click(null, null);
                    }
                }
            }
            catch { }
        }

        private void btnAutoManual_Click(object sender, EventArgs e)
        {
            lock (changeAutoManualLockObject)
            {
                if (changeAutoManualThread == null || !changeAutoManualThread.IsAlive)
                {
                    changeAutoManualThread = new Thread(ChangeAutoManualThread);
                    changeAutoManualThread.Start();
                }
            }
        }

        private object changeAutoManualLockObject = new object();
        private Thread changeAutoManualThread = null;

        private void ChangeAutoManualThread()
        {
            string errorMessage = "";

            switch (localData.AutoManual)
            {
                case EnumAutoState.Auto:
                    mainFlow.ChangeAutoManual(EnumAutoState.Manual, ref errorMessage);
                    break;

                case EnumAutoState.Manual:
                    mainFlow.ChangeAutoManual(EnumAutoState.Auto, ref errorMessage);
                    break;
                case EnumAutoState.PreAuto:
                default:
                    break;
            }
        }

        private void button_BuzzOff_Click(object sender, EventArgs e)
        {
            mainFlow.AlarmHandler.BuzzOff = true;
        }
    }
}