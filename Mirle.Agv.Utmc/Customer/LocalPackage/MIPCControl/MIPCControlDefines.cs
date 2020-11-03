namespace Mirle.Agv
{
    public enum EnumMovingDirection
    {
        None,
        Front,
        Back,
        Left,
        Right,
        FrontLeft,
        FrontRight,
        BackLeft,
        BackRight,
        SpinTurn,
        LoadUnload,
    }

    public enum EnumBuzzerType
    {
        None,
        Moving,
        MoveShift,
        Turning,
        SpinTurn,

        LoadUnload,

        Charging,

        Warn,
        Alarm,
    }

    public enum EnumDirectionLight
    {
        // Move
        None,
        Front,
        Back,
        Left,
        Right,
        SpinTurn,
        FrontLeft,
        FrontRight,
        BackLeft,
        BackRight,
        RTurnLeft,
        RTurnRight,

        // Fork
        LoadUnload,

        // Other
        Charging,
    }

    public enum EnumSafetySensorType
    {
        None,
        BeamSensor,
        AreaSensor,
        Bumper,
        EMO,
    }

    public enum EnumDeviceType
    {
        None,
        Tim781,
        Bumper,
        EMO,
    }

    public enum EnumMIPCConnectType
    {
        None,
        TCP_Server,
        TCP_Client,
    }

    public enum EnumDataType
    {
        UInt32,
        Int32,
        Double_1,
        Float,
        Boolean
    }

    public enum EnumIOType
    {
        Write,
        Read
    }

    public enum EnumSendAndRecieve
    {
        None,
        OK,
        Error
    }

    public enum EnumMecanumIPCdefaultTag
    {
        Command_MapX,
        Command_MapY,
        Command_MapTheta,
        Command_線速度,
        Command_線加速度,
        Command_線減速度,
        Command_線急跳度,
        Command_角速度,
        Command_角加速度,
        Command_角減速度,
        Command_角急跳度,
        Command_Start,

        Command_Stop,

        SetPosition_MapX,
        SetPosition_MapY,
        SetPosition_MapTheta,
        SetPosition_TimeStmap,
        SetPosition_Start,

        Turn_MapX,
        Turn_MapY,
        Turn_MapTheta,
        Turn_R,
        Turn_Theta,
        Turn_Velocity,
        Turn_MovingAngle,
        Turn_DeltaTheta,
        Turn_Start,

        Feedback_X,
        Feedback_Y,
        Feedback_Theta,
        Feedback_線速度,
        Feedback_線速度方向,
        Feedback_線加速度,
        Feedback_線減速度,
        Feedback_線急跳度,
        Feedback_角速度,
        Feedback_角加速度,
        Feedback_角減速度,
        Feedback_角急跳度,
        Feedback_MoveStatus,
        Feedback_TimeStamp,

        XFL_Encoder,
        XFL_RPM,
        XFL_DA,
        XFL_QA,
        XFL_V,
        XFL_ServoStatus,
        XFL_EC,
        XFL_MF,
        XFL_GetwayError,

        XFR_Encoder,
        XFR_RPM,
        XFR_DA,
        XFR_QA,
        XFR_V,
        XFR_ServoStatus,
        XFR_EC,
        XFR_MF,
        XFR_GetwayError,

        XRL_Encoder,
        XRL_RPM,
        XRL_DA,
        XRL_QA,
        XRL_V,
        XRL_ServoStatus,
        XRL_EC,
        XRL_MF,
        XRL_GetwayError,

        XRR_Encoder,
        XRR_RPM,
        XRR_DA,
        XRR_QA,
        XRR_V,
        XRR_ServoStatus,
        XRR_EC,
        XRR_MF,
        XRR_GetwayError,

        ResetAlarm,

        EMS,

        Battery_SOC,
        Battery_V,

        HeartBeat_ByPass,
        Heartbeat_IPC,
        Heartbeat_System,
        Heartbeat_Motion,
        ServoOnOff,


        MIPCAlarmCode_1,
        MIPCAlarmCode_2,
        MIPCAlarmCode_3,
        MIPCAlarmCode_4,
        MIPCAlarmCode_5,
        MIPCAlarmCode_6,
        MIPCAlarmCode_7,
        MIPCAlarmCode_8,
        MIPCAlarmCode_9,
        MIPCAlarmCode_10,
        MIPCAlarmCode_11,
        MIPCAlarmCode_12,
        MIPCAlarmCode_13,
        MIPCAlarmCode_14,
        MIPCAlarmCode_15,
        MIPCAlarmCode_16,
        MIPCAlarmCode_17,
        MIPCAlarmCode_18,
        MIPCAlarmCode_19,
        MIPCAlarmCode_20,

        RS485_Error,
        Battery_Alarm,
        Meter_A,
        Meter_V,
        Meter_W,
        Meter_WH,

        MIPC_Test1,
        MIPC_Test2,
        MIPC_Test3,
        MIPC_Test4,
        MIPC_Test5,
        MIPC_Test6,
        MIPC_Test7,
        MIPC_Test8,
        MIPC_Test9,
        MIPC_Test10,

        JoystickOnOff,
        Joystick_LineVelocity,
        Joystick_LineAcc,
        Joystick_LineDec,
        Joystick_ThetaVelocity,
        Joystick_ThetaAcc,
        Joystick_ThetaDec,

        MIPCReady,
        SafetyRelay,

        Light_Red,
        Light_Yellow,
        Light_Green,

        Reset_Front,
        Reset_Back,
        Start_Front,
        Start_Back,

        AGV_Type,
    }

    public enum DefaultAxisTag
    {
        Encoder,
        RPM,
        DA,
        QA,
        V,
        ServoStatus,
        EC,
        MF,
        GetwayError,
    }

    public enum EnumMIPCControlErrorCode
    {
        MIPC初始化失敗 = 200000,
        MIPC連線失敗 = 200001,
        MIPC斷線 = 200002,
        MIPC通訊異常 = 200003,
        MIPC回傳資料異常 = 200004,

        MIPC_DeviceHeartBeatLoss = 200100,
        MIPC_IPCHeartBeatLoss = 200101,
        MIPC_Motin_EMO = 200102,
        MIPC_誤差過大 = 200103,
        MIPC_SLAM過久沒更新 = 200104,
        MIPC_Alarm6 = 200105,
        MIPC_Alarm7 = 200106,
        MIPC_Alarm8 = 200107,
        MIPC_Alarm9 = 200108,
        MIPC_Alarm10 = 200109,

        LowBattery = 200200,
        SafetyRelayNotOK = 200201,


        SensorSafety_AlarmByPass = 201000,
        SensorSafety_IPCEMOByPass = 201001,
        SensorSafety_EMSByPass = 201002,
        SensorSafety_SlowStopByPass = 201003,
        SensorSafety_停止訊號Timeout = 201004,

        EMO觸發 = 201100,

        Bumper觸發 = 201200,

        AreaSensorAlarm = 201300,
        AreaSensor觸發 = 201301,


        //異常相關 = 


    }

    public enum EnumMIPCSocketName
    {
        Normal,
        Polling,
        MotionCommand
    }

    public enum EnumVehicleSafetyAction
    {
        Normal = 0,
        LowSpeed_High = 1,
        LowSpeed_Low = 2,
        SlowStop = 3,
        EMS = 4,
    }

    public enum EnumSafetyLevel
    {
        Alarm = 8,
        Warn = 7,
        EMO = 6,
        IPCEMO = 5,
        EMS = 4,
        SlowStop = 3,
        LowSpeed_Low = 2,
        LowSpeed_High = 1,
        Normal = 0,
    }


    public enum EnumVehicleStopLevel
    {
        None,
        Normal,
        EMS,
        EMO
    }
}
