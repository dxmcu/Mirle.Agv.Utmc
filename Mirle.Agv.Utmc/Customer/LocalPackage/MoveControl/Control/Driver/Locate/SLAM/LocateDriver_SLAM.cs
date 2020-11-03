using Mirle.Agv.INX.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Controller
{
    public class LocateDriver_SLAM : LocateDriver
    {
        protected LocateAGVPosition originAGVPosition = null;
        protected LocateAGVPosition offsetAGVPosition = null;
        protected LocateAGVPosition nowAGVPosition = null;

        protected Dictionary<string, MapAGVPosition> slamPosition = new Dictionary<string, MapAGVPosition>();
        protected Dictionary<string, MapAGVPosition> setSlamPosition = new Dictionary<string, MapAGVPosition>();
        protected Dictionary<string, SectionLine> findSectionList = new Dictionary<string, SectionLine>();
        protected Dictionary<string, double> sectionSLAMAngleRange = new Dictionary<string, double>();
        protected Dictionary<string, double> sectionSLAMAngle = new Dictionary<string, double>();

        protected Dictionary<string, SLAMTransfer> sectionSLAMTransferLit = new Dictionary<string, SLAMTransfer>();

        protected Dictionary<string, List<string>> sectionConnectSectionList = new Dictionary<string, List<string>>();

        protected string writeSlamsPositionPath = "";

        protected SectionLineTransferData transferData = new SectionLineTransferData();
        //protected SLAMTransfer transferData_Manual = new SLAMTransfer();
        protected string manualSection = "";

        protected Thread searchSectionThread = null;
        protected DateTime lastSearchSectionTime = DateTime.Now;

        protected SLAMOffseet slamOffset = null;

        override public void InitialDriver(LocateDriverConfig driverConfig, AlarmHandler alarmHandler, string normalLogName)
        {
            LocateType = EnumLocateType.SLAM;

            this.normalLogName = normalLogName;
            this.DriverConfig = driverConfig;
            this.alarmHandler = alarmHandler;
            device = driverConfig.LocateDriverType.ToString();
            InitialConfig(driverConfig.Path);
        }

        protected void SetFindSectionData()
        {
            SectionLine slamSection;
            SLAMTransfer slamTransfer;
            MapAddress from;
            MapAddress to;
            double sectionAngle;
            double sectionDistance;

            foreach (MapSection section in localData.TheMapInfo.AllSection.Values)
            {
                if (slamPosition.ContainsKey(section.FromAddress.Id) && slamPosition[section.FromAddress.Id] != null &&
                    slamPosition.ContainsKey(section.ToAddress.Id) && slamPosition[section.ToAddress.Id] != null)
                {
                    from = new MapAddress();
                    from.Id = section.FromAddress.Id;
                    from.AGVPosition = slamPosition[section.FromAddress.Id];


                    to = new MapAddress();
                    to.Id = section.ToAddress.Id;
                    to.AGVPosition = slamPosition[section.ToAddress.Id];

                    sectionAngle = computeFunction.ComputeAngle(from.AGVPosition, to.AGVPosition);
                    sectionDistance = computeFunction.GetDistanceFormTwoAGVPosition(from.AGVPosition, to.AGVPosition);

                    slamSection = new SectionLine(section, from, to, sectionAngle, sectionDistance, 0, true, 0);
                    findSectionList.Add(section.Id, slamSection);

                    slamTransfer = new SLAMTransfer();
                    slamTransfer.Step1Offset = slamPosition[section.FromAddress.Id].Position;


                    sectionSLAMTransferLit.Add(section.Id, GetTransferBySection(section));

                    //sectionSLAMTransferLit.Add(section.Id, GetTransferByFourPosition(section.FromAddress.AGVPosition.Position, section.ToAddress.AGVPosition.Position,
                    //                           slamPosition[section.FromAddress.Id].Position, slamPosition[section.ToAddress.Id].Position, section.FromVehicleAngle, section.ToVehicleAngle));

                    SLAMTransfer temp = sectionSLAMTransferLit[section.Id];
                    WriteLog(7, "", String.Concat("Section : ", section.Id,
                       ", Step1Offset (", computeFunction.GetMapPositionString(temp.Step1Offset, "0"),
                       "), Step2Cos : ", temp.Step2Cos.ToString("0.0"),
                       ", Step2Sin : ", temp.Step2Sin.ToString("0.0"),
                       ", Step3Mag : ", temp.Step3Mag.ToString("0.0"),
                       ", Step4Offset (", computeFunction.GetMapPositionString(temp.Step4Offset, "0"),
                       "), ThetaOffset : ", temp.ThetaOffset.ToString("0.0"),
                       ", ThetaOffsetStart : ", temp.ThetaOffsetStart.ToString("0.0"),
                       ", ThetaOffsetEnd : ", temp.ThetaOffsetEnd.ToString("0.0"),
                       ", Distance : ", temp.Distance.ToString("0.0")));
                }
            }

            List<string> tempList;

            foreach (MapSection section in localData.TheMapInfo.AllSection.Values)
            {
                if (findSectionList.ContainsKey(section.Id))
                {
                    tempList = new List<string>();

                    for (int i = 0; i < section.NearbySection.Count; i++)
                    {
                        if (findSectionList.ContainsKey(section.NearbySection[i].Id))
                            tempList.Add(section.NearbySection[i].Id);
                    }

                    sectionConnectSectionList.Add(section.Id, tempList);
                }
            }
        }

        private SLAMTransfer GetTransferBySection(MapSection section)
        {
            SLAMTransfer temp = new SLAMTransfer();
            temp.Step1Offset = slamPosition[section.FromAddress.Id].Position;
            temp.Step4Offset = section.FromAddress.AGVPosition.Position;

            temp.Step3Mag = section.Distance / computeFunction.GetTwoPositionDistance(slamPosition[section.FromAddress.Id].Position, slamPosition[section.ToAddress.Id].Position);

            double mapAngle = computeFunction.ComputeAngle(section.FromAddress.AGVPosition.Position, section.ToAddress.AGVPosition.Position);
            MapPosition start = new MapPosition(slamPosition[section.FromAddress.Id].Position.X, -slamPosition[section.FromAddress.Id].Position.Y);
            MapPosition end = new MapPosition(slamPosition[section.ToAddress.Id].Position.X, -slamPosition[section.ToAddress.Id].Position.Y);

            double slamAngle = computeFunction.ComputeAngle(start, end);
            temp.ThetaOffset = mapAngle - slamAngle;

            // theta 要帶-/+?
            temp.Step2Sin = Math.Sin(-temp.ThetaOffset * Math.PI / 180);
            temp.Step2Cos = Math.Cos(-temp.ThetaOffset * Math.PI / 180);
            temp.ThetaOffset = (slamPosition[section.FromAddress.Id].Angle + slamPosition[section.ToAddress.Id].Angle) / 2;
            temp.ThetaOffsetStart = section.FromVehicleAngle + slamPosition[section.FromAddress.Id].Angle + (section.FromAddress.AGVPosition.Angle - section.FromVehicleAngle);
            temp.ThetaOffsetEnd = section.ToVehicleAngle + slamPosition[section.ToAddress.Id].Angle + (section.ToAddress.AGVPosition.Angle - section.ToVehicleAngle);

            double angle = computeFunction.ComputeAngle(start, end);
            temp.SinTheta = Math.Sin(-angle * Math.PI / 180);
            temp.CosTheta = Math.Cos(-angle * Math.PI / 180);
            temp.Distance = computeFunction.GetTwoPositionDistance(start, end);

            return temp;

        }

        private SLAMTransfer GetTransferByFourPosition(MapPosition startMap, MapPosition endMap, MapPosition startSLAM, MapPosition endSLAM, double startAngle, double endAngle)
        {
            SLAMTransfer temp = new SLAMTransfer();
            temp.Step1Offset = startSLAM;
            temp.Step4Offset = startMap;
            temp.Step3Mag = computeFunction.GetTwoPositionDistance(startMap, endMap) / computeFunction.GetTwoPositionDistance(startSLAM, endSLAM);

            double mapAngle = computeFunction.ComputeAngle(startMap, endMap);
            MapPosition start = new MapPosition(startSLAM.X, -startSLAM.Y);
            MapPosition end = new MapPosition(endSLAM.X, -endSLAM.Y);

            double slamAngle = computeFunction.ComputeAngle(start, end);
            temp.ThetaOffset = mapAngle - slamAngle;

            // theta 要帶-/+?
            temp.Step2Sin = Math.Sin(-temp.ThetaOffset * Math.PI / 180);
            temp.Step2Cos = Math.Cos(-temp.ThetaOffset * Math.PI / 180);

            temp.Distance = computeFunction.GetTwoPositionDistance(start, end);

            return temp;
        }

        protected void SetSLAMOffset()
        {

        }

        private void ReadOffsetXML(string localPath)
        {
            string path = Path.Combine(new DirectoryInfo(localPath).Parent.FullName, "SlamOffset.txt");

            try
            {
                if (!File.Exists(path))
                {
                    WriteLog(7, "", "SlamOffset.txt不存在, SlamOffset = 0");
                    return;
                }

                string[] allRows = File.ReadAllLines(path);

                if (allRows.Length != 1)
                {
                    WriteLog(3, "", "SlamOffset.txt 格式不對(行數)");
                    return;
                }

                string[] data = Regex.Split(allRows[0], ",", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));

                if (data == null || data.Length != 3)
                    WriteLog(3, "", "SlamOffset.txt 格式不對(thetaOffset,極座標角度,極座標距離)");
                else
                {
                    double thetaOffset;
                    double polar_Theta;
                    double polar_Distance;


                    if (double.TryParse(data[0], out thetaOffset) && double.TryParse(data[1], out polar_Theta) && double.TryParse(data[2], out polar_Distance))
                    {
                        slamOffset = new SLAMOffseet(thetaOffset, polar_Theta, polar_Distance);

                        WriteLog(7, "", String.Concat("SlamOffset : thetaOffset = ", thetaOffset.ToString("0.000"), ", polar_Theta = ", polar_Theta.ToString("0.000"), ", polar_Distance = ", polar_Distance.ToString("0.00")));

                        if (thetaOffset == 0 && polar_Theta == 0 && polar_Distance == 0)
                            slamOffset = null;
                    }
                    else
                        WriteLog(3, "", "SlamOffset.txt 格式不對(有非浮點數)");
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        protected bool ReadSLAMAddress(string path)
        {
            if (path == null || path == "")
            {
                WriteLog(3, "", "SLAMAddress 路徑錯誤為null或空值.");
                return false;
            }
            else if (!File.Exists(path))
            {
                WriteLog(3, "", String.Concat("找不到 SLAMAddress.csv, path : ", path));
                return false;
            }

            try
            {
                writeSlamsPositionPath = path.Replace(".csv", "_out.csv");

                string[] allRows = File.ReadAllLines(path);
                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                Dictionary<string, int> dicHeaderIndexes = new Dictionary<string, int>();

                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();

                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        if (dicHeaderIndexes.ContainsKey(keyword))
                            WriteLog(3, "", String.Concat("Title repeat : ", keyword));
                        else
                            dicHeaderIndexes.Add(keyword, i);
                    }
                }

                if (dicHeaderIndexes.ContainsKey("Address") && dicHeaderIndexes.ContainsKey("SLAM_X") &&
                    dicHeaderIndexes.ContainsKey("SLAM_Y")/* && dicHeaderIndexes.ContainsKey("SLAM_Theta")*/)
                    ;
                else
                {
                    WriteLog(3, "", String.Concat("Title must be : Address,SLAM_X,SLAM_Y,SLAM_Theta"));
                    return false;
                }

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');
                    MapAGVPosition temp = new MapAGVPosition();

                    if (getThisRow.Length == 4 || getThisRow.Length == 3)
                    {
                        temp.Position.X = double.Parse(getThisRow[dicHeaderIndexes["SLAM_X"]]);
                        temp.Position.Y = double.Parse(getThisRow[dicHeaderIndexes["SLAM_Y"]]);

                        if (dicHeaderIndexes.ContainsKey("SLAM_Theta") && getThisRow.Length == 4)
                            temp.Angle = double.Parse(getThisRow[dicHeaderIndexes["SLAM_Theta"]]);

                        if (!localData.TheMapInfo.AllAddress.ContainsKey(getThisRow[dicHeaderIndexes["Address"]]))
                        {
                            WriteLog(3, "", String.Concat("Address : ", getThisRow[dicHeaderIndexes["Address"]], " not find in mapAddress!"));
                            return false;
                        }
                        else if (slamPosition.ContainsKey(getThisRow[dicHeaderIndexes["Address"]]))
                        {
                            WriteLog(3, "", String.Concat("Address : ", getThisRow[dicHeaderIndexes["Address"]], " repeat in SLAMAddress!"));
                            return false;
                        }

                        slamPosition.Add(getThisRow[dicHeaderIndexes["Address"]], temp);
                        //originSlamPosition.Add(getThisRow[dicHeaderIndexes["Address"]], temp);
                    }
                    else if (getThisRow.Length == 1)
                    {
                        slamPosition.Add(getThisRow[dicHeaderIndexes["Address"]], null);
                        //originSlamPosition.Add(getThisRow[dicHeaderIndexes["Address"]], null);
                    }
                    else if (getThisRow.Length > 1)
                    {
                        WriteLog(5, "", String.Concat("Address : ", getThisRow[dicHeaderIndexes["Address"]], " 資料格式錯誤"));
                        slamPosition.Add(getThisRow[dicHeaderIndexes["Address"]], null);
                        //originSlamPosition.Add(getThisRow[dicHeaderIndexes["Address"]], null);
                    }
                }

                if (slamPosition.Count != localData.TheMapInfo.AllAddress.Count)
                {
                    WriteLog(3, "", "SLAM Address count not math in Map Address count!");
                    return false;
                }

                ReadOffsetXML(path);
                SetFindSectionData();
                return true;
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
                return false;
            }
        }

        virtual public void ResetAlarm()
        {
        }

        virtual public bool SetPositionByAddressID(string addressID, ref string errorMessage)
        {
            return false;
        }

        public LocateAGVPosition GetOriginAGVPosition
        {
            get
            {
                if (Status != EnumControlStatus.NotInitial && Status != EnumControlStatus.Initial)
                    return originAGVPosition;
                else
                    return null;
            }
        }

        override public LocateAGVPosition GetLocateAGVPosition
        {
            get
            {
                if (Status == EnumControlStatus.Ready)
                    return nowAGVPosition;
                else
                    return null;
            }
        }

        public bool SetSlamPosition(string address, int averageCount)
        {
            double timeOutValue = averageCount * 1000;
            MapAGVPosition agvPosition = new MapAGVPosition();
            MapAGVPosition temp;
            MapAGVPosition lastMapAGVPosition = null;
            double angleDelta = 0;

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Restart();

            for (int i = 0; i < averageCount;)
            {
                temp = originAGVPosition.AGVPosition;

                if (temp != null && (lastMapAGVPosition == null || lastMapAGVPosition != temp))
                {
                    lastMapAGVPosition = temp;
                    agvPosition.Position.X += (temp.Position.X / averageCount);
                    agvPosition.Position.Y += (temp.Position.Y / averageCount);

                    if (i == 0)
                        agvPosition.Angle = temp.Angle;

                    angleDelta += (computeFunction.GetCurrectAngle(temp.Angle - agvPosition.Angle) / averageCount);
                    i++;
                }
                else if (timer.ElapsedMilliseconds > timeOutValue)
                {
                    return false;
                }
            }

            agvPosition.Angle = computeFunction.GetCurrectAngle(agvPosition.Angle + angleDelta);

            if (setSlamPosition.ContainsKey(address))
                setSlamPosition[address] = agvPosition;
            else
                setSlamPosition.Add(address, agvPosition);

            return true;
        }

        public bool SetSlamPosition_AutoFindAddress(int averageCount)
        {
            double timeOutValue = averageCount * 1000;
            MapAGVPosition agvPosition = new MapAGVPosition();
            MapAGVPosition temp;
            MapAGVPosition lastMapAGVPosition = null;
            double angleDelta = 0;

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Restart();

            for (int i = 0; i < averageCount;)
            {
                temp = originAGVPosition.AGVPosition;

                if (temp != null && (lastMapAGVPosition == null || lastMapAGVPosition != temp))
                {
                    lastMapAGVPosition = temp;
                    agvPosition.Position.X += (temp.Position.X / averageCount);
                    agvPosition.Position.Y += (temp.Position.Y / averageCount);

                    if (i == 0)
                        agvPosition.Angle = temp.Angle;

                    angleDelta += (computeFunction.GetCurrectAngle(temp.Angle - agvPosition.Angle) / averageCount);
                    i++;
                }
                else if (timer.ElapsedMilliseconds > timeOutValue)
                {
                    return false;
                }
            }

            agvPosition.Angle = computeFunction.GetCurrectAngle(agvPosition.Angle + angleDelta);

            double min = -1;
            double tempMin;
            string addressID = "";

            foreach (var old in slamPosition)
            {
                if (old.Value != null)
                {
                    tempMin = computeFunction.GetTwoPositionDistance(old.Value, agvPosition);

                    if (min == -1 || tempMin < min)
                    {
                        addressID = old.Key;
                        min = tempMin;
                    }
                }
            }

            if (min != -1)
            {
                if (setSlamPosition.ContainsKey(addressID))
                    setSlamPosition[addressID] = agvPosition;
                else
                    setSlamPosition.Add(addressID, agvPosition);

                return true;
            }
            else
                return false;
        }

        public bool WirteSlamPosition()
        {
            try
            {
                string line = "";
                List<string> csvList = new List<string>();
                line = "Address,SLAM_X,SLAM_Y,SLAM_Theta";
                csvList.Add(line);

                foreach (var temp in setSlamPosition)
                {
                    line = String.Concat(temp.Key, ",", temp.Value.Position.X.ToString("0.000"), ",", temp.Value.Position.Y.ToString("0.000"), ",", temp.Value.Angle.ToString("0.000"));
                    csvList.Add(line);
                }

                using (StreamWriter outputFile = new StreamWriter(writeSlamsPositionPath))
                {
                    foreach (string t in csvList)
                        outputFile.WriteLine(t);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool WirteSlamPosition_All()
        {
            try
            {
                string line = "";
                List<string> csvList = new List<string>();
                line = "Address,SLAM_X,SLAM_Y,SLAM_Theta";
                csvList.Add(line);

                foreach (var temp in slamPosition)
                {
                    if (setSlamPosition.ContainsKey(temp.Key))
                        line = String.Concat(temp.Key, ",", setSlamPosition[temp.Key].Position.X.ToString("0.000"), ",", setSlamPosition[temp.Key].Position.Y.ToString("0.000"), ",", setSlamPosition[temp.Key].Angle.ToString("0.000"));
                    else
                        line = String.Concat(temp.Key, ",", temp.Value.Position.X.ToString("0.000"), ",", temp.Value.Position.Y.ToString("0.000"), ",", temp.Value.Angle.ToString("0.000"));

                    csvList.Add(line);
                }

                foreach (var temp in setSlamPosition)
                {
                    if (!slamPosition.ContainsKey(temp.Key))
                    {
                        line = String.Concat(temp.Key, ",", temp.Value.Position.X.ToString("0.000"), ",", temp.Value.Position.Y.ToString("0.000"), ",", temp.Value.Angle.ToString("0.000"));
                        csvList.Add(line);
                    }
                }

                using (StreamWriter outputFile = new StreamWriter(writeSlamsPositionPath))
                {
                    foreach (string t in csvList)
                        outputFile.WriteLine(t);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
