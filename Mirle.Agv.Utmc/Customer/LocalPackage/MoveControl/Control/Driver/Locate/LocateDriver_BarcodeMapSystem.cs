using Mirle.Agv.INX.Model;
using Mirle.Agv.INX.Model.Configs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Mirle.Agv.INX.Controller
{
    public class LocateDriver_BarcodeMapSystem : LocateDriver
    {
        private bool allConnected = false;
        private bool allNotError = true;

        private BarcodeMapSystemConfig config = new BarcodeMapSystemConfig();
        public BarcodeMap BarcodeMap { get; set; } = new BarcodeMap();

        public List<BarcodeReader> BarcodeReaderList = new List<BarcodeReader>();
        private Thread[] barcodeReaderPollingThread = null;
        private List<BarcodeReaderData> barcodeReaderDataList = new List<BarcodeReaderData>();

        private object lockObejct = new object();

        override public void InitialDriver(LocateDriverConfig driverConfig, AlarmHandler alarmHandler, string normalLogName)
        {
            LocateType = EnumLocateType.Normal;
            this.normalLogName = normalLogName;
            this.DriverConfig = driverConfig;
            this.alarmHandler = alarmHandler;
            device = driverConfig.LocateDriverType.ToString();
            logger = LoggerAgent.Instance.GetLooger(driverConfig.LocateDriverType.ToString());
            InitialConfig(driverConfig.Path);
        }

        #region Read BarcodeMap.csv.
        private bool ReadBarcodeMap()
        {
            string path = @"D:\MecanumConfigs\MoveControl\LocateControl\BarocdeMap.csv";

            if (!File.Exists(path))
            {
                WriteLog(3, "", "找不到 BarocdeMap.csv.");
                return false;
            }

            try
            {
                string[] allRows = File.ReadAllLines(path);
                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                Dictionary<string, int> dicHeaderIndexes = new Dictionary<string, int>();

                for (int i = 0; i < nColumns; i++)
                {
                    string keyword = titleRow[i].Trim();

                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        if (dicHeaderIndexes.ContainsKey(keyword))
                            WriteLog(3, "", String.Concat("Title repeat : ", keyword));
                        else
                            dicHeaderIndexes.Add(keyword, i);
                    }
                }

                if (dicHeaderIndexes.ContainsKey("ID") && dicHeaderIndexes.ContainsKey("Type") &&
                    dicHeaderIndexes.ContainsKey("Start_Number") && dicHeaderIndexes.ContainsKey("Start_X") && dicHeaderIndexes.ContainsKey("Start_Y") &&
                    dicHeaderIndexes.ContainsKey("End_Number") && dicHeaderIndexes.ContainsKey("End_X") && dicHeaderIndexes.ContainsKey("End_Y"))
                    ;
                else
                {
                    WriteLog(3, "", String.Concat("Title must be : ID,Start_Number,Start_X,Start_Y,End_Number,End_X,End_Y,Type"));
                    return false;
                }

                BarocdeLineMap barcodeLine;
                EnumAGVPositionType type;

                for (int i = 0; i < nRows; i++)
                {
                    try
                    {
                        string[] getThisRow = allRows[i].Split(',');

                        barcodeLine = new BarocdeLineMap();
                        barcodeLine.ID = getThisRow[dicHeaderIndexes["ID"]];
                        barcodeLine.Start_Number = Int32.Parse(getThisRow[dicHeaderIndexes["Start_Number"]]);
                        barcodeLine.Start_Position.X = double.Parse(getThisRow[dicHeaderIndexes["Start_X"]]);
                        barcodeLine.Start_Position.Y = double.Parse(getThisRow[dicHeaderIndexes["Start_Y"]]);
                        barcodeLine.End_Number = Int32.Parse(getThisRow[dicHeaderIndexes["End_Number"]]);
                        barcodeLine.End_Position.X = double.Parse(getThisRow[dicHeaderIndexes["End_X"]]);
                        barcodeLine.End_Position.Y = double.Parse(getThisRow[dicHeaderIndexes["End_Y"]]);

                        if (Math.Abs(barcodeLine.Start_Number - barcodeLine.End_Number) % config.BarcodeIDInterval != 0)
                            WriteLog(3, "", String.Concat("line : ", (i + 2).ToString(), ", data : ", allRows[i], ", Start_Number - End_Number 和 BarcodeIDInterval不合"));
                        else if (Math.Abs(Math.Abs(barcodeLine.Start_Number - barcodeLine.End_Number) * config.BarcodeDistanceInterval -
                                          computeFunction.GetTwoPositionDistance(barcodeLine.Start_Position, barcodeLine.End_Position))
                                / (Math.Abs(barcodeLine.Start_Number - barcodeLine.End_Number) * config.BarcodeDistanceInterval) > config.DistanceTolerance)
                            WriteLog(3, "", String.Concat("line : ", (i + 2).ToString(), ", data : ", allRows[i], " 長度和Position位置差不符合設定數值"));
                        else if (!Enum.TryParse(getThisRow[dicHeaderIndexes["Type"]], out type))
                            WriteLog(3, "", String.Concat("line : ", (i + 2).ToString(), ", data : ", allRows[i], " EnumTypeParse failed"));
                        else if (BarcodeMap.AllBarcodeLine.ContainsKey(barcodeLine.ID))
                            WriteLog(3, "", String.Concat("line : ", (i + 2).ToString(), ", data : ", allRows[i], " Key 重複"));
                        else
                        {
                            barcodeLine.Type = type;
                            BarcodeMap.AllBarcodeLine.Add(barcodeLine.ID, barcodeLine);
                        }
                    }
                    catch
                    {
                        WriteLog(3, "", String.Concat("line : ", (i + 1).ToString(), ", data : ", allRows[i], " Exception"));
                    }
                }

                foreach (BarocdeLineMap temp in BarcodeMap.AllBarcodeLine.Values)
                {
                    int delta = (temp.Start_Number < temp.End_Number ? config.BarcodeIDInterval : -config.BarcodeIDInterval);

                    for (int id = temp.Start_Number; true; id += delta)
                    {
                        if (BarcodeMap.AllBarcode.ContainsKey(id))
                        {
                            WriteLog(3, "", String.Concat("BarcodeLine ID : ", temp.ID, " 中的 ", id.ToString(), " 和BarcodeLine ID : ", BarcodeMap.AllBarcode[id].BarcodeLineID, "重複"));
                            break;
                        }
                        else
                            BarcodeMap.AllBarcode.Add(id, new BarcodeDataInMap(temp, id));

                        if (id == temp.End_Number)
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

        #region ReadXML.
        private MapPosition ReadPositionXML(XmlElement element)
        {
            MapPosition returnPosition = new MapPosition();
            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "X":
                        returnPosition.X = double.Parse(item.InnerText);
                        break;
                    case "Y":
                        returnPosition.Y = double.Parse(item.InnerText);
                        break;
                    default:
                        break;
                }
            }

            return returnPosition;
        }

        private MapAGVPosition ReadOffsetXML(XmlElement element)
        {
            MapAGVPosition returnPosition = new MapAGVPosition();
            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "OffsetX":
                        returnPosition.Position.X = double.Parse(item.InnerText);
                        break;
                    case "OffsetY":
                        returnPosition.Position.Y = double.Parse(item.InnerText);
                        break;
                    case "OffsetTheta":
                        returnPosition.Angle = double.Parse(item.InnerText);
                        break;
                    default:
                        break;
                }
            }

            return returnPosition;
        }

        private void ReadTrapezoidalCrrection(XmlElement element, LocateDriver_SR2000Config tempConfig)
        {
            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "Up":
                        tempConfig.Up = double.Parse(item.InnerText);
                        break;
                    case "Down":
                        tempConfig.Down = double.Parse(item.InnerText);
                        break;
                    default:
                        break;
                }
            }
        }

        private void ReadBarcodeReaderXML(XmlElement element)
        {
            LocateDriver_SR2000Config temp = new LocateDriver_SR2000Config();

            foreach (XmlNode item in element.ChildNodes)
            {
                switch (item.Name)
                {
                    case "ID":
                        temp.ID = item.InnerText;
                        break;
                    case "IpOrComport":
                        temp.IpOrComport = item.InnerText;
                        break;
                    case "BarcodeReaderType":
                        EnumBarcodeReaderType tempEnum;

                        if (Enum.TryParse(item.InnerText, out tempEnum))
                            temp.BarcodeReaderType = tempEnum;
                        else
                            temp.BarcodeReaderType = EnumBarcodeReaderType.None;
                        break;

                    case "ReaderToCenterDegree":
                        temp.ReaderToCenterDegree = double.Parse(item.InnerText);
                        break;
                    case "ReaderToCenterDistance":
                        temp.ReaderToCenterDistance = double.Parse(item.InnerText);
                        break;
                    case "ReaderSetupAngle":
                        temp.ReaderSetupAngle = double.Parse(item.InnerText);
                        break;
                    case "ViewOffset":
                        temp.Offset = ReadOffsetXML((XmlElement)item);
                        break;
                    case "ViewCenter":
                        temp.ViewCenter = ReadPositionXML((XmlElement)item);
                        break;
                    case "Change":
                        temp.Change = ReadPositionXML((XmlElement)item);
                        break;
                    case "TrapezoidalCrrection":
                        ReadTrapezoidalCrrection((XmlElement)item, temp);
                        break;
                    default:
                        break;
                }
            }

            temp.Target = new MapPosition(temp.ViewCenter.X + temp.Offset.Position.X, temp.ViewCenter.Y + temp.Offset.Position.Y);

            if (temp.BarcodeReaderType != EnumBarcodeReaderType.None)
                config.BarcodeReaderConfigs.Add(temp);
        }

        private bool ReadXML(string path)
        {
            try
            {
                if (path == null || path == "")
                {
                    WriteLog(1, "", "BarocdeMapSystemConfig.mxl 路徑錯誤為null或空值.");
                    return false;
                }

                if (!File.Exists(path))
                {
                    WriteLog(1, "", "找不到BarocdeMapSystemConfig.xml.");
                    return false;
                }

                XmlDocument doc = new XmlDocument();

                doc.Load(path);
                XmlElement rootNode = doc.DocumentElement;

                foreach (XmlNode item in rootNode.ChildNodes)
                {
                    switch (item.Name)
                    {
                        case "BarcodeReader":
                            ReadBarcodeReaderXML((XmlElement)item);
                            break;
                        case "LogMode":
                            config.LogMode = bool.Parse(item.InnerText);
                            break;
                        case "SleepTime":
                            config.SleepTime = Int32.Parse(item.InnerText);
                            break;
                        case "TimeoutValue":
                            config.TimeoutValue = Int32.Parse(item.InnerText);
                            break;
                        case "BarcodeIDInterval":
                            config.BarcodeIDInterval = Int32.Parse(item.InnerText);
                            break;
                        case "SignalOnStopAndSendAlarm":
                            config.BarcodeDistanceInterval = double.Parse(item.InnerText);
                            break;
                        case "VChangeWitchIsokinetic":
                            config.DistanceTolerance = double.Parse(item.InnerText);
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

        override protected void InitialConfig(string path)
        {
            if (ReadXML(path) && ReadBarcodeMap())
            {
                status = EnumControlStatus.Initial;
                barcodeReaderPollingThread = new Thread[config.BarcodeReaderConfigs.Count];

                for (int i = 0; i < config.BarcodeReaderConfigs.Count; i++)
                {
                    switch (config.BarcodeReaderConfigs[i].BarcodeReaderType)
                    {
                        case EnumBarcodeReaderType.Keyence:
                            BarcodeReaderList.Add(new BarcodeReader_Keyence());
                            barcodeReaderDataList.Add(null);

                            break;
                        case EnumBarcodeReaderType.Datalogic:
                            BarcodeReaderList.Add(new BarcodeReader_Datalogic());
                            barcodeReaderDataList.Add(null);
                            break;
                        default:
                            break;
                    }
                }
            }
            else
                SendAlarmCode(EnumMoveCommandControlErrorCode.LocateDriver_BarcodeMapSystem初始化失敗);
        }

        override public void ConnectDriver()
        {
            string errorMessage;
            bool allOK = true;

            for (int i = 0; i < BarcodeReaderList.Count; i++)
            {
                errorMessage = "";

                if (!BarcodeReaderList[i].Connected)
                {
                    if (!BarcodeReaderList[i].Connect(config.BarcodeReaderConfigs[i].IpOrComport, ref errorMessage))
                    {
                        WriteLog(5, "", String.Concat("BarcodeReader : ", config.BarcodeReaderConfigs[i].ID, " Connect failed, ", errorMessage));
                        allOK = false;
                    }
                    else
                    {
                        barcodeReaderPollingThread[i] = new Thread(new ParameterizedThreadStart(PollingThread));
                        barcodeReaderPollingThread[i].Start((object)i);
                    }
                }
            }

            allConnected = allOK;

            if (allConnected && allNotError)
                status = EnumControlStatus.Ready;
        }

        override public void CloseDriver()
        {
            status = EnumControlStatus.Closing;

            string errorMessage;

            for (int i = 0; i < BarcodeReaderList.Count; i++)
            {
                errorMessage = "";

                if (BarcodeReaderList[i].Connected)
                    BarcodeReaderList[i].Disconnect(ref errorMessage);
            }
        }

        override public void ResetAlarm()
        {
            resetAlarm = true;
            bool allOK = true;

            if (!allConnected)
                ConnectDriver();

            if (!allNotError)
            {
                for (int i = 0; i < BarcodeReaderList.Count; i++)
                {
                    if (BarcodeReaderList[i].Error)
                        BarcodeReaderList[i].ResetError();

                    if (BarcodeReaderList[i].Error)
                        allOK = false;
                }
            }

            allNotError = allOK;

            if (allConnected && allNotError)
                status = EnumControlStatus.Ready;

            resetAlarm = false;
        }

        override public void ResendAlarm()
        {
            if (!allConnected)
                SendAlarmCode(EnumMoveCommandControlErrorCode.LocateDriver_BarcodeMapSystem連線失敗);

            if (!allNotError)
                SendAlarmCode(EnumMoveCommandControlErrorCode.LocateDriver_BarcodeMapSystemError);
        }

        #region 梯形補正.
        private MapPosition XChangeTheta55Single(int index, MapPosition barcode)
        {
            double mid = (config.BarcodeReaderConfigs[index].Up + config.BarcodeReaderConfigs[index].Down) / 2;
            double value = config.BarcodeReaderConfigs[index].Down + (config.BarcodeReaderConfigs[index].Up - config.BarcodeReaderConfigs[index].Down) * (barcode.Y / (2 * config.BarcodeReaderConfigs[index].ViewCenter.Y)); ;
            double x = config.BarcodeReaderConfigs[index].ViewCenter.X + (barcode.X - config.BarcodeReaderConfigs[index].ViewCenter.X) * (mid / value);
            MapPosition returnPosition = new MapPosition((float)x, barcode.Y);
            return returnPosition;
        }

        private void XChangeTheta55ALL(int index, BarcodeReaderData sr2000ReadData)
        {
            sr2000ReadData.Barcode1.ViewPosition = XChangeTheta55Single(index, sr2000ReadData.Barcode1.ViewPosition);
            sr2000ReadData.Barcode2.ViewPosition = XChangeTheta55Single(index, sr2000ReadData.Barcode2.ViewPosition);
        }
        #endregion

        #region 從BarcodeMap中取得Barcode座標.
        private bool GetBarcodePosition(BarcodeData barcodeData)
        {
            if (BarcodeMap.AllBarcode.ContainsKey(barcodeData.ID))
            {
                barcodeData.MapPosition = BarcodeMap.AllBarcode[barcodeData.ID].Position;
                barcodeData.LineId = BarcodeMap.AllBarcode[barcodeData.ID].BarcodeLineID;
                barcodeData.Type = BarcodeMap.AllBarcode[barcodeData.ID].Type;
                return true;
            }
            else
                return false;
        }

        private bool GetReadDataPosition(BarcodeReaderData barcodeReaderData)
        {
            if (GetBarcodePosition(barcodeReaderData.Barcode1) && GetBarcodePosition(barcodeReaderData.Barcode2))
                return barcodeReaderData.Barcode1.LineId == barcodeReaderData.Barcode2.LineId;
            else
                return false;
        }
        #endregion

        #region
        private void ComputeMapPosition(int index, BarcodeReaderData barcodeReaderData)
        {
            double barcodeAngleInView = computeFunction.ComputeAngle(barcodeReaderData.Barcode1.ViewPosition, barcodeReaderData.Barcode2.ViewPosition); // pointer barcode1 to barcode2.
                                                                                                                                                        // barcode在reader上面的角度 = barcode - reader.
            double barcodeAngleInMap = computeFunction.ComputeAngle(barcodeReaderData.Barcode1.MapPosition, barcodeReaderData.Barcode2.MapPosition);    // barcode in Map's angle.
                                                                                                                                                        // barcode在Map上的角度 = barcode - Map.
            double barcode1ToCenterAngleInView = computeFunction.ComputeAngle(barcodeReaderData.Barcode1.ViewPosition, config.BarcodeReaderConfigs[index].Target);
            double barcode1ToCenterAngleInMap = 0;
            double agvAngleInMap = 0;
            double barcode1ToCenterDistance = Math.Sqrt(Math.Pow((config.BarcodeReaderConfigs[index].Target.X - barcodeReaderData.Barcode1.ViewPosition.X) * config.BarcodeReaderConfigs[index].Change.X, 2) +
                                                        Math.Pow((config.BarcodeReaderConfigs[index].Target.Y - barcodeReaderData.Barcode1.ViewPosition.Y) * config.BarcodeReaderConfigs[index].Change.Y, 2));
            barcodeAngleInView += config.BarcodeReaderConfigs[index].Offset.Angle;
            barcode1ToCenterAngleInView += config.BarcodeReaderConfigs[index].Offset.Angle;
            // Map在reader上的角度 = Map -reader = barcodeInViewAngle - barcodeInMapAngle;
            agvAngleInMap = barcodeAngleInMap - barcodeAngleInView + config.BarcodeReaderConfigs[index].ReaderSetupAngle;
            if (agvAngleInMap > 180)
                agvAngleInMap -= 360;
            else if (agvAngleInMap <= -180)
                agvAngleInMap += 360;

            barcode1ToCenterAngleInMap = barcodeAngleInMap - barcodeAngleInView + barcode1ToCenterAngleInView;

            barcodeReaderData.TargetCenter =
                new MapPosition(barcodeReaderData.Barcode1.MapPosition.X + (float)(barcode1ToCenterDistance * Math.Cos(-barcode1ToCenterAngleInMap / 180 * Math.PI)),
                                barcodeReaderData.Barcode1.MapPosition.Y + (float)(barcode1ToCenterDistance * Math.Sin(-barcode1ToCenterAngleInMap / 180 * Math.PI)));

            MapPosition agvPosition = new MapPosition(
                barcodeReaderData.TargetCenter.X + (float)(config.BarcodeReaderConfigs[index].ReaderToCenterDistance *
                Math.Cos((agvAngleInMap + config.BarcodeReaderConfigs[index].ReaderToCenterDegree + 180) / 180 * Math.PI)),
                barcodeReaderData.TargetCenter.Y + (float)(config.BarcodeReaderConfigs[index].ReaderToCenterDistance *
                Math.Sin((agvAngleInMap + config.BarcodeReaderConfigs[index].ReaderToCenterDegree + 180) / 180 * Math.PI)));

            barcodeReaderData.LocateData = new LocateAGVPosition(agvPosition, agvAngleInMap, barcodeAngleInMap, barcodeReaderData.ScanTime,
                barcodeReaderData.GetDataTime, barcodeReaderData.Count, barcodeReaderData.Barcode1.Type, device, DriverConfig.Order);
        }
        #endregion

        #region WriteCSV.
        private void WriteCSVLog(BarcodeReaderData barcodeReaderData, string barcodeReaderName)
        {
            try
            {
                string csvLog = String.Concat(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));

                if (barcodeReaderData != null)
                {
                    csvLog = String.Concat(csvLog, ",", barcodeReaderData.GetDataTime.ToString("yyyy/MM/dd HH:mm:ss.fff"), ",", barcodeReaderName, ",",
                                                        barcodeReaderData.Count.ToString(), ",", barcodeReaderData.LocateData.ScanTime, ",",
                                                        barcodeReaderData.LocateData.Type.ToString());

                    if (barcodeReaderData.Barcode1 != null && barcodeReaderData.Barcode2 != null)
                    {
                        csvLog = String.Concat(csvLog, ",", barcodeReaderData.Barcode1.ID.ToString(), ",",
                                                            barcodeReaderData.Barcode1.ViewPosition.X.ToString(), ",", barcodeReaderData.Barcode1.ViewPosition.Y.ToString(), ",",
                                                            barcodeReaderData.Barcode1.MapPosition.X.ToString(), ",", barcodeReaderData.Barcode1.MapPosition.Y.ToString(), ",",
                                                            barcodeReaderData.Barcode2.ID.ToString(), ",",
                                                            barcodeReaderData.Barcode2.ViewPosition.X.ToString(), ",", barcodeReaderData.Barcode2.ViewPosition.Y.ToString(), ",",
                                                            barcodeReaderData.Barcode2.MapPosition.X.ToString(), ",", barcodeReaderData.Barcode2.MapPosition.Y.ToString());

                        if (barcodeReaderData.LocateData != null)
                            csvLog = String.Concat(csvLog, ",", barcodeReaderData.LocateData.AGVPosition.Position.X.ToString(), ",", barcodeReaderData.LocateData.AGVPosition.Position.Y.ToString(), ",", barcodeReaderData.LocateData.AGVPosition.Angle.ToString());
                        //csvLog = String.Concat(csvLog, ",", barcodeReaderData.TargetCenter.X.ToString(), ",", barcodeReaderData.TargetCenter.Y.ToString(), ",", barcodeReaderData.LocateData.AGVPosition.Angle.ToString());
                    }
                }

                logger.LogString(csvLog);
            }
            catch { }
        }
        #endregion

        private void PollingThread(object index)
        {
            string data = "";
            uint count = 0;
            string errorMessage = "";
            int intIndex = (int)index;
            int noDataCount = 0;
            BarcodeReaderData barcodeReaderData;
            Stopwatch timeoutTimer = new Stopwatch();
            bool dataNoMath = false;

            while (Status != EnumControlStatus.Closing)
            {
                barcodeReaderData = null;

                timeoutTimer.Restart();

                if (PoolingOnOff)
                {
                    try
                    {
                        if (BarcodeReaderList[intIndex].Error)
                            ;
                        else if (BarcodeReaderList[intIndex].ReadBarcode(ref data, config.TimeoutValue, ref errorMessage))
                        {
                            barcodeReaderData = new BarcodeReaderData(data, config.BarcodeReaderConfigs[intIndex].BarcodeReaderType, count);

                            if (barcodeReaderData.DataNotMatch)
                            {
                                if (!dataNoMath)
                                {
                                    WriteLog(3, "", String.Concat("BarcodeReader ID : ", config.BarcodeReaderConfigs[intIndex].ID, " DataNotMath, Data : ", data));
                                    SendAlarmCode(EnumMoveCommandControlErrorCode.LocateDriver_BarcodeMapSystem回傳資料格式錯誤);
                                    dataNoMath = true;
                                }
                            }
                            else
                            {
                                if (dataNoMath)
                                {
                                    WriteLog(3, "", String.Concat("BarcodeReader ID : ", config.BarcodeReaderConfigs[intIndex].ID, " Data cahnge to Math"));
                                    dataNoMath = false;
                                }

                                #region Barcode資料正常,計算.
                                //XChangeTheta55ALL(intIndex, barcodeReaderData);

                                if (GetReadDataPosition(barcodeReaderData))
                                {
                                    ComputeMapPosition(intIndex, barcodeReaderData);

                                    if (barcodeReaderData.ScanTime > config.TimeoutValue || noDataCount >= 3)
                                    {
                                        if (barcodeReaderData.LocateData != null)
                                            barcodeReaderData.LocateData.Type = EnumAGVPositionType.OnlyRead;
                                    }

                                    noDataCount = 0;
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            noDataCount++;

                            if (BarcodeReaderList[intIndex].Error)
                            {
                                WriteLog(3, "", String.Concat("BarcodeReader ID : ", config.BarcodeReaderConfigs[intIndex].ID, ", Error : ", errorMessage.ToString()));
                                allNotError = true;
                                status = EnumControlStatus.Error;
                            }
                        }

                        count++;
                    }
                    catch (Exception ex)
                    {
                        WriteLog(3, "", String.Concat("BarcodeReader ID : ", config.BarcodeReaderConfigs[intIndex].ID, ", Exception : ", ex.ToString()));
                        SendAlarmCode(EnumMoveCommandControlErrorCode.LocateDriver_BarcodeMapSystemTriggerException);
                    }
                }

                barcodeReaderDataList[intIndex] = barcodeReaderData;

                #region 寫入CSV.
                if (config.LogMode /*&& barcodeReaderData != null && barcodeReaderData.AGV != null*/)
                {
                    lock (lockObejct)
                    {
                        WriteCSVLog(barcodeReaderData, config.BarcodeReaderConfigs[intIndex].ID);
                    }
                }
                #endregion

                while (timeoutTimer.ElapsedMilliseconds < config.TimeoutValue)
                    Thread.Sleep(1);

                Thread.Sleep(config.SleepTime);
            }
        }

        public LocateAGVPosition GetLocateAGVPositionByIndex(int index)
        {
            if (index < config.BarcodeReaderConfigs.Count)
            {
                BarcodeReaderData temp = barcodeReaderDataList[index];

                if (temp == null)
                    return null;
                else
                    return temp.LocateData;
            }
            else
                return null;
        }

        override public LocateAGVPosition GetLocateAGVPosition
        {
            get
            {
                if (status == EnumControlStatus.NotInitial)
                    return null;

                try
                {
                    LocateAGVPosition returnValue = null;
                    BarcodeReaderData temp = null;

                    for (int i = 0; i < config.BarcodeReaderConfigs.Count; i++)
                    {
                        temp = barcodeReaderDataList[i];

                        if (temp != null && temp.LocateData != null)
                        {
                            if (returnValue == null || returnValue.Type < temp.LocateData.Type)
                            {
                                returnValue = temp.LocateData;

                                if (returnValue.Type == EnumAGVPositionType.Normal)
                                    break;
                            }
                        }
                    }

                    return returnValue;
                }
                catch (Exception ex)
                {
                    WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                    return null;
                }
            }
        }
    }
}
