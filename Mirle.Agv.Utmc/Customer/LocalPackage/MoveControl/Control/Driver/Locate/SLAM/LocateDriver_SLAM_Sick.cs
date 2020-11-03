using Mirle.Agv.INX.Controller;
using Mirle.Agv.INX.Controller.Tools;
using Mirle.Agv.INX.Model;
using Mirle.Agv.INX.Model.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Net;

namespace Mirle.Agv.INX.Controller
{
    public class LocateDriver_SLAM_Sick : LocateDriver_SLAM
    {
        private LocateDriver_SLAM_SickConfig config = new LocateDriver_SLAM_SickConfig();

        private Socket sendCommandSocket = null;
        private Socket getDataSocket = null;

        private uint count = 0;

        private string startByte = "\x2";
        private string endByte = "\x3";

        private string ask = "sRN";
        private string askFeedback = "sRA";
        private string cmd = "sMN";
        private string cmdFeedback = "sAN";
        private Dictionary<string, double> lastEncoder = new Dictionary<string, double>();
        private DateTime encoderTime;

        private int axisNameOKAndSendMotionData = 0;

        private TimeStampData timeStampData = null;
        private const long overflowValue = 4294967296;
        private long sendDataCount = 0;

        #region Read XML.
        private bool ReadXML(string path)
        {
            if (path == null || path == "")
            {
                WriteLog(3, "", String.Concat(device, "Config 路徑錯誤為null或空值."));
                return false;
            }
            else if (!File.Exists(path))
            {
                WriteLog(3, "", String.Concat("找不到 ", device, "Config.xml."));
                return false;
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                var rootNode = doc.DocumentElement;

                foreach (XmlNode item in rootNode.ChildNodes)
                {
                    switch (item.Name)
                    {
                        case "ID":
                            config.ID = item.InnerText;
                            break;
                        case "IP":
                            config.IP = item.InnerText;
                            break;
                        case "CommandPort":
                            config.CommandPort = Int32.Parse(item.InnerText);
                            break;
                        case "SleepTime":
                            config.SleepTime = Int32.Parse(item.InnerText);
                            break;
                        case "PercentageStandard":
                            config.PercentageStandard = Int32.Parse(item.InnerText);
                            break;
                        case "FeedbackPort":
                            config.FeedbackPort = Int32.Parse(item.InnerText);
                            break;
                        case "TransferAddressPath":
                            config.TransferAddressPath = item.InnerText;
                            break;
                        case "LogMode":
                            config.LogMode = bool.Parse(item.InnerText);
                            break;
                        case "UseOdometry":
                            config.UseOdometry = bool.Parse(item.InnerText);
                            break;
                        case "SetPositionRange":
                            config.SetPositionRange = double.Parse(item.InnerText);
                            break;
                        case "SetPositionForceUpdateCount":
                            config.SetPositionForceUpdateCount = Int32.Parse(item.InnerText);
                            break;
                        case "SectionAngleRange":
                            config.SectionAngleRange = double.Parse(item.InnerText);
                            break;
                        case "SectionDistanceMagnification":
                            config.SectionDistanceMagnification = double.Parse(item.InnerText);
                            break;
                        case "SectionDistanceConstant":
                            config.SectionDistanceConstant = double.Parse(item.InnerText);
                            break;
                        default:
                            break;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }
        #endregion

        #region CRC16測試.
        public long calcrc16(byte[] dataP, int length)
        {
            long crc16; // calculation
            crc16 = 0xffff; // PRESET value
            for (int i = 0; i < length; i++) // check each Byte in the array
            {
                crc16 ^= dataP[i];
                for (int j = 0; j < 8; j++) // test each bit in the Byte
                {
                    if (crc16 % 2 == 1)//(crc16 & 0x0001 )
                    {
                        crc16 >>= 1;
                        crc16 ^= 0x8408; // POLYNOMIAL x^16 + x^12 + x^5 + 1
                        //crc16 ^= 0x1021; // POLYNOMIAL x^16 + x^12 + x^5 + 1
                    }
                    else
                    {
                        crc16 >>= 1;
                    }
                }
            }

            return (crc16); // returns calculated crc (16 bits)
        }

        public ushort CalcCRC16(byte[] bytes, int start, int end)
        {
            ushort crc = 0xFFFF;

            for (int j = start; j < end; j++)
            {
                crc = (ushort)(crc ^ bytes[j]);
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x0001) == 1)
                        crc = (ushort)((crc >> 1) ^ 0x1021);
                    else
                        crc >>= 1;
                }
            }

            crc = (ushort)~(uint)crc;
            return crc;
        }

