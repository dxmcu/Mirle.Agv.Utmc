﻿namespace Mirle.Agv.Utmc
{
    #region MainEnums

    public enum CommandState
    {
        None,
        LoadEnroute,
        UnloadEnroute
    }

    public enum EnumSectionType
    {
        None,
        Horizontal,
        Vertical,
        R2000
    }
    public enum EnumMoveToEndReference
    {
        Load,
        Unload,
        Avoid
    }

    public enum EnumCommandDirection
    {
        None,
        Forward,
        Backward
    }

    public enum EnumTransferStepType
    {
        Move,
        MoveToCharger,
        Load,
        Unload,
        Empty
    }

    public enum EnumAgvcTransCommandType
    {
        Move,
        Load,
        Unload,
        LoadUnload,
        Override,
        MoveToCharger,
        Else
    }

    public enum EnumAutoState
    {
        Auto,
        Manual,
        None
    }

    public enum EnumCommandInfoStep
    {
        Begin,
        End
    }

    public enum EnumLoginLevel
    {
        Op,
        Engineer,
        Admin,
        OneAboveAll
    }

    public enum EnumCmdNum
    {
        Cmd000_EmptyCommand = 0,
        Cmd11_CouplerInfoReport = 11,
        Cmd31_TransferRequest = 31,
        Cmd32_TransferCompleteResponse = 32,
        Cmd35_CarrierIdRenameRequest = 35,
        Cmd36_TransferEventResponse = 36,
        Cmd37_TransferCancelRequest = 37,
        Cmd38_GuideInfoResponse = 38,
        Cmd39_PauseRequest = 39,
        Cmd41_ModeChange = 41,
        Cmd43_StatusRequest = 43,
        Cmd44_StatusRequest = 44,
        Cmd45_PowerOnoffRequest = 45,
        Cmd51_AvoidRequest = 51,
        Cmd52_AvoidCompleteResponse = 52,
        Cmd71_RangeTeachRequest = 71,
        Cmd72_RangeTeachCompleteResponse = 72,
        Cmd74_AddressTeachResponse = 74,
        Cmd91_AlarmResetRequest = 91,
        Cmd94_AlarmResponse = 94,
        Cmd111_CouplerInfoResponse = 111,
        Cmd131_TransferResponse = 131,
        Cmd132_TransferCompleteReport = 132,
        Cmd133_ControlZoneCancelResponse = 133,
        Cmd134_TransferEventReport = 134,
        Cmd135_CarrierIdRenameResponse = 135,
        Cmd136_TransferEventReport = 136,
        Cmd137_TransferCancelResponse = 137,
        Cmd139_PauseResponse = 139,
        Cmd141_ModeChangeResponse = 141,
        Cmd143_StatusResponse = 143,
        Cmd144_StatusReport = 144,
        Cmd145_PowerOnoffResponse = 145,
        Cmd151_AvoidResponse = 151,
        Cmd152_AvoidCompleteReport = 152,
        Cmd171_RangeTeachResponse = 171,
        Cmd172_RangeTeachCompleteReport = 172,
        Cmd174_AddressTeachReport = 174,
        Cmd191_AlarmResetResponse = 191,
        Cmd194_AlarmReport = 194,
    }

    public enum EnumAlarmLevel
    {
        Warn,
        Alarm
    }

    public enum EnumCstIdReadResult
    {
        Normal,
        Mismatch,
        Fail
    }

    public enum EnumBeamDirection
    {
        Front,
        Back,
        Left,
        Right
    }

    public enum EnumAseMoveCommandIsEnd
    {
        None,
        End,
        Begin
    }

    public enum EnumAddressDirection
    {
        None = 0,
        Left = 1,
        Right = 2
    }

    public enum EnumSlotSelect
    {
        None,
        Left,
        Right,
        Both
    }


    public enum PsMessageType
    {
        P,
        S
    }

    public enum EnumRobotState
    {
        Idle,
        Busy,
        Error
    }

    public enum EnumMoveState
    {
        Idle,
        Working,
        Pausing,
        Pause,
        Stoping,
        Block,
        Error,
        ReserveStop
    }

    public enum EnumCarrierSlotState
    {
        Empty,
        Loading,
        PositionError,
        ReadFail
    }

    public enum EnumMoveComplete
    {
        Success,
        Fail,
        Pause,
        Cancel
    }

    public enum EnumSlotNumber
    {
        L,
        R
    }

    public enum EnumLocalArrival
    {
        Fail,
        Arrival,
        EndArrival        
    }

    public enum EnumIsExecute
    {
        Keep,
        Go
    }

    public enum EnumLDUD
    {
        LD,
        UD,
        None
    }

    public enum EnumChargingStage
    {
        Idle,
        ArrivalCharge,
        WaitChargingOn,
        LowPowerCharge,
        DisCharge,
        WaitChargingOff
    }

    public enum EnumTransferStep
    {
        Idle,
        MoveToAddress,
        MoveToLoad,
        MoveToUnload,
        MoveToAvoid,
        MoveToAvoidWaitArrival,
        AvoidMoveComplete,
        MoveToAddressWaitArrival,
        MoveToAddressWaitEnd,
        WaitMoveArrivalVitualPortReply,
        LoadArrival,
        WaitLoadArrivalReply,
        Load,
        LoadWaitEnd,
        WaitLoadCompleteReply,
        WaitCstIdReadReply,
        UnloadArrival,
        WaitUnloadArrivalReply,
        Unload,
        UnloadWaitEnd,
        WaitUnloadCompleteReply,
        TransferComplete,      
        MoveFail,
        WaitOverrideToContinue,
        RobotFail,
        Abort
    }

    public enum EnumRobotEndType
    {
        Finished,
        InterlockError,
        RobotError
    }

    public enum EnumAgvcReplyCode
    {
        Accept,
        Reject,
        Unknow
    }

    #endregion      
}
