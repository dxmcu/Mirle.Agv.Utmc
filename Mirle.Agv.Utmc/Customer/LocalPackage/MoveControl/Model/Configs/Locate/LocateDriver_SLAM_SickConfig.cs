using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class LocateDriver_SLAM_SickConfig
    {
        public string ID { get; set; } = "SLAM_Sick";
        public string IP { get; set; } = "192.168.1.1";
        public int CommandPort { get; set; } = 2111;
        public int FeedbackPort { get; set; } = 2201;
        public int SleepTime { get; set; } = 5;
        public int PercentageStandard { get; set; } = 80;
        public string TransferAddressPath { get; set; } = @"D:\AgvConfigs\MoveControl\LocateControl\SLAM_SickAddress.csv";
        public bool LogMode { get; set; } = true;
        public bool UseOdometry { get; set; } = false;
        public double SetPositionRange { get; set; } = 1000;
        public int SetPositionForceUpdateCount { get; set; } = 20;
        public double SectionAngleRange { get; set; } = 10;

        public double SectionRange { get; set; } = 100;

        public bool ErrorNeedReset { get; set; } = false;

        public double SectionDistanceMagnification { get; set; } = 0.05;
        public double SectionDistanceConstant { get; set; } = 30;

        public bool UsingAverage { get; set; } = true;
        public int AverageTimeRange { get; set; } = 120;
    }
}
