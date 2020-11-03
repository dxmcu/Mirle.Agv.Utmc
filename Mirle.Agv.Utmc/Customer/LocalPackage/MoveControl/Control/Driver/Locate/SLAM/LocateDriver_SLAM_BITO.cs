using Mirle.Agv.INX.Controller.Tools;
using Mirle.Agv.INX.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;

namespace Mirle.Agv.INX.Controller
{
    public class LocateDriver_SLAM_BITO : LocateDriver_SLAM
    {
        private LocateDriver_SLAM_SickConfig config = new LocateDriver_SLAM_SickConfig();

        private Socket socketPort19000 = null;
        private Socket socketPort19001 = null;
        private Socket socketPort19002 = null;

        private double BITO_x = 0;
        private double BITO_y = 0;
        private double BITO_Angle = 0;
        private int BITO_Status = 0;

        private uint count = 0;

        ///╔══════╦══════════╦═══╦═══════════╗
        ///║    名稱    ║        內容        ║ byte ║        描述          ║
        ///╠══════╬══════════╬═══╬═══════════╣
        ///║   Header   ║0xAA 0x55 0xAA 0x55 ║   4  ║       固定內容       ║
        ///╠══════╬══════════╬═══╬═══════════╣
        ///║  協議版本  ║        0x01        ║   1  ║      協議版本號      ║
        ///╠══════╬══════════╬═══╬═══════════╣
        ///║ 協定頭長度 ║        0x14        ║   1  ║協議頭長度取決協議版本║
        ///╠══════╬══════════╬═══╬═══════════╣
        ///║   SeqNo    ║                    ║   2  ║        流水號        ║
        ///╠══════╬══════════╬═══╬═══════════╣
        ///║Command Type║ EnumSLAMBITOCommand║   1  ║協議頭長度取決協議版本║
        ///╠══════╬══════════╬═══╬═══════════╣
        ///║ Data Length║                    ║   4  ║     取決Data長度     ║
        ///╠══════╬══════════╬═══╬═══════════╣
        ///║   保留區   ║                    ║   6  ║         空白         ║
        ///╠══════╬══════════╬═══╬═══════════╣
        ///║    Data    ║                    ║ 不定 ║         資料         ║
        ///╠══════╬══════════╬═══╬═══════════╣
        ///║     CRC    ║                    ║   4  ║         資料         ║
        ///╚══════╩══════════╩═══╩═══════════╝

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
                SendAlarmCode(EnumMoveCommandControlErrorCode.LocateDriver_SLAM_BITO初始化失敗);
        }

        override public void CloseDriver()
        {
            if (Status == EnumControlStatus.NotInitial || Status == EnumControlStatus.Initial)
                return;

            WriteLog(7, "", "CloseDriver!");

            resetAlarm = false;

            status = EnumControlStatus.Closing;

            try
            {
                socketPort19000.Close();
                socketPort19001.Close();
                socketPort19002.Close();
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
                    SendAlarmCode(EnumMoveCommandControlErrorCode.LocateDriver_SLAM_BITO初始化失敗);
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
                    SendAlarmCode(EnumMoveCommandControlErrorCode.LocateDriver_SLAM_BITO初始化失敗);
                    break;
                case EnumControlStatus.Initial:
                    SendAlarmCode(EnumMoveCommandControlErrorCode.LocateDriver_SLAM_BITO連線失敗);
                    break;
                case EnumControlStatus.Error:
                    SendAlarmCode(EnumMoveCommandControlErrorCode.LocateDriver_SLAM_BITO經度迷航);
                    break;
                default:
                    break;
            }
        }

        #region 送/收資料.

        private bool SendCommand(Socket socket, Byte[] byteArray)
        {
            try
            {
                socket.Send(byteArray, 0, byteArray.GetLength(0), SocketFlags.None);
                return true;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Socket Exception : ", ex.ToString()));
                return false;
            }
        }
        #endregion

        private bool ConnectAndStart()
        {
            if (Status == EnumControlStatus.Initial)
            {
                // 之後要再修改.. 需要判定起頭特殊字元x02結束字元x03等等資料...
                if (Connect_19000()/*&& Connect_19001() && Connect_19002()*//* && Command_SendGetAngle()*/ && Command_SendGetStatus() && Command_SendGetPosition() && StartThread())
                    status = EnumControlStatus.Ready;
            }

            return true;
        }

        override public void ConnectDriver()
        {
            if (Status == EnumControlStatus.NotInitial)
                SendAlarmCode(EnumMoveCommandControlErrorCode.LocateDriver_SLAM_BITO初始化失敗);
            else if (!ConnectAndStart())
                SendAlarmCode(EnumMoveCommandControlErrorCode.LocateDriver_SLAM_BITO連線失敗);
        }

        #region Connect Socket.
        private bool connect_19000 = false;
        private bool connect_19001 = false;
        private bool connect_19002 = false;

        private bool Connect_19000()
        {
            try
            {
                if (connect_19000)
                    return true;

                socketPort19000 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.socketPort19000.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 500);

                this.socketPort19000.Connect(IPAddress.Parse(config.IP), 19000);
                connect_19000 = true;
                return true;
            }
            catch (SocketException ex)
            {
                WriteLog(5, "", String.Concat("Socke Exception : ", ex.ToString()));
                return false;
            }
            catch (Exception ex)
            {
                WriteLog(5, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        private bool Connect_19001()
        {
            try
            {
                if (connect_19001)
                    return true;

                socketPort19001 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.socketPort19001.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 500);

                this.socketPort19001.Connect(IPAddress.Parse(config.IP), 19001);
                connect_19001 = true;
                return true;
            }
            catch (SocketException ex)
            {
                WriteLog(5, "", String.Concat("Socke Exception : ", ex.ToString()));
                return false;
            }
            catch (Exception ex)
            {
                WriteLog(5, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        private bool Connect_19002()
        {
            try
            {
                if (connect_19002)
                    return true;

                socketPort19002 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.socketPort19002.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 500);

                this.socketPort19002.Connect(IPAddress.Parse(config.IP), 19002);
                connect_19002 = true;
                return true;
            }
            catch (SocketException ex)
            {
                WriteLog(5, "", String.Concat("Socke Exception : ", ex.ToString()));
                return false;
            }
            catch (Exception ex)
            {
                WriteLog(5, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }
        #endregion

        #region Sick Command.
        private bool Command_SendGetPosition()
        {
            try
            {
                byte[] byteArray = new byte[24]{ 0x55, 0xaa, 0x55, 0xaa, 0x01, 0x14,
                                                 0x01, 0x00, 0x02, 0x00, 0x04, 0x00,
                                                 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                 0x00, 0x00, 0x6b, 0x2a, 0x9f, 0xfe };

                if (SendCommand(socketPort19000, byteArray))
                    return true;
                else
                {
                    WriteLog(5, "", "Send Get Status Fail");
                    return false;
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Socket Exception : ", ex.ToString()));
                return false;
            }
        }

        private bool Command_SendGetAngle()
        {
            try
            {
                //byte[] byteArray = new byte[24]{ 0x55, 0xaa, 0x55, 0xaa, 0x01, 0x14,
                //                                 0x01, 0x00, 0x06, 0x00, 0x04, 0x00,
                //                                 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                //                                 0x00, 0x00, 0x14, 0x11, 0x99, 0xfd};

                //if (SendCommand(socketPort19000, byteArray))
                return true;
                //else
                //{
                //    WriteLog(5, "", "Send Get Status Fail");
                //    return false;
                //}
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Socket Exception : ", ex.ToString()));
                return false;
            }
        }

        private bool Command_SendGetStatus()
        {
            try
            {
                byte[] byteArray = new byte[24]{ 0x55, 0xaa, 0x55, 0xaa, 0x01, 0x14,
                                                 0x01, 0x00, 0x03, 0x00, 0x04, 0x00,
                                                 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                 0x00, 0x00, 0x04, 0x66, 0x3a, 0x65};

                if (SendCommand(socketPort19000, byteArray))
                    return true;
                else
                {
                    WriteLog(5, "", "Send Get Status Fail");
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
                return false;
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

        private byte[] buffer = new byte[0];
        private byte[] receiveBytes = new byte[0];

        private int FindFirstHaderIndex()
        {
            for (int i = 0; i < receiveBytes.Length - 4; i++)
            {
                if (receiveBytes[i] == 0x55 && receiveBytes[i + 1] == 0xAA && receiveBytes[i + 2] == 0x55 && receiveBytes[i + 3] == 0xAA)
                    return i;
            }

            return -1;
        }

        private bool SplitTCPIPPackage()
        {
            int index = FindFirstHaderIndex();

            if (index == -1)
            {
                WriteLog(7, "", "index = -1");
                buffer = new byte[0];
                return false;
            }
            else
            {
                ///╔══════╦══════════╦═══╦═══════════╗
                ///║    名稱    ║        內容        ║ byte ║        描述          ║
                ///╠══════╬══════════╬═══╬═══════════╣
                ///║   Header   ║0xAA 0x55 0xAA 0x55 ║   4  ║       固定內容       ║
                ///╠══════╬══════════╬═══╬═══════════╣
                ///║  協議版本  ║        0x01        ║   1  ║      協議版本號      ║
                ///╠══════╬══════════╬═══╬═══════════╣
                ///║ 協定頭長度 ║        0x14        ║   1  ║協議頭長度取決協議版本║
                ///╠══════╬══════════╬═══╬═══════════╣
                ///║   SeqNo    ║                    ║   2  ║        流水號        ║
                ///╠══════╬══════════╬═══╬═══════════╣
                ///║Command Type║ EnumSLAMBITOCommand║   2  ║協議頭長度取決協議版本║
                ///╠══════╬══════════╬═══╬═══════════╣
                ///║ Data Length║                    ║   4  ║     取決Data長度     ║
                ///╠══════╬══════════╬═══╬═══════════╣
                ///║   保留區   ║                    ║   6  ║         空白         ║
                ///╠══════╬══════════╬═══╬═══════════╣
                ///║    Data    ║                    ║ 不定 ║         資料         ║
                ///╠══════╬══════════╬═══╬═══════════╣
                ///║     CRC    ║                    ║   4  ║         資料         ║
                ///╚══════╩══════════╩═══╩═══════════╝

                if (index + 10 + 4 > receiveBytes.Length)
                {
                    buffer = receiveBytes;
                    return false;
                }

                UInt32 length = BitConverter.ToUInt32(receiveBytes, index + 10);

                byte[] onePackage = new byte[9 + 6 + 1 + length + 4];

                if (index + onePackage.Length > receiveBytes.Length)
                {
                    buffer = receiveBytes;
                    return false;
                }

                Array.Copy(receiveBytes, index, onePackage, 0, onePackage.Length);

                byte[] newReceiveByte = new byte[receiveBytes.Length - index - onePackage.Length];
                Array.Copy(receiveBytes, index + onePackage.Length, newReceiveByte, 0, newReceiveByte.Length);

                receiveBytes = newReceiveByte;

                if (CheckCRC16(onePackage))
                {
                    if (onePackage[8] == 03)
                    {
                        //WriteLog(5, "", "BITO Status");
                        string Text_Status = System.Text.Encoding.ASCII.GetString(onePackage);
                        Text_Status = Text_Status.Substring(Text_Status.IndexOf("{"), (Text_Status.IndexOf("}") - Text_Status.IndexOf("{")) + 1);

                        Dictionary<string, string> Jsonstring = JsonConvert.DeserializeObject<Dictionary<string, string>>(Text_Status);
                        BITO_Status = Convert.ToInt32(Jsonstring["robot_localization_state"]);
                    }
                    else
                    {
                        if (onePackage[8] == 02)
                        {
                            //WriteLog(5, "", "BITO Position");
                            string Text_Position = System.Text.Encoding.ASCII.GetString(onePackage);
                            Text_Position = Text_Position.Substring(Text_Position.IndexOf("{"), (Text_Position.IndexOf("}") - Text_Position.IndexOf("{")) + 1);

                            Dictionary<string, string> Jsonstring = JsonConvert.DeserializeObject<Dictionary<string, string>>(Text_Position);
                            BITO_x = Convert.ToDouble(Jsonstring["x"]) * 1000;
                            BITO_y = Convert.ToDouble(Jsonstring["y"]) * 1000;

                            double qx = Convert.ToDouble(Jsonstring["qx"]);
                            double qy = Convert.ToDouble(Jsonstring["qy"]);
                            double qz = Convert.ToDouble(Jsonstring["qz"]);
                            double qw = Convert.ToDouble(Jsonstring["qw"]);

                            BITO_Angle = Math.Atan(2 * (qw * qz + qx * qy) / (1 - 2 * (qy * qy + qz * qz))) * 180 / Math.PI;
                            WriteLog(7, "", System.Text.Encoding.ASCII.GetString(onePackage));
                        }
                        //else if (onePackage[8] == 06)
                        //{
                        //    //WriteLog(5, "", "BITO Angle");
                        //    string Text_Angle = System.Text.Encoding.ASCII.GetString(onePackage);
                        //    Text_Angle = Text_Angle.Substring(Text_Angle.IndexOf("{"), (Text_Angle.IndexOf("}") - Text_Angle.IndexOf("{")) + 1);
                        //    Text_Angle = Text_Angle.Substring(Text_Angle.LastIndexOf("{"), (Text_Angle.IndexOf("}") - Text_Angle.LastIndexOf("{")) + 1);

                        //    Dictionary<string, string> Jsonstring = JsonConvert.DeserializeObject<Dictionary<string, string>>(Text_Angle);
                        //    BITO_Angle = Convert.ToDouble(Jsonstring["angle"]);
                        //}
                        else
                        {
                            WriteLog(1, "", "我也不知道為什麼會跑到這邊");
                            return false;
                        }

                        LocateAGVPosition newLocateAGVPosition = new LocateAGVPosition(new MapPosition(BITO_x, BITO_y), BITO_Angle, 0, 0, DateTime.Now, count, EnumAGVPositionType.Normal, device, DriverConfig.Order);
                        originAGVPosition = newLocateAGVPosition;

                        if (PoolingOnOff && Status != EnumControlStatus.Error)
                        {
                            originAGVPosition = newLocateAGVPosition;
                            SLAMPositionOffset();
                            TransferToMapPosition();
                        }
                        else if (Status == EnumControlStatus.Error)
                        {
                            originAGVPosition = newLocateAGVPosition;
                            nowAGVPosition = null;
                        }
                        else
                        {
                            originAGVPosition = null;
                            nowAGVPosition = null;
                        }

                        WriteCSV();
                        count++;
                    }

                    return true;
                }
                else
                {
                    if (FindFirstHaderIndex() == -1)
                    {
                        WriteLog(3, "", String.Concat("[-1]", System.Text.Encoding.ASCII.GetString(onePackage)));
                        buffer = new byte[0];
                        return false;
                    }
                    else
                    {
                        WriteLog(7, "", String.Concat("[Buffer]", System.Text.Encoding.ASCII.GetString(receiveBytes)));
                        return true;
                    }
                }
            }
        }

        private bool CheckCRC16(byte[] onePackage)
        {
            Crc32 crc32 = new Crc32();

            byte[] newArray = new byte[onePackage.Length - 4];

            Array.Copy(onePackage, 0, newArray, 0, newArray.Length);

            crc32.AddData(newArray);

            UInt32 crcValue = BitConverter.ToUInt32(onePackage, onePackage.Length - 4);

            return crc32.Crc32Value == BitConverter.ToUInt32(onePackage, onePackage.Length - 4);
        }

        private int readSize = 4096;

        private bool GetData()
        {
            bool returnValue = true;

            try
            {
                receiveBytes = new byte[buffer.Length + readSize];
                buffer.CopyTo(receiveBytes, 0);

                this.socketPort19000.Receive(receiveBytes, buffer.Length, readSize, SocketFlags.None);

                int packageCount = 0;

                while (SplitTCPIPPackage())
                    packageCount++;

                WriteLog(7, "", String.Concat("封包數量 : ", packageCount.ToString()));
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Socket Exception : ", ex.ToString()));
                originAGVPosition = null;
                nowAGVPosition = null;
                returnValue = false;
            }

            return returnValue;
        }

        private void GetDataThread()
        {
            while (Status != EnumControlStatus.Closing)
            {
                if (GetData())
                    pollingTimer.Restart();


                Thread.Sleep(config.SleepTime);
            }

            pollingTimer.Restart();
            pollingTimer.Stop();
            status = EnumControlStatus.Closed;
        }

        private bool StartThread()
        {
            try
            {
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
            return true;
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


        private LocateAGVPosition lastData = null;
        private double lastMovingAngle = 0;
        private double lastMovingVelocity = 0;
        private double lastThetaVelocity = 0;

        private LocateAGVPosition GetNewLocateAGVPositionByOldDataAndDeltaTime(LocateAGVPosition oldData, double deltaTime, double movingAngle, double movingVelocity, double thetaVelocity)
        {
            LocateAGVPosition returnData = new LocateAGVPosition(oldData);

            returnData.AGVPosition.Angle = computeFunction.GetCurrectAngle(oldData.AGVPosition.Angle + deltaTime / 1000 * thetaVelocity);

            MapPosition newMapPosition = new MapPosition();
            newMapPosition.X = oldData.AGVPosition.Position.X + deltaTime / 1000 * movingVelocity * Math.Cos(movingAngle);
            newMapPosition.Y = oldData.AGVPosition.Position.Y + deltaTime / 1000 * movingVelocity * Math.Sin(movingAngle);

            returnData.AGVPosition.Position = newMapPosition;
            return returnData;
        }

        private LocateAGVPosition GetNewLocateAGVPositionWithTwoLocationAGVPosition(LocateAGVPosition base1, LocateAGVPosition another)
        {
            LocateAGVPosition returnData = new LocateAGVPosition(base1);
            returnData.AGVPosition.Position.X = (base1.AGVPosition.Position.X + another.AGVPosition.Position.X);
            returnData.AGVPosition.Position.Y = (base1.AGVPosition.Position.Y + another.AGVPosition.Position.Y);
            returnData.AGVPosition.Angle = computeFunction.GetCurrectAngle(base1.AGVPosition.Angle + computeFunction.GetCurrectAngle(another.AGVPosition.Angle - base1.AGVPosition.Angle) / 2);
            return returnData;
        }

        private void SetAverageFunction()
        {
            if (config.UsingAverage)
            {
                if (lastData != null && offsetAGVPosition != null)
                {
                    double deltaTime = (offsetAGVPosition.GetDataTime - lastData.GetDataTime).TotalMilliseconds;

                    if (deltaTime <= config.AverageTimeRange)
                    {
                        //LocateAGVPosition newLocateAGVPosition = GetNewLocateAGVPositionWithTwoLocationAGVPosition(offsetAGVPosition,
                        //                                                 GetNewLocateAGVPositionByOldDataAndDeltaTime(offsetAGVPosition, deltaTime,
                        //                                                 localData.MoveControlData.MotionControlData.LineVelocityAngle + computeFunction.GetCurrectAngle(
                        //                                                         lastMovingAngle - localData.MoveControlData.MotionControlData.LineVelocityAngle),
                        //                                                 (lastMovingVelocity + localData.MoveControlData.MotionControlData.LineVelocity) / 2,
                        //                                                 (lastThetaVelocity + localData.MoveControlData.MotionControlData.ThetaVelocity) / 2));

                        //lastData = originAGVPosition;
                        //lastMovingAngle = localData.MoveControlData.MotionControlData.LineVelocityAngle;
                        //lastMovingVelocity = localData.MoveControlData.MotionControlData.LineVelocity;
                        //lastThetaVelocity = localData.MoveControlData.MotionControlData.ThetaVelocity;


                        //originAGVPosition = newLocateAGVPosition;

                        deltaTime = 10 + (DateTime.Now - offsetAGVPosition.GetDataTime).TotalMilliseconds;

                        originAGVPosition = GetNewLocateAGVPositionByOldDataAndDeltaTime(offsetAGVPosition, deltaTime, lastMovingAngle, lastMovingVelocity, lastThetaVelocity);
                    }
                }
                else
                {
                    lastData = originAGVPosition;
                    lastMovingAngle = localData.MoveControlData.MotionControlData.LineVelocityAngle;
                    lastMovingVelocity = localData.MoveControlData.MotionControlData.LineVelocity;
                    lastThetaVelocity = localData.MoveControlData.MotionControlData.ThetaVelocity;
                }
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
                    csvLog = String.Concat("N/A", ",", count.ToString(), ",", "N/A", ",", "N/A", ",", "N/A", ",", "N/A");
                else
                    csvLog = String.Concat(originAGVPosition.GetDataTime.ToString("yyyy/MM/dd HH:mm:ss.fff"), ",",
                                           originAGVPosition.Count.ToString(), ",", originAGVPosition.AGVPosition.Position.X.ToString(), ",",
                                           originAGVPosition.AGVPosition.Position.Y.ToString(), ",", originAGVPosition.AGVPosition.Angle.ToString(), ",",
                                           originAGVPosition.ScanTime.ToString());

                if (nowAGVPosition == null)
                    csvLog = String.Concat(csvLog, ",", "N/A", ",", "N/A", ",", "N/A");
                else
                    csvLog = String.Concat(csvLog, ",", nowAGVPosition.AGVPosition.Position.X.ToString(), ",",
                                                        nowAGVPosition.AGVPosition.Position.Y.ToString(), ",", nowAGVPosition.AGVPosition.Angle.ToString());

                if (originAGVPosition == null)
                    csvLog = String.Concat(csvLog, ",", "N/A");
                else
                    csvLog = String.Concat(csvLog, ",", originAGVPosition.Value);

                logger.LogString(csvLog);
            }
        }
    }
}
