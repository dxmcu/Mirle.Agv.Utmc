using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Utmc.Controller;
using TcpIpClientSample;
using Google.Protobuf.Collections;
using System.Reflection;

using Mirle.Tools;
using System.Drawing;

namespace Mirle.Agv.Utmc.Model
{

    public class AgvcTransferCommand
    {
        public EnumTransferStep TransferStep { get; set; } = EnumTransferStep.Idle;
        public string CommandId { get; set; } = "";
        public EnumAgvcTransCommandType AgvcTransCommandType { get; set; } = EnumAgvcTransCommandType.Else;
        public string LoadAddressId { get; set; } = "";
        public string UnloadAddressId { get; set; } = "";
        public string CassetteId { get; set; } = "";
        public ushort SeqNum { get; set; }
        public CompleteStatus CompleteStatus { get; set; }
        public EnumSlotNumber SlotNumber { get; set; } = EnumSlotNumber.L;
        public CommandState EnrouteState { get; set; } = CommandState.None;
        public string LotId { get; set; } = "";
        public string LoadPortId { get; set; } = "";
        public string UnloadPortId { get; set; } = "";
        public bool IsRobotEnd { get; set; } = false;
        public bool IsStopAndClear { get; set; } = false;
        public bool IsLoadArrivalReply { get; set; } = false;
        public bool IsLoadCompleteReply { get; set; } = false;
        public bool IsCstIdReadReply { get; set; } = false;
        public bool IsUnloadArrivalReply { get; set; } = false;
        public bool IsUnloadCompleteReply { get; set; } = false;
        public bool IsVitualPortUnloadArrivalReply { get; set; } = false;
        public List<string> ToLoadAddressIds { get; set; } = new List<string>();
        public List<string> ToLoadSectionIds { get; set; } = new List<string>();
        public List<string> ToUnloadAddressIds { get; set; } = new List<string>();
        public List<string> ToUnloadSectionIds { get; set; } = new List<string>();

        public AgvcTransferCommand()
        {
        }

        public AgvcTransferCommand(ID_31_TRANS_REQUEST transRequest, ushort aSeqNum)
        {
            CommandId = transRequest.CmdID.Trim();
            CassetteId = string.IsNullOrEmpty(transRequest.CSTID) ? "" : transRequest.CSTID.Trim();
            SeqNum = aSeqNum;

            InitialCommand(transRequest.ActType);

            LoadAddressId = string.IsNullOrEmpty(transRequest.LoadAdr) ? "" : transRequest.LoadAdr.Trim();
            UnloadAddressId = string.IsNullOrEmpty(transRequest.DestinationAdr) ? "" : transRequest.DestinationAdr.Trim();

            ToLoadAddressIds = transRequest.GuideAddressesStartToLoad == null ? new List<string>() : transRequest.GuideAddressesStartToLoad.ToList();
            ToLoadSectionIds = transRequest.GuideSectionsStartToLoad == null ? new List<string>() : transRequest.GuideSectionsStartToLoad.ToList();

            ToUnloadAddressIds = transRequest.GuideAddressesToDestination == null ? new List<string>() : transRequest.GuideAddressesToDestination.ToList();
            ToUnloadSectionIds = transRequest.GuideSectionsToDestination == null ? new List<string>() : transRequest.GuideSectionsToDestination.ToList();
        }

        private void InitialCommand(ActiveType activeType)
        {
            switch (activeType)
            {
                case ActiveType.Move:
                    AgvcTransCommandType = EnumAgvcTransCommandType.Move;
                    CompleteStatus = CompleteStatus.CmpStatusMove;
                    TransferStep = EnumTransferStep.MoveToAddress;
                    EnrouteState = CommandState.None;
                    break;
                case ActiveType.Load:
                    AgvcTransCommandType = EnumAgvcTransCommandType.Load;
                    CompleteStatus = CompleteStatus.CmpStatusLoad;
                    TransferStep = EnumTransferStep.MoveToLoad;
                    EnrouteState = CommandState.LoadEnroute;
                    break;
                case ActiveType.Unload:
                    AgvcTransCommandType = EnumAgvcTransCommandType.Unload;
                    CompleteStatus = CompleteStatus.CmpStatusUnload;
                    TransferStep = EnumTransferStep.MoveToUnload;
                    EnrouteState = CommandState.UnloadEnroute;
                    break;
                case ActiveType.Loadunload:
                    AgvcTransCommandType = EnumAgvcTransCommandType.LoadUnload;
                    CompleteStatus = CompleteStatus.CmpStatusLoadunload;
                    TransferStep = EnumTransferStep.MoveToLoad;
                    EnrouteState = CommandState.LoadEnroute;
                    break;
                case ActiveType.Movetocharger:
                    AgvcTransCommandType = EnumAgvcTransCommandType.MoveToCharger;
                    CompleteStatus = CompleteStatus.CmpStatusMoveToCharger;
                    TransferStep = EnumTransferStep.MoveToAddress;
                    EnrouteState = CommandState.None;
                    break;
                case ActiveType.Home:
                    break;
                case ActiveType.Override:
                    AgvcTransCommandType = EnumAgvcTransCommandType.Override;
                    //CompleteStatus = CompleteStatus.Loadunload;
                    break;
                default:
                    break;
            }
        }

        public ActiveType GetCommandActionType()
        {
            switch (AgvcTransCommandType)
            {
                case EnumAgvcTransCommandType.Move:
                    return ActiveType.Move;
                case EnumAgvcTransCommandType.Load:
                    return ActiveType.Load;
                case EnumAgvcTransCommandType.Unload:
                    return ActiveType.Unload;
                case EnumAgvcTransCommandType.LoadUnload:
                    return ActiveType.Loadunload;
                case EnumAgvcTransCommandType.Override:
                    return ActiveType.Override;
                case EnumAgvcTransCommandType.MoveToCharger:
                    return ActiveType.Movetocharger;
                case EnumAgvcTransCommandType.Else:
                default:
                    return ActiveType.Home;
            }
        }

        protected void LogException(string source, string exMsg)
        {
            MirleLogger.Instance.Log(new LogFormat("Error", "5", source, "Device", "CarrierID", exMsg));
        }

        public bool IsAbortByAgvc()
        {
            switch (CompleteStatus)
            {
                case CompleteStatus.CmpStatusInterlockError:
                case CompleteStatus.CmpStatusIdreadFailed:
                case CompleteStatus.CmpStatusIdmisMatch:
                case CompleteStatus.CmpStatusVehicleAbort:
                case CompleteStatus.CmpStatusAbort:
                case CompleteStatus.CmpStatusCancel:
                    return true;
                case CompleteStatus.CmpStatusMove:
                case CompleteStatus.CmpStatusLoad:
                case CompleteStatus.CmpStatusUnload:
                case CompleteStatus.CmpStatusLoadunload:
                case CompleteStatus.CmpStatusMoveToCharger:
                default:
                    return false;
            }
        }
    }
}
