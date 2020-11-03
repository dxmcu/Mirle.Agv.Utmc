namespace Mirle.Agv
{
    public enum EnumLoadUnload
    {
        Load,
        Unload,
        PreCheck,
    }

    public enum EnumPIOType
    {
        None,
    }

    public enum EnumPIOStatus
    {
        Idle,
        T1,
        T2,
        T3,
        T4,
        T5,
        T6,
        T7,
        T8,
        T9,
        T10,
        Complete,
        NG,
    }

    public enum EnumPIOTimeout
    {
        None,
        T1_Timeout,
        T2_Timeout,
        T3_Timeout,
        T4_Timeout,
        T5_Timeout,
        T6_Timeout,
        T7_Timeout,
        T8_Timeout,
        T9_Timeout,
        T10_Timeout,
    }
}
