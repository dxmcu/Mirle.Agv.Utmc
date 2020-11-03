namespace Mirle.Agv
{
    public enum EnumMoveCommandStartStatus
    {
        WaitStart,
        Start,
        Reporting,
        End
    }

    public enum EnumLineReviseType
    {
        None,
        Theta,
        SectionDeviation
    }

    public enum EnumCommandType
    {
        STurn,
        RTurn,
        SpinTurn,
        Vchange,
        ChangeSection,
        //ReviseOpen,
        //ReviseClose,
        Move,
        SlowStop,
        Stop,
        End
    }


    #region Motion相關.

    public enum EnumAxisStatus
    {
        Normal = 0,
        Error = 1
    }

    public enum EnumAxisMoveStatus
    {
        None = 4,         // 空狀態.
        PreMove = 2,      // 下命令了,但是撈取資料還不是運動中.
        Move = 1,         // 運動中.
        PreStop = 3,      // 下停止命令了,但是撈取資料還不是停止中.
        Stop = 0          // 停止中.
    }

    public enum EnumDefaultAxisName
    {
        XFL,
        XFR,
        XRL,
        XRR,
    }

    public enum EnumDefaultAxisNameChinese
    {
        左前輪 = EnumDefaultAxisName.XFL,
        右前輪 = EnumDefaultAxisName.XFR,
        左後輪 = EnumDefaultAxisName.XRL,
        右後輪 = EnumDefaultAxisName.XRR
    }
    #endregion

    #region LocateControl相關.
    public enum EnumMIPCSetPostionStep
    {
        Step0_WaitLocateReady,
        Step1_WaitMIPCDataOK,
        Ready
    }
    #endregion

    #region 資料格式相關.
    public enum EnumByteChangeType
    {
        LittleEndian,
        BigEndian
    }
    #endregion

    #region 模擬相關.
    public enum EnumSimulateVelocityType
    {
        AccJerkUp,      // 加速段 & Acc增加.
        Accing,         // 加速段 Acc不變.
        AccJerkDown,    // 加速段 & Acc減少.
        Isokinetic,     // 等速段.
        DecJerkUp,      // 減速段 & Acc增加.
        Decing,         // 減速段 Acc不變.
        DecJerkDown     // 減速段 & Acc減少.
    }
    #endregion

    public enum EnumMoveStatus
    {
        Moving = 100,
        Stop = 101,
        STurn = 102,
        RTurn = 103,
        SpinTurn = 104,
        Error = 201
    }

    public enum EnumStatus
    {
        Normal,
        Request,
        Already
    }

    public enum EnumMoveComplete
    {
        End,
        Error,
        Cancel
    }

    public enum EnumLoadUnloadComplete
    {
        End,
        Error,
    }


    public enum EnumRTurnParameter
    {
        InnerWheelTurn,
        OuterWheelTurn,
        InnerWheelMove,
        OuterWheelMove
    }

    public enum EnumMoveControlSafetyType
    {
        TurnOut,
        LineBarcodeInterval,
        OntimeReviseTheta,
        OntimeReviseSectionDeviationLine,
        OntimeReviseLowSpeedValue,
        //UpdateLocateMagnification,
        //UpdateLocateConstant,
        //UpdateLocateLongTimeCount,
        VChangeSafetyDistance,
        TRPathMonitoring,
        IdleNotWriteLog,
        MotionPollingTimer,
        AutoResetValue,
        AutoRetry
    }

    public enum EnumSensorSafetyType
    {
        Charging,
        ForkHome,
        BeamSensor,
        STurnStop,
        STurnStart,
        RTurnStop,
        RTurnStart,
        Bumper,
        PLC_Pause,
        ReviseForFork,
        ReviseForCharge,
        DrawMotionData
    }

    public enum EnumEndGetPlcDataStatus
    {
        Wait,
        NoData,
        NeedRevise,
        NeedRevise_OnlyX,
        Warning,
        OK
    }

    public enum EnumMoveStartType
    {
        FirstMove,
        ChangeDirFlagMove,
        ReserveStopMove,
        SensorStopMove
    }

    public enum EnumSlowStopType
    {
        ChangeMovingAngle,
        End
    }

    public enum EnumVChangeType
    {
        MoveStart,
        Normal,
        STurn,
        RTurn,
        SensorSlow,
        EQ,
        SlowStop,
        TurnOut
    }

    public enum EnumDefaultAction
    {
        ST,
        BST,
        End,
        SlowStop,
        SpinTurn,
        StopTurn
    }

    public enum EnumActionType
    {
        None,
        FrontOrTurn,
        BackOrBackTurn,
        End
    }


    public enum EnumTurnType
    {
        None,
        STurn,
        RTurn,
        SpinTurn,
        StopTurn
    }

    public enum EnumLocateDriverType
    {
        None,
        BarcodeMapSystem,
        SLAM_Sick,
        SLAM_BITO,
    }

    public enum EnumBarcodeReaderType
    {
        None,
        Keyence,
        Datalogic
    }

    public enum EnumLocateType
    {
        Normal,
        SLAM
    }

    public enum EnumAGVPositionType
    {
        None = 0,
        Normal = 3,
        OnlyRevise = 2,
        OnlyRead = 1
    }

    public enum EnumTimeoutValueType
    {
        EnableTimeoutValue,
        TurnWheelTimeoutValue,
        DisableTimeoutValue,
        SlowStopTimeoutValue,
        EndTimeoutValue,
        OverrideTimeoutValue,
        RTurnFlowTimeoutValue,
        SpinTurnFlowTimeoutValue,
        CloseProgameTimeoutValue,
        BeamSensorStopTimeout
    }

    public enum EnumIntervalTimeType
    {
        ThreadSleepTime,
        CSVLogInterval,
        ManualFindSectionInterval,
        SetPositionInterval
    }

    public enum EnumDelayTimeType
    {
        CommandStartDelayTime,
        OntimeReviseAlarmDelayTime,
        SafetySensorStartDelayTime,
        Local_PauseStartDelayTime,        // 未實作.
    }

    public enum EnumAxisServoOnOff
    {
        ServoOn = 1,
        ServoOff = 0
    }

    public enum EnumPositionUpdateSafteyType
    {
        None,
        Line,
        Turning,
        TurnOut
    }

    public enum EnumEMSResetFlow
    {
        None = 0,
        EMS_Stopping = 1,
        EMS_WaitReset = 2,
        EMS_WaitStart = 3,
        EMS_Delaying = 4,
    }

    public enum EnumSLAMBITOCommand
    {
        relocation_req = 0x0001, // 19001-重定位请求
        relocation_res = 0x8001, // 19001-重定位响应

        get_laserscan_req = 0x0006, // 19001-激光雷达点云数据请求(频率不低于 50hz)
        get_laserscan_res = 0x8006, // 19001-轮子里程计数据响应(接收异常时响应)

        get_position_req = 0x0002, // 19000-获得位姿请求
        get_position_res = 0x8002, // 19000-获得位姿响应(周期性传输)

        localization_state_req = 0x0003, // 19000-查询定位状态请求
        localization_state_res = 0x8003, // 19000-查询定位状态响应(周期性传输)

        imu_data_req = 0x0004, // 19002-imu 数据输入(频率不低于 200hz)
        imu_data_res = 0x8004, // 19002-imu 数据输入响应(接收异常时响应)

        odom_data_req = 0x0005, // 19002-imu 数据输入(频率不低于 200hz)
        odom_data_res = 0x8005, // 19002-imu 数据输入响应(接收异常时响应)
    }

    public enum EnumMoveCommandControlErrorCode
    {
        None = 0,

        //●100XXX : MoveControl層
        MoveControl主Thread跳Exception = 100000,
        通知PLC斷電 = 100001,
        超過觸發區間 = 100002,

        //    ●1001XX : CommandType : Move
        Move_啟動前旋轉角度不到位 = 100100,
        Move_EnableTimeout = 100101,

        //    ●1002XX : CommandType : Reserve
        Reserve_在Idle下取得Reserve = 100200,
        Reserve_取得不匹配的Reserve點 = 100201,

        //    ●1003XX : CommandType : SlowStop
        SlowStop_Timeout = 100300,

        //    ●1004XX : CommandType : TR
        STurn_入彎超速 = 100400,
        STurn_入彎過慢 = 100401,
        STurn_GT動作中 = 100402,
        STurn_不再路徑上 = 100403,
        STurn_舵輪旋轉過慢 = 100404,
        STurn_未開啟TR中停止 = 100405,
        STurn_未開啟STurn中重新啟動 = 100406,

        //    ●1005XX : CommandType : RTurn
        RTurn_入彎超速 = 100500,
        RTurn_入彎過慢 = 100501,
        RTurn_虛擬軸沒有Link = 100502,
        RTurn_轉向資料格式錯誤 = 100503,
        RTurn_未開啟RTurn中重新啟動 = 100504,
        RTurn_流程Timeout = 100505,
        RTurn_停止在無法再啟動步驟 = 100506,
        RTurn_未開啟RTurn中停止 = 100507,

        //    ●1006XX : CommandType : SpinTurn
        SpinTurn_Timeout = 100600,

        //    ●1007XX : CommandType : StopAndClear
        StopAndClear_Timeout = 100700,

        //    ●1008XX : CommandType : End
        End_SecondCorrectionTimeout = 100800,
        End_ServoOffeTimeout = 100801,

        //●1010XX : MoveMethod層
        MoveMethod層_初始化失敗 = 101000,
        MoveMethod層_DriverReturnFalse = 101001,

        //    ●1011XX : ElmoDriver
        MotionDriver_Elmo初始化失敗 = 101100,
        MotionDriver_Elmo連線失敗 = 101101,
        MotionDriver_Elmo軸設定資料格式錯誤 = 101102,
        MotionDriver_ElmoBulkException = 101103,
        MotionDriver_Elmo斷線IPC與控制卡 = 101104,
        MotionDriver_Elmo斷線控制卡與Driver = 101105,

        MotionDriver_軸異常 = 101110,
        MotionDriver_軸異常_XFL = 101111,
        MotionDriver_軸異常_XFR = 101112,
        MotionDriver_軸異常_XRL = 101113,
        MotionDriver_軸異常_XRR = 101114,
        MotionDriver_軸異常_TFL = 101115,
        MotionDriver_軸異常_TFR = 101116,
        MotionDriver_軸異常_TRL = 101117,
        MotionDriver_軸異常_TRR = 101118,
        MotionDriver_軸異常_VXFL = 101119,
        MotionDriver_軸異常_VXFR = 101120,
        MotionDriver_軸異常_VXRL = 101121,
        MotionDriver_軸異常_VXRR = 101122,
        MotionDriver_軸異常_VTFL = 101123,
        MotionDriver_軸異常_VTFR = 101124,
        MotionDriver_軸異常_VTRL = 101125,
        MotionDriver_軸異常_VTRR = 101126,
        MotionDriver_軸異常_GX = 101127,
        MotionDriver_軸異常_GT = 101128,

        //●102XXX : LocateControl層
        LocateControl初始化失敗 = 102000,

        //    ●1021XX : LocateDriver_BaroceMapSystem
        LocateDriver_BarcodeMapSystem初始化失敗 = 102100,
        LocateDriver_BarcodeMapSystem連線失敗 = 102101,
        LocateDriver_BarcodeMapSystem回傳資料格式錯誤 = 102102,
        LocateDriver_BarcodeMapSystemTriggerException = 102103,
        LocateDriver_BarcodeMapSystemError = 102104,

        //    ●1022XX : LocateDriver_SLAM_Sick
        LocateDriver_SLAM_Sick初始化失敗 = 102200,
        LocateDriver_SLAM_Sick連線失敗 = 102201,
        LocateDriver_SLAM_Sick資料格式錯誤 = 102202,
        LocateDriver_SLAM_Sick經度迷航 = 102203,

        //    ●1023XX : LocateDriver_SLAM_BITO
        LocateDriver_SLAM_BITO初始化失敗 = 102300,
        LocateDriver_SLAM_BITO連線失敗 = 102301,
        LocateDriver_SLAM_BITO資料格式錯誤 = 102302,
        LocateDriver_SLAM_BITO經度迷航 = 102303,

        //   

        //●103XXX : CreateCommandList層

        //    ●1030XX TransferMove
        命令分解失敗 = 103000,

        //    ●1031XX TransferMove_Override
        拒絕Override_當下不能Pause = 103100,
        拒絕Override_Timeout = 103101,

        //    ●1032XX TransferMove_RetryMove


        //●104XXX : 安全保護層

        //    ●1040XX : 切換Auto保護
        不能Auto_MoveControl不在Idle狀態 = 104000,
        不能Auto_MoveCommandControlIntailFail = 104001,
        不能Auto_MotionDriverDisconnect = 104002,
        不能Auto_程式關閉中 = 104003,
        不能Auto_有軸異常 = 104004,
        不能Auto_ResetAlarm中 = 104005,
        不能Auto_JogPitch控制中 = 104006,
        不能Auto_LocateControlIntailFail = 104007,
        不能Auto_LocateDriverDisconnect = 104008,
        不能Auto_LocateDriverError = 104009,
        不能Auto_AutoActionFail = 104010,

        //    ●1041XX : 接受命令前的保護
        拒絕移動命令_資料格式錯誤 = 104100,
        拒絕移動命令_MoveControlNotReady = 104101,
        拒絕移動命令_MoveControlErrorBitOn = 104102,
        拒絕移動命令_充電中 = 104103,
        拒絕移動命令_Fork不在Home點 = 104104,
        拒絕移動命令_MoveControl有Driver狀態不是Ready = 104105,
        拒絕移動命令_迷航中 = 104106,
        拒絕移動命令_和起點座標偏差過大 = 104107,
        拒絕移動命令_和起點車姿角度偏差過大 = 104108,
        拒絕移動命令_Cancel流程中 = 104109,
        拒絕移動命令_AutoRetry流程中 = 104110,

        //    ●1042XX : 移動中的保護
        安全保護停止_Fork不在Home點 = 104200,
        安全保護停止_充電中 = 104201,
        安全保護停止_角度偏差過大 = 104202,
        安全保護停止_軌道偏差過大 = 104203,
        安全保護停止_出彎過久沒讀到Barcode = 104204,
        安全保護停止_直線過久沒讀到Barcode = 104205,
        安全保護停止_AGV默停 = 104206,
        安全保護停止_速度變化異常 = 104207,
        安全保護停止_軸異常 = 104208,
        安全保護停止_定位Control異常 = 104209,
        安全保護停止_人為控制 = 104210,
        安全保護停止_Bumper觸發 = 104211,
        安全保護停止_EMO停止 = 104212,
        安全保護停止_更新數值變化過大 = 104213,
        安全保護停止_SafetySensorAlarm = 104214,
        安全保護停止_MotionAlarm = 104215,

        //    ●1043XX : 無法Reset的Alarm
        無法ResetAlarm_主流成Exception = 104300,
        無法ResetAlarm_停在轉彎流程中 = 104301,
        無法ResetAlarm_不再Section上 = 104302,
        無法ResetAlarm_定位Control異常 = 104303,
        無法ResetAlarm_MotionControl異常 = 104304,

        //    ●1044XX : 無法AutoReset的Alarm
        無法AutoReset_為特殊異常停止 = 104400,

        //    ●1045XX : AutoRety失敗的Alarm
        無法AutoRetry_失敗次數超過設定值 = 104500,
        無法AutoRetry_AutoReset失敗 = 104501,
        無法AutoRetry_MoveCmdInfo產生失敗 = 104502,

        //    ●1046XX : Timeout保護
        BeamSensorOn_Timeout = 104600,
    }

    public enum AlarmLevel
    {
        Alarm,
        Warn
    }
}
