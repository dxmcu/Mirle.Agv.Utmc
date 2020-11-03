using Mirle.Agv.INX.Controller;
using Mirle.Agv.INX.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mirle.Agv.INX.View
{
    public partial class MoveControlForm : Form
    {
        private LocalData localData = LocalData.Instance;
        private ComputeFunction computeFunction = ComputeFunction.Instance;
        private MoveControlHandler moveControl;

        private MoveCmdInfo moveCmdInfo = null;

        private List<string> originAddressList;

        #region userControl.
        #region CreateCommand.
        private LabelAndTextBox lineSpeed;
        private LabelAndTextBox otherSpeed;
        private LabelNameAndValue moveControlCommandStauts;
        private LabelNameAndValue moveControlReady;
        private LabelNameAndValue moveControlError;
        private LabelNameAndValue motionControlStatus;
        private LabelNameAndValue locateControlStatus;
        #endregion

        #region CommandList.
        private LabelNameAndValue agvVelocity;
        private LabelNameAndValue commandStatus;
        private LabelNameAndValue moveStatus;
        private LabelNameAndValue commandEncoder;
        private LabelNameAndValue real;

        private LabelNameAndValue sensorStatus;
        #endregion
        #endregion

        #region CommandList define.
        private MoveCommandData lastMoveCommand = null;
        private List<string> commandStringList = new List<string>();
        private List<string> reserveStringList = new List<string>();

        private int indexOfCommandList = 0;
        private int indexOfreserveList = 0;
        #endregion

        public MoveControlForm(MoveControlHandler moveControl)
        {
            this.moveControl = moveControl;
            InitializeComponent();

            InitialTabForm();
        }

        private void InitialTabForm()
        {
            InitialCreateCommmnadForm();
        }

        private void InitialCreateCommmnadForm()
        {
            #region CreateCommand.
            rB_SettingSpeed.Checked = true;

            lineSpeed = new LabelAndTextBox("直線速度");
            lineSpeed.Location = new Point(500, 60);
            tP_CreateMoveCommand.Controls.Add(lineSpeed);
            lineSpeed.ValueString = "400";

            otherSpeed = new LabelAndTextBox("非直線速度");
            otherSpeed.Location = new Point(500, 120);
            tP_CreateMoveCommand.Controls.Add(otherSpeed);
            otherSpeed.ValueString = "400";

            moveControlCommandStauts = new LabelNameAndValue("Command");
            moveControlCommandStauts.Location = new Point(920, 350);
            tP_CreateMoveCommand.Controls.Add(moveControlCommandStauts);

            moveControlReady = new LabelNameAndValue("Status");
            moveControlReady.Location = new Point(920, 410);
            tP_CreateMoveCommand.Controls.Add(moveControlReady);

            moveControlError = new LabelNameAndValue("Error");
            moveControlError.Location = new Point(920, 380);
            tP_CreateMoveCommand.Controls.Add(moveControlError);

            motionControlStatus = new LabelNameAndValue("Motion");
            motionControlStatus.Location = new Point(820, 60);
            tP_CreateMoveCommand.Controls.Add(motionControlStatus);

            locateControlStatus = new LabelNameAndValue("Locate");
            locateControlStatus.Location = new Point(820, 100);
            tP_CreateMoveCommand.Controls.Add(locateControlStatus);
            #endregion

            #region CommandList.
            agvVelocity = new LabelNameAndValue("Velocity");
            agvVelocity.Location = new Point(780, 410);
            tP_MoveCommand.Controls.Add(agvVelocity);

            commandStatus = new LabelNameAndValue("Command");
            commandStatus.Location = new Point(320, 448);
            tP_MoveCommand.Controls.Add(commandStatus);

            moveStatus = new LabelNameAndValue("Status");
            moveStatus.Location = new Point(550, 448);
            tP_MoveCommand.Controls.Add(moveStatus);

            commandEncoder = new LabelNameAndValue("Encoder");
            commandEncoder.Location = new Point(550, 410);
            tP_MoveCommand.Controls.Add(commandEncoder);

            real = new LabelNameAndValue("Real");
            real.Location = new Point(320, 410);
            tP_MoveCommand.Controls.Add(real);

            sensorStatus = new LabelNameAndValue("Sensor");
            sensorStatus.Location = new Point(90, 448);
            tP_MoveCommand.Controls.Add(sensorStatus);
            #endregion
        }

        #region ShowAndHide.
        public void ShowForm()
        {
            if (!timer.Enabled)
            {
                this.Show();
                timer.Enabled = true;
            }

            this.BringToFront();
        }

        private void button_Hide_Click(object sender, EventArgs e)
        {
            timer.Enabled = false;
            this.Hide();
        }
        #endregion

        #region Tabpage1-CreateCommand.
        private void UpdateTimer_Page1_CreateCommand()
        {
            label_TargetSection.Text = localData.MoveControlData.LocateControlData.TargetSection;

            MoveCommandData temp = localData.MoveControlData.MoveCommand;

            if (temp == null)
                moveControlCommandStauts.SetValueAndColor("");
            else
                moveControlCommandStauts.SetValueAndColor(temp.CommandStatus.ToString(), (int)temp.CommandStatus);

            button_AutoCycleRun.BackColor = (loopThread != null && loopThread.IsAlive ? Color.Green : Color.Transparent);

            if (localData.LoginLevel == EnumLoginLevel.MirleAdmin)
            {
                panel_CreateCommand_Hide.Visible = true;
                panel_AdminAdressIDAdd.Visible = true;
            }
            else
            {
                panel_CreateCommand_Hide.Visible = false;
                panel_AdminAdressIDAdd.Visible = false;
            }

            moveControlReady.SetReady(localData.MoveControlData.Ready);
            moveControlError.SetError(localData.MoveControlData.ErrorBit);
            motionControlStatus.SetValueAndColor(moveControl.MotionControl.Status.ToString(), (int)moveControl.MotionControl.Status);
            locateControlStatus.SetValueAndColor(moveControl.LocateControl.Status.ToString(), (int)moveControl.LocateControl.Status);
        }

        private void button_SetRealPosition_Click(object sender, EventArgs e)
        {
            try
            {
                MapAGVPosition agvPosition = new MapAGVPosition();

                double x;
                double y;
                double angle;

                if (double.TryParse(tB_CreateCommand_X.Text, out x) && double.TryParse(tB_CreateCommand_Y.Text, out y) &&
                    double.TryParse(tB_CreateCommand_Angle.Text, out angle))
                {
                    agvPosition.Position = new MapPosition(x, y);
                    agvPosition.Angle = computeFunction.GetCurrectAngle(angle);
                    moveControl.LocateControl.SetAGVPosition(agvPosition);
                }
            }
            catch { }

        }

        #region Recieve from MainForm.
        public void RecieveAddresListFromMainForm(List<string> addressList)
        {
            try
            {
                ClearList();
                originAddressList = addressList;

                for (int i = 0; i < addressList.Count; i++)
                {
                    MainForm_AddressList.Items.Add(addressList[i]);
                }

                string errorMessage = "";

                if (CheckMovingAddress(ref errorMessage))
                    SetMovingAddressList();

                tC_MoveControl.SelectedIndex = 0;
            }
            catch { }
        }

        public void SetAddress(string addressID)
        {
            try
            {
                if (localData.TheMapInfo.AllAddress.ContainsKey(addressID))
                {
                    tB_CreateCommand_X.Text = localData.TheMapInfo.AllAddress[addressID].AGVPosition.Position.X.ToString("0");
                    tB_CreateCommand_Y.Text = localData.TheMapInfo.AllAddress[addressID].AGVPosition.Position.Y.ToString("0");
                    tB_CreateCommand_Angle.Text = localData.TheMapInfo.AllAddress[addressID].AGVPosition.Angle.ToString("0");

                    if (localData.SimulateMode && localData.Real == null)
                        moveControl.LocateControl.SetAGVPosition(localData.TheMapInfo.AllAddress[addressID].AGVPosition);
                }
            }
            catch { }
        }
        #endregion
        private void button_AdmiinAddressIDAdd_Click(object sender, EventArgs e)
        {
            try
            {
                string addressID = tB_AdmiinAddressID.Text;

                if (localData.TheMapInfo.AllAddress.ContainsKey(addressID))
                {
                    if (originAddressList == null)
                        originAddressList = new List<string>();

                    originAddressList.Add(addressID);
                    MainForm_AddressList.Items.Add(addressID);
                }
            }
            catch { }
        }

        #region list處理相關.
        private void ClearList()
        {
            try
            {
                moveCmdInfo = null;
                MainForm_AddressList.Items.Clear();
                AddressList.Items.Clear();
            }
            catch { }
        }

        private void button_CheckAddress_Click(object sender, EventArgs e)
        {
            string errorMessage = "";

            if (!CheckMovingAddress(ref errorMessage))
                MessageBox.Show(String.Concat("搜尋失敗 : ", errorMessage));
            else
                SetMovingAddressList();
        }

        private bool CheckMovingAddress(ref string errorMessage)
        {
            try
            {
                moveCmdInfo = new MoveCmdInfo();

                if (moveControl.CreateMoveCommandList.Step0_CheckMovingAddress(originAddressList, ref moveCmdInfo, ref errorMessage))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                errorMessage = ex.ToString();
                return false;
            }
        }

        private void SetMovingAddressList()
        {
            try
            {
                AddressList.Items.Clear();

                MoveCmdInfo tempMoveCmdInfo = moveCmdInfo;

                if (tempMoveCmdInfo != null)
                {
                    for (int i = 0; i < tempMoveCmdInfo.MovingAddress.Count; i++)
                        AddressList.Items.Add(tempMoveCmdInfo.MovingAddress[i].Id);
                }
            }
            catch { }
        }

        private void btnClearMoveCmdInfo_Click(object sender, EventArgs e)
        {
            ClearList();
        }
        #endregion

        private void SettingSpeed(MoveCmdInfo temp)
        {
            double lineVelocity;
            double otherVelocity;

            if (!double.TryParse(lineSpeed.ValueString, out lineVelocity) || lineVelocity < localData.MoveControlData.CreateMoveCommandConfig.EQ.Velocity)
            {
                lineVelocity = 400;
                lineSpeed.ValueString = lineVelocity.ToString();
            }

            if (!double.TryParse(otherSpeed.ValueString, out otherVelocity) || otherVelocity < localData.MoveControlData.CreateMoveCommandConfig.EQ.Velocity)
            {
                otherVelocity = 400;
                otherSpeed.ValueString = otherVelocity.ToString();
            }

            double deltaAngle;

            temp.SpecifySpeed = new List<double>();
            for (int i = 0; i < temp.MovingSections.Count; i++)
            {
                if (temp.MovingSections[i].FromVehicleAngle != temp.MovingSections[i].ToVehicleAngle)
                    temp.SpecifySpeed.Add(temp.MovingSections[i].Speed);
                else
                {
                    deltaAngle = computeFunction.GetCurrectAngle(temp.MovingSections[i].FromVehicleAngle - temp.MovingSections[i].SectionAngle);

                    if (deltaAngle == 0 || deltaAngle == 180)
                        temp.SpecifySpeed.Add(lineVelocity);
                    else
                        temp.SpecifySpeed.Add(otherVelocity);
                }
            }
        }

        private void button_DebugModeSend_Click(object sender, EventArgs e)
        {
            button_DebugModeSend.Enabled = false;

            try
            {
                if (localData.AutoManual == EnumAutoState.Manual)
                {
                    MoveCmdInfo temp = moveCmdInfo;

                    if (temp != null)
                    {
                        if (rB_SettingSpeed.Checked)
                            SettingSpeed(temp);

                        temp.CommandID = string.Concat("localCommand ", DateTime.Now.ToString("MM/dd HH:mm:ss"));

                        string errorMessage = "";

                        if (moveControl.VehicleMove_DebugForm(temp, ref errorMessage))
                            tC_MoveControl.SelectedIndex = 1;
                        else
                            MessageBox.Show(String.Concat("命令產生失敗 : ", errorMessage));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception : ", ex.ToString());
            }

            button_DebugModeSend.Enabled = true;
        }
        #endregion

        #region Tabpage2-MoveCommand.
        private void UpdateTimer_Page2_MoveCommand()
        {
            agvVelocity.SetValueAndColor(String.Concat(localData.MoveControlData.MotionControlData.LineVelocity.ToString("0"), "/",
                                                       localData.MoveControlData.MotionControlData.SimulateLineVelocity.ToString("0")));

            MoveCommandData temp = localData.MoveControlData.MoveCommand;

            if (temp != null)
            {
                commandStatus.SetValueAndColor(temp.CommandStatus.ToString());
                moveStatus.SetValueAndColor(temp.MoveStatus.ToString());
                commandEncoder.SetValueAndColor(temp.CommandEncoder.ToString("0"));
                sensorStatus.SetValueAndColor(temp.SensorStatus.ToString());
            }
            else
            {
                commandStatus.SetValueAndColor("");
                moveStatus.SetValueAndColor("");
                commandEncoder.SetValueAndColor("");
                sensorStatus.SetValueAndColor("");
            }

            sensorStatus.SetValueAndColor(localData.MoveControlData.MotionControlData.AllServoStatus.ToString());

            double sleepTime = moveControl.LoopTime;

            if (sleepTime > localData.MoveControlData.MoveControlConfig.TimeValueConfig.IntervalTimeList[EnumIntervalTimeType.ThreadSleepTime] * 2)
                label_LoopTime.ForeColor = Color.Red;
            else
                label_LoopTime.ForeColor = Color.Green;

            label_LoopTime.Text = sleepTime.ToString("0");

            UpdateMoveCommandInfo(temp);

            MapAGVPosition agvPosition = localData.Real;

            real.SetValueAndColor(computeFunction.GetMapAGVPositionString(agvPosition, "0"));
        }

        private void UpdateMoveCommandInfo(MoveCommandData temp)
        {
            if (temp != lastMoveCommand)
            {
                if (temp == null)
                    ClearCommandAndReserveList();
                else
                    InitialCommandAndReserveList(temp);
            }
            else
                RefreshCommandAndReserveList(temp);

            lastMoveCommand = temp;
        }

        private void ClearCommandAndReserveList()
        {
            CommandList.Items.Clear();
            ReserveList.Items.Clear();

            commandStringList = new List<string>();
            reserveStringList = new List<string>();

            indexOfCommandList = 0;
            indexOfreserveList = 0;
        }

        private void InitialCommandAndReserveList(MoveCommandData temp)
        {
            ClearCommandAndReserveList();
            commandStringList = moveControl.CreateMoveCommandList.GetCommandList(temp);
            reserveStringList = moveControl.CreateMoveCommandList.GetReserveListAndSectionID(temp);

            for (int i = 0; i < reserveStringList.Count; i++)
                ReserveList.Items.Add(String.Concat("▷ ", reserveStringList[i]));

            for (int i = 0; i < commandStringList.Count; i++)
                CommandList.Items.Add(String.Concat("▷ ", commandStringList[i]));
        }

        private void RefreshCommandAndReserveList(MoveCommandData temp)
        {
            if (indexOfreserveList < temp.IndexOfReserveList)
            {
                for (; indexOfreserveList < temp.IndexOfReserveList; indexOfreserveList++)
                    ReserveList.Items[indexOfreserveList] = String.Concat("▶ ", reserveStringList[indexOfreserveList]);
            }

            if (indexOfCommandList < temp.IndexOfCommandList)
            {
                for (; indexOfCommandList < temp.IndexOfCommandList; indexOfCommandList++)
                    CommandList.Items[indexOfCommandList] = String.Concat("▶ ", commandStringList[indexOfCommandList]);
            }
        }

        private void button_StopMove_Click(object sender, EventArgs e)
        {
            moveControl.VehicleStop();
        }

        private void button_ResetAlarm_Click(object sender, EventArgs e)
        {
            moveControl.ResetAlarm();
        }

        private void ReserveList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (localData.AutoManual == EnumAutoState.Auto && (int)localData.LoginLevel < (int)EnumLoginLevel.MirleAdmin)
                return;

            int index = this.ReserveList.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                if (ReserveList.Items.Count < 1)
                    return;
                if (ReserveList.SelectedIndex < 0)
                    return;

                moveControl.AddReservedIndexForDebugModeTest(ReserveList.SelectedIndex);
            }
        }

        private void button_SendList_Click(object sender, EventArgs e)
        {
            MoveCommandData temp = localData.MoveControlData.MoveCommand;

            if (temp != null)
            {
                moveControl.StartCommand();

                if (cB_GetAllReserve.Checked)
                {
                    for (int i = 0; i < temp.ReserveList.Count; i++)
                        moveControl.AddReservedIndexForDebugModeTest(i);
                }
            }
        }

        private void button_CreateCommandList_Stop_Click(object sender, EventArgs e)
        {
            moveControl.VehicleStop();
        }

        #region CycleRun.
        private Thread loopThread = null;

        private void button_AutoCycleRun_Click(object sender, EventArgs e)
        {
            button_AutoCycleRun.Enabled = false;

            if (loopThread == null || !loopThread.IsAlive)
            {
                if (localData.MoveControlData.Ready)
                {
                    loopThread = new Thread(moveControl.MoveControlLocalAutoCycleRun);
                    loopThread.Start();
                }
            }
            else
                moveControl.StopAutoCycleRun = true;

            button_AutoCycleRun.Enabled = true;
        }
        #endregion
        #endregion

        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                tbxLogView.Text = moveControl.DebugFlowLog;

                switch (tC_MoveControl.SelectedIndex)
                {
                    case 0:
                        UpdateTimer_Page1_CreateCommand();
                        break;
                    case 1:
                        UpdateTimer_Page2_MoveCommand();
                        break;
                    default:
                        break;
                }
            }
            catch { }
        }

        private void button_TargetButton_Click(object sender, EventArgs e)
        {
            localData.MoveControlData.LocateControlData.TargetSection = tB_TargetSection.Text;
        }
    }
}
