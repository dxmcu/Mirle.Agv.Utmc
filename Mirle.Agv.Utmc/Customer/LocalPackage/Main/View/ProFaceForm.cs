using Mirle.Agv.INX.Controller;
using Mirle.Agv.INX.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mirle.Agv.INX.View
{
    public partial class ProFaceForm : Form
    {
        #region pageIndex.
        private const int mainPageIndex = 0;

        private const int moveControlPageIndex = 1;
        private const int moveControlPage_JogIndex = 7;
        private const int moveControlPage_MapIndex = 8;
        private const int moveControlPage_SensorDataIndex = 9;
        private const int moveControlPage_AxisDataIndex = 10;
        private const int moveControlPage_AddressIndex = 11;
        private const int moveControlPage_AddressRecordIndex = 12;
        private const int moveControlPage_LocateControlIndex = 13;
        private const int moveControlPage_CommandRecordIndexIndex = 14;

        private const int loadUnloadPageIndex = 2;
        private const int loadUnloadPage_JogIndex = 15;
        private const int loadUnloadPage_HomeIndex = 16;
        private const int loadUnloadPage_ManualCommandIndex = 17;
        private const int loadUnloadPage_AlignmentCheckIndex = 18;
        private const int loadUnloadPage_CommandRecordIndex = 19;
        private const int loadUnloadPage_PIOIndex = 20;

        private const int chargingPageIndex = 3;
        private const int chargingPage_BatteryInfoIndex = 21;
        private const int chargingPage_FlowIndex = 22;
        private const int chargingPage_PIOIndex = 23;
        private const int chargingPage_RecordIndex = 24;

        private const int ioPageIndex = 4;

        private const int alarmPageIndex = 5;

        private const int parameterPageIndex = 6;

        #endregion

        private LocalData localData = LocalData.Instance;
        private ComputeFunction computeFunction = ComputeFunction.Instance;

        private object lastSender = null;
        private Color changeColor = Color.Green;

        private KeyboardNumber keyboardNumber;

        private MainForm mainForm;
        private MainFlowHandler mainFlow;

        public ProFaceForm(MainFlowHandler mainFlow, MainForm mainForm)
        {
            this.mainFlow = mainFlow;
            this.mainForm = mainForm;
            InitializeComponent();
            button_Main.BackColor = changeColor;
            lastSender = button_Main;
            button_AlarmNow.BackColor = changeColor;

            InitialMoveControlPage();
            InitialIOControlPage();

            keyboardNumber = new KeyboardNumber();
            keyboardNumber.Location = new Point(280, 50);
            this.Controls.Add(keyboardNumber);
            keyboardNumber.BringToFront();
            keyboardNumber.Visible = false;
        }

        private void HideAll()
        {
            keyboardNumber.Hide();
        }

        private bool ChageTabControlByPageIndex(int pageIndex)
        {
            if (pageIndex < tC_Info.TabCount)
            {
                tC_Info.SelectedIndex = pageIndex;
                HideAll();
                return true;
            }
            else
                return false;
        }

        #region 下方主切換按鈕.
        private void button_Main_Click(object sender, EventArgs e)
        {
            if (ChageTabControlByPageIndex(mainPageIndex))
            {
                if (lastSender != null)
                    ((Button)lastSender).BackColor = Color.Transparent;

                lastSender = sender;
                ((Button)lastSender).BackColor = changeColor;
            }
        }

        private void button_MoveControl_Click(object sender, EventArgs e)
        {
            if (ChageTabControlByPageIndex(moveControlPageIndex))
            {
                if (lastSender != null)
                    ((Button)lastSender).BackColor = Color.Transparent;

                lastSender = sender;
                ((Button)lastSender).BackColor = changeColor;
            }
        }

        private void button_LoadUnload_Click(object sender, EventArgs e)
        {
            if (ChageTabControlByPageIndex(loadUnloadPageIndex))
            {
                if (lastSender != null)
                    ((Button)lastSender).BackColor = Color.Transparent;

                lastSender = sender;
                ((Button)lastSender).BackColor = changeColor;
            }
        }

        private void button_Charging_Click(object sender, EventArgs e)
        {
            if (ChageTabControlByPageIndex(chargingPageIndex))
            {
                if (lastSender != null)
                    ((Button)lastSender).BackColor = Color.Transparent;

                lastSender = sender;
                ((Button)lastSender).BackColor = changeColor;
            }
        }

        private void button_IO_Click(object sender, EventArgs e)
        {
            if (ChageTabControlByPageIndex(ioPageIndex))
            {
                if (lastSender != null)
                    ((Button)lastSender).BackColor = Color.Transparent;

                lastSender = sender;
                ((Button)lastSender).BackColor = changeColor;
            }
        }

        private void button_Alarm_Click(object sender, EventArgs e)
        {
            if (ChageTabControlByPageIndex(alarmPageIndex))
            {
                if (lastSender != null)
                    ((Button)lastSender).BackColor = Color.Transparent;

                lastSender = sender;
                ((Button)lastSender).BackColor = changeColor;
            }
        }

        private void button_Parameter_Click(object sender, EventArgs e)
        {
            if (ChageTabControlByPageIndex(parameterPageIndex))
            {
                if (lastSender != null)
                    ((Button)lastSender).BackColor = Color.Transparent;

                lastSender = sender;
                ((Button)lastSender).BackColor = changeColor;
            }
        }
        #endregion

        #region MoveControl切換.
        private void button_MoveControl_Jog_Click(object sender, EventArgs e)
        {
            ChageTabControlByPageIndex(moveControlPage_JogIndex);
        }

        private void button_MoveControl_Map_Click(object sender, EventArgs e)
        {
            ChageTabControlByPageIndex(moveControlPage_MapIndex);
        }

        private void button_MoveControl_SensorData_Click(object sender, EventArgs e)
        {
            ChageTabControlByPageIndex(moveControlPage_SensorDataIndex);
        }

        private void button_MoveControl_AxisData_Click(object sender, EventArgs e)
        {
            ChageTabControlByPageIndex(moveControlPage_AxisDataIndex);
        }

        private void button_MoveControl_Address_Click(object sender, EventArgs e)
        {
            ChageTabControlByPageIndex(moveControlPage_AddressIndex);
        }

        private void button_MoveControl_AddressRecord_Click(object sender, EventArgs e)
        {
            ChageTabControlByPageIndex(moveControlPage_AddressRecordIndex);
        }

        private void button_MoveControl_LocateControl_Click(object sender, EventArgs e)
        {
            ChageTabControlByPageIndex(moveControlPage_LocateControlIndex);
        }

        private void button_MoveControl_CommandRecord_Click(object sender, EventArgs e)
        {
            ChageTabControlByPageIndex(moveControlPage_CommandRecordIndexIndex);
        }
        #endregion

        #region Fork切換.
        private void button_LoadUnload_JogPitch_Click(object sender, EventArgs e)
        {
            ChageTabControlByPageIndex(loadUnloadPage_JogIndex);
        }

        private void button_LoadUnload_Home_Click(object sender, EventArgs e)
        {
            ChageTabControlByPageIndex(loadUnloadPage_HomeIndex);
        }

        private void button_LoadUnload_ManualCommand_Click(object sender, EventArgs e)
        {
            ChageTabControlByPageIndex(loadUnloadPage_ManualCommandIndex);
        }

        private void button_LoadUnload_AlignmentCheck_Click(object sender, EventArgs e)
        {
            ChageTabControlByPageIndex(loadUnloadPage_AlignmentCheckIndex);
        }

        private void button_LoadUnload_CommandRecord_Click(object sender, EventArgs e)
        {
            ChageTabControlByPageIndex(loadUnloadPage_CommandRecordIndex);
        }

        private void button_LoadUnload_PIO_Click(object sender, EventArgs e)
        {
            ChageTabControlByPageIndex(loadUnloadPage_PIOIndex);
        }
        #endregion

        #region Charging切換.
        private void button_Charging_BatteryInfo_Click(object sender, EventArgs e)
        {
            ChageTabControlByPageIndex(chargingPage_BatteryInfoIndex);
        }

        private void button_Charging_Flow_Click(object sender, EventArgs e)
        {
            ChageTabControlByPageIndex(chargingPage_FlowIndex);
        }

        private void button_Charging_PIO_Click(object sender, EventArgs e)
        {
            ChageTabControlByPageIndex(chargingPage_PIOIndex);
        }

        private void button_Charging_Record_Click(object sender, EventArgs e)
        {
            ChageTabControlByPageIndex(chargingPage_RecordIndex);
        }
        #endregion

        #region MoveControl.
        private List<EnumDefaultAxisName> axisList = new List<EnumDefaultAxisName>() { EnumDefaultAxisName.XFL, EnumDefaultAxisName.XFR, EnumDefaultAxisName.XRL, EnumDefaultAxisName.XRR };
        private Dictionary<EnumDefaultAxisName, Label> allAxisID = new Dictionary<EnumDefaultAxisName, Label>();
        private Dictionary<EnumDefaultAxisName, Label> allAxisName = new Dictionary<EnumDefaultAxisName, Label>();
        private Dictionary<EnumDefaultAxisName, Label> allAxisEncoder = new Dictionary<EnumDefaultAxisName, Label>();
        private Dictionary<EnumDefaultAxisName, Label> allAxisRPM = new Dictionary<EnumDefaultAxisName, Label>();
        private Dictionary<EnumDefaultAxisName, Label> allAxisServoOnOff = new Dictionary<EnumDefaultAxisName, Label>();
        private Dictionary<EnumDefaultAxisName, Label> allAxisEC = new Dictionary<EnumDefaultAxisName, Label>();
        private Dictionary<EnumDefaultAxisName, Label> allAxisMF = new Dictionary<EnumDefaultAxisName, Label>();
        private Dictionary<EnumDefaultAxisName, Label> allAxisV = new Dictionary<EnumDefaultAxisName, Label>();
        private Dictionary<EnumDefaultAxisName, Label> allAxisDA = new Dictionary<EnumDefaultAxisName, Label>();
        private Dictionary<EnumDefaultAxisName, Label> allAxisQA = new Dictionary<EnumDefaultAxisName, Label>();

        private LabelNameAndValue sensorData_Address;
        //private LabelNameAndValue sensorData_InAddress;
        private LabelNameAndValue sensorData_Section;
        private LabelNameAndValue sensorData_Distance;

        private LabelNameAndValue sensorData_Real;
        private LabelNameAndValue sensorData_LocationAGVPosition;
        private LabelNameAndValue sensorData_MIPCAGVPosition;

        private LabelNameAndValue sensorData_CommandID;
        private LabelNameAndValue sensorData_CommandStartTime;
        private LabelNameAndValue sensorData_CommandStstus;

        private LabelNameAndValue sensorData_MoveStatus;
        private LabelNameAndValue sensorData_CommandEncoder;
        private LabelNameAndValue sensorData_Velocity;

        private List<JogPitchLocateData> jogPitchLocateDataList = new List<JogPitchLocateData>();

        private void InitialMoveControlPage()
        {
            panel_MoveControlMap.AutoScroll = true;
            pB_MoveControlMap.Size = mainForm.GetMapSize;
            pB_MoveControlMap.Image = mainForm.GetMapImage;

            //panel_MoveControlMap = mainForm.GetPanel;

            PictureBox temp;
            foreach (AddressPicture picture in mainForm.GetAllAddressPicture.Values)
            {
                temp = new PictureBox();
                temp.Size = picture.Size;
                temp.Image = picture.Bitmap_Address;
                temp.Location = picture.Location;
                temp.BackColor = Color.White;
                pB_MoveControlMap.Controls.Add(temp);
            }

            //panel_MoveControlMap.Controls.Add(mainForm.GetMapImage);
            //pB_MoveControlMap = mainForm.GetMapImage;
            //pB_MoveControlMap.Visible = false;// = mainForm.GetMapImage;


            #region AxisData.
            int startX;
            int startY;

            Label tempLabel;

            for (int i = 0; i < axisList.Count; i++)
            {
                startY = label_AxisData_AxisID.Location.Y + (i + 1) * (label_AxisData_AxisID.Size.Height - 1);

                #region AxisID.
                startX = label_AxisData_AxisID.Location.X;
                tempLabel = new Label();

                tempLabel.AutoSize = false;
                tempLabel.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                tempLabel.Location = new System.Drawing.Point(startX, startY);
                tempLabel.Name = String.Concat(axisList[i], "_AxisID");
                tempLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                tempLabel.BorderStyle = BorderStyle.FixedSingle;
                tempLabel.Size = label_AxisData_AxisID.Size;
                tempLabel.Text = axisList[i].ToString();
                tP_MoveControl_AxisData.Controls.Add(tempLabel);
                allAxisID.Add(axisList[i], tempLabel);
                #endregion

                #region AxisName.
                startX = label_AxisData_AxisName.Location.X;
                tempLabel = new Label();

                tempLabel.AutoSize = false;
                tempLabel.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                tempLabel.Location = new System.Drawing.Point(startX, startY);
                tempLabel.Name = String.Concat(axisList[i], "_AxisName");
                tempLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                tempLabel.BorderStyle = BorderStyle.FixedSingle;
                tempLabel.Size = label_AxisData_AxisName.Size;
                tempLabel.Text = ((EnumDefaultAxisNameChinese)(int)axisList[i]).ToString();
                tP_MoveControl_AxisData.Controls.Add(tempLabel);
                allAxisName.Add(axisList[i], tempLabel);
                #endregion

                #region Encoder.
                startX = label_AxisData_Encoder.Location.X;
                tempLabel = new Label();

                tempLabel.AutoSize = false;
                tempLabel.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                tempLabel.Location = new System.Drawing.Point(startX, startY);
                tempLabel.Name = String.Concat(axisList[i], "_Encoder");
                tempLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                tempLabel.BorderStyle = BorderStyle.FixedSingle;
                tempLabel.Size = label_AxisData_Encoder.Size;
                tempLabel.Text = "";
                tP_MoveControl_AxisData.Controls.Add(tempLabel);
                allAxisEncoder.Add(axisList[i], tempLabel);
                #endregion

                #region RPM
                startX = label_AxisData_RPM.Location.X;
                tempLabel = new Label();

                tempLabel.AutoSize = false;
                tempLabel.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                tempLabel.Location = new System.Drawing.Point(startX, startY);
                tempLabel.Name = String.Concat(axisList[i], "_RPM");
                tempLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                tempLabel.BorderStyle = BorderStyle.FixedSingle;
                tempLabel.Size = label_AxisData_RPM.Size;
                tempLabel.Text = "";
                tP_MoveControl_AxisData.Controls.Add(tempLabel);
                allAxisRPM.Add(axisList[i], tempLabel);
                #endregion

                #region ServoOnOff.
                startX = label_AxisData_ServoOnOff.Location.X;
                tempLabel = new Label();

                tempLabel.AutoSize = false;
                tempLabel.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                tempLabel.Location = new System.Drawing.Point(startX, startY);
                tempLabel.Name = String.Concat(axisList[i], "_ServoOnOff");
                tempLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                tempLabel.BorderStyle = BorderStyle.FixedSingle;
                tempLabel.Size = label_AxisData_ServoOnOff.Size;
                tempLabel.Text = "";
                tP_MoveControl_AxisData.Controls.Add(tempLabel);
                allAxisServoOnOff.Add(axisList[i], tempLabel);
                #endregion

                #region EC.
                startX = label_AxisData_EC.Location.X;
                tempLabel = new Label();

                tempLabel.AutoSize = false;
                tempLabel.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                tempLabel.Location = new System.Drawing.Point(startX, startY);
                tempLabel.Name = String.Concat(axisList[i], "_EC");
                tempLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                tempLabel.BorderStyle = BorderStyle.FixedSingle;
                tempLabel.Size = label_AxisData_EC.Size;
                tempLabel.Text = "";
                tP_MoveControl_AxisData.Controls.Add(tempLabel);
                allAxisEC.Add(axisList[i], tempLabel);
                #endregion

                #region MF.
                startX = label_AxisData_MF.Location.X;
                tempLabel = new Label();

                tempLabel.AutoSize = false;
                tempLabel.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                tempLabel.Location = new System.Drawing.Point(startX, startY);
                tempLabel.Name = String.Concat(axisList[i], "_MF");
                tempLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                tempLabel.BorderStyle = BorderStyle.FixedSingle;
                tempLabel.Size = label_AxisData_MF.Size;
                tempLabel.Text = "";
                tP_MoveControl_AxisData.Controls.Add(tempLabel);
                allAxisMF.Add(axisList[i], tempLabel);
                #endregion

                #region V.
                startX = label_AxisData_V.Location.X;
                tempLabel = new Label();

                tempLabel.AutoSize = false;
                tempLabel.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                tempLabel.Location = new System.Drawing.Point(startX, startY);
                tempLabel.Name = String.Concat(axisList[i], "_V");
                tempLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                tempLabel.BorderStyle = BorderStyle.FixedSingle;
                tempLabel.Size = label_AxisData_V.Size;
                tempLabel.Text = "";
                tP_MoveControl_AxisData.Controls.Add(tempLabel);
                allAxisV.Add(axisList[i], tempLabel);
                #endregion

                #region DA.
                startX = label_AxisData_DA.Location.X;
                tempLabel = new Label();

                tempLabel.AutoSize = false;
                tempLabel.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                tempLabel.Location = new System.Drawing.Point(startX, startY);
                tempLabel.Name = String.Concat(axisList[i], "_DA");
                tempLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                tempLabel.BorderStyle = BorderStyle.FixedSingle;
                tempLabel.Size = label_AxisData_DA.Size;
                tempLabel.Text = "";
                tP_MoveControl_AxisData.Controls.Add(tempLabel);
                allAxisDA.Add(axisList[i], tempLabel);
                #endregion

                #region QA.
                startX = label_AxisData_QA.Location.X;
                tempLabel = new Label();

                tempLabel.AutoSize = false;
                tempLabel.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                tempLabel.Location = new System.Drawing.Point(startX, startY);
                tempLabel.Name = String.Concat(axisList[i], "_QA");
                tempLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                tempLabel.BorderStyle = BorderStyle.FixedSingle;
                tempLabel.Size = label_AxisData_QA.Size;
                tempLabel.Text = "";
                tP_MoveControl_AxisData.Controls.Add(tempLabel);
                allAxisQA.Add(axisList[i], tempLabel);
                #endregion
            }
            #endregion

            #region MoveControlSensorData.
            sensorData_Address = new LabelNameAndValue("Address");
            sensorData_Address.Location = new Point(30, 20);
            tP_MoveControl_SensorData.Controls.Add(sensorData_Address);

            sensorData_Section = new LabelNameAndValue("Section");
            sensorData_Section.Location = new Point(280, 20);
            tP_MoveControl_SensorData.Controls.Add(sensorData_Section);

            sensorData_Distance = new LabelNameAndValue("Distance");
            sensorData_Distance.Location = new Point(530, 20);
            tP_MoveControl_SensorData.Controls.Add(sensorData_Distance);


            sensorData_Real = new LabelNameAndValue("Real");
            sensorData_Real.Location = new Point(30, 70);
            sensorData_Real.SetLabelValueBigerr(100);
            tP_MoveControl_SensorData.Controls.Add(sensorData_Real);


            sensorData_MIPCAGVPosition = new LabelNameAndValue("MIPC");
            sensorData_MIPCAGVPosition.Location = new Point(400, 70);
            sensorData_MIPCAGVPosition.SetLabelValueBigerr(100);
            tP_MoveControl_SensorData.Controls.Add(sensorData_MIPCAGVPosition);

            sensorData_CommandID = new LabelNameAndValue("CommandID", false, 12);
            sensorData_CommandID.Location = new Point(30, 120);
            sensorData_CommandID.SetLabelValueBigerr(100);
            tP_MoveControl_SensorData.Controls.Add(sensorData_CommandID);

            sensorData_LocationAGVPosition = new LabelNameAndValue("Locate");
            sensorData_LocationAGVPosition.Location = new Point(400, 120);
            sensorData_LocationAGVPosition.SetLabelValueBigerr(100);
            tP_MoveControl_SensorData.Controls.Add(sensorData_LocationAGVPosition);



            sensorData_CommandStartTime = new LabelNameAndValue("StartTime");
            sensorData_CommandStartTime.Location = new Point(280, 170);
            tP_MoveControl_SensorData.Controls.Add(sensorData_CommandStartTime);

            sensorData_CommandStstus = new LabelNameAndValue("CmdStatus", false, 12);
            sensorData_CommandStstus.Location = new Point(30, 170);
            tP_MoveControl_SensorData.Controls.Add(sensorData_CommandStstus);


            sensorData_MoveStatus = new LabelNameAndValue("MoveStatus", false, 12);
            sensorData_MoveStatus.Location = new Point(30, 220);
            tP_MoveControl_SensorData.Controls.Add(sensorData_MoveStatus);

            sensorData_CommandEncoder = new LabelNameAndValue("CmdEncoder", false, 12);
            sensorData_CommandEncoder.Location = new Point(280, 220);
            tP_MoveControl_SensorData.Controls.Add(sensorData_CommandEncoder);

            sensorData_Velocity = new LabelNameAndValue("Velocity");
            sensorData_Velocity.Location = new Point(530, 220);
            tP_MoveControl_SensorData.Controls.Add(sensorData_Velocity);
            #endregion

            #region LocateDriver.
            TabPage tempTabPage;
            JogPitchLocateData tempJogPitchLocateData;

            for (int i = 0; i < mainFlow.MoveControl.LocateControl.LocateControlDriverList.Count; i++)
            {
                tempJogPitchLocateData = new JogPitchLocateData(mainFlow.MoveControl.LocateControl.LocateControlDriverList[i]);
                tempJogPitchLocateData.Size = new Size(800, 400);

                tempTabPage = new TabPage();
                tempTabPage.Text = mainFlow.MoveControl.LocateControl.LocateControlDriverList[i].DriverConfig.Device;

                tempTabPage.Controls.Add(tempJogPitchLocateData);

                tC_LocateDriverList.TabPages.Add(tempTabPage);
                jogPitchLocateDataList.Add(tempJogPitchLocateData);
            }

            //tC_LocateDriverList

            #endregion

        }


        private void button_JoystickValueEnable_Click(object sender, EventArgs e)
        {
            HideAll();

            if (localData.AutoManual == EnumAutoState.Manual && localData.MoveControlData.MoveCommand == null)
                mainFlow.MipcControl.JogjoystickOnOff(true);
        }

        private void button_JoystickValueDisable_Click(object sender, EventArgs e)
        {
            HideAll();

            if (localData.AutoManual == EnumAutoState.Manual && localData.MoveControlData.MoveCommand == null)
                mainFlow.MipcControl.JogjoystickOnOff(false);
        }

        private void button_JogPitch_Set_Click(object sender, EventArgs e)
        {
            button_JogPitch_Set.Enabled = false;
            HideAll();
            double lineVelocity;
            double lineAcc;
            double lineDec;

            double thetaVelocity;
            double thetaAcc;
            double thetaDec;

            if (!double.TryParse(tB_JogPitch_LineVelocity.Text, out lineVelocity) || lineVelocity <= 0)
                lineVelocity = -1;

            if (!double.TryParse(tB_JogPitch_LineAcc.Text, out lineAcc) || lineAcc <= 0)
                lineAcc = -1;

            if (!double.TryParse(tB_JogPitch_LineDec.Text, out lineDec) || lineDec <= 0)
                lineDec = -1;

            if (!double.TryParse(tB_JogPitch_ThetaVelocity.Text, out thetaVelocity) || thetaVelocity <= 0)
                thetaVelocity = -1;

            if (!double.TryParse(tB_JogPitch_ThetaAcc.Text, out thetaAcc) || thetaAcc <= 0)
                thetaAcc = -1;

            if (!double.TryParse(tB_JogPitch_ThetaDec.Text, out thetaDec) || thetaDec <= 0)
                thetaDec = -1;

            mainFlow.MipcControl.WriteJogjoystickData(lineVelocity, lineAcc, lineDec, thetaVelocity, thetaAcc, thetaDec);

            button_JogPitch_Set.Enabled = true;
        }

        private void button_SetPosition_Click(object sender, EventArgs e)
        {
            button_SetPosition.Enabled = false;
            mainFlow.MoveControl.LocateControl.SetSlamPositionAuto();
            button_SetPosition.Enabled = true;
        }

        private void button_WriteSlamPosition_Click(object sender, EventArgs e)
        {
            button_WriteSlamPosition.Enabled = false;
            mainFlow.MoveControl.LocateControl.WriteSlamPositionAll();
            button_WriteSlamPosition.Enabled = true;
        }

        private void button_JogPitch_Clear_Click(object sender, EventArgs e)
        {
            HideAll();
            tB_JogPitch_LineVelocity.Text = "";
            tB_JogPitch_LineAcc.Text = "";
            tB_JogPitch_LineDec.Text = "";
            tB_JogPitch_ThetaVelocity.Text = "";
            tB_JogPitch_ThetaAcc.Text = "";
            tB_JogPitch_ThetaDec.Text = "";
        }

        private void UpdateMoveControlJogPitch()
        {
            if (localData.MoveControlData.MotionControlData.JoystickMode)
            {
                label_JogPitch_JoystickValue.Text = "開啟中";
                label_JogPitch_JoystickValue.ForeColor = Color.Green;
            }
            else
            {
                label_JogPitch_JoystickValue.Text = "關閉中";
                label_JogPitch_JoystickValue.ForeColor = Color.Red;
            }

            pB_JogPitch_AllServoOn.BackColor = (mainFlow.MoveControl.MotionControl.AllServoOn ? Color.Green : Color.Transparent);
            pB_JogPitch_AllServoOff.BackColor = (mainFlow.MoveControl.MotionControl.AllServoOff ? Color.Red : Color.Transparent);

            label_JogPitch_LineVelocityValue.Text = localData.MoveControlData.MotionControlData.JoystickLineAxisData.Velocity.ToString("0");
            label_JogPitch_LineAccValue.Text = localData.MoveControlData.MotionControlData.JoystickLineAxisData.Acceleration.ToString("0");
            label_JogPitch_LineDecValue.Text = localData.MoveControlData.MotionControlData.JoystickLineAxisData.Deceleration.ToString("0");

            label_JogPitch_ThetaVelocityValue.Text = localData.MoveControlData.MotionControlData.JoystickThetaAxisData.Velocity.ToString("0");
            label_JogPitch_ThetaAccValue.Text = localData.MoveControlData.MotionControlData.JoystickThetaAxisData.Acceleration.ToString("0");
            label_JogPitch_ThetaDecValue.Text = localData.MoveControlData.MotionControlData.JoystickThetaAxisData.Deceleration.ToString("0");

            ThetaSectionDeviation temp = localData.MoveControlData.ThetaSectionDeviation;

            if (temp != null)
            {
                label_JogPitch_SectionDeviationValue.Text = temp.SectionDeviation.ToString("0.0");
                label_JogPitch_SectionThetaValue.Text = temp.Theta.ToString("0.0");

                if (Math.Abs(temp.SectionDeviation) <= localData.MoveControlData.MoveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseSectionDeviationLine].Range / 2)
                    label_JogPitch_SectionDeviationValue.ForeColor = Color.Green;
                else
                    label_JogPitch_SectionDeviationValue.ForeColor = Color.Red;

                if (Math.Abs(temp.Theta) <= localData.MoveControlData.MoveControlConfig.Safety[EnumMoveControlSafetyType.OntimeReviseTheta].Range / 2)
                    label_JogPitch_SectionThetaValue.ForeColor = Color.Green;
                else
                    label_JogPitch_SectionThetaValue.ForeColor = Color.Red;
            }
            else
            {
                label_JogPitch_SectionDeviationValue.Text = "---";
                label_JogPitch_SectionDeviationValue.ForeColor = Color.Red;
                label_JogPitch_SectionThetaValue.Text = "---";
                label_JogPitch_SectionThetaValue.ForeColor = Color.Red;
            }

            if (localData.MoveControlData.MoveControlCanAuto)
            {
                label_JogPitch_CanAuto.Text = "可以Auto";
                label_JogPitch_CanAuto.ForeColor = Color.Green;
                label_JogPitch_CantAutoReason.Text = "";
            }
            else
            {
                label_JogPitch_CanAuto.Text = "不能Auto";
                label_JogPitch_CanAuto.ForeColor = Color.Red;
                label_JogPitch_CantAutoReason.Text = localData.MoveControlData.MoveControlCantAutoReason;
            }

            label_JogPitch_RealPositionValue.Text = computeFunction.GetMapAGVPositionStringWithAngle(localData.Real);
        }

        private void UpdateMoveControlLocateControl()
        {
            int index = tC_LocateDriverList.SelectedIndex;

            if (tC_LocateDriverList.TabPages.Count > 0 && index < tC_LocateDriverList.TabPages.Count)
            {
                jogPitchLocateDataList[index].UpdateData();
            }
        }

        private void UpdateMoveControlMapPage()
        {

        }

        private void SetLabelTextAndColor(Label label, EnumVehicleSafetyAction type)
        {
            label.Text = type.ToString();

            switch (type)
            {
                case EnumVehicleSafetyAction.Normal:
                    label.ForeColor = Color.Green;
                    break;
                case EnumVehicleSafetyAction.LowSpeed_High:
                    label.ForeColor = Color.Yellow;
                    break;
                case EnumVehicleSafetyAction.LowSpeed_Low:
                    label.ForeColor = Color.OrangeRed;
                    break;
                case EnumVehicleSafetyAction.SlowStop:
                    label.ForeColor = Color.Red;
                    break;
            }
        }

        private void SetLabelTextAndColor(Label label, EnumSafetyLevel type)
        {
            label.Text = type.ToString();

            switch (type)
            {
                case EnumSafetyLevel.Alarm:
                case EnumSafetyLevel.EMO:
                case EnumSafetyLevel.IPCEMO:
                    label.ForeColor = Color.DarkRed;
                    break;
                case EnumSafetyLevel.EMS:
                case EnumSafetyLevel.SlowStop:
                    label.ForeColor = Color.Red;

                    break;
                case EnumSafetyLevel.LowSpeed_Low:
                    label.ForeColor = Color.OrangeRed;

                    break;
                case EnumSafetyLevel.LowSpeed_High:
                    label.ForeColor = Color.Yellow;
                    break;
                default:
                    label.ForeColor = Color.Green;
                    break;
            }
        }

        private void UpdateMoveControlSensorData()
        {
            VehicleLocation location = localData.Location;

            if (localData != null)
            {
                sensorData_Address.SetValueAndColor(location.LastAddress, (location.InAddress ? 100 : 0));
                sensorData_Section.SetValueAndColor(location.NowSection);
                sensorData_Distance.SetValueAndColor(location.DistanceFormSectionHead.ToString("0"));
            }
            else
            {
                sensorData_Address.SetValueAndColor("");
                sensorData_Section.SetValueAndColor("");
                sensorData_Distance.SetValueAndColor("");
            }

            sensorData_Real.SetValueAndColor(computeFunction.GetMapAGVPositionStringWithAngle(localData.Real));
            sensorData_LocationAGVPosition.SetValueAndColor(computeFunction.GetLocateAGVPositionStringWithAngle(localData.MoveControlData.LocateControlData.LocateAGVPosition));
            sensorData_MIPCAGVPosition.SetValueAndColor(computeFunction.GetLocateAGVPositionStringWithAngle(localData.MoveControlData.MotionControlData.EncoderAGVPosition));

            MoveCommandData command = localData.MoveControlData.MoveCommand;

            if (command != null)
            {
                sensorData_CommandID.SetValueAndColor(command.CommandID);
                sensorData_CommandStartTime.SetValueAndColor(command.StartTime.ToString("HH:mm:ss"));
                sensorData_CommandStstus.SetValueAndColor(command.CommandStatus.ToString());
                sensorData_MoveStatus.SetValueAndColor(command.MoveStatus.ToString(), (int)command.MoveStatus);
                sensorData_CommandEncoder.SetValueAndColor(command.CommandEncoder.ToString("0"));
                SetLabelTextAndColor(label_MoveControlSensorData_SensorSstatusValue, command.SensorStatus);
                SetLabelTextAndColor(label_MoveControlSensorData_ReserveValue, (command.WaitReserveIndex == -1 ? EnumVehicleSafetyAction.Normal : EnumVehicleSafetyAction.SlowStop));
                SetLabelTextAndColor(label_MoveControlSensorData_PauseValue, command.AGVPause);
            }
            else
            {
                sensorData_CommandID.SetValueAndColor("");
                sensorData_CommandStartTime.SetValueAndColor("");
                sensorData_CommandStstus.SetValueAndColor("");
                sensorData_MoveStatus.SetValueAndColor("");
                sensorData_CommandEncoder.SetValueAndColor("");
                label_MoveControlSensorData_SensorSstatusValue.Text = "";
                label_MoveControlSensorData_ReserveValue.Text = "";
                label_MoveControlSensorData_PauseValue.Text = "";
            }

            SetLabelTextAndColor(label_MoveControlSensorData_SafetySensorValue, localData.MIPCData.SafetySensorStatus);
            SetLabelTextAndColor(label_MoveControlSensorData_LocalPauseValue, localData.MoveControlData.SensorStatus.LocalPause);

            sensorData_Velocity.SetValueAndColor(String.Concat(localData.MoveControlData.MotionControlData.LineVelocity.ToString("0"), "/",
                                                                     localData.MoveControlData.MotionControlData.SimulateLineVelocity.ToString("0")));
        }

        private string lastCommandID = "";

        private void UpdateMoveControlMoveCommandRecord()
        {
            lock (localData.MoveControlData.LockMoveCommandRecordObject)
            {
                if (lastCommandID != localData.MoveControlData.LastCommandID)
                {
                    listBox_MoveCommandRecordString.Items.Clear();

                    for (int i = 0; i < localData.MoveControlData.MoveCommandRecordList.Count; i++)
                        listBox_MoveCommandRecordString.Items.Add(localData.MoveControlData.MoveCommandRecordList[i].LogString);

                    lastCommandID = localData.MoveControlData.LastCommandID;
                }
            }
        }

        private void listBox_MoveCommandRecordString_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = listBox_MoveCommandRecordString.IndexFromPoint(e.Location);

            if (index >= 0)
            {
                lock (localData.MoveControlData.LockMoveCommandRecordObject)
                {
                    if (index < localData.MoveControlData.MoveCommandRecordList.Count)
                    {
                        MessageBox.Show(localData.MoveControlData.MoveCommandRecordList[index].CommandID);
                    }
                }
            }
        }

        private void UpdateMoveControlAxisData()
        {
            if (localData.MoveControlData.MotionControlData.AllAxisFeedbackData != null)
            {
                AxisFeedbackData temp;

                for (int i = 0; i < axisList.Count; i++)
                {
                    if (localData.MoveControlData.MotionControlData.AllAxisFeedbackData.ContainsKey(axisList[i]))
                    {
                        temp = localData.MoveControlData.MotionControlData.AllAxisFeedbackData[axisList[i]];

                        if (temp != null)
                        {
                            allAxisEncoder[axisList[i]].Text = temp.Position.ToString("0");
                            allAxisRPM[axisList[i]].Text = temp.Velocity.ToString("0.0");
                            allAxisServoOnOff[axisList[i]].Text = temp.AxisServoOnOff.ToString();
                            allAxisEC[axisList[i]].Text = temp.EC.ToString();
                            allAxisMF[axisList[i]].Text = temp.MF.ToString();
                            allAxisV[axisList[i]].Text = temp.V.ToString("0.0");
                            allAxisDA[axisList[i]].Text = temp.DA.ToString("0.000");
                            allAxisQA[axisList[i]].Text = temp.QA.ToString("0.000");
                        }
                    }
                }
            }
        }

        #endregion

        #region IO Page.
        private LabelList safetySensorTitle;
        private LabelList safetySensorStatus;
        private List<LabelList> allSafetySensor = new List<LabelList>();

        public void InitialIOControlPage()
        {
            safetySensorTitle = new LabelList();
            safetySensorTitle.Location = new Point(2, 10);
            tP_SafetySensor.Controls.Add(safetySensorTitle);
            safetySensorTitle.SetLabelByStringList(new List<string>()
            {
                "DeviceName", EnumSafetyLevel.Alarm.ToString(),
                EnumSafetyLevel.Warn.ToString(), EnumSafetyLevel.EMO.ToString(),
                EnumSafetyLevel.IPCEMO.ToString(), EnumSafetyLevel.EMS.ToString(),
                "緩停", "降速(低)",
                "降速(高)", EnumSafetyLevel.Normal.ToString()
            });

            safetySensorStatus = new LabelList();
            safetySensorStatus.Location = new Point(2, 44);
            tP_SafetySensor.Controls.Add(safetySensorStatus);
            safetySensorStatus.SetLabelByStringList(new List<string>() { "Status" });

            panel_SafetySensorList.AutoScroll = true;

            LabelList temp;

            for (int i = 0; i < mainFlow.MipcControl.SafetySensorControl.AllSafetySensor.Count; i++)
            {
                temp = new LabelList();
                temp.Location = new Point(1, i * 34);
                panel_SafetySensorList.Controls.Add(temp);
                temp.SetLabelByStringList(new List<string>() { mainFlow.MipcControl.SafetySensorControl.AllSafetySensor[i].Config.Device });
                allSafetySensor.Add(temp);
            }
        }

        public void UpdateIOPage()
        {
            if (tC_IO.SelectedIndex == 0)
            {
                safetySensorStatus.SetLabelBackColorByUint(mainFlow.MipcControl.SafetySensorControl.AllStatus);

                for (int i = 0; i < mainFlow.MipcControl.SafetySensorControl.AllSafetySensor.Count; i++)
                    allSafetySensor[i].SetLabelBackColorByUint(mainFlow.MipcControl.SafetySensorControl.AllSafetySensor[i].Status);
            }
        }
        #endregion


        #region Alarm Page.
        private bool showAlarmNow = true;

        private void UpdateAlarmPage()
        {
            tbxNowAlarm.Text = (showAlarmNow ? mainFlow.AlarmHandler.NowAlarm : mainFlow.AlarmHandler.AlarmHistory);
        }

        private void button_AlarmNow_Click(object sender, EventArgs e)
        {
            button_AlarmNow.BackColor = changeColor;
            button_AlarmHistory.BackColor = Color.Transparent;

            showAlarmNow = true;
        }

        private void button_AlarmHistory_Click(object sender, EventArgs e)
        {
            button_AlarmNow.BackColor = Color.Transparent;
            button_AlarmHistory.BackColor = changeColor;

            showAlarmNow = false;
        }

        private void button_ResetAlarm_Click(object sender, EventArgs e)
        {
            mainFlow.ResetAlarm();
        }

        private void button_BuzzOff_Click(object sender, EventArgs e)
        {
            mainFlow.AlarmHandler.BuzzOff = true;
        }

        private void pB_Warn_Click(object sender, EventArgs e)
        {
            if (mainFlow.AlarmHandler.HasWarn)
                button_Alarm_Click((object)button_Alarm, e);
            else
                HideAll();
        }

        private void pB_Alarm_Click(object sender, EventArgs e)
        {
            if (mainFlow.AlarmHandler.HasAlarm)
                button_Alarm_Click((object)button_Alarm, e);
            else
                HideAll();
        }
        #endregion

        public void ShowForm()
        {
            if (!timer.Enabled)
            {
                this.Show();
                timer.Enabled = true;
            }

            this.BringToFront();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            pB_Alarm.BackColor = (mainFlow.AlarmHandler.HasAlarm ? Color.Red : Color.Transparent);
            pB_Warn.BackColor = (mainFlow.AlarmHandler.HasWarn ? Color.Yellow : Color.Transparent);
            label_SOC.Text = String.Concat("SOC : ", localData.BatteryInfo.SOC.ToString("0"), "% 電壓 : ", localData.BatteryInfo.V.ToString("0.0"), "V");
            label_LoginLevel.Text = String.Concat("LoginLevel : ", localData.LoginLevel.ToString());

            label_AutoManual.Text = localData.AutoManual.ToString();
            label_AutoManual.BackColor = (localData.AutoManual == EnumAutoState.Auto ? Color.Green : Color.Red);

            switch (tC_Info.SelectedIndex)
            {
                #region Main.
                case mainPageIndex:
                    break;
                #endregion

                #region MoveControl.
                case moveControlPageIndex:
                    break;
                case moveControlPage_JogIndex:
                    UpdateMoveControlJogPitch();
                    break;
                case moveControlPage_MapIndex:
                    UpdateMoveControlMapPage();
                    break;
                case moveControlPage_SensorDataIndex:
                    UpdateMoveControlSensorData();
                    break;
                case moveControlPage_AxisDataIndex:
                    UpdateMoveControlAxisData();
                    break;
                case moveControlPage_AddressIndex:
                    break;
                case moveControlPage_AddressRecordIndex:
                    break;
                case moveControlPage_LocateControlIndex:
                    UpdateMoveControlLocateControl();
                    break;
                case moveControlPage_CommandRecordIndexIndex:
                    UpdateMoveControlMoveCommandRecord();
                    break;
                #endregion

                #region LoadUnload.
                case loadUnloadPageIndex:
                    break;
                case loadUnloadPage_JogIndex:
                    break;
                case loadUnloadPage_HomeIndex:
                    break;
                case loadUnloadPage_ManualCommandIndex:
                    break;
                case loadUnloadPage_AlignmentCheckIndex:
                    break;
                case loadUnloadPage_CommandRecordIndex:
                    break;
                case loadUnloadPage_PIOIndex:
                    break;
                #endregion

                #region Charging.
                case chargingPageIndex:
                    break;
                case chargingPage_BatteryInfoIndex:
                    break;
                case chargingPage_FlowIndex:
                    break;
                case chargingPage_PIOIndex:
                    break;
                case chargingPage_RecordIndex:
                    break;
                #endregion

                #region IO.
                case ioPageIndex:
                    UpdateIOPage();
                    break;
                #endregion

                #region Alarm.
                case alarmPageIndex:
                    UpdateAlarmPage();
                    break;
                #endregion

                #region Parameter.
                case parameterPageIndex:
                    break;
                #endregion

                default:
                    break;
            }
        }

        private void CallKeyboardNumber(object sender, EventArgs e)
        {
            keyboardNumber.SetTextBoxAndShow((TextBox)sender);
            keyboardNumber.BringToFront();
        }

        private void Hide_Click(object sender, EventArgs e)
        {
            HideAll();
        }

        private void label_MoveControlSensorData_LocalPauseValue_Click(object sender, EventArgs e)
        {
            if (localData.LoginLevel >= EnumLoginLevel.Admin)
            {
                if (localData.MoveControlData.SensorStatus.localPause == EnumVehicleSafetyAction.SlowStop)
                    localData.MoveControlData.SensorStatus.LocalPause = EnumVehicleSafetyAction.Normal;
                else
                    localData.MoveControlData.SensorStatus.LocalPause = EnumVehicleSafetyAction.SlowStop;
            }
        }
    }
}