        private void CRCTest()
        {
            //byte[] test = new byte[104] { 0x53, 0x49, 0x43, 0x4B, 0x00, 0x00, 0x00, 0x6A, 0x06, 0x42, 0x00, 0x01, 0x00, 0x10, 0xC0, 0x58,
            //                              0x01, 0x22, 0xA2, 0x72, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x4C, 0x4C, 0x53, 0x20, 0x56,
            //                              0x30, 0x2E, 0x31, 0x2E, 0x39, 0x2E, 0x78, 0x42, 0x00, 0x00, 0x02, 0x6D, 0x83, 0xAA, 0x8C, 0x0C,
            //                              0x8E, 0x14, 0x78, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x6F, 0x00, 0x34, 0xEC, 0xF3, 0x00, 0x00,
            //                              0x00, 0x5D, 0x00, 0x00, 0x00, 0x21, 0x00, 0x00, 0x45, 0xE7, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //                              0x00, 0x00, 0x37, 0x00, 0x00, 0x00, 0x80, 0x89, 0x00, 0x00, 0x99, 0x93, 0x00, 0x12, 0x78, 0x9F,
            //                              0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

            byte[] test = new byte[104] {0x53, 0x49, 0x43, 0x4B, 0x00, 0x00, 0x00, 0x6A, 0x06, 0x42, 0x00, 0x01, 0x00, 0x10, 0xC0, 0x58,
            0x01, 0x28, 0xE4, 0x7D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x4C, 0x4C, 0x53, 0x2D, 0x31, 0x2E,
            0x32, 0x2E, 0x30, 0x2E, 0x33, 0x30, 0x30, 0x52, 0x00, 0x00, 0x5D, 0xFB, 0x83, 0xAA, 0x86, 0x32,
            0x8A, 0xC0, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF9, 0x4A, 0x00, 0x1E, 0x11, 0x48, 0xFF, 0xFF,
            0xFA, 0xF0, 0x00, 0x00, 0x00, 0x1E, 0x00, 0x02, 0xBE, 0x3E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x60, 0x07, 0x00, 0x00, 0x22, 0x29, 0x00, 0x00, 0x1E, 0xFE, 0x00, 0x00, 0x7D, 0xD9,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};


            //            byte[] test = new byte[104] {         0x53, 0x49, 0x43, 0x4B, 0x00, 0x00, 0x00, 0x6A, 0x06, 0x42, 0x00, 0x01, 0x00, 0x10, 0xC0, 0x58,
            //0x01, 0x28, 0xE4, 0x7D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x4C, 0x4C, 0x53, 0x2D, 0x31, 0x2E,
            //0x32, 0x2E, 0x30, 0x2E, 0x33, 0x30, 0x30, 0x52, 0x00, 0x00, 0x5D, 0xFC, 0x83, 0xAA, 0x86, 0x32,
            //0x92, 0x2D, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF9, 0x4B, 0x00, 0x1E, 0x11, 0x65, 0xFF, 0xFF,
            //0xFA, 0xEC, 0x00, 0x00, 0x00, 0x1C, 0x00, 0x02, 0xBE, 0x33, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //0x00, 0x00, 0x60, 0x07, 0x00, 0x00, 0x22, 0x29, 0x00, 0x00, 0x1E, 0xFE, 0x00, 0x00, 0x7D, 0xD9,
            //0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            //byte[] test = new byte[106] { 0x53, 0x49, 0x43, 0x4B, 0x00, 0x00, 0x00, 0x6A, 0x06, 0x42, 0x00, 0x01, 0x00, 0x10, 0xC0, 0x58,
            //                              0x01, 0x22, 0xA2, 0x72, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x4C, 0x4C, 0x53, 0x20, 0x56,
            //                              0x30, 0x2E, 0x31, 0x2E, 0x39, 0x2E, 0x78, 0x42, 0x00, 0x00, 0x02, 0x6D, 0x83, 0xAA, 0x8C, 0x0C,
            //                              0x8E, 0x14, 0x78, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x6F, 0x00, 0x34, 0xEC, 0xF3, 0x00, 0x00,
            //                              0x00, 0x5D, 0x00, 0x00, 0x00, 0x21, 0x00, 0x00, 0x45, 0xE7, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //                              0x00, 0x00, 0x37, 0x00, 0x00, 0x00, 0x80, 0x89, 0x00, 0x00, 0x99, 0x93, 0x00, 0x12, 0x78, 0x9F,
            //                              0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x62, 0x11};



            //Console.WriteLine(calcrc16(test, 104));


            Console.WriteLine(CalcCRC16(test, 0, 104).ToString());

            ;
        }
        #endregion

        private bool CheckSumOK(byte[] data)
        {
            long crc104 = CalcCRC16(data, 0, 104);
            UInt16 sum = ByteToUInt16(data, 104, EnumByteChangeType.BigEndian);

            //WriteLog(7, "", String.Concat("CRC104 : ", crc104.ToString(), ", CheckSUm : ", sum.ToString()));

            return data[0] == 0x53 && data[1] == 0x49 && data[2] == 0x43 && data[3] == 0x4B;

            //return true;
        }

        private void CheckSectionDistance()
        {
            double realDistance;
            double slamDistacne;
            double allowDistanceDelta;

            foreach (MapSection section in localData.TheMapInfo.AllSection.Values)
            {
                if (section.FromVehicleAngle == section.ToVehicleAngle &&
                    slamPosition[section.FromAddress.Id] != null && slamPosition[section.ToAddress.Id] != null)
                {
                    realDistance = computeFunction.GetTwoPositionDistance(section.FromAddress.AGVPosition, section.ToAddress.AGVPosition);
                    slamDistacne = computeFunction.GetTwoPositionDistance(slamPosition[section.FromAddress.Id].Position, slamPosition[section.ToAddress.Id].Position);
                    allowDistanceDelta = realDistance * config.SectionDistanceMagnification + config.SectionDistanceConstant;

                    if (Math.Abs(realDistance - slamDistacne) > allowDistanceDelta)
                    {
                        WriteLog(3, "", String.Concat("Section : ", section.Id, " 長度異常, 圖資長度 = ", realDistance.ToString("0"), ", Slam圖資長度 = ", slamDistacne.ToString("0"),
                                                       ", config.SectionDistanceMagnification = ", config.SectionDistanceMagnification.ToString("0.00"),
                                                       ", config.SectionDistanceConstant = ", config.SectionDistanceConstant.ToString("0")));
                    }
                }
            }
        }

