using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.Agv.INX.Controller;
using Mirle.Agv.INX.Model;
using System.Threading;

namespace Mirle.Agv.INX.View
{
    public partial class MIPCMotionCommandForm : UserControl
    {
        private LocalData localData = LocalData.Instance;
        private MIPCControlHandler mipcControl;
        private ComputeFunction computeFunction = ComputeFunction.Instance;


        public MIPCMotionCommandForm(MIPCControlHandler mipcControl)
        {
            this.mipcControl = mipcControl;
            InitializeComponent();
        }

        private void button_CommandStart_Click(object sender, EventArgs e)
        {
            //if (localData.MoveControlData.MotionControlData.MoveStatus != EnumAxisMoveStatus.Stop)
            //    return;

            button_CommandStart.Enabled = false;

            if (localData.AutoManual == EnumAutoState.Manual && localData.MoveControlData.MoveCommand == null && localData.LoadUnloadData.CommandStatus == EnumCommandStatus.Idle)
            {
                double mapX;
                double mapY;
                double mapTheta;
                double lineVelocity;
                double lineAcc;
                double lineDec;
                double lineJerk;
                double thetaVelocity;
                double thetaAcc;
                double thetaDec;
                double thetaJerk;

                if (double.TryParse(tB_X.Text, out mapX) && double.TryParse(tB_Y.Text, out mapY) && double.TryParse(tB_Angle.Text, out mapTheta) &&
                    double.TryParse(tB_LineVelocity.Text, out lineVelocity) && double.TryParse(tB_LineAcc.Text, out lineAcc) &&
                     double.TryParse(tB_LineDec.Text, out lineDec) && double.TryParse(tB_LineJerk.Text, out lineJerk) &&
                     double.TryParse(tB_ThetaVelocity.Text, out thetaVelocity) && double.TryParse(tB_ThetaAcc.Text, out thetaAcc) &&
                     double.TryParse(tB_ThetaDec.Text, out thetaDec) && double.TryParse(tB_ThetaJerk.Text, out thetaJerk) &&
                     lineVelocity > 0 && lineAcc > 0 && lineDec > 0 && lineJerk > 0 &&
                     thetaVelocity > 0 && thetaAcc > 0 && thetaDec > 0 && thetaJerk > 0)
                {
                    MapAGVPosition agvPosition = new MapAGVPosition();
                    agvPosition.Position = new MapPosition(mapX, mapY);
                    agvPosition.Angle = mapTheta;

                    mipcControl.AGV_Move(agvPosition, lineVelocity, lineAcc, lineDec, lineJerk, thetaVelocity, thetaAcc, thetaDec, thetaJerk);
                }
                else
                    MessageBox.Show("有資料輸入錯誤");
            }
            else
            {
                MessageBox.Show("Auto 或 半自動移動中 無法使用");
            }

            button_CommandStart.Enabled = true;
        }

        public void UpdateData()
        {
            try
            {
                button_Start.BackColor = ((cycleRunTestThread != null && cycleRunTestThread.IsAlive) ? Color.Red : Color.Transparent);

                label_FeedbackNowValue.Text = computeFunction.GetLocateAGVPositionStringWithAngle(localData.MoveControlData.MotionControlData.EncoderAGVPosition);
                label_SLAMLocateValue.Text = computeFunction.GetLocateAGVPositionStringWithAngle(localData.MoveControlData.LocateControlData.LocateAGVPosition);

                if (localData.MoveControlData.MotionControlData.MoveStatus != EnumAxisMoveStatus.Stop)
                {
                    label_MoveStatusValue.Text = "Move";
                    label_MoveStatusValue.ForeColor = Color.Red;
                }
                else
                {
                    label_MoveStatusValue.Text = "Stop";
                    label_MoveStatusValue.ForeColor = Color.Green;
                }

                if (cycleRunTestThread != null && !cycleRunTestThread.IsAlive)
                    button_Start.Enabled = true;
            }
            catch
            {

            }
        }

        #region CycleRunTest.
        private List<MapAGVPosition> cycleRunMovingAGVPosition = new List<MapAGVPosition>();
        private List<MapAGVPosition> cycleRunMovingAGVPositionRun;
        private double cycleRunLineVelocity;
        private double cycleRunLineAcc;
        private double cycleRunLineDec;
        private double cycleRunLineJerk;
        private double cycleRunThetaVelocity;
        private double cycleRunThetaAcc;
        private double cycleRunThetaDec;
        private double cycleRunThetaJerk;
        private Thread cycleRunTestThread;
        private bool cycleRunStop = false;
        private bool goNext = false;
        private bool waitGoNext = false;
        private bool pass = false;

        private void button_Add_Click(object sender, EventArgs e)
        {
            double x;
            double y;
            double theta;

            if (double.TryParse(tB_CycleRunX.Text, out x) && double.TryParse(tB_CycleRunY.Text, out y) && double.TryParse(tB_CycleRunTheta.Text, out theta))
            {
                MapAGVPosition temp = new MapAGVPosition();
                temp.Angle = theta;
                temp.Position = new MapPosition(x, y);
                cycleRunMovingAGVPosition.Add(temp);
                listBox_MovingPoint.Items.Add(computeFunction.GetMapAGVPositionStringWithAngle(temp));
            }
        }

        private void button_Clear_Click(object sender, EventArgs e)
        {
            cycleRunMovingAGVPosition = new List<MapAGVPosition>();
            listBox_MovingPoint.Items.Clear();
        }

        private void CycleRunThread()
        {
            try
            {
                int index = 0;
                goNext = false;
                pass = false;

                while (!cycleRunStop)
                {
                    if (localData.MoveControlData.MotionControlData.MoveStatus == EnumAxisMoveStatus.Stop)
                    {
                        if (!waitGoNext)
                        {
                            waitGoNext = true;
                            goNext = false;
                        }

                        if (pass || (waitGoNext && goNext))
                        {
                            waitGoNext = false;

                            if (index < cycleRunMovingAGVPositionRun.Count)
                            {
                                mipcControl.AGV_Move(cycleRunMovingAGVPositionRun[index], cycleRunLineVelocity, cycleRunLineAcc, cycleRunLineDec, cycleRunLineJerk,
                                                     cycleRunThetaVelocity, cycleRunThetaAcc, cycleRunThetaDec, cycleRunThetaJerk);
                                index++;
                                Thread.Sleep(1000);
                            }
                            else
                                break;
                        }
                    }

                    Thread.Sleep(50);
                }
            }
            catch { }
        }

        private void button_Start_Click(object sender, EventArgs e)
        {
            cycleRunMovingAGVPositionRun = cycleRunMovingAGVPosition;

            if (cycleRunMovingAGVPositionRun.Count > 0 &&
                double.TryParse(tB_LineVelocity.Text, out cycleRunLineVelocity) && cycleRunLineVelocity > 0 &&
                double.TryParse(tB_LineAcc.Text, out cycleRunLineAcc) && cycleRunLineAcc > 0 &&
                double.TryParse(tB_LineDec.Text, out cycleRunLineDec) && cycleRunLineDec > 0 &&
                double.TryParse(tB_LineJerk.Text, out cycleRunLineJerk) && cycleRunLineJerk > 0 &&
                double.TryParse(tB_ThetaVelocity.Text, out cycleRunThetaVelocity) && cycleRunThetaVelocity > 0 &&
                double.TryParse(tB_ThetaAcc.Text, out cycleRunThetaAcc) && cycleRunThetaAcc > 0 &&
                double.TryParse(tB_ThetaDec.Text, out cycleRunThetaDec) && cycleRunThetaDec > 0 &&
                double.TryParse(tB_ThetaJerk.Text, out cycleRunThetaJerk) && cycleRunThetaJerk > 0)
            {
                if (localData.AutoManual == EnumAutoState.Manual && localData.MoveControlData.MoveCommand == null && localData.LoadUnloadData.CommandStatus == EnumCommandStatus.Idle)
                {
                    if (cycleRunTestThread == null || !cycleRunTestThread.IsAlive)
                    {
                        cycleRunStop = false;
                        cycleRunTestThread = new Thread(CycleRunThread);
                        cycleRunTestThread.Start();
                    }
                }
                else
                    MessageBox.Show("必須在Manual且MoveControlIdle狀態下才可使用");
            }
            else
                MessageBox.Show("參數格式錯誤");
        }

        private void button_NextStep_Click(object sender, EventArgs e)
        {
            goNext = true;
        }

        private void button_AllPass_Click(object sender, EventArgs e)
        {
            pass = true;
        }

        private void button_Stop_Click(object sender, EventArgs e)
        {
            cycleRunStop = true;

            double lineDec;
            double lineJerk;
            double thetaDec;
            double thetaJerk;

            if (double.TryParse(tB_LineDec.Text, out lineDec) && lineDec > 0 &&
                double.TryParse(tB_LineJerk.Text, out lineJerk) && lineJerk > 0 &&
                double.TryParse(tB_ThetaDec.Text, out thetaDec) && thetaDec > 0 &&
                double.TryParse(tB_ThetaJerk.Text, out thetaJerk) && thetaJerk > 0)
                ;
            else
            {
                lineDec = 200;
                lineJerk = 400;
                thetaDec = 10;
                thetaJerk = 20;
            }

            mipcControl.AGV_Stop(lineDec, lineJerk, thetaDec, thetaJerk);
        }
        #endregion

        private void button_TurnStart_Click(object sender, EventArgs e)
        {
            double x;
            double y;
            double angle;

            double R;
            double RAngle;
            double velocity;
            double movingAngle;
            double deltaAngle;

            if (double.TryParse(tB_TurnX.Text, out x) && double.TryParse(tB_TurnY.Text, out y) && double.TryParse(tB_TurnAngle.Text, out angle) &&
                double.TryParse(tB_TurnR.Text, out R) && R > 0 && double.TryParse(tB_TurnRTheta.Text, out RAngle) && double.TryParse(tB_TurnMovingVelocity.Text, out velocity) && velocity > 0 &&
                double.TryParse(tB_TurnMovingAngle.Text, out movingAngle) && double.TryParse(tB_TurnDletaAngle.Text, out deltaAngle))
            {
                MapAGVPosition start = new MapAGVPosition();
                start.Angle = angle;
                start.Position.X = x;
                start.Position.Y = y;

                mipcControl.AGV_Turn(start, R, RAngle, velocity, movingAngle, deltaAngle);
            }
        }

        private void button_Case1_Click(object sender, EventArgs e)
        {
            List<MapAGVPosition> data = new List<MapAGVPosition>();

            data.Add(new MapAGVPosition(new MapPosition(2500, -700), 0));
            data.Add(new MapAGVPosition(new MapPosition(2500, 1300), 0));
            data.Add(new MapAGVPosition(new MapPosition(5000, 1300), 0));
            data.Add(new MapAGVPosition(new MapPosition(5000, -700), 0));
            data.Add(new MapAGVPosition(new MapPosition(2500, -700), 0));
            data.Add(new MapAGVPosition(new MapPosition(2500, 1300), 0));
            data.Add(new MapAGVPosition(new MapPosition(0, 1300), 0));
            data.Add(new MapAGVPosition(new MapPosition(0, -700), 0));

            double lvel = 200;
            double lacc = 200;
            double ldec = 200;
            double tvel = 10;
            double tacc = 10;
            double tdec = 10;
            SetCaseData(data, lvel, lacc, ldec, tvel, tacc, tdec);
        }

        private void button_Case2_Click(object sender, EventArgs e)
        {
            List<MapAGVPosition> data = new List<MapAGVPosition>();

            data.Add(new MapAGVPosition(new MapPosition(4500, -1000), 0));
            data.Add(new MapAGVPosition(new MapPosition(-5500, -10000), 0));

            double lvel = 200;
            double lacc = 200;
            double ldec = 200;
            double tvel = 10;
            double tacc = 10;
            double tdec = 10;
            SetCaseData(data, lvel, lacc, ldec, tvel, tacc, tdec);
        }

        private void SetCaseData(List<MapAGVPosition> data, double lvel, double lacc, double ldec, double tvel, double tacc, double tdec)
        {
            try
            {
                tB_LineVelocity.Text = lvel.ToString("0");
                tB_LineAcc.Text = lacc.ToString("0");
                tB_LineDec.Text = ldec.ToString("0");
                tB_ThetaVelocity.Text = tvel.ToString("0");
                tB_ThetaAcc.Text = tacc.ToString("0");
                tB_ThetaDec.Text = tdec.ToString("0");

                mipcControl.AGV_ServoOn();
                button_Clear_Click(null, null);

                for (int i = 0; i < data.Count; i++)
                {
                    cycleRunMovingAGVPosition.Add(data[i]);
                    listBox_MovingPoint.Items.Add(computeFunction.GetMapAGVPositionStringWithAngle(data[i]));
                }
            }
            catch { }
        }

        private void button_ServoOn_Click(object sender, EventArgs e)
        {
            mipcControl.AGV_ServoOn();
        }

        private void button_ServoOff_Click(object sender, EventArgs e)
        {
            mipcControl.AGV_ServoOff();
        }
    }
}