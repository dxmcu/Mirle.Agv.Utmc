using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcpIpClientSample;
using Mirle.Agv.Utmc.Tools;
using Newtonsoft.Json;

namespace Mirle.Agv.Utmc.Model
{
    public class MovingGuide
    {
        public List<string> GuideSectionIds { get; set; } = new List<string>();
        public List<string> GuideAddressIds { get; set; } = new List<string>();
        public string FromAddressId { get; set; } = "";
        public string ToAddressId { get; set; } = "";
        public uint GuideDistance { get; set; } = 0;
        public VhStopSingle ReserveStop { get; set; } = VhStopSingle.StopSingleOff;
        public List<MapSection> MovingSections { get; set; } = new List<MapSection>();
        public int MovingSectionsIndex { get; set; } = 0;
        public ushort SeqNum { get; set; }
        public string CommandId { get; set; } = "";
        public EnumMoveComplete MoveComplete { get; set; } = EnumMoveComplete.Fail;
        public bool IsAvoidMove { get; set; } = false;
        public bool IsAvoidComplete { get; set; } = false;

        public bool IsOverrideMove { get; set; } = false;

        public MovingGuide() { }

        //public MovingGuide(ID_38_GUIDE_INFO_RESPONSE response)
        //{
        //    var info = response.GuideInfoList[0];
        //    this.GuideSectionIds = info.GuideSections.ToList();
        //    this.GuideAddressIds = info.GuideAddresses.ToList();
        //    this.FromAddressId = info.FromTo.From;
        //    this.ToAddressId = info.FromTo.To;
        //    this.GuideDistance = info.Distance;
        //    this.CommandId = Vehicle.Instance.MovingGuide.CommandId;
        //}

        public MovingGuide(MovingGuide movingGuide)
        {
            this.GuideSectionIds = movingGuide.GuideSectionIds;
            this.GuideAddressIds = movingGuide.GuideAddressIds;
            this.FromAddressId = movingGuide.FromAddressId;
            this.ToAddressId = movingGuide.ToAddressId;
            this.GuideDistance = movingGuide.GuideDistance;
            this.ReserveStop = movingGuide.ReserveStop;
            this.IsAvoidComplete = movingGuide.IsAvoidComplete;
            this.MovingSections = movingGuide.MovingSections;
            this.MovingSectionsIndex = movingGuide.MovingSectionsIndex;
            this.CommandId = movingGuide.CommandId;
            this.SeqNum = movingGuide.SeqNum;
        }

        public MovingGuide(ID_51_AVOID_REQUEST request, ushort seqNum)
        {
            this.ToAddressId = string.IsNullOrEmpty(request.DestinationAdr.Trim()) ? "" : request.DestinationAdr.Trim();
            this.GuideSectionIds = request.GuideSections.Any() ? request.GuideSections.ToList() : new List<string>();
            this.GuideAddressIds = request.GuideAddresses.Any() ? request.GuideAddresses.ToList() : new List<string>();
            this.SeqNum = seqNum;
            this.CommandId = string.IsNullOrEmpty(Vehicle.Instance.MovingGuide.CommandId) ? "" : Vehicle.Instance.MovingGuide.CommandId;
            this.ReserveStop = VhStopSingle.StopSingleOn;
        }

        public string GetJsonInfo()
        {
            return $"[GuideSectionIds={GuideSectionIds.GetJsonInfo()}]\r\n" +
                   $"[GuideAddressIds={GuideAddressIds.GetJsonInfo()}]\r\n" +
                   $"[FromAddressId={FromAddressId}]\r\n" +
                   $"[ToAddressId={ToAddressId}]\r\n" +
                   $"[ReserveStop={ReserveStop}]\r\n" +
                   $"[MovingSections={MovingSections.Count}]\r\n" +
                   $"[SeqNum={SeqNum}]\r\n" +
                   $"[CommandId={CommandId}]\r\n" +
                   $"[MoveComplete ={MoveComplete}]\r\n" +
                   $"[IsAvoidComplete ={IsAvoidComplete}]\r\n" +
                   $"[IsAvoidMove ={IsAvoidMove}]\r\n" +
                   $"[IsOverrideMove ={IsOverrideMove}]\r\n";
        }
    }
}