        override protected void InitialConfig(string path)
        {
            if (ReadXML(path) && ReadSLAMAddress(Path.Combine(localData.MapConfig.FileDirectory, config.TransferAddressPath)))
            {
                CheckSectionDistance();
                status = EnumControlStatus.Initial;

                logger = LoggerAgent.Instance.GetLooger(config.ID);
            }
            else
                SendAlarmCode(EnumMoveCommandControlErrorCode.LocateDriver_SLAM_Sick初始化失敗);
        }

        override public void CloseDriver()
        {
            if (Status == EnumControlStatus.NotInitial || Status == EnumControlStatus.Initial)
                return;

            WriteLog(7, "", "CloseDriver!");

            resetAlarm = false;

            if (config.UseOdometry)
            {
                config.UseOdometry = false;
                Command_LocSetOdometryActive(0);
            }

            status = EnumControlStatus.Closing;
            //if (Command_Close())
            //    WriteLog(7, "", "Driver關閉成功!");
            //else
            //    WriteLog(7, "", "Driver關閉失敗!");

            try
            {
                sendCommandSocket.Close();
                getDataSocket.Close();
            }
            catch
            {
            }
        }

        override public void ResetAlarm()
        {
            switch (Status)
            {
                case EnumControlStatus.NotInitial:
                    SendAlarmCode(EnumMoveCommandControlErrorCode.LocateDriver_SLAM_Sick初始化失敗);
                    break;
                case EnumControlStatus.Initial:
                    ConnectDriver();
                    break;
                case EnumControlStatus.Error:
                    break;
                default:
                    break;
            }
        }

        override public void ResendAlarm()
        {
            switch (Status)
            {
                case EnumControlStatus.NotInitial:
                    SendAlarmCode(EnumMoveCommandControlErrorCode.LocateDriver_SLAM_Sick初始化失敗);
                    break;
                case EnumControlStatus.Initial:
                    SendAlarmCode(EnumMoveCommandControlErrorCode.LocateDriver_SLAM_Sick連線失敗);
                    break;
                case EnumControlStatus.Error:
                    SendAlarmCode(EnumMoveCommandControlErrorCode.LocateDriver_SLAM_Sick經度迷航);
                    break;
                default:
                    break;
            }
        }

        #region 送/收資料.
        private string GetFeedbackString(byte[] receiveBytes)
        {
            if (receiveBytes == null)
                return "";

            int start = -1;
            int end = -1;

            for (int i = 0; i < receiveBytes.Length; i++)
            {
                if (receiveBytes[i] == startByte[0])
                    start = i;
                else if (receiveBytes[i] == endByte[0])
                    end = i;

                if (start != -1 && end != -1)
                    break;
            }

            if (start != -1 && end != -1 && end > start)
                return System.Text.Encoding.ASCII.GetString(receiveBytes, start + 1, end - 1);
            else
                return "";
        }

        private string[] SendCommandAndGetReceiveString(Socket socket, string command)
        {
            try
            {
                string commandString = String.Concat(startByte, command, endByte);
                WriteLog(7, "", String.Concat("Socket Command : ", commandString));

                byte[] ASCIIbytes = Encoding.ASCII.GetBytes(commandString);
                byte[] receiveBytes = new byte[256];
                socket.Send(ASCIIbytes, 0, ASCIIbytes.GetLength(0), SocketFlags.None);
                socket.ReceiveTimeout = 1000;
                socket.Receive(receiveBytes, 0, 256, SocketFlags.None);

                string reciveString = GetFeedbackString(receiveBytes);
                WriteLog(7, "", String.Concat("Receive Data : ", startByte.ToString(), reciveString, endByte.ToString()));

                string[] splitResult = Regex.Split(reciveString, " ", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));
                return splitResult;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Socket Exception : ", ex.ToString()));
                return null;
            }
        }
        #endregion

        private bool ConnectToSLAMSick()
        {
            if (Status == EnumControlStatus.Initial)
            {
                // 之後要再修改.. 需要判定起頭特殊字元x02結束字元x03等等資料...
                if (ConnectToSick() && Command_Start() && Command_SetMode() && SetTimeStamp() && SetOdmetry() && StartThread())
                    status = EnumControlStatus.Ready;
            }

            return true;
        }

        private bool SetOdmetry()
        {
            if (config.UseOdometry)
            {
                Command_LocSetOdometryActive(1);
                //Command_LocSetOdometryPort(3000);
                //Thread.Sleep(1000);

                if (!ConnectMotionDataSocket())
                {
                    WriteLog(3, "", "MotionData Soecket 開啟失敗,關閉功能");
                    Command_LocSetOdometryActive(0);
                    config.UseOdometry = false;
                }
            }

            return true;
        }

        UdpClient udpClient;

