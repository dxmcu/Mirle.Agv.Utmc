using Mirle.Agv.INX.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Controller
{
    public class ComputeFunction
    {
        static public ComputeFunction Instance = new ComputeFunction();

        private LocalData localData = LocalData.Instance;

        public double GetDistanceFormTwoAGVPosition(MapAGVPosition start, MapAGVPosition end)
        {
            if (start == null || end == null)
                return -1;
            else
                return GetDistanceFormTwoPosition(start.Position, end.Position);
        }

        public double GetDistanceFormTwoPosition(MapPosition start, MapPosition end)
        {
            if (start == null || end == null)
                return -1;
            else
                return Math.Sqrt(Math.Pow(start.X - end.X, 2) + Math.Pow(start.Y - end.Y, 2));
        }

        public string GetMapAGVPositionString(MapAGVPosition agvPosition, string digit = "0.0")
        {
            if (agvPosition == null)
                return GetMapPositionString(null, digit);
            else
                return GetMapPositionString(agvPosition.Position, digit);
        }

        public string GetMapPositionString(MapPosition position, string digit = "0.0")
        {
            if (position == null)
                return "---,---";
            else
                return String.Concat(position.X.ToString(digit), ", ", position.Y.ToString(digit));
        }

        public string GetMapAGVPositionStringWithAngle(MapAGVPosition agvPosition, string digit = "0.0")
        {
            return String.Concat("( ", GetMapAGVPositionString(agvPosition, digit), " ) ", (agvPosition == null ? "--" : agvPosition.Angle.ToString(digit)));
        }

        public string GetLocateAGVPositionStringWithAngle(LocateAGVPosition agvPosition, string digit = "0.0")
        {
            if (agvPosition == null)
                return GetMapAGVPositionStringWithAngle(null, digit);
            else
                return GetMapAGVPositionStringWithAngle(agvPosition.AGVPosition, digit);
        }

        public double ComputeAngle(MapPosition start, MapPosition end)
        {
            if (start == null || end == null)
                return -1;

            double returnAngle = 0;

            if (start.X == end.X)
            {
                if (start.Y > end.Y)
                    returnAngle = -90;
                else
                    returnAngle = 90;
            }
            else
            {
                returnAngle = Math.Atan((start.Y - end.Y) / (start.X - end.X)) * 180 / Math.PI;

                if (start.X > end.X)
                {
                    if (returnAngle > 0)
                        returnAngle -= 180;
                    else
                        returnAngle += 180;
                }
            }

            return returnAngle;
        }

        public double ComputeAngle(MapAGVPosition start, MapAGVPosition end)
        {
            if (start == null || end == null)
                return -1;

            return ComputeAngle(start.Position, end.Position);
        }

        public MapAGVPosition GetAGVPositionByVehicleLocation(VehicleLocation now)
        {
            if (localData.TheMapInfo.AllSection.ContainsKey(now.NowSection))
            {
                MapAGVPosition returnValue = new MapAGVPosition();

                returnValue.Angle = localData.TheMapInfo.AllSection[now.NowSection].FromVehicleAngle;
                returnValue.Position.X = localData.TheMapInfo.AllSection[now.NowSection].FromAddress.AGVPosition.Position.X +
                                         (localData.TheMapInfo.AllSection[now.NowSection].ToAddress.AGVPosition.Position.X -
                                          localData.TheMapInfo.AllSection[now.NowSection].FromAddress.AGVPosition.Position.X) *
                                         (now.DistanceFormSectionHead / localData.TheMapInfo.AllSection[now.NowSection].Distance);
                returnValue.Position.Y = localData.TheMapInfo.AllSection[now.NowSection].FromAddress.AGVPosition.Position.Y +
                                         (localData.TheMapInfo.AllSection[now.NowSection].ToAddress.AGVPosition.Position.Y -
                                          localData.TheMapInfo.AllSection[now.NowSection].FromAddress.AGVPosition.Position.Y) *
                                         (now.DistanceFormSectionHead / localData.TheMapInfo.AllSection[now.NowSection].Distance);
                
                return returnValue;
            }
            else
                return null;
        }

        public double ComputeAngle_PI(MapPosition start, MapPosition end)
        {
            double returnAngle = 0;

            if (start.X == end.X)
            {
                if (start.Y > end.Y)
                    returnAngle = -Math.PI / 2;
                else
                    returnAngle = Math.PI / 2;
            }
            else
            {
                returnAngle = Math.Atan((start.Y - end.Y) / (start.X - end.X));

                if (start.X > end.X)
                {
                    if (returnAngle > 0)
                        returnAngle -= Math.PI;
                    else
                        returnAngle += Math.PI;
                }
            }

            return returnAngle;
        }

        public bool IsSamePosition(MapPosition a, MapPosition b)
        {
            if (a == null || b == null || a.X != b.X || a.Y != b.Y)
                return false;
            else
                return true;
        }

        public double GetCurrectAngle(double angle)
        {
            while (angle > 180 || angle <= -180)
            {
                if (angle > 180)
                    angle -= 360;
                else
                    angle += 360;
            }

            return angle;
        }

        public MapAGVPosition GetAGVPositionFormEndDistance(MapPosition start, MapAGVPosition end, double endDistance)
        {
            double distance = Math.Sqrt(Math.Pow(start.X - end.Position.X, 2) + Math.Pow(start.Y - end.Position.Y, 2));
            double x = end.Position.X + (start.X - end.Position.X) * endDistance / distance;
            double y = end.Position.Y + (start.Y - end.Position.Y) * endDistance / distance;

            return new MapAGVPosition(new MapPosition(x, y), end.Angle);
        }

        public MapPosition GetTransferPosition(MapSection section, MapPosition position)
        {
            if (section == null || position == null)
                return null;

            double x = position.X - section.FromAddress.AGVPosition.Position.X;
            double y = position.Y - section.FromAddress.AGVPosition.Position.Y;

            double newX = x * section.CosTheta + y * section.SinTheta;
            double newY = -x * section.SinTheta + y * section.CosTheta;

            return new MapPosition(newX, newY);
        }

        public MapPosition GetTransferPosition(SectionLine section, MapPosition position)
        {
            if (section == null || position == null)
                return null;

            double x = position.X - section.Start.AGVPosition.Position.X;
            double y = position.Y - section.Start.AGVPosition.Position.Y;

            double newX = x * section.CosTheta + y * section.SinTheta;
            double newY = -x * section.SinTheta + y * section.CosTheta;

            return new MapPosition(newX, newY);
        }

        #region From-to Address、SectionList.
        public MapAddress CheckNowAndFirstSectionAndAddress(MapSection section, MapAddress address, ref string errorMessage)
        {
            MapAGVPosition agvPosition = localData.Real;
            VehicleLocation nowLocation = localData.Location;

            if (agvPosition == null || agvPosition.Position == null || !localData.TheMapInfo.AllAddress.ContainsKey(nowLocation.LastAddress))
            {
                errorMessage = "迷航中";
                return null;
            }

            if (nowLocation.InAddress)
                return localData.TheMapInfo.AllAddress[nowLocation.LastAddress];

            MapAddress returnAddress = null;

            if (section.Id != nowLocation.NowSection)
            {
                if (section.FromAddress == localData.TheMapInfo.AllSection[nowLocation.NowSection].FromAddress ||
                    section.FromAddress == localData.TheMapInfo.AllSection[nowLocation.NowSection].ToAddress)
                    returnAddress = section.FromAddress;
                else if (section.ToAddress == localData.TheMapInfo.AllSection[nowLocation.NowSection].FromAddress ||
                         section.ToAddress == localData.TheMapInfo.AllSection[nowLocation.NowSection].ToAddress)
                    returnAddress = section.ToAddress;
                else
                {
                    errorMessage = "判定不再Address上且命令起始Section和local判定Section不同,且Section本身無相交,異常";
                    return null;
                }

                if (GetDistanceFormTwoAGVPosition(returnAddress.AGVPosition, agvPosition) > localData.MoveControlData.CreateMoveCommandConfig.SafteyDistance[EnumCommandType.Move] / 2)
                {
                    errorMessage = "判定不再Address上且命令起始Section和local判定Section不同,兩條Section交點和命令起始Address差異過大";
                    return null;
                }
                else
                    return returnAddress;
            }
            else
            {
                if (section.FromVehicleAngle == section.ToVehicleAngle)
                {
                    returnAddress = new MapAddress();
                    returnAddress.InsideSection = section;
                    returnAddress.AGVPosition.Angle = section.FromVehicleAngle;
                    returnAddress.AGVPosition.Position = GetTransferPosition(agvPosition, section);

                    if (returnAddress.AGVPosition.Position == null)
                    {
                        errorMessage = "ComputeFunction.GetTransferPosition return null";
                        return null;
                    }

                    return returnAddress;
                }
                else
                {
                    errorMessage = "不能再RTurn Section啟動";
                    return null;
                }
            }
        }

        public MapPosition GetTransferPosition(MapAGVPosition agvPosition, MapSection section)
        {
            if (section == null || agvPosition == null || agvPosition.Position == null)
                return null;

            double x = agvPosition.Position.X - section.FromAddress.AGVPosition.Position.X;
            double y = agvPosition.Position.Y - section.FromAddress.AGVPosition.Position.Y;

            double newX = x * section.CosTheta + y * section.SinTheta;

            if (newX < 0)
                newX = 0;
            else if (newX > section.Distance)
                newX = section.Distance;

            double returnX = section.FromAddress.AGVPosition.Position.X + (section.ToAddress.AGVPosition.Position.X - section.FromAddress.AGVPosition.Position.X) * newX / section.Distance;
            double returnY = section.FromAddress.AGVPosition.Position.Y + (section.ToAddress.AGVPosition.Position.Y - section.FromAddress.AGVPosition.Position.Y) * newX / section.Distance;

            return new MapPosition(returnX, returnY);
        }

        public MapAddress FindMapAddressByMapAGVPosition(MapAGVPosition now, string nowSection)
        {
            MapSection section = localData.TheMapInfo.AllSection[nowSection];
            MapPosition position = GetTransferPosition(now, section);

            MapAddress returnAddress = new MapAddress();
            returnAddress.InsideSection = section;
            returnAddress.AGVPosition.Angle = section.FromVehicleAngle;
            returnAddress.AGVPosition.Position = GetTransferPosition(now, section);

            return returnAddress;
        }

        #region From-to.
        private void FindAGVFromToPath(MapAddress endAddress, List<MapAddress> nodeList, double nowDistance, ref List<MapAddress> minNodeList, ref double minDistance, List<MapSection> tempSectionList, ref List<MapSection> mapSectionList)
        {
            if (endAddress == nodeList[nodeList.Count - 1])
            {
                if (minDistance == -1 || minDistance > nowDistance)
                {
                    minNodeList = new List<MapAddress>();
                    for (int i = 0; i < nodeList.Count; i++)
                        minNodeList.Add(nodeList[i]);

                    mapSectionList = new List<MapSection>();
                    for (int i = 0; i < tempSectionList.Count; i++)
                        mapSectionList.Add(tempSectionList[i]);

                    minDistance = nowDistance;
                }
            }
            else
            {
                if (minDistance == -1 || nowDistance < minDistance)
                {
                    bool notRepeat;
                    MapAddress nextNode;

                    foreach (MapSection tempSection in nodeList[nodeList.Count - 1].NearbySection)
                    {
                        if (nodeList[nodeList.Count - 1] == tempSection.FromAddress)
                            nextNode = tempSection.ToAddress;
                        else
                            nextNode = tempSection.FromAddress;

                        notRepeat = true;

                        for (int i = 0; i < nodeList.Count && notRepeat; i++)
                        {
                            if (nodeList[i] == nextNode)
                                notRepeat = false;
                        }

                        if (notRepeat)
                        {
                            nodeList.Add(nextNode);
                            tempSectionList.Add(tempSection);
                            FindAGVFromToPath(endAddress, nodeList, nowDistance + tempSection.Distance / tempSection.Speed, ref minNodeList, ref minDistance, tempSectionList, ref mapSectionList);
                            nodeList.RemoveAt(nodeList.Count - 1);
                            tempSectionList.RemoveAt(tempSectionList.Count - 1);
                        }
                    }
                }
            }
        }

        public bool FindFromToAddressSectionList(MapAddress start, MapAddress end, ref MoveCmdInfo moveCmdInfo)
        {
            try
            {
                if (start != end)
                {
                    #region 終點在目前SectionList最後一個之中.
                    if (moveCmdInfo.MovingSections.Count != 0 && moveCmdInfo.MovingSections[moveCmdInfo.MovingSections.Count - 1] == end.InsideSection)
                    {
                        double sectionAngle = ComputeAngle(moveCmdInfo.MovingAddress[moveCmdInfo.MovingAddress.Count - 2].AGVPosition,
                                                           moveCmdInfo.MovingAddress[moveCmdInfo.MovingAddress.Count - 1].AGVPosition);

                        double endAngle = ComputeAngle(moveCmdInfo.MovingAddress[moveCmdInfo.MovingAddress.Count - 2].AGVPosition, end.AGVPosition);

                        if (sectionAngle != endAngle)
                        {
                            moveCmdInfo.MovingAddress.Add(moveCmdInfo.MovingAddress[moveCmdInfo.MovingAddress.Count - 2]);
                            moveCmdInfo.MovingSections.Add(moveCmdInfo.MovingSections[moveCmdInfo.MovingSections.Count - 1]);
                        }

                        moveCmdInfo.EndAddress = end;
                        return true;
                    }
                    #endregion

                    #region 起點和終點都是內點.
                    if (start.InsideSection != null && end.InsideSection != null &&
                        start.InsideSection == end.InsideSection)
                    {
                        double angleStartToEnd = ComputeAngle(start.AGVPosition, end.AGVPosition);
                        double sectionAngle = ComputeAngle(start.InsideSection.FromAddress.AGVPosition, start.InsideSection.ToAddress.AGVPosition);

                        moveCmdInfo.MovingSections.Add(start.InsideSection);

                        if (angleStartToEnd == sectionAngle)
                        {
                            moveCmdInfo.MovingAddress.Add(start.InsideSection.FromAddress);
                            moveCmdInfo.MovingAddress.Add(start.InsideSection.ToAddress);
                        }
                        else
                        {
                            moveCmdInfo.MovingAddress.Add(start.InsideSection.ToAddress);
                            moveCmdInfo.MovingAddress.Add(start.InsideSection.FromAddress);
                        }

                        moveCmdInfo.EndAddress = end;

                        return true;
                    }
                    #endregion

                    List<MapAddress> movingAddress = new List<MapAddress>();
                    List<MapSection> movingSection = new List<MapSection>();

                    List<double> startAddressToSectionNodeDistance = new List<double>();
                    List<MapAddress> startSectionNode = new List<MapAddress>();
                    List<MapAddress> startSectionNode2 = new List<MapAddress>();
                    List<double> endAddressToSectionNodeDistance = new List<double>();
                    List<MapAddress> endSectionNode = new List<MapAddress>();

                    double distance = 0;

                    if (start.InsideSection == null)
                    {
                        startAddressToSectionNodeDistance.Add(0);
                        startSectionNode.Add(start);
                    }
                    else
                    {
                        distance = GetTwoPositionDistance(start.AGVPosition, start.InsideSection.FromAddress.AGVPosition);
                        startAddressToSectionNodeDistance.Add(distance / start.InsideSection.Speed);
                        startSectionNode.Add(start.InsideSection.FromAddress);
                        startSectionNode2.Add(start.InsideSection.ToAddress);

                        distance = GetTwoPositionDistance(start.AGVPosition, start.InsideSection.ToAddress.AGVPosition);
                        startAddressToSectionNodeDistance.Add(distance / start.InsideSection.Speed);
                        startSectionNode.Add(start.InsideSection.ToAddress);
                        startSectionNode2.Add(start.InsideSection.FromAddress);
                    }

                    if (end.InsideSection == null)
                    {
                        endAddressToSectionNodeDistance.Add(0);
                        endSectionNode.Add(end);
                    }
                    else
                    {
                        distance = GetTwoPositionDistance(end.AGVPosition, end.InsideSection.FromAddress.AGVPosition);
                        endAddressToSectionNodeDistance.Add(distance / end.InsideSection.Speed);
                        endSectionNode.Add(end.InsideSection.ToAddress);

                        distance = GetTwoPositionDistance(end.AGVPosition, end.InsideSection.FromAddress.AGVPosition);
                        endAddressToSectionNodeDistance.Add(distance / end.InsideSection.Speed);
                        endSectionNode.Add(end.InsideSection.FromAddress);
                    }

                    if (startSectionNode.Count > 1 && (startSectionNode[0] == endSectionNode[0] || startSectionNode[1] == endSectionNode[0]))
                    {
                        if (startSectionNode[0] == endSectionNode[0])
                            moveCmdInfo.MovingAddress.Add(startSectionNode[1]);
                        else
                            moveCmdInfo.MovingAddress.Add(startSectionNode[0]);

                        moveCmdInfo.MovingAddress.Add(endSectionNode[0]);
                        moveCmdInfo.MovingSections.Add(start.InsideSection);
                        return true;
                    }

                    List<MapSection> tempSectionList = new List<MapSection>();
                    double minDistance = -1;
                    List<MapAddress> nodeList = new List<MapAddress>();

                    for (int i = 0; i < startSectionNode.Count; i++)
                    {
                        for (int j = 0; j < endSectionNode.Count; j++)
                        {
                            //if (start.InsideSection != null)
                            //    nodeList.Add(start);
                            tempSectionList = new List<MapSection>();

                            if (start.InsideSection != null)
                                tempSectionList.Add(start.InsideSection);

                            nodeList.Add(startSectionNode[i]);
                            if (startSectionNode2.Count > 0)
                                nodeList.Add(startSectionNode2[i]);

                            FindAGVFromToPath(endSectionNode[j], nodeList, startAddressToSectionNodeDistance[i] + endAddressToSectionNodeDistance[j], ref movingAddress, ref minDistance, tempSectionList, ref movingSection);
                            nodeList = new List<MapAddress>();
                        }
                    }

                    if (minDistance != -1)
                    {
                        double sectionAngle;
                        double tempAngle;

                        //if (start.InsideSection != null)
                        //{
                        //    movingSection.Insert(0, start.InsideSection);
                        //    sectionAngle = ComputeAngle(movingSection[0].FromAddress.AGVPosition, movingSection[0].ToAddress.AGVPosition);
                        //    tempAngle = ComputeAngle(start.AGVPosition, movingSection[0].ToAddress.AGVPosition);

                        //    if (sectionAngle == tempAngle)
                        //        movingAddress[0] = movingSection[0].FromAddress;
                        //    else
                        //        movingAddress[0] = movingSection[0].ToAddress;
                        //}

                        if (end.InsideSection != null)
                        {
                            movingSection.Add(end.InsideSection);
                            sectionAngle = ComputeAngle(movingSection[movingSection.Count - 1].FromAddress.AGVPosition, movingSection[movingSection.Count - 1].ToAddress.AGVPosition);
                            tempAngle = ComputeAngle(start.AGVPosition, movingSection[movingSection.Count - 1].ToAddress.AGVPosition);

                            if (sectionAngle == tempAngle)
                                movingAddress[movingAddress.Count - 1] = movingSection[movingSection.Count - 1].ToAddress;
                            else
                                movingAddress[movingAddress.Count - 1] = movingSection[movingSection.Count - 1].FromAddress;
                        }

                        if (moveCmdInfo.MovingAddress.Count == 0)
                            moveCmdInfo.MovingAddress.Add(movingAddress[0]);

                        for (int i = 1; i < movingAddress.Count; i++)
                            moveCmdInfo.MovingAddress.Add(movingAddress[i]);

                        for (int i = 0; i < movingSection.Count; i++)
                            moveCmdInfo.MovingSections.Add(movingSection[i]);

                        return true;
                    }
                    else
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
        #endregion

        #region 分解命令相關.
        public bool TurnAndBTurnTypeCheck(string action, EnumTurnType type)
        {
            if (action[0] == 'B')
                action = action.Substring(1, action.Length - 1);

            return TurnTypeCheck(action, type);
        }

        public bool TurnTypeCheck(string action, EnumTurnType type)
        {
            if (localData.MoveControlData.TurnParameter.ContainsKey(action))
                return type == localData.MoveControlData.TurnParameter[action].Type;
            else if (type == EnumTurnType.SpinTurn)
                return action == EnumDefaultAction.SpinTurn.ToString();
            else if (type == EnumTurnType.StopTurn)
                return action == EnumDefaultAction.StopTurn.ToString();
            else
                return false;
        }

        public bool GetMovingAngleAfterRTurn(AGVTurnParameter turn, MapAddress start, MapAddress end, ref double newAGVMovingAngle, ref string errorMessage)
        {
            double rTurnSectionAngle = ComputeAngle(start.AGVPosition, end.AGVPosition);
            double agvGoAngle = start.AGVPosition.Angle;
            bool dirFlag = true;

            if (Math.Abs(GetCurrectAngle(agvGoAngle - rTurnSectionAngle)) >= 90)
            {
                dirFlag = false;
                agvGoAngle = GetCurrectAngle(agvGoAngle - 180);
            }

            double delta = GetCurrectAngle(rTurnSectionAngle - agvGoAngle);

            double endAngle = GetCurrectAngle(rTurnSectionAngle + delta);

            if (!dirFlag)
                endAngle = GetCurrectAngle(endAngle - 180);

            newAGVMovingAngle = GetCurrectAngle(end.AGVPosition.Angle + (dirFlag ? 0 : 180));

            if (Math.Abs(GetCurrectAngle(endAngle - end.AGVPosition.Angle)) > turn.SectionAngleChangeMax)
            {
                errorMessage = String.Concat("turnType : ", turn.TurnName,
                                             ", start (", GetMapAGVPositionString(start.AGVPosition), ") VehicleHeadAngle = ", start.AGVPosition.Angle.ToString("0"),
                                             ", end ", GetMapAGVPositionString(end.AGVPosition), " VehicleHeadAngle = ", end.AGVPosition.Angle.ToString("0"),
                                             " 判定角度變化與Config不符合");
                return false;
            }

            return true;
        }
        #endregion

        #region 距離相關.
        public double GetTwoPositionDistance(MapPosition start, MapPosition end)
        {
            if (start == null || end == null)
                return -1;
            else
                return Math.Sqrt(Math.Pow(start.X - end.X, 2) + Math.Pow(start.Y - end.Y, 2));
        }

        public double GetTwoPositionDistance(MapAGVPosition start, MapAGVPosition end)
        {
            if (start == null || end == null)
                return -1;
            else
                return GetTwoPositionDistance(start.Position, end.Position);
        }

        #endregion

        #region VChange.
        private void GetAccDecDistanceAndTime(double startVel, double endVel, double accOrDec, double jerk, ref double distance, ref double time)
        {
            if (startVel == endVel)
            {
                distance = 0;
                time = 0;
            }
            else
            {
                double accTime = accOrDec / jerk; // acc = 0 > acc的時間, acc = jerk * t.
                double deltaVelocity = accTime * accOrDec / 2 * 2; // deltaVelocity = 1/2 * t * acc, Acc從0到Acc 1次,再從Acc到0 第2次,所以要*2.

                if (deltaVelocity == Math.Abs(startVel - endVel))   // 速度變化剛好和Acc變化相等.
                {
                    distance = (startVel + endVel) * accTime; // t = time * 2, distance = (sV+eV)*t/2.
                    time = accTime * 2;
                }
                else if (deltaVelocity > Math.Abs(startVel - endVel))
                {
                    deltaVelocity = Math.Abs(startVel - endVel) / 2;
                    accTime = Math.Sqrt(deltaVelocity * 2 / jerk);
                    distance = (startVel + endVel) * accTime; // t = time * 2, distance = (sV+eV)*t/2.
                    time = accTime * 2;
                }
                else
                {
                    double lastDeltaVelocity = Math.Abs(startVel - endVel) - deltaVelocity;
                    double lastDeltaTime = lastDeltaVelocity / accOrDec;
                    distance = (startVel + endVel) * (accTime + lastDeltaTime / 2); // ( start + end ) * (2*time + lastDeltaTime) / 2.
                    time = 2 * accTime + lastDeltaTime;
                }
            }
        }

        public double GetAccDecDistance(double startVel, double endVel, double accOrDec, double jerk)
        {
            double time = 0;
            double distance = 0;
            GetAccDecDistanceAndTime(startVel, endVel, accOrDec, jerk, ref distance, ref time);

            return distance;
        }

        public double GetAccDecTime(double startVel, double endVel, double accOrDec, double jerk)
        {
            double time = 0;
            double distance = 0;
            GetAccDecDistanceAndTime(startVel, endVel, accOrDec, jerk, ref distance, ref time);

            return time;
        }

        public double GetDecDistanceOneJerk(double startVel, double decVelocity, double dec, double jerk, ref double vel)
        {
            double time = dec / jerk; // acc = 0 > acc的時間.
            double deltaVelocity = time * dec / 2;

            if (startVel - decVelocity >= deltaVelocity * 2)
            {
                vel = startVel - deltaVelocity;
                double jerkDistance = startVel * time - jerk * Math.Pow(time, 3) / 6;
                return jerkDistance;
            }
            else
            {
                vel = decVelocity;
                return GetAccDecDistance(startVel, decVelocity, dec, jerk);
            }
        }
        #endregion
    }
}
