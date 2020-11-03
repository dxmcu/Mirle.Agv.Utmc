using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.Agv.INX.Controller;
using Mirle.Agv.INX.Model;
using Mirle.Agv.INX.Configs;

namespace Mirle.Agv.INX.View
{
    public partial class MIPCInfo : UserControl
    {
        private MIPCData mipcData;
        private MIPCControlHandler mipcControl;

        public MIPCInfo(MIPCControlHandler mipcControl, MIPCData mipcData)
        {
            this.mipcControl = mipcControl;
            this.mipcData = mipcData;
            InitializeComponent();
            InitialData();
        }

        private void InitialData()
        {
            if (mipcData != null)
            {
                label_MIPCName.Text = mipcData.DataName;

                if (mipcData.DataName.Length > 10)
                    this.label_MIPCName.Font = new System.Drawing.Font("新細明體", 12F);
                else if (mipcData.DataName.Length > 16)
                    this.label_MIPCName.Font = new System.Drawing.Font("新細明體", 10F);
                else if (mipcData.DataName.Length > 20)
                    this.label_MIPCName.Font = new System.Drawing.Font("新細明體", 8F);

                label_IPCName.Text = mipcData.IPCName;
                if (mipcData.IPCName.Length > 10)
                    this.label_IPCName.Font = new System.Drawing.Font("新細明體", 12F);
                else if (mipcData.IPCName.Length > 16)
                    this.label_IPCName.Font = new System.Drawing.Font("新細明體", 10F);
                else if (mipcData.IPCName.Length > 20)
                    this.label_IPCName.Font = new System.Drawing.Font("新細明體", 8F);

                if (mipcData.IoStatus == EnumIOType.Read)
                {
                    label_WriteRead.Text = "R";
                    tB_Write.Enabled = false;
                    button_Send.Enabled = false;
                    tB_Write.Visible = false;
                    button_Send.Visible = false;
                }
                else
                    label_WriteRead.Text = "W";

                label_Value.Text = mipcData.Value;
            }
        }

        public void UpdateData()
        {
            if (mipcData != null)
                label_Value.Text = mipcData.Value;
        }

        private void button_Send_Click(object sender, EventArgs e)
        {
            try
            {
                button_Send.Enabled = false;
                SendAndReceive sendHeartbeat = new SendAndReceive();

                List<Byte[]> data = new List<byte[]>();

                switch (mipcData.DataType)
                {
                    case EnumDataType.Boolean:
                        int boolNum = int.Parse(tB_Write.Text);

                        if (boolNum == 0)
                            data.Add(new Byte[4] { 0, 0, 0, 0 });
                        else if (boolNum == 1)
                            data.Add(new Byte[4] { 1, 0, 0, 0 });
                        else
                        {
                            button_Send.Enabled = true;
                            return;
                        }

                        break;

                    case EnumDataType.Double_1:
                        //Int32 double_1Num = (Int32)(double.Parse(tB_Write.Text) * 10);
                        //sendHeartbeat.Send = mipcControl.Write_連續(mipcData.Address, 1, new Int32[1] { double_1Num });
                        button_Send.Enabled = true;
                        return;
                        break;

                    case EnumDataType.Float:
                        data.Add(BitConverter.GetBytes(float.Parse(tB_Write.Text)));
                        break;

                    case EnumDataType.Int32:
                        data.Add(BitConverter.GetBytes(Int32.Parse(tB_Write.Text)));
                        break;

                    case EnumDataType.UInt32:
                        data.Add(BitConverter.GetBytes(UInt32.Parse(tB_Write.Text)));
                        break;

                    default:
                        button_Send.Enabled = true;
                        return;
                }

                sendHeartbeat.Send = mipcControl.Write_連續(mipcData.Address, 1, data);
                mipcControl.AddWriteQueue(EnumMIPCSocketName.Normal.ToString(), sendHeartbeat);
                button_Send.Enabled = true;
            }
            catch
            {
                button_Send.Enabled = true;
            }
        }
    }
}