        private bool ConnectMotionDataSocket()
        {
            try
            {
                WriteLog(7, "", String.Concat("UDP IP : ", config.IP, ", port : 3000 "));
                //udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(config.IP), 3000));
                udpClient = new UdpClient(config.IP, 3000);

                return true;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        override public void ConnectDriver()
        {
            if (Status == EnumControlStatus.NotInitial)
                SendAlarmCode(EnumMoveCommandControlErrorCode.LocateDriver_SLAM_Sick初始化失敗);
            else if (!ConnectToSLAMSick())
                SendAlarmCode(EnumMoveCommandControlErrorCode.LocateDriver_SLAM_Sick連線失敗);
        }

        private bool ConnectToSick()
        {
            try
            {
                sendCommandSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.sendCommandSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 500);

                this.sendCommandSocket.Connect(IPAddress.Parse(config.IP), config.CommandPort);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region Sick Command.
        private bool Command_Start()
        {
            try
            {
                string command = "LocStartLocalizing";
                string[] reciveData = SendCommandAndGetReceiveString(sendCommandSocket, String.Concat(cmd, " ", command));

                if (reciveData == null || reciveData.Length == 1)
                    return false;

                if (reciveData.Length != 3)
                    return false;
                else
                {
                    if (reciveData[0] == cmdFeedback &&
                        reciveData[1] == command)
                    {
                        if (reciveData[2] == "1" || reciveData[2] == "0")
                            return true;
                        else
                            return false;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Socket Exception : ", ex.ToString()));
                return false;
            }
        }

        private bool Command_Close()
        {
            try
            {
                string command = "LocStop ";
                string[] reciveData = SendCommandAndGetReceiveString(sendCommandSocket, String.Concat(cmd, " ", command));

                if (reciveData == null || reciveData.Length == 1)
                    return false;

                if (reciveData.Length != 3)
                    return false;
                else
                {
                    if (reciveData[0] == cmdFeedback &&
                        reciveData[1] == command)
                    {
                        if (reciveData[2] == "1")
                            return true;
                        else
                            return false;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Socket Exception : ", ex.ToString()));
                return false;
            }
        }

        private bool Command_SetMode()
        {
            try
            {
                string command = "LocSetResultMode";
                string mode = "0";
                string[] reciveData = SendCommandAndGetReceiveString(sendCommandSocket, String.Concat(cmd, " ", command, " ", mode));

                if (reciveData == null || reciveData.Length == 1)
                    return false;

                if (reciveData.Length != 3)
                    return false;
                else
                {
                    if (reciveData[0] == cmdFeedback &&
                        reciveData[1] == command)
                    {
                        if (reciveData[2] == "1")
                            return true;
                        else
                            return false;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Socket Exception : ", ex.ToString()));
                return false;
            }
        }

        public bool Command_LocForceUpdate()
        {
            try
            {
                string command = "LocForceUpdate";
                string[] reciveData = SendCommandAndGetReceiveString(sendCommandSocket, String.Concat(cmd, " ", command));

                if (reciveData.Length != 3)
                    return false;
                else
                {
                    if (reciveData[0] == cmdFeedback &&
                        reciveData[1] == command)
                    {
                        if (reciveData[2] == "1")
                            return true;
                        else
                            return false;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Socket Exception : ", ex.ToString()));
                return false;
            }
        }

        private bool Command_LocSetOdometryActive(int OnOff)
        {
            try
            {
                string command = "LocSetOdometryActive";
                string[] reciveData = SendCommandAndGetReceiveString(sendCommandSocket, String.Concat(cmd, " ", command, " ", OnOff.ToString()));

                if (reciveData.Length != 4)
                    return false;
                else
                {
                    if (reciveData[0] == cmdFeedback &&
                        reciveData[1] == command)
                        return true;
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Socket Exception : ", ex.ToString()));
                return false;
            }
        }

        private bool Command_LocSetOdometryPort(int port)
        {
            try
            {
                string command = "LocSetOdometryPort";
                string[] reciveData = SendCommandAndGetReceiveString(sendCommandSocket, String.Concat(cmd, " ", command, " ", port.ToString()));

                if (reciveData.Length != 3)
                    return false;
                else
                {
                    if (reciveData[0] == cmdFeedback &&
                        reciveData[1] == command)
                        return true;
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Socket Exception : ", ex.ToString()));
                return false;
            }
        }

        private bool Command_SetPosition(MapAGVPosition now)
        {
            try
            {
                if (now == null || now.Position == null)
                    return false;

                string command = "LocSetPose";
                string x = String.Concat((now.Position.X > 0 ? "+" : ""), now.Position.X.ToString("0"));
                string y = String.Concat((now.Position.Y > 0 ? "+" : ""), now.Position.Y.ToString("0"));
                string theta = String.Concat((now.Angle > 0 ? "+" : ""), (now.Angle * 1000).ToString("0"));

                string range = String.Concat("+", config.SetPositionRange.ToString("0"));

                string[] reciveData = SendCommandAndGetReceiveString(sendCommandSocket, String.Concat(cmd, " ", command, " ", x, " ", y, " ", theta, " ", range));

                if (reciveData.Length != 3)
                    return false;
                else
                {
                    if (reciveData[0] == cmdFeedback &&
                        reciveData[1] == command)
                    {
                        if (reciveData[2] == "1")
                            return true;
                        else
                            return false;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Socket Exception : ", ex.ToString()));
                return false;
            }
        }
        #endregion

        #region Byte to data.
        private Int32 ByteToInt32(byte[] data, int index, EnumByteChangeType type)
        {
            if (type == EnumByteChangeType.LittleEndian)
                return BitConverter.ToInt32(data, index);
            else
            {
                byte[] byteData = new byte[4] { data[index + 3], data[index + 2], data[index + 1], data[index] };
                return BitConverter.ToInt32(byteData, 0);
            }
        }

        private UInt32 ByteToUInt32(byte[] data, int index, EnumByteChangeType type)
        {
            if (type == EnumByteChangeType.LittleEndian)
                return BitConverter.ToUInt32(data, index);
            else
            {
                byte[] byteData = new byte[4] { data[index + 3], data[index + 2], data[index + 1], data[index] };
                return BitConverter.ToUInt32(byteData, 0);
            }
        }

        private UInt16 ByteToUInt16(byte[] data, int index, EnumByteChangeType type)
        {
            if (type == EnumByteChangeType.LittleEndian)
                return BitConverter.ToUInt16(data, index);
            else
            {
                byte[] byteData = new byte[2] { data[index + 1], data[index] };
                return BitConverter.ToUInt16(byteData, 0);
            }
        }
        #endregion

        private double GetScanTimeAndProcessOverflow(DateTime now, byte[] ReceiveBytes, EnumByteChangeType byteType)
        {
            TimeStampData nowTimeStampData = timeStampData;

            double scanTime = (now - nowTimeStampData.GetTime).TotalMilliseconds - ByteToUInt32(ReceiveBytes, 58, byteType);

            while (scanTime > overflowValue)
            {
                WriteLog(7, "", "simTimeStamp_GetData overflow!");
                nowTimeStampData.GetTime = nowTimeStampData.GetTime.AddMilliseconds(overflowValue);
                scanTime -= overflowValue;
            }

            if (scanTime < 0)
            {
                WriteLog(1, "", "TimeStampError < 0!");
                return 0;
            }
            else if (scanTime > 100)
            {
                WriteLog(1, "", "TimeStampError > 100!");
                return 0;
            }

            return scanTime;
        }

        private bool GetData()
        {
            byte[] ReceiveBytes = new byte[256];

            bool returnValue = true;

            try
            {
                this.getDataSocket.Receive(ReceiveBytes, 0, 256, SocketFlags.None);

                if (!CheckSumOK(ReceiveBytes))
                {
                    WriteLog(3, "", String.Concat("CRC16 CheckSum Error : ", BitConverter.ToString(ReceiveBytes)));
                    originAGVPosition = null;
                    nowAGVPosition = null;
                    returnValue = false;
                }
                else
                {
                    EnumByteChangeType byteType = (ReceiveBytes[8] == 0x06 && ReceiveBytes[8] == 0xC2) ? EnumByteChangeType.LittleEndian : EnumByteChangeType.BigEndian;
                    DateTime now = DateTime.Now;
                    double scanTime = GetScanTimeAndProcessOverflow(now, ReceiveBytes, byteType);
                    Int32 x = ByteToInt32(ReceiveBytes, 62, byteType);
                    Int32 y = ByteToInt32(ReceiveBytes, 66, byteType);
                    Int32 theta = ByteToInt32(ReceiveBytes, 70, byteType);
                    MapPosition position = new MapPosition((double)x, (double)y);
                    int percent = (int)ReceiveBytes[82];
                    LocateAGVPosition temp = new LocateAGVPosition(position, (double)theta / 1000, percent, (int)scanTime, now, count, EnumAGVPositionType.Normal, device, DriverConfig.Order);

                    //UInt32 a = ByteToUInt32(ReceiveBytes, 58, byteType);

                    //WriteLog(7, "", a.ToString());

                    int errorCode = ByteToUInt16(ReceiveBytes, 52, byteType);

                    if (errorCode != 0)
                    {
                        WriteLog(5, "", String.Concat("ErrorCode : ", errorCode.ToString()));
                        originAGVPosition = temp;
                        nowAGVPosition = null;
                    }
                    else if (percent == 0 && x == 0 && y == 0 && theta == 0)
                    {
                        WriteLog(3, "", "All Zero!");
                        originAGVPosition = temp;
                        nowAGVPosition = null;
                        returnValue = false;
                    }
                    else
                    {
                        if (percent < config.PercentageStandard && status == EnumControlStatus.Ready)
                        {
                            WriteLog(3, "", String.Concat("percent : ", percent.ToString(), ", status Change to Error!"));
                            status = EnumControlStatus.Error;
                        }
                        else if (percent >= config.PercentageStandard && status == EnumControlStatus.Error)
                        {
                            if (!config.ErrorNeedReset)
                            {
                                WriteLog(3, "", String.Concat("percent : ", percent.ToString(), " & conig.ErrorNeedReset = false, status Change to Ready!"));
                                status = EnumControlStatus.Ready;
                            }
                        }

                        if (PoolingOnOff && Status != EnumControlStatus.Error)
                        {
                            originAGVPosition = temp;
                            SLAMPositionOffset();
                            TransferToMapPosition();
                        }
                        else if (Status == EnumControlStatus.Error)
                        {
                            originAGVPosition = temp;
                            nowAGVPosition = null;
                        }
                        else
                        {
                            originAGVPosition = null;
                            nowAGVPosition = null;
                            returnValue = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Socket Exception : ", ex.ToString()));
                originAGVPosition = null;
                nowAGVPosition = null;
                returnValue = false;
            }

            WriteCSV();
            count++;
            return returnValue;
        }

        private UInt32 GetTimeStamp()
        {
            TimeStampData nowTimeStampData = timeStampData;

            double timeStamp = (encoderTime - nowTimeStampData.SendTime).TotalMilliseconds;

            while (timeStamp > overflowValue)
            {
                WriteLog(7, "", "simTimeStamp_SendData overflow!");
                nowTimeStampData.SendTime = nowTimeStampData.SendTime.AddMilliseconds(overflowValue);
                timeStamp -= overflowValue;
            }

            if (timeStamp < 0)
            {
                WriteLog(3, "", "TimeStampError < 0!");
                return 0;
            }

            return (UInt32)timeStamp;
        }

        #region Send encoder to sick slam.
        private void SendMotionDataSocket(int xVelocity, int yVelocity, int thetaVelocity)
        {
            ///  LocSetOdometryActive / LocSetOdometryPort 
            /// ┌──────────────────────────────────────┐
            /// │                                 Header                                     │
            /// ├────────────┬────┬────────────┬───────┤
            /// │       Description      │  Type  │Range of Value (decimal)│ Size [byte]  │
            /// ├────────────┼────┼────────────┼───────┤
            /// │MagicWord               │ Char32 │          SMOP          │      4       │
            /// │PayloadLength           │ UInt16 │       0 ~ 65,535       │      2       │
            /// │PayloadType(endianness) │ UInt16 │ BE: 0x0642, LE: 0x06c2 │      2       │
            /// │MsgType                 │ UInt16 │       0 ~ 65,535       │      2       │
            /// │MsgTypeVersion          │ UInt16 │       0 ~ 65,535       │      2       │
            /// │SourceID                │ UInt16 │       0 ~ 65,535       │      2       │
            /// ├────────────┴────┴────────────┴───────┤
            /// │                                Payload                                     │
            /// ├────────────┬────┬────────────┬───────┤
            /// │       Description      │  Type  │Range of Value (decimal)│ Size [byte]  │
            /// ├────────────┼────┼────────────┼───────┤
            /// │TelegramCount           │ UInt32 │    0 ~ 4,294,967,295   │      4       │      
            /// │Time stamp              │ UInt32 │   0 ~ 4,294,967,295ms  │      4       │
            /// │X-component of velocity │ Int16  │ -32,768 ~ 32,768 mm/s  │      2       │
            /// │Y-component of velocit  │ Int16  │ -32,768 ~ 32,768 mm/s  │      2       │
            /// │Angular velocity        │ Int32  │ ± 2,147,483,648 mdeg/s │      4       │
            /// └────────────┴────┴────────────┴───────┘
            /// header : SMOP 0010 0642 0001 0001 0001  // SMOP : 534D 4F50
            /// header : 534D 4F50 0010 0642 0001 0001 0001
            /// 

            byte[] count = BitConverter.GetBytes(sendDataCount);
            byte[] timeStampArray = BitConverter.GetBytes(GetTimeStamp());
            byte[] vel_X = BitConverter.GetBytes(xVelocity);
            byte[] vel_Y = BitConverter.GetBytes(yVelocity);
            byte[] vel_Theta = BitConverter.GetBytes(thetaVelocity);
            byte[] sendArray = new byte[30] { 0x53, 0x4D, 0x4F, 0x50, 0x00, 0x10, 0x06, 0x42, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01 ,
                                              count[3], count[2], count[1], count[0],                                       // count
                                              timeStampArray[3], timeStampArray[2], timeStampArray[1], timeStampArray[0],   // timeStamp
                                              vel_X[1], vel_X[0],
                                              vel_Y[1], vel_Y[0],
                                              vel_Theta[3], vel_Theta[2], vel_Theta[1], vel_Theta[0]};


            //logger.LogString(String.Concat(xVelocity.ToString(), ",", yVelocity.ToString(), ",", thetaVelocity.ToString()));
            udpClient.Send(sendArray, sendArray.Length);
            //udpClient.Send(sendArray, sendArray.Length, new IPEndPoint("192.168.1.120", 3000));
        }
        #endregion

        private void SendMotionData()
        {
        }

        private void GetDataThread()
        {
            TimeStampData nowTimStampData = null;

            while (Status != EnumControlStatus.Closing)
            {
                if (GetData())
                    pollingTimer.Restart();

                nowTimStampData = timeStampData;

                if (nowTimStampData != null && nowTimStampData.UpdateTime.Day != DateTime.Now.Day)
                    SetTimeStamp();

                Thread.Sleep(config.SleepTime);
            }

            pollingTimer.Restart();
            pollingTimer.Stop();
            status = EnumControlStatus.Closed;
        }

        private bool Command_RequestTimestamp(ref UInt32 timeStamp)
        {
            try
            {
                string command = "LocRequestTimestamp";
                string[] reciveData = SendCommandAndGetReceiveString(sendCommandSocket, String.Concat(cmd, " ", command));

                if (reciveData.Length != 3)
                    return false;
                else
                {
                    if (reciveData[0] == cmdFeedback &&
                        reciveData[1] == command)
                    {
                        timeStamp = UInt32.Parse(reciveData[2], System.Globalization.NumberStyles.HexNumber);

                        return true;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Socket Exception : ", ex.ToString()));
                return false;
            }
        }

        private bool SetTimeStamp()
        {
            DateTime beforeSend = DateTime.Now;
            UInt32 simTimeStamp = 0;

            if (!Command_RequestTimestamp(ref simTimeStamp))
                return false;

            DateTime afterSend = DateTime.Now;

            double socketTime = ((afterSend - beforeSend).TotalMilliseconds / 2);

            //WriteLog(7, "", String.Concat("beforeSend : ", beforeSend.ToString("HH:mm:ss.fff")));
            //WriteLog(7, "", String.Concat("afterSend : ", afterSend.ToString("HH:mm:ss.fff")));
            //WriteLog(7, "", String.Concat("Sim TimeStap : ", simTimeStamp.ToString()));

            TimeStampData newTimeStampData = new TimeStampData();

            newTimeStampData.Time = afterSend.AddMilliseconds(-socketTime - simTimeStamp);
            newTimeStampData.GetTime = newTimeStampData.Time;
            newTimeStampData.SendTime = newTimeStampData.Time;

            TimeStampData oldTimeStampData = timeStampData;

            if (oldTimeStampData == null)
                WriteLog(7, "", String.Concat("newTimeStamp : ", newTimeStampData.Time.ToString("HH:mm:ss.fff")));
            else
                WriteLog(7, "", String.Concat("newTimeStamp : ", newTimeStampData.Time.ToString("HH:mm:ss.fff"),
                                              ", delta time : ", (oldTimeStampData.GetTime - newTimeStampData.GetTime).TotalMilliseconds, " ms"));

            timeStampData = newTimeStampData;
            return true;
        }

        private bool StartThread()
        {
            try
            {
                getDataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.getDataSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 500);

                this.getDataSocket.Connect(config.IP, config.FeedbackPort);

                pollingThread = new Thread(GetDataThread);
                pollingThread.Start();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool SetPositionAndForceUpdateAndCheck(MapAGVPosition now)
        {
            if (Status != EnumControlStatus.Ready &&
                Status != EnumControlStatus.Error)
                return false;

            bool needSetPosition = true;

            LocateAGVPosition origin = originAGVPosition;

            if (origin != null && origin.Value >= config.PercentageStandard)
            {
                if (computeFunction.GetTwoPositionDistance(now, origin.AGVPosition) < config.SetPositionRange / 2)
                    needSetPosition = false;
            }

            if (needSetPosition)
            {
                if (!Command_SetPosition(now))
                    return false;
                else
                {
                    for (int i = 0; i < config.SetPositionForceUpdateCount; i++)
                    {
                        Command_LocForceUpdate();
                        Thread.Sleep(100);
                    }

                    LocateAGVPosition temp = originAGVPosition;

                    if (temp != null && temp.Value > config.PercentageStandard)
                    {
                        status = EnumControlStatus.Ready;
                        return true;
                    }
                    else
                        return false;
                }
            }
            else
            {
                status = EnumControlStatus.Ready;
                return true;
            }
        }

        override public bool SetPositionByAddressID(string addressID, ref string errorMessage)
        {
            if (slamPosition.ContainsKey(addressID))
            {
                if (slamPosition[addressID] != null)
                    return SetPositionAndForceUpdateAndCheck(slamPosition[addressID]);
                else
                {
                    errorMessage = String.Concat("Address ID : ", addressID, " not set position!");
                    return false;
                }
            }
            else
            {
                errorMessage = String.Concat("Address ID : ", addressID, " not find!");
                return false;
            }
        }

        private bool SetPosition(MapAGVPosition now)
        { // 會咬住
            if (Status == EnumControlStatus.Ready)
                return true;
            else if (now == null)
                return false;
            else if (Status == EnumControlStatus.Error)
            {
                resetAlarm = true;
                bool returnBoolean = true;

                if (now == null || now.Position == null)
                    returnBoolean = false;
                else
                {
                    MapAGVPosition temp = new MapAGVPosition();// GetSLAMPostionByMapPositoin(now.Position);

                    if (temp == null || temp.Position == null)
                        returnBoolean = false;
                    else
                    {
                        returnBoolean = SetPositionAndForceUpdateAndCheck(temp);
                    }
                }

                resetAlarm = false;
                return returnBoolean;
            }
            else
                return false;
        }

        private void TransferToMapPosition_BySLAMTransferData(SLAMTransfer transfer)
        {
            if (transfer == null)
                nowAGVPosition = null;
            else
            {
                LocateAGVPosition temp = new LocateAGVPosition(offsetAGVPosition);

                double x = offsetAGVPosition.AGVPosition.Position.X - transfer.Step1Offset.X;
                double y = offsetAGVPosition.AGVPosition.Position.Y - transfer.Step1Offset.Y;

                double newX = x * transfer.CosTheta + y * transfer.SinTheta;

                if (newX < 0)
                    newX = 0;
                else if (newX > transfer.Distance)
                    newX = transfer.Distance;

                // Theta顛倒.
                temp.AGVPosition.Angle = -temp.AGVPosition.Angle;

                temp.AGVPosition.Angle = computeFunction.GetCurrectAngle(temp.AGVPosition.Angle +
                    (transfer.ThetaOffsetStart + computeFunction.GetCurrectAngle(transfer.ThetaOffsetEnd - transfer.ThetaOffsetStart) * newX / transfer.Distance));

                // 上下顛倒.
                y = -y;

                // 旋轉.
                temp.AGVPosition.Position.X = x * transfer.Step2Cos + y * transfer.Step2Sin;
                temp.AGVPosition.Position.Y = -x * transfer.Step2Sin + y * transfer.Step2Cos;

                temp.AGVPosition.Position.X *= transfer.Step3Mag;
                temp.AGVPosition.Position.Y *= transfer.Step3Mag;

                temp.AGVPosition.Position.X += transfer.Step4Offset.X;
                temp.AGVPosition.Position.Y += transfer.Step4Offset.Y;
                nowAGVPosition = temp;
            }
        }

        private void SLAMPositionOffset()
        {
            if (originAGVPosition == null)
                return;

            if (slamOffset == null)
                offsetAGVPosition = originAGVPosition;
            else
            {
                LocateAGVPosition temp = new LocateAGVPosition(originAGVPosition);
                temp.AGVPosition.Angle = computeFunction.GetCurrectAngle(temp.AGVPosition.Angle + slamOffset.ThetaOffset);
                temp.AGVPosition.Position.X += Math.Cos((temp.AGVPosition.Angle + slamOffset.Polar_Theta) / 180 * Math.PI) * slamOffset.Polar_Distance;
                temp.AGVPosition.Position.X += Math.Sin((temp.AGVPosition.Angle + slamOffset.Polar_Theta) / 180 * Math.PI) * slamOffset.Polar_Distance;
                offsetAGVPosition = temp;
            }
        }

        private void FindManualSection()
        {
            try
            {
                LocateAGVPosition slamNow = originAGVPosition;
                string tempManualSectionID = manualSection;

                if (slamNow == null)
                {
                    WriteLog(5, "", String.Concat("目前沒位置, 因此不更新"));
                    return;
                }

                MapPosition now = new MapPosition();

                if (sectionSLAMTransferLit.ContainsKey(tempManualSectionID))
                {
                    now = computeFunction.GetTransferPosition(findSectionList[tempManualSectionID], slamNow.AGVPosition.Position);

                    if (Math.Abs(now.Y) < config.SectionRange && (-config.SectionRange < now.X & now.X < findSectionList[tempManualSectionID].Distance + config.SectionRange))
                    {
                        WriteLog(5, "", String.Concat("目前還在 : ", tempManualSectionID));
                        return;
                    }

                    for (int i = 0; i < sectionConnectSectionList[tempManualSectionID].Count; i++)
                    {
                        now = computeFunction.GetTransferPosition(findSectionList[sectionConnectSectionList[tempManualSectionID][i]], slamNow.AGVPosition.Position);

                        if (Math.Abs(now.Y) < config.SectionRange &&
                            (-config.SectionRange < now.X & now.X < findSectionList[sectionConnectSectionList[tempManualSectionID][i]].Distance + config.SectionRange))
                        {
                            manualSection = sectionConnectSectionList[tempManualSectionID][i];
                            WriteLog(5, "", String.Concat("切換至 : ", sectionConnectSectionList[tempManualSectionID][i]));
                            return;
                        }
                    }
                }

                double min = -1;
                double tempMin;

                foreach (SectionLine sectionLine in findSectionList.Values)
                {
                    now = computeFunction.GetTransferPosition(sectionLine, slamNow.AGVPosition.Position);

                    if (-config.SectionRange > now.X)
                        tempMin = Math.Abs(now.Y) - now.X;
                    else if (now.X > sectionLine.Distance + config.SectionRange)
                        tempMin = Math.Abs(now.Y) + (now.X - sectionLine.Distance);
                    else
                        tempMin = Math.Abs(now.Y);

                    if (min == -1 || min < tempMin)
                    {
                        tempManualSectionID = sectionLine.Section.Id;
                        min = tempMin;
                    }
                }

                if (min != -1)
                {
                    manualSection = tempManualSectionID;
                    WriteLog(5, "", String.Concat("切換至 : ", tempManualSectionID, " deltaY : ", now.Y.ToString("0.0")));
                }
                else
                    WriteLog(5, "", "Not Find in Section (min == -1)");
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        private void TransferToMapPosition()
        {
            string nowSection = localData.Location.NowSection;

            string a = localData.MoveControlData.LocateControlData.TargetSection;
            if (localData.LoginLevel == EnumLoginLevel.MirleAdmin && localData.AutoManual == EnumAutoState.Manual && localData.TheMapInfo.AllSection.ContainsKey(a))
            {
                TransferToMapPosition_BySLAMTransferData(sectionSLAMTransferLit[a]);
                return;
            }

            if (sectionSLAMTransferLit.ContainsKey(nowSection))
            {
                manualSection = nowSection;
                TransferToMapPosition_BySLAMTransferData(sectionSLAMTransferLit[nowSection]);
            }
            else
            {
                string tempManualSectionID = manualSection;

                if ((DateTime.Now - lastSearchSectionTime).TotalMilliseconds > localData.MoveControlData.MoveControlConfig.TimeValueConfig.IntervalTimeList[EnumIntervalTimeType.ManualFindSectionInterval])
                {
                    if (searchSectionThread == null || !searchSectionThread.IsAlive)
                    {
                        lastSearchSectionTime = DateTime.Now;
                        searchSectionThread = new Thread(FindManualSection);
                        searchSectionThread.Start();
                    }
                }

                if (sectionSLAMTransferLit.ContainsKey(tempManualSectionID))
                    TransferToMapPosition_BySLAMTransferData(sectionSLAMTransferLit[tempManualSectionID]);
                else
                    TransferToMapPosition_BySLAMTransferData(null);
            }
        }

        private void WriteCSV()
        {
            string csvLog = "";

            if (config.LogMode)
            {
                if (originAGVPosition == null)
                    csvLog = String.Concat(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"), ",", count.ToString(), ",,,,");
                else
                    csvLog = String.Concat(originAGVPosition.GetDataTime.ToString("yyyy/MM/dd HH:mm:ss.fff"), ",",
                                           originAGVPosition.Count.ToString(), ",", originAGVPosition.AGVPosition.Position.X.ToString(), ",",
                                           originAGVPosition.AGVPosition.Position.Y.ToString(), ",", originAGVPosition.AGVPosition.Angle.ToString(), ",",
                                           originAGVPosition.ScanTime.ToString());

                if (nowAGVPosition == null)
                    csvLog = String.Concat(csvLog, ",,,");
                else
                    csvLog = String.Concat(csvLog, ",", nowAGVPosition.AGVPosition.Position.X.ToString(), ",",
                                                        nowAGVPosition.AGVPosition.Position.Y.ToString(), ",", nowAGVPosition.AGVPosition.Angle.ToString());

                if (originAGVPosition == null)
                    csvLog = String.Concat(csvLog, ",");
                else
                    csvLog = String.Concat(csvLog, ",", originAGVPosition.Value);

                logger.LogString(csvLog);
            }
        }
    }
}