using Mirle.Agv.INX.Controller.Tools;
using Mirle.Agv.INX.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Mirle.Agv.INX.Controller
{
    public class SimulateControl
    {
        private Thread pollingThread;
        private bool close = false;
        private int treadSleepTime = 30;

        private LoggerAgent loggerAgent = LoggerAgent.Instance;

        private string normalLogName = "";
        private string device = MethodInfo.GetCurrentMethod().ReflectedType.Name;

        private LocalData localData = LocalData.Instance;
        private ComputeFunction computeFunction = ComputeFunction.Instance;


        public SimulateControl(string normalLogName)
        {
            this.normalLogName = normalLogName;

            pollingThread = new Thread(SimulateVelocityThread);
            pollingThread.Start();
        }

        public void CloseSimulate()
        {
            close = true;
        }

        private void WriteLog(int logLevel, string carrierId, string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            LogFormat logFormat = new LogFormat(normalLogName, logLevel.ToString(), memberName, device, carrierId, message);

            loggerAgent.Log(logFormat.Category, logFormat);

            if (logLevel <= localData.ErrorLevel)
            {
                logFormat = new LogFormat(localData.ErrorLogName, logLevel.ToString(), memberName, device, carrierId, message);
                loggerAgent.Log(logFormat.Category, logFormat);
            }
        }

        private bool GetSimulateVelocityByTime_All(AxisData axisData, SimulateData[] simulateDataList, double time, ref double velocity)
        {
            if (time < simulateDataList[0].EndTime)
            {
                time = time - simulateDataList[0].StartTime;
                velocity = axisData.Jerk * Math.Pow(time, 2) / 2;
            }
            else if (time < simulateDataList[1].EndTime)
            {
                time = time - simulateDataList[1].StartTime;
                velocity = simulateDataList[1].StartVelocity + simulateDataList[1].StartAcc * time;
            }
            else if (time < simulateDataList[2].EndTime)
            {
                time = simulateDataList[2].EndTime - time;
                velocity = simulateDataList[2].EndVelocity - axisData.Jerk * Math.Pow(time, 2) / 2;
            }
            else if (time < simulateDataList[3].EndTime)
            {
                time = time - simulateDataList[3].StartTime;
                velocity = simulateDataList[3].StartVelocity;
            }
            else if (time < simulateDataList[4].EndTime)
            {
                time = time - simulateDataList[4].StartTime;
                velocity = simulateDataList[4].StartVelocity - axisData.Jerk * Math.Pow(time, 2) / 2;
            }
            else if (time < simulateDataList[5].EndTime)
            {
                time = time - simulateDataList[5].StartTime;
                velocity = simulateDataList[5].StartVelocity - simulateDataList[5].StartAcc * time;
            }
            else if (time < simulateDataList[6].EndTime)
            {
                time = simulateDataList[6].EndTime - time;
                velocity = axisData.Jerk * Math.Pow(time, 2) / 2;
            }
            else
            {
                velocity = 0;
                return true;
            }

            return false;
        }

        private void WriteVChangeLog(EnumSimulateVelocityType type, double newVelocity)
        {
            SimulateVelocityData temp = simulateVelocityData;

            if (temp == null)
                return;

            string message = String.Concat("VChange Type : ", type.ToString(), ", newVelocity : ", newVelocity.ToString("0"), ", Time : ", temp.Time.ToString("mm:ss.fff"));

            for (int i = 0; i < temp.SimulateDataList.Count; i++)
            {
                message = String.Concat(message, "\r\n", "index : ", i.ToString(),
                    ", Type : ", temp.SimulateDataList[i].Type.ToString(),
                    ", StartTime : ", temp.SimulateDataList[i].StartTime.ToString("0.00"),
                    ", EndTime : ", temp.SimulateDataList[i].EndTime.ToString("0.00"),
                    ", DeltaTime : ", temp.SimulateDataList[i].DeltaTime.ToString("0.00"),
                    ", StartAcc : ", temp.SimulateDataList[i].StartAcc.ToString("0.00"),
                    ", EndAcc : ", temp.SimulateDataList[i].EndAcc.ToString("0.00"),
                    ", StartVelocity : ", temp.SimulateDataList[i].StartVelocity.ToString("0.00"),
                    ", EndVelocity : ", temp.SimulateDataList[i].EndVelocity.ToString("0.00"),
                    ", DeltaVelocity : ", temp.SimulateDataList[i].DeltaVelocity.ToString("0.00"),
                    ", Jerk : ", temp.SimulateDataList[i].Jerk.ToString("0.00"));
            }

            WriteLog(7, "", message);
        }

        private SimulateVelocityData simulateVelocityData = null;

        #region 設定移動的模擬參數.
        private void CheckAxisDataOK(ref AxisData axisData, bool notEQMove)
        {
            double accJerkTime = axisData.Acceleration / axisData.Jerk;
            if (2 * axisData.Jerk * Math.Pow(accJerkTime, 2) > axisData.Velocity)
                axisData.Acceleration = Math.Sqrt(axisData.Jerk * axisData.Velocity);

            double decJerkTime = axisData.Deceleration / axisData.Jerk;
            if (2 * axisData.Jerk * Math.Pow(decJerkTime, 2) > axisData.Velocity)
                axisData.Deceleration = Math.Sqrt(axisData.Jerk * axisData.Velocity);

            if (!notEQMove)
            {
                double accDistance = computeFunction.GetAccDecDistance(0, axisData.Velocity, axisData.Acceleration, axisData.Jerk);

                if (2 * accDistance > axisData.Position)
                {
                    //axisData.Velocity = Math.Pow(Math.Pow(axisData.Position, 2) * axisData.Jerk / 4, (1.0 / 3));
                    axisData.Velocity = Math.Pow(Math.Pow(axisData.Position, 2) * axisData.Jerk / 4, (1.0 / 3));

                    accJerkTime = axisData.Acceleration / axisData.Jerk;
                    if (2 * axisData.Jerk * Math.Pow(accJerkTime, 2) > axisData.Velocity)
                        axisData.Acceleration = Math.Sqrt(axisData.Jerk * axisData.Velocity);

                    decJerkTime = axisData.Deceleration / axisData.Jerk;
                    if (2 * axisData.Jerk * Math.Pow(decJerkTime, 2) > axisData.Velocity)
                        axisData.Deceleration = Math.Sqrt(axisData.Jerk * axisData.Velocity);
                }
            }
        }

        private void SetSimulateParameter(ref SimulateVelocityData simulateData, AxisData axisData, bool notEQMove)
        {
            simulateData.SimulateDataList.Add(new SimulateData()); // index 0
            simulateData.SimulateDataList.Add(new SimulateData()); // index 1
            simulateData.SimulateDataList.Add(new SimulateData()); // index 2
            simulateData.SimulateDataList.Add(new SimulateData()); // index 3
            simulateData.SimulateDataList.Add(new SimulateData()); // index 4
            simulateData.SimulateDataList.Add(new SimulateData()); // index 5
            simulateData.SimulateDataList.Add(new SimulateData()); // index 6
            simulateData.SimulateDataList.Add(new SimulateData()); // index 7

            CheckAxisDataOK(ref axisData, notEQMove);
            simulateData.SimulateDataList[0].StartTime = 0;
            simulateData.SimulateDataList[0].StartPosition = 0;
            simulateData.SimulateDataList[0].StartAcc = 0;
            simulateData.SimulateDataList[0].StartVelocity = 0;

            simulateData.SimulateDataList[0].DeltaTime = axisData.Acceleration / axisData.Jerk;
            simulateData.SimulateDataList[0].EndTime = simulateData.SimulateDataList[0].StartTime + simulateData.SimulateDataList[0].DeltaTime;
            simulateData.SimulateDataList[0].EndAcc = axisData.Acceleration;
            simulateData.SimulateDataList[0].DeltaVelocity = axisData.Jerk * Math.Pow(simulateData.SimulateDataList[0].DeltaTime, 2) / 2;// 1/2 jerk * t^2
            simulateData.SimulateDataList[0].EndVelocity = simulateData.SimulateDataList[0].StartVelocity + simulateData.SimulateDataList[0].DeltaVelocity;
            simulateData.SimulateDataList[0].DeltaPosition = axisData.Jerk * Math.Pow(simulateData.SimulateDataList[0].DeltaTime, 3) / 6;// 1/6 jerk * t^3
            simulateData.SimulateDataList[0].EndPosition = simulateData.SimulateDataList[0].StartPosition + simulateData.SimulateDataList[0].DeltaPosition;
            simulateData.SimulateDataList[0].Jerk = axisData.Jerk;
            simulateData.SimulateDataList[0].Type = EnumSimulateVelocityType.AccJerkUp;

            simulateData.SimulateDataList[2].DeltaTime = axisData.Acceleration / axisData.Jerk;
            simulateData.SimulateDataList[2].EndAcc = 0;
            simulateData.SimulateDataList[2].EndVelocity = axisData.Velocity;
            simulateData.SimulateDataList[2].DeltaVelocity = axisData.Jerk * Math.Pow(simulateData.SimulateDataList[2].DeltaTime, 2) / 2;// 1/2 jerk * t^2
            simulateData.SimulateDataList[2].DeltaPosition = simulateData.SimulateDataList[2].EndVelocity * simulateData.SimulateDataList[2].DeltaTime - axisData.Jerk * Math.Pow(simulateData.SimulateDataList[2].DeltaTime, 3) / 6;// 1/6 jerk * t^3
            simulateData.SimulateDataList[2].Jerk = axisData.Jerk;
            simulateData.SimulateDataList[2].Type = EnumSimulateVelocityType.AccJerkDown;

            simulateData.SimulateDataList[1].StartAcc = simulateData.SimulateDataList[0].EndAcc;
            simulateData.SimulateDataList[1].StartTime = simulateData.SimulateDataList[0].EndTime;
            simulateData.SimulateDataList[1].StartPosition = simulateData.SimulateDataList[0].EndPosition;
            simulateData.SimulateDataList[1].StartVelocity = simulateData.SimulateDataList[0].EndVelocity;
            simulateData.SimulateDataList[1].DeltaVelocity = axisData.Velocity - simulateData.SimulateDataList[0].DeltaVelocity - simulateData.SimulateDataList[2].DeltaVelocity;
            simulateData.SimulateDataList[1].EndAcc = simulateData.SimulateDataList[1].StartAcc;
            simulateData.SimulateDataList[1].EndVelocity = simulateData.SimulateDataList[1].StartVelocity + simulateData.SimulateDataList[1].DeltaVelocity;
            simulateData.SimulateDataList[1].DeltaTime = simulateData.SimulateDataList[1].DeltaVelocity / simulateData.SimulateDataList[1].StartAcc;
            simulateData.SimulateDataList[1].EndTime = simulateData.SimulateDataList[1].StartTime + simulateData.SimulateDataList[1].DeltaTime;
            simulateData.SimulateDataList[1].DeltaPosition = simulateData.SimulateDataList[1].DeltaTime * (simulateData.SimulateDataList[1].StartVelocity + simulateData.SimulateDataList[1].EndVelocity) / 2;
            simulateData.SimulateDataList[1].EndPosition = simulateData.SimulateDataList[1].StartPosition + simulateData.SimulateDataList[1].DeltaPosition;
            simulateData.SimulateDataList[1].Jerk = axisData.Jerk;
            simulateData.SimulateDataList[1].Type = EnumSimulateVelocityType.Accing;

            simulateData.SimulateDataList[2].StartTime = simulateData.SimulateDataList[1].EndTime;
            simulateData.SimulateDataList[2].StartVelocity = simulateData.SimulateDataList[1].EndVelocity;
            simulateData.SimulateDataList[2].StartPosition = simulateData.SimulateDataList[1].EndPosition;
            simulateData.SimulateDataList[2].StartAcc = simulateData.SimulateDataList[1].EndAcc;
            simulateData.SimulateDataList[2].EndTime = simulateData.SimulateDataList[2].StartTime + simulateData.SimulateDataList[2].DeltaTime;
            simulateData.SimulateDataList[2].EndPosition = simulateData.SimulateDataList[2].StartPosition + simulateData.SimulateDataList[2].DeltaPosition;

            simulateData.SimulateDataList[4].DeltaTime = axisData.Deceleration / axisData.Jerk;
            simulateData.SimulateDataList[4].StartVelocity = simulateData.SimulateDataList[2].EndVelocity;
            simulateData.SimulateDataList[4].StartAcc = 0;
            simulateData.SimulateDataList[4].EndAcc = axisData.Deceleration;
            simulateData.SimulateDataList[4].DeltaVelocity = axisData.Jerk * Math.Pow(simulateData.SimulateDataList[4].DeltaTime, 2) / 2;
            simulateData.SimulateDataList[4].EndVelocity = simulateData.SimulateDataList[4].StartVelocity - simulateData.SimulateDataList[4].DeltaVelocity;
            simulateData.SimulateDataList[4].DeltaPosition = simulateData.SimulateDataList[4].StartVelocity * simulateData.SimulateDataList[4].DeltaTime - axisData.Jerk * Math.Pow(simulateData.SimulateDataList[4].DeltaTime, 3) / 6;// 1/6 jerk * t^3
            simulateData.SimulateDataList[4].Jerk = axisData.Jerk;
            simulateData.SimulateDataList[4].Type = EnumSimulateVelocityType.DecJerkUp;

            simulateData.SimulateDataList[6].EndVelocity = 0;
            simulateData.SimulateDataList[6].EndAcc = 0;
            simulateData.SimulateDataList[6].StartAcc = simulateData.SimulateDataList[4].EndAcc;
            simulateData.SimulateDataList[6].EndPosition = axisData.Position;
            simulateData.SimulateDataList[6].DeltaTime = axisData.Deceleration / axisData.Jerk;
            simulateData.SimulateDataList[6].DeltaVelocity = axisData.Jerk * Math.Pow(simulateData.SimulateDataList[6].DeltaTime, 2) / 2;// 1/2 jerk * t^2
            simulateData.SimulateDataList[6].DeltaPosition = axisData.Jerk * Math.Pow(simulateData.SimulateDataList[6].DeltaTime, 3) / 6;// 1/6 jerk * t^3
            simulateData.SimulateDataList[6].StartVelocity = simulateData.SimulateDataList[6].EndVelocity + simulateData.SimulateDataList[6].DeltaVelocity;
            simulateData.SimulateDataList[6].StartPosition = simulateData.SimulateDataList[6].EndPosition - simulateData.SimulateDataList[6].DeltaPosition;
            simulateData.SimulateDataList[6].Jerk = axisData.Jerk;
            simulateData.SimulateDataList[6].Type = EnumSimulateVelocityType.DecJerkDown;

            simulateData.SimulateDataList[5].StartVelocity = simulateData.SimulateDataList[4].EndVelocity;
            simulateData.SimulateDataList[5].EndVelocity = simulateData.SimulateDataList[6].StartVelocity;
            simulateData.SimulateDataList[5].StartAcc = simulateData.SimulateDataList[4].EndAcc;
            simulateData.SimulateDataList[5].EndAcc = simulateData.SimulateDataList[6].StartAcc;
            simulateData.SimulateDataList[5].EndPosition = simulateData.SimulateDataList[6].StartPosition;
            simulateData.SimulateDataList[5].DeltaVelocity = simulateData.SimulateDataList[5].StartVelocity - simulateData.SimulateDataList[5].EndVelocity;
            simulateData.SimulateDataList[5].DeltaTime = simulateData.SimulateDataList[5].DeltaVelocity / simulateData.SimulateDataList[5].StartAcc;
            simulateData.SimulateDataList[5].DeltaPosition = simulateData.SimulateDataList[5].DeltaTime * (simulateData.SimulateDataList[5].StartVelocity + simulateData.SimulateDataList[5].EndVelocity) / 2;
            simulateData.SimulateDataList[5].StartPosition = simulateData.SimulateDataList[5].EndPosition - simulateData.SimulateDataList[5].DeltaPosition;
            simulateData.SimulateDataList[5].Jerk = axisData.Jerk;
            simulateData.SimulateDataList[5].Type = EnumSimulateVelocityType.Decing;

            simulateData.SimulateDataList[4].EndPosition = simulateData.SimulateDataList[5].StartPosition;
            simulateData.SimulateDataList[4].StartPosition = simulateData.SimulateDataList[4].EndPosition - simulateData.SimulateDataList[4].DeltaPosition;

            simulateData.SimulateDataList[3].StartPosition = simulateData.SimulateDataList[2].EndPosition;
            simulateData.SimulateDataList[3].EndPosition = simulateData.SimulateDataList[4].StartPosition;
            simulateData.SimulateDataList[3].DeltaPosition = simulateData.SimulateDataList[3].EndPosition - simulateData.SimulateDataList[3].StartPosition;
            simulateData.SimulateDataList[3].StartVelocity = simulateData.SimulateDataList[2].EndVelocity;
            simulateData.SimulateDataList[3].EndVelocity = simulateData.SimulateDataList[3].StartVelocity;
            simulateData.SimulateDataList[3].DeltaVelocity = 0;
            simulateData.SimulateDataList[3].StartAcc = 0;
            simulateData.SimulateDataList[3].EndAcc = 0;
            simulateData.SimulateDataList[3].StartTime = simulateData.SimulateDataList[2].EndTime;
            simulateData.SimulateDataList[3].DeltaTime = simulateData.SimulateDataList[3].DeltaPosition / simulateData.SimulateDataList[3].StartVelocity;
            simulateData.SimulateDataList[3].EndTime = simulateData.SimulateDataList[3].StartTime + simulateData.SimulateDataList[3].DeltaTime;
            simulateData.SimulateDataList[3].Jerk = axisData.Jerk;
            simulateData.SimulateDataList[3].Type = EnumSimulateVelocityType.Isokinetic;

            simulateData.SimulateDataList[4].StartTime = simulateData.SimulateDataList[3].EndTime;
            simulateData.SimulateDataList[4].EndTime = simulateData.SimulateDataList[4].StartTime + simulateData.SimulateDataList[4].DeltaTime;

            simulateData.SimulateDataList[5].StartTime = simulateData.SimulateDataList[4].EndTime;
            simulateData.SimulateDataList[5].EndTime = simulateData.SimulateDataList[5].StartTime + simulateData.SimulateDataList[5].DeltaTime;

            simulateData.SimulateDataList[6].StartTime = simulateData.SimulateDataList[5].EndTime;
            simulateData.SimulateDataList[6].EndTime = simulateData.SimulateDataList[6].StartTime + simulateData.SimulateDataList[6].DeltaTime;

            simulateData.SimulateDataList[7].StartTime = simulateData.SimulateDataList[6].EndTime;
            simulateData.SimulateDataList[7].EndTime = simulateData.SimulateDataList[6].EndTime + 1;
            simulateData.SimulateDataList[7].StartVelocity = simulateData.SimulateDataList[6].EndVelocity;
            simulateData.SimulateDataList[7].EndVelocity = simulateData.SimulateDataList[6].EndVelocity;
        }
        #endregion

        #region 下移動命令.
        public void SetMoveCommandSimulateData(double distance, double velocity, double acc, double dec, double jerk, bool notEQMove = true)
        {
            SimulateVelocityData temp = new SimulateVelocityData();

            AxisData axis = new AxisData(acc, dec, jerk, velocity, Math.Abs(distance));
            temp.Command = axis;
            axis = new AxisData(acc, dec, jerk, velocity, Math.Abs(distance));
            temp.Axis = axis;
            axis = new AxisData(acc, dec, jerk, velocity, Math.Abs(distance));
            SetSimulateParameter(ref temp, axis, notEQMove);
            temp.Time = DateTime.Now;
            simulateVelocityData = temp;

            WriteVChangeLog(EnumSimulateVelocityType.Isokinetic, velocity);
        }
        #endregion

        private double GetNowAccOrDec(SimulateData simulateData, double time)
        {
            if (time > simulateData.EndTime)
                return 0;

            switch (simulateData.Type)
            {
                case EnumSimulateVelocityType.AccJerkUp:
                    time = time - simulateData.StartTime;
                    return simulateData.StartAcc + simulateData.Jerk * time;

                case EnumSimulateVelocityType.Accing:
                    return simulateData.StartAcc;

                case EnumSimulateVelocityType.AccJerkDown:
                    time = time - simulateData.StartTime;
                    return simulateData.StartAcc - simulateData.Jerk * time;

                case EnumSimulateVelocityType.Isokinetic:
                    return 0;

                case EnumSimulateVelocityType.DecJerkUp:
                    time = time - simulateData.StartTime;
                    return simulateData.StartAcc + simulateData.Jerk * time;

                case EnumSimulateVelocityType.Decing:
                    return simulateData.StartAcc;

                case EnumSimulateVelocityType.DecJerkDown:
                    time = time - simulateData.StartTime;
                    return simulateData.StartAcc - simulateData.Jerk * time;

                default:
                    return 0;
            }
        }

        private double GetNowVelocity(SimulateData simulateData, double time)
        {
            if (time > simulateData.EndTime)
                return simulateData.EndVelocity;

            switch (simulateData.Type)
            {
                case EnumSimulateVelocityType.AccJerkUp:
                    time = time - simulateData.StartTime;
                    return simulateData.StartVelocity + simulateData.Jerk * Math.Pow(time, 2) / 2;

                case EnumSimulateVelocityType.Accing:
                    time = time - simulateData.StartTime;
                    return simulateData.StartVelocity + simulateData.StartAcc * time;

                case EnumSimulateVelocityType.AccJerkDown:
                    time = simulateData.EndTime - time;
                    return simulateData.EndVelocity - simulateData.Jerk * Math.Pow(time, 2) / 2;

                case EnumSimulateVelocityType.DecJerkUp:
                    time = time - simulateData.StartTime;
                    return simulateData.StartVelocity - simulateData.Jerk * Math.Pow(time, 2) / 2;

                case EnumSimulateVelocityType.Decing:
                    time = time - simulateData.StartTime;
                    return simulateData.StartVelocity - simulateData.StartAcc * time;

                case EnumSimulateVelocityType.DecJerkDown:
                    time = simulateData.EndTime - time;
                    return simulateData.EndVelocity + simulateData.Jerk * Math.Pow(time, 2) / 2;

                case EnumSimulateVelocityType.Isokinetic:
                default:
                    return simulateData.EndVelocity;
            }
        }

        private void CheckAxisDataOKWithVelocityChange(ref AxisData axisData, double nowVelocity, double nextVelocity)
        {
            double deltaVelocity = nextVelocity - nowVelocity;

            if (deltaVelocity < 0)
            {
                double decJerkTime = axisData.Deceleration / axisData.Jerk;
                if (2 * axisData.Jerk * Math.Pow(decJerkTime, 2) > Math.Abs(deltaVelocity))
                    axisData.Deceleration = Math.Sqrt(axisData.Jerk * Math.Abs(deltaVelocity));
            }
            else if (deltaVelocity > 0)
            {
                double accJerkTime = axisData.Acceleration / axisData.Jerk;
                if (2 * axisData.Jerk * Math.Pow(accJerkTime, 2) > Math.Abs(deltaVelocity))
                    axisData.Acceleration = Math.Sqrt(axisData.Jerk * Math.Abs(deltaVelocity));
            }
        }

        #region Acc.
        private SimulateVelocityData SetVChange_Acc(SimulateVelocityData nowData, AxisData axis, SimulateData data, DateTime time)
        {
            double nowAcc = GetNowAccOrDec(data, (time - nowData.Time).TotalSeconds);
            double deltaVelocity = Math.Pow(nowAcc, 2) / (2 * axis.Jerk);
            double nowVelocity = GetNowVelocity(data, (time - nowData.Time).TotalSeconds);
            double isokineticVelocity = nowVelocity + deltaVelocity;
            double startVelocity = nowVelocity - deltaVelocity;
            double startTime = -nowAcc / axis.Jerk; ;

            SimulateVelocityData returnData = new SimulateVelocityData();
            SimulateData temp;
            returnData.Command = nowData.Command;
            returnData.Time = time;

            if (axis.Velocity > isokineticVelocity)
            {   // 升速度.
                CheckAxisDataOKWithVelocityChange(ref axis, startVelocity, axis.Velocity);

                temp = new SimulateData();
                temp.SetSimulateData_AccJerkUp(startVelocity, axis.Acceleration, axis.Jerk, startTime);
                returnData.SimulateDataList.Add(temp);

                temp = new SimulateData();
                temp.SetSimulateData_Accing(returnData.SimulateDataList[returnData.SimulateDataList.Count - 1], axis);
                returnData.SimulateDataList.Add(temp);

                temp = new SimulateData();
                temp.SetSimulateData_AccJerkDown(returnData.SimulateDataList[returnData.SimulateDataList.Count - 1].EndVelocity,
                                                 axis.Acceleration, axis.Jerk,
                                                 returnData.SimulateDataList[returnData.SimulateDataList.Count - 1].EndTime);
                returnData.SimulateDataList.Add(temp);
            }
            else if (axis.Velocity < isokineticVelocity)
            {   // 減速度.
                CheckAxisDataOKWithVelocityChange(ref axis, startVelocity, isokineticVelocity);
                CheckAxisDataOKWithVelocityChange(ref axis, isokineticVelocity, axis.Velocity);

                temp = new SimulateData();
                temp.SetSimulateData_AccJerkDown(nowVelocity, axis.Acceleration, axis.Jerk, 0);
                returnData.SimulateDataList.Add(temp);

                temp = new SimulateData();
                temp.SetSimulateData_DecJerkUp(isokineticVelocity, axis.Deceleration, axis.Jerk,
                                               returnData.SimulateDataList[returnData.SimulateDataList.Count - 1].EndTime);
                returnData.SimulateDataList.Add(temp);

                temp = new SimulateData();
                temp.SetSimulateData_Decing(returnData.SimulateDataList[returnData.SimulateDataList.Count - 1], axis);
                returnData.SimulateDataList.Add(temp);

                temp = new SimulateData();
                temp.SetSimulateData_DecJerkDown(returnData.SimulateDataList[returnData.SimulateDataList.Count - 1].EndVelocity,
                                                 axis.Deceleration, axis.Jerk,
                                                 returnData.SimulateDataList[returnData.SimulateDataList.Count - 1].EndTime);
                returnData.SimulateDataList.Add(temp);
            }
            else
            {   // 等速度.
                CheckAxisDataOKWithVelocityChange(ref axis, startVelocity, axis.Velocity);

                temp = new SimulateData();
                temp.SetSimulateData_AccJerkDown(nowVelocity, axis.Acceleration, axis.Jerk, 0);
                returnData.SimulateDataList.Add(temp);
            }

            temp = new SimulateData();
            temp.SetSimulateData_Isokinetic(returnData.SimulateDataList[returnData.SimulateDataList.Count - 1].EndVelocity,
                                            returnData.SimulateDataList[returnData.SimulateDataList.Count - 1].EndTime);
            returnData.SimulateDataList.Add(temp);

            returnData.Axis = axis;
            return returnData;
        }
        #endregion

        #region Isokinetic.
        private SimulateVelocityData SetVChange_Isokinetic(SimulateVelocityData nowData, AxisData axis, SimulateData data, DateTime time)
        {
            double nowVelocity = GetNowVelocity(data, (time - nowData.Time).TotalSeconds);
            double startTime = 0;

            SimulateVelocityData returnData = new SimulateVelocityData();
            SimulateData temp;
            returnData.Command = nowData.Command;
            returnData.Time = time;

            if (axis.Velocity > nowVelocity)
            {   // 升速度.
                CheckAxisDataOKWithVelocityChange(ref axis, nowVelocity, axis.Velocity);

                temp = new SimulateData();
                temp.SetSimulateData_AccJerkUp(nowVelocity, axis.Acceleration, axis.Jerk, startTime);
                returnData.SimulateDataList.Add(temp);

                temp = new SimulateData();
                temp.SetSimulateData_Accing(returnData.SimulateDataList[returnData.SimulateDataList.Count - 1], axis);
                returnData.SimulateDataList.Add(temp);

                temp = new SimulateData();
                temp.SetSimulateData_AccJerkDown(returnData.SimulateDataList[returnData.SimulateDataList.Count - 1].EndVelocity,
                                                 axis.Acceleration, axis.Jerk,
                                                 returnData.SimulateDataList[returnData.SimulateDataList.Count - 1].EndTime);
                returnData.SimulateDataList.Add(temp);
            }
            else if (axis.Velocity < nowVelocity)
            {   // 減速度.
                CheckAxisDataOKWithVelocityChange(ref axis, nowVelocity, axis.Velocity);

                temp = new SimulateData();
                temp.SetSimulateData_DecJerkUp(nowVelocity, axis.Deceleration, axis.Jerk, startTime);
                returnData.SimulateDataList.Add(temp);

                temp = new SimulateData();
                temp.SetSimulateData_Decing(returnData.SimulateDataList[returnData.SimulateDataList.Count - 1], axis);
                returnData.SimulateDataList.Add(temp);

                temp = new SimulateData();
                temp.SetSimulateData_DecJerkDown(returnData.SimulateDataList[returnData.SimulateDataList.Count - 1].EndVelocity,
                                                 axis.Deceleration, axis.Jerk,
                                                 returnData.SimulateDataList[returnData.SimulateDataList.Count - 1].EndTime);
                returnData.SimulateDataList.Add(temp);
            }
            else
            {   // 等速度.
                temp = new SimulateData();
                temp.SetSimulateData_Isokinetic(axis.Velocity, startTime);
                returnData.SimulateDataList.Add(temp);
            }

            temp = new SimulateData();
            temp.SetSimulateData_Isokinetic(returnData.SimulateDataList[returnData.SimulateDataList.Count - 1].EndVelocity,
                                            returnData.SimulateDataList[returnData.SimulateDataList.Count - 1].EndTime);
            returnData.SimulateDataList.Add(temp);

            returnData.Axis = axis;
            return returnData;
        }
        #endregion

        #region Dec.
        private SimulateVelocityData SetVChange_Dec(SimulateVelocityData nowData, AxisData axis, SimulateData data, DateTime time)
        {
            double nowDec = GetNowAccOrDec(data, (time - nowData.Time).TotalSeconds);
            double deltaVelocity = Math.Pow(nowDec, 2) / (2 * axis.Jerk);
            double nowVelocity = GetNowVelocity(data, (time - nowData.Time).TotalSeconds);
            double isokineticVelocity = nowVelocity - deltaVelocity;
            double startVelocity = nowVelocity + deltaVelocity;
            double startTime = -nowDec / axis.Jerk; ;

            SimulateVelocityData returnData = new SimulateVelocityData();
            SimulateData temp;
            returnData.Command = nowData.Command;
            returnData.Time = time;

            if (axis.Velocity > isokineticVelocity)
            {   // 升速度.
                CheckAxisDataOKWithVelocityChange(ref axis, startVelocity, isokineticVelocity);
                CheckAxisDataOKWithVelocityChange(ref axis, isokineticVelocity, axis.Velocity);

                temp = new SimulateData();
                temp.SetSimulateData_DecJerkDown(nowVelocity, axis.Deceleration, axis.Jerk, 0);
                returnData.SimulateDataList.Add(temp);

                temp = new SimulateData();
                temp.SetSimulateData_AccJerkUp(returnData.SimulateDataList[returnData.SimulateDataList.Count - 1].EndVelocity,
                                               axis.Acceleration, axis.Jerk,
                                               returnData.SimulateDataList[returnData.SimulateDataList.Count - 1].EndTime);
                returnData.SimulateDataList.Add(temp);

                temp = new SimulateData();
                temp.SetSimulateData_Accing(returnData.SimulateDataList[returnData.SimulateDataList.Count - 1], axis);
                returnData.SimulateDataList.Add(temp);

                temp = new SimulateData();
                temp.SetSimulateData_AccJerkDown(returnData.SimulateDataList[returnData.SimulateDataList.Count - 1].EndVelocity,
                                                 axis.Acceleration, axis.Jerk,
                                                 returnData.SimulateDataList[returnData.SimulateDataList.Count - 1].EndTime);
                returnData.SimulateDataList.Add(temp);
            }
            else if (axis.Velocity < isokineticVelocity)
            {   // 減速度.
                CheckAxisDataOKWithVelocityChange(ref axis, startVelocity, axis.Velocity);

                temp = new SimulateData();
                temp.SetSimulateData_DecJerkUp(startVelocity, axis.Deceleration, axis.Jerk, startTime);
                returnData.SimulateDataList.Add(temp);

                temp = new SimulateData();
                temp.SetSimulateData_Decing(returnData.SimulateDataList[returnData.SimulateDataList.Count - 1], axis);
                returnData.SimulateDataList.Add(temp);

                temp = new SimulateData();
                temp.SetSimulateData_DecJerkDown(returnData.SimulateDataList[returnData.SimulateDataList.Count - 1].EndVelocity,
                                                 axis.Deceleration, axis.Jerk,
                                                 returnData.SimulateDataList[returnData.SimulateDataList.Count - 1].EndTime);
                returnData.SimulateDataList.Add(temp);
            }
            else
            {   // 等速度.
                CheckAxisDataOKWithVelocityChange(ref axis, startVelocity, axis.Velocity);

                temp = new SimulateData();
                temp.SetSimulateData_DecJerkDown(nowVelocity, axis.Deceleration, axis.Jerk, 0);
                returnData.SimulateDataList.Add(temp);
            }

            temp = new SimulateData();
            temp.SetSimulateData_Isokinetic(returnData.SimulateDataList[returnData.SimulateDataList.Count - 1].EndVelocity,
                                            returnData.SimulateDataList[returnData.SimulateDataList.Count - 1].EndTime);
            returnData.SimulateDataList.Add(temp);

            returnData.Axis = axis;
            return returnData;
        }
        #endregion

        #region 變速命令.
        public void SetVChangeSimulateData(double velocity, double acc, double dec, double jerk)
        {
            WriteLog(7, "", "start");

            try
            {
                SimulateVelocityData temp = simulateVelocityData;

                if (temp == null)
                    return;

                DateTime now = DateTime.Now;
                double deltaTime = (now - temp.Time).TotalSeconds;

                int index = 0;

                while (index < temp.SimulateDataList.Count && temp.SimulateDataList[index].EndTime < deltaTime)
                    index++;

                SimulateData data;
                if (index < temp.SimulateDataList.Count)
                    data = temp.SimulateDataList[index];
                else
                    data = temp.SimulateDataList[index - 1];

                AxisData axis = new AxisData(acc, dec, jerk, velocity);

                switch (data.Type)
                {
                    case EnumSimulateVelocityType.AccJerkUp:
                    case EnumSimulateVelocityType.Accing:
                    case EnumSimulateVelocityType.AccJerkDown:
                        simulateVelocityData = SetVChange_Acc(temp, axis, data, now);
                        break;
                    case EnumSimulateVelocityType.Isokinetic:
                        simulateVelocityData = SetVChange_Isokinetic(temp, axis, data, now);
                        break;
                    case EnumSimulateVelocityType.DecJerkUp:
                    case EnumSimulateVelocityType.Decing:
                    case EnumSimulateVelocityType.DecJerkDown:
                        simulateVelocityData = SetVChange_Dec(temp, axis, data, now);
                        break;
                    default:
                        break;
                }

                WriteVChangeLog(data.Type, velocity);
            }
            catch { }

            WriteLog(7, "", "end");
        }
        #endregion

        private double GetSimulateVelocity(SimulateVelocityData simulateData, DateTime now)
        {
            double deltaTime = (now - simulateData.Time).TotalSeconds;
            bool isIsokinetic = false;

            int index = 0;

            while (index < simulateData.SimulateDataList.Count && simulateData.SimulateDataList[index].EndTime < deltaTime)
                index++;

            if (index == simulateData.SimulateDataList.Count)
            {
                localData.MoveControlData.MotionControlData.SimulateIsIsokinetic = true;
                return simulateData.SimulateDataList[index - 1].EndVelocity;
            }

            SimulateData data = simulateData.SimulateDataList[index];

            double velocity = 0;

            switch (data.Type)
            {
                case EnumSimulateVelocityType.AccJerkUp:
                    deltaTime = deltaTime - data.StartTime;
                    velocity = data.StartVelocity + data.Jerk * Math.Pow(deltaTime, 2) / 2;
                    break;

                case EnumSimulateVelocityType.Accing:
                    deltaTime = deltaTime - data.StartTime;
                    velocity = data.StartVelocity + data.StartAcc * deltaTime;
                    break;

                case EnumSimulateVelocityType.AccJerkDown:
                    deltaTime = data.EndTime - deltaTime;
                    velocity = data.EndVelocity - data.Jerk * Math.Pow(deltaTime, 2) / 2;
                    break;

                case EnumSimulateVelocityType.Isokinetic:
                    isIsokinetic = true;
                    velocity = data.StartVelocity;
                    break;

                case EnumSimulateVelocityType.DecJerkUp:
                    deltaTime = deltaTime - data.StartTime;
                    velocity = data.StartVelocity - data.Jerk * Math.Pow(deltaTime, 2) / 2;
                    break;

                case EnumSimulateVelocityType.Decing:
                    deltaTime = deltaTime - data.StartTime;
                    velocity = data.StartVelocity - data.StartAcc * deltaTime;
                    break;

                case EnumSimulateVelocityType.DecJerkDown:
                    deltaTime = data.EndTime - deltaTime;
                    velocity = data.EndVelocity + data.Jerk * Math.Pow(deltaTime, 2) / 2;
                    break;

                default:
                    break;
            }

            localData.MoveControlData.MotionControlData.SimulateIsIsokinetic = isIsokinetic;
            return velocity;
        }

        private void SimulateVelocityThread()
        {
            try
            {
                double velocity = 0;
                SimulateVelocityData temp;
                Stopwatch timer = new Stopwatch();

                while (!close)
                {
                    timer.Restart();
                    temp = simulateVelocityData;

                    if (temp == null)
                        velocity = 0;
                    else
                        velocity = GetSimulateVelocity(temp, DateTime.Now);

                    localData.MoveControlData.MotionControlData.SimulateLineVelocity = velocity;

                    while (timer.ElapsedMilliseconds < treadSleepTime)
                        Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                localData.MoveControlData.MotionControlData.SimulateLineVelocity = -999;
            }
        }

        public double GetStopDistanceBySimulate(AxisData axis)
        {
            try
            {
                double velocity = 0;
                double accdecDistance = 0;

                SimulateVelocityData temp = simulateVelocityData;

                if (temp == null)
                {
                    WriteLog(3, "", "TatalError simulateVelocityData == null");
                    return -1;
                }

                DateTime now = DateTime.Now;
                double deltaTime = (now - temp.Time).TotalSeconds;

                double vel = GetSimulateVelocity(temp, now);

                if (vel == 0)
                    return 0;

                int index = 0;

                while (index < temp.SimulateDataList.Count && temp.SimulateDataList[index].EndTime < deltaTime)
                    index++;

                SimulateData data;
                if (index < temp.SimulateDataList.Count)
                    data = temp.SimulateDataList[index];
                else
                    data = temp.SimulateDataList[index - 1];

                double accOrDec = GetNowAccOrDec(data, deltaTime);
                double t = accOrDec / axis.Jerk;
                double deltaVelocity = axis.Jerk / 2 * Math.Pow(t, 2);
                double deltaDistance = axis.Jerk / 6 * Math.Pow(t, 3);

                switch (data.Type)
                {
                    case EnumSimulateVelocityType.AccJerkUp:
                    case EnumSimulateVelocityType.Accing:
                    case EnumSimulateVelocityType.AccJerkDown:
                        velocity = vel + deltaVelocity;
                        accdecDistance = -(velocity * t - deltaDistance);
                        break;
                    case EnumSimulateVelocityType.Isokinetic:
                        velocity = vel;
                        accdecDistance = 0;
                        break;
                    case EnumSimulateVelocityType.DecJerkUp:
                    case EnumSimulateVelocityType.Decing:
                    case EnumSimulateVelocityType.DecJerkDown:
                        velocity = vel + deltaVelocity;
                        accdecDistance = (velocity * t - deltaDistance);
                        break;
                    default:
                        break;
                }


                double distance = computeFunction.GetAccDecDistance(velocity, 0, axis.Deceleration, axis.Jerk);

                if (distance - accdecDistance < 0)
                    WriteLog(1, "", String.Concat("accOrDec : ", accOrDec.ToString("0.0"), ", vel : ", vel.ToString("0.0")));

                return distance - accdecDistance;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return -1;
            }
        }

        public void GetAccOrDecZeroDistanceAndVelocity_SpeedUp(ref double distance, ref double velocity)
        {
            try
            {
                SimulateVelocityData temp = simulateVelocityData;

                if (temp == null)
                {
                    WriteLog(3, "", "TatalError simulateVelocityData == null");
                    velocity = localData.MoveControlData.MotionControlData.SimulateLineVelocity;
                    distance = 1000;
                }

                DateTime now = DateTime.Now;
                double deltaTime = (now - temp.Time).TotalSeconds;

                int index = 0;

                while (index < temp.SimulateDataList.Count && temp.SimulateDataList[index].EndTime < deltaTime)
                    index++;

                SimulateData data;
                if (index < temp.SimulateDataList.Count)
                    data = temp.SimulateDataList[index];
                else
                    data = temp.SimulateDataList[index - 1];

                double accOrDec = GetNowAccOrDec(data, deltaTime);
                double t = accOrDec / temp.Command.Jerk;
                double deltaVelocity = temp.Command.Jerk / 2 * Math.Pow(t, 2);
                double deltaDistance = temp.Command.Jerk / 6 * Math.Pow(t, 3);

                switch (data.Type)
                {
                    case EnumSimulateVelocityType.AccJerkUp:
                    case EnumSimulateVelocityType.Accing:
                    case EnumSimulateVelocityType.AccJerkDown:
                        velocity = localData.MoveControlData.MotionControlData.SimulateLineVelocity - deltaVelocity;
                        distance = -(velocity * t + deltaDistance);
                        break;
                    case EnumSimulateVelocityType.Isokinetic:
                        velocity = localData.MoveControlData.MotionControlData.SimulateLineVelocity;
                        distance = 0;
                        break;
                    case EnumSimulateVelocityType.DecJerkUp:
                    case EnumSimulateVelocityType.Decing:
                    case EnumSimulateVelocityType.DecJerkDown:
                        velocity = localData.MoveControlData.MotionControlData.SimulateLineVelocity - deltaVelocity;
                        distance = (velocity * t + deltaDistance);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                velocity = localData.MoveControlData.MotionControlData.LineVelocity;
                distance = 1000;
            }
        }
    }
}
