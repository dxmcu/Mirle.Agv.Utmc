using Mirle.Agv.INX.Model;
using System;
using System.Collections.Generic;
using System.IO;
using Mirle.Agv.INX.Model.Configs;
using System.Linq;
using Mirle.Agv.INX.Controller.Tools;
using System.Reflection;
using System.Drawing;

namespace Mirle.Agv.INX.Controller
{
    public class MapHandler
    {
        private string normalLogName = "";
        private string device = MethodInfo.GetCurrentMethod().ReflectedType.Name;
        private ComputeFunction computeFunction = ComputeFunction.Instance;

        private LoggerAgent loggerAgent = LoggerAgent.Instance;
        private MapInfo mapInfo;


        private LocalData localData = LocalData.Instance;


        #region aaddress.
        private string address_ID = "Id";
        private string address_PositionX = "PositionX";
        private string address_PositionY = "PositionY";
        private string address_Angle = "Angle";
        private string address_SpecialTurn = "SpecialTurn";
        private string address_InsideSectionId = "InsideSectionId";
        #endregion

        #region section.
        private string section_ID = "Id";
        private string section_FromAddress = "FromAddress";
        private string section_ToAddress = "ToAddress";
        private string section_Speed = "Speed";
        private string section_FromVehicleAngle = "FromVehicleAngle";
        private string section_ToVehicleAngle = "ToVehicleAngle";
        #endregion

        public MapHandler(string normalLogName)
        {
            this.normalLogName = normalLogName;
            InitialMap();
        }

        private void InitialMap()
        {
            mapInfo = new MapInfo();
            ReadAddressCsv(Path.Combine(localData.MapConfig.FileDirectory, localData.MapConfig.AddressCSVFileName));
            ReadSectionCsv(Path.Combine(localData.MapConfig.FileDirectory, localData.MapConfig.SectionCSVFileName));
            ReadObject(Path.Combine(localData.MapConfig.FileDirectory, localData.MapConfig.ObjectFileName));
            ProcessSectionData();
            ProcessAddressInsideSectionAndNearbySection();
            ProcessSectionNearbySection();
            WriteAddressSectionData();

            localData.TheMapInfo = mapInfo;
        }

        public void WriteLog(int logLevel, string carrierId, string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            LogFormat logFormat = new LogFormat(normalLogName, logLevel.ToString(), memberName, device, carrierId, message);

            loggerAgent.Log(logFormat.Category, logFormat);

            if (logLevel <= localData.ErrorLevel)
            {
                logFormat = new LogFormat(localData.ErrorLogName, logLevel.ToString(), memberName, device, carrierId, message);
                loggerAgent.Log(logFormat.Category, logFormat);
            }
        }

        #region 讀取Address.csv / Section.csv .
        public void ReadAddressCsv(string aaddressPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(aaddressPath))
                {
                    WriteLog(5, "", "MapConfig (FileDirectory + AddressCSVFileName) is null or whiteSpace");
                    return;
                }

                string[] allRows = File.ReadAllLines(aaddressPath);

                if (allRows == null || allRows.Length < 1)
                {
                    WriteLog(5, "", String.Concat("aadress.csv line == 0"));
                    return;
                }

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
                        {
                            WriteLog(3, "", String.Concat("Title : ", keyword, " repeat"));
                            return;
                        }
                        else
                            dicHeaderIndexes.Add(keyword, i);
                    }
                }

                if (dicHeaderIndexes.ContainsKey(address_ID) && dicHeaderIndexes.ContainsKey(address_PositionX) &&
                    dicHeaderIndexes.ContainsKey(address_PositionY))
                    ;
                else
                {
                    WriteLog(3, "", String.Concat(aaddressPath, " title 必須要有", address_ID, ",", address_PositionX, ",", address_PositionY, ",", address_Angle));
                    return;
                }

                int index = 0;

                for (; index < nRows; index++)
                {
                    string[] getThisRow = allRows[index].Split(',');
                    MapAddress oneRow = new MapAddress();

                    try
                    {
                        oneRow.Id = getThisRow[dicHeaderIndexes[address_ID]];
                        oneRow.AGVPosition.Position.X = double.Parse(getThisRow[dicHeaderIndexes[address_PositionX]]);
                        oneRow.AGVPosition.Position.Y = double.Parse(getThisRow[dicHeaderIndexes[address_PositionY]]);
                        oneRow.AGVPosition.Angle = double.Parse(getThisRow[dicHeaderIndexes[address_Angle]]);

                        if (dicHeaderIndexes.ContainsKey(address_SpecialTurn))
                            oneRow.SpecialTurn = getThisRow[dicHeaderIndexes[address_SpecialTurn]];

                        if (dicHeaderIndexes.ContainsKey(address_InsideSectionId))
                            oneRow.InsideSectionId = getThisRow[dicHeaderIndexes[address_InsideSectionId]];

                        mapInfo.AllAddress.Add(oneRow.Id, oneRow);
                    }
                    catch (Exception ex)
                    {
                        WriteLog(3, "", String.Concat("Exception (line = ", (index + 2).ToString(), ") : ", ex.ToString()));
                    }
                }

                WriteLog(7, "", String.Concat("ReadAddressCsv Scuess, address count : ", mapInfo.AllAddress.Count.ToString("0")));
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        public void ReadSectionCsv(string sectionPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sectionPath))
                {
                    WriteLog(5, "", "MapConfig (FileDirectory + SectionCSVFileName) is null or whiteSpace");
                    return;
                }

                string[] allRows = File.ReadAllLines(sectionPath);

                if (allRows == null || allRows.Length < 1)
                {
                    WriteLog(5, "", String.Concat("section.csv line == 0"));
                    return;
                }

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
                        {
                            WriteLog(3, "", String.Concat("Title : ", keyword, " repeat"));
                            return;
                        }
                        else
                            dicHeaderIndexes.Add(keyword, i);
                    }
                }

                if (dicHeaderIndexes.ContainsKey(section_ID) && dicHeaderIndexes.ContainsKey(section_FromAddress) &&
                    dicHeaderIndexes.ContainsKey(section_ToAddress) && dicHeaderIndexes.ContainsKey(section_Speed) &&
                    dicHeaderIndexes.ContainsKey(section_FromVehicleAngle) && dicHeaderIndexes.ContainsKey(section_ToVehicleAngle))
                    ;
                else
                {
                    WriteLog(3, "", String.Concat(sectionPath, " title 必須要有", section_ID, ",", section_FromAddress, ",", section_ToAddress, ",", section_Speed, ",", section_FromVehicleAngle, ",", section_ToVehicleAngle));
                    return;
                }

                int index = 0;

                for (; index < nRows; index++)
                {
                    string[] getThisRow = allRows[index].Split(',');
                    MapSection oneRow = new MapSection();

                    try
                    {

                        if (!mapInfo.AllAddress.ContainsKey(getThisRow[dicHeaderIndexes[section_FromAddress]]))
                            WriteLog(3, "", String.Concat("line : ", (index + 2).ToString(), ", ", section_FromAddress, " not find in address list"));
                        else if (!mapInfo.AllAddress.ContainsKey(getThisRow[dicHeaderIndexes[section_ToAddress]]))
                            WriteLog(3, "", String.Concat("line : ", (index + 2).ToString(), ", ", section_ToAddress, " not find in address list"));
                        else
                        {

                            oneRow.Id = getThisRow[dicHeaderIndexes[section_ID]];
                            oneRow.FromAddress = mapInfo.AllAddress[getThisRow[dicHeaderIndexes[section_FromAddress]]];
                            oneRow.ToAddress = mapInfo.AllAddress[getThisRow[dicHeaderIndexes[section_ToAddress]]];
                            oneRow.FromVehicleAngle = double.Parse(getThisRow[dicHeaderIndexes[section_FromVehicleAngle]]);
                            oneRow.ToVehicleAngle = double.Parse(getThisRow[dicHeaderIndexes[section_ToVehicleAngle]]);
                            oneRow.Distance = computeFunction.GetDistanceFormTwoAGVPosition(oneRow.FromAddress.AGVPosition, oneRow.ToAddress.AGVPosition);
                            oneRow.Speed = double.Parse(getThisRow[dicHeaderIndexes[section_Speed]]);

                            mapInfo.AllSection.Add(oneRow.Id, oneRow);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(3, "", String.Concat("Exception (line = ", (index + 2).ToString(), ") : ", ex.ToString()));
                    }
                }

                WriteLog(7, "", String.Concat("ReadAddressCsv Scuess, section count : ", mapInfo.AllSection.Count.ToString("0")));
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        public void ReadObject(string obejctPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(obejctPath))
                {
                    WriteLog(5, "", "MapConfig (FileDirectory + ObejctPath) is null or whiteSpace");
                    return;
                }

                string[] allRows = File.ReadAllLines(obejctPath);

                int nRows = allRows.Length;
                ObjectData tempObjectData;

                for (int j = 0; j < nRows; j++)
                {
                    string[] oldGetThisRow = allRows[j].Split(new char[3] { ',', '(', ')' });

                    List<string> getThisRow = new List<string>();
                    getThisRow.Add(oldGetThisRow[0]);

                    for (int i = 1; i < oldGetThisRow.Length; i++)
                    {
                        if (oldGetThisRow[i] != "")
                            getThisRow.Add(oldGetThisRow[i]);
                    }

                    try
                    {
                        if (getThisRow == null || (getThisRow.Count - 1) % 2 != 0 || ((getThisRow.Count - 1) / 2 <= 3))
                            WriteLog(3, "", String.Concat("line : ", (j + 1).ToString(), ", 資料有問題, 需要Name + 至少三個點"));
                        else
                        {
                            tempObjectData = new ObjectData();
                            tempObjectData.Name = getThisRow[0];

                            for (int i = 1; i < getThisRow.Count; i += 2)
                                tempObjectData.PositionList.Add(new MapPosition(double.Parse(getThisRow[i]), double.Parse(getThisRow[i + 1])));

                            mapInfo.ObjectDataList.Add(tempObjectData);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(3, "", String.Concat("Exception (line = ", (j + 1).ToString(), ") : ", ex.ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }
        #endregion

        #region 處理相鄰Section ( address 和 section皆有).
        private void ProcessSectionData()
        {
            try
            {
                foreach (MapSection section in mapInfo.AllSection.Values)
                {
                    double sectionAngle = computeFunction.ComputeAngle(section.FromAddress.AGVPosition, section.ToAddress.AGVPosition);

                    section.SectionAngle = sectionAngle;
                    section.CosTheta = Math.Cos(sectionAngle / 180 * Math.PI);
                    section.SinTheta = Math.Sin(sectionAngle / 180 * Math.PI);
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        private void ProcessAddressInsideSectionAndNearbySection()
        {
            try
            {
                bool first;
                double angle;

                foreach (MapAddress address in mapInfo.AllAddress.Values)
                {
                    if (mapInfo.AllSection.ContainsKey(address.InsideSectionId))
                        address.InsideSection = mapInfo.AllSection[address.InsideSectionId];


                    foreach (MapSection section in mapInfo.AllSection.Values)
                    {
                        if (section.FromAddress == address || section.ToAddress == address)
                        {
                            if (!address.NearbySection.Contains(section))
                                address.NearbySection.Add(section);
                        }
                    }

                    foreach (MapSection section in address.NearbySection)
                    {
                        if (section.FromAddress == address)
                            angle = section.FromVehicleAngle;
                        else
                            angle = section.ToVehicleAngle;

                        if (address.AGVPosition.Angle != angle)
                            address.CanSpin = true;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }

        private void ProcessSectionNearbySection()
        {
            try
            {
                foreach (MapSection section in mapInfo.AllSection.Values)
                {
                    foreach (MapSection section2 in mapInfo.AllSection.Values)
                    {
                        if (section.FromAddress == section2.FromAddress || section.FromAddress == section2.ToAddress ||
                            section.ToAddress == section2.FromAddress || section.ToAddress == section2.ToAddress)
                        {
                            if (!section.NearbySection.Contains(section2))
                                section.NearbySection.Add(section2);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }
        #endregion

        private void WriteAddressSectionData()
        {
            try
            {
                string logMessage = "Address Data :";

                foreach (MapAddress address in mapInfo.AllAddress.Values)
                {
                    logMessage = String.Concat(logMessage, " \r\naddress ID : ", address.Id, "( ", computeFunction.GetMapAGVPositionString(address.AGVPosition), " ) ", address.AGVPosition.Angle.ToString("0"),
                                                           ", CanSpin : ", address.CanSpin.ToString(), ", SpecialTurn : ", address.SpecialTurn, ", InsideSection : ",
                                                           (address.InsideSection == null ? "" : address.InsideSection.Id), ", NearbySection : ");

                    for (int i = 0; i < address.NearbySection.Count; i++)
                    {
                        if (i == 0)
                            logMessage = String.Concat(logMessage, address.NearbySection[i].Id);
                        else
                            logMessage = String.Concat(logMessage, ", ", address.NearbySection[i].Id);
                    }
                }

                logMessage = String.Concat(logMessage, "\r\n,Section Data :");

                foreach (MapSection section in mapInfo.AllSection.Values)
                {

                    logMessage = String.Concat(logMessage, " \r\nsection ID : ", section.Id, ", form : ", section.FromAddress.Id, ", to : ", section.ToAddress.Id,
                                                           ", speed : ", section.Speed.ToString("0"), ", Distance : ", section.Distance.ToString("0"),
                                                           ", startAngle : ", section.FromVehicleAngle.ToString("0"), ", endAngle : ", section.ToVehicleAngle.ToString("0"),
                                                           ", NearbySection : ");

                    for (int i = 0; i < section.NearbySection.Count; i++)
                    {
                        if (i == 0)
                            logMessage = String.Concat(logMessage, section.NearbySection[i].Id);
                        else
                            logMessage = String.Concat(logMessage, ", ", section.NearbySection[i].Id);
                    }
                }

                WriteLog(7, "", logMessage);
            }
            catch (Exception ex)
            {
                WriteLog(3, "", String.Concat("Exception : ", ex.ToString()));
            }
        }
    }
}
