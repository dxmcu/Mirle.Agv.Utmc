using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.Agv.INX.Model;
using Mirle.Agv.INX.Controller;
using System.Threading;

namespace Mirle.Agv
{
    public partial class JogPitchLocateData : UserControl
    {
        LocateDriver driver;
        private int index = -1;
        private LocalData localData = LocalData.Instance;

        private bool triggerOnOff = true;

        public JogPitchLocateData(LocateDriver driver)
        {
            this.driver = driver;
            InitializeComponent();
            label_DriverNameValue.Text = driver.DriverConfig.Device;

            if (((LocateDriver)driver).LocateType == EnumLocateType.SLAM)
                label_SpecialTurn.Text = "信心度 : ";
            else
            {
                switch (driver.DriverConfig.LocateDriverType)
                {
                    case EnumLocateDriverType.BarcodeMapSystem:
                        label_SpecialTurn.Text = "Barcode角度 : ";
                        break;
                    default:
                        label_SpecialTurn.Text = "";
                        break;
                }
            }
        }

        public void UpdateData()
        {
            if (driver == null)
                return;

            if (triggerOnOff != driver.PoolingOnOff)
            {
                triggerOnOff = driver.PoolingOnOff;

                if (triggerOnOff)
                {
                    label_TriggerValue.Text = "On";
                    label_TriggerValue.ForeColor = Color.Green;
                    button_TriggerOnOff.Text = "關閉";
                    driver.PoolingOnOff = true;
                }
                else
                {
                    label_TriggerValue.Text = "Off";
                    label_TriggerValue.ForeColor = Color.Red;
                    button_TriggerOnOff.Text = "開啟";
                    driver.PoolingOnOff = false;
                }
            }

            EnumControlStatus status = driver.Status;
            label_DriverStatusValue.Text = status.ToString();

            switch (status)
            {
                case EnumControlStatus.Initial:
                case EnumControlStatus.Error:
                case EnumControlStatus.NotReady:
                    label_DriverStatusValue.ForeColor = Color.Red;
                    break;
                case EnumControlStatus.ResetAlarm:
                    label_DriverStatusValue.ForeColor = Color.Yellow;
                    break;
                case EnumControlStatus.Ready:
                    label_DriverStatusValue.ForeColor = Color.Green;
                    break;
                default:
                    label_DriverStatusValue.ForeColor = Color.DarkRed;
                    break;
            }

            button_TriggerOnOff.Enabled = (localData.LoginLevel >= EnumLoginLevel.Engineer);

            LocateAGVPosition locateAGVPosition = null;

            if (index == -1)
                locateAGVPosition = driver.GetLocateAGVPosition;
            else
            {
                if (((LocateDriver)driver).LocateType == EnumLocateType.SLAM)
                    locateAGVPosition = ((LocateDriver_SLAM)driver).GetOriginAGVPosition;
                else
                {
                    switch (driver.DriverConfig.LocateDriverType)
                    {
                        case EnumLocateDriverType.BarcodeMapSystem:
                            locateAGVPosition = ((LocateDriver_BarcodeMapSystem)driver).GetLocateAGVPositionByIndex(index);
                            break;
                        default:
                            locateAGVPosition = driver.GetLocateAGVPosition;
                            break;
                    }
                }
            }

            if (locateAGVPosition != null)
            {
                if (((LocateDriver)driver).LocateType == EnumLocateType.SLAM)
                    label_SpecialTurnValue.ForeColor = Color.Green;

                label_SpecialTurnValue.Text = locateAGVPosition.Value.ToString("0");
                label_DataTypeValue.Text = locateAGVPosition.Type.ToString();
                label_MapXValue.Text = locateAGVPosition.AGVPosition.Position.X.ToString("0.0");
                label_MapYValue.Text = locateAGVPosition.AGVPosition.Position.Y.ToString("0.0");
                label_MapThetaValue.Text = locateAGVPosition.AGVPosition.Angle.ToString("0.0");
            }
            else
            {
                if (((LocateDriver)driver).LocateType == EnumLocateType.SLAM)
                {
                    label_SpecialTurnValue.ForeColor = Color.Red;
                    locateAGVPosition = ((LocateDriver_SLAM)driver).GetOriginAGVPosition;
                }

                if (locateAGVPosition != null)
                    label_SpecialTurnValue.Text = locateAGVPosition.Value.ToString("0");
                else
                    label_SpecialTurnValue.Text = "";

                label_DataTypeValue.Text = "";
                label_MapXValue.Text = "";
                label_MapYValue.Text = "";
                label_MapThetaValue.Text = "";
            }
        }

        private void button_TriggerOnOff_Click(object sender, EventArgs e)
        {
            button_TriggerOnOff.Enabled = false;

            if (driver.PoolingOnOff)
            {
                label_TriggerValue.Text = "Off";
                label_TriggerValue.ForeColor = Color.Red;
                button_TriggerOnOff.Text = "開啟";
                driver.PoolingOnOff = false;
            }
            else
            {
                label_TriggerValue.Text = "On";
                label_TriggerValue.ForeColor = Color.Green;
                button_TriggerOnOff.Text = "關閉";
                driver.PoolingOnOff = true;
            }

            triggerOnOff = driver.PoolingOnOff;

            button_TriggerOnOff.Enabled = true;
        }

        private void button_ChangeType_Click(object sender, EventArgs e)
        {
            button_ChangeType.Enabled = false;

            if (driver.LocateType == EnumLocateType.SLAM)
            {
                if (button_ChangeType.Text == "真")
                    index = 0;
                else
                    index = -1;

                button_ChangeType.Text = (index==-1) ? "真" : "原";
                button_ChangeType.BackColor = (index == -1) ? Color.Transparent : Color.Red;
            }
            else if (driver.DriverConfig.LocateDriverType == EnumLocateDriverType.BarcodeMapSystem)
            {
                index++;

                if (index == ((LocateDriver_BarcodeMapSystem)driver).BarcodeReaderList.Count)
                    index = -1;

                button_ChangeType.Text = (index == -1) ? "真" : index.ToString();
                button_ChangeType.BackColor = (index == -1) ? Color.Transparent : Color.Red;
            }

            button_ChangeType.Enabled = true;
        }
    }
}