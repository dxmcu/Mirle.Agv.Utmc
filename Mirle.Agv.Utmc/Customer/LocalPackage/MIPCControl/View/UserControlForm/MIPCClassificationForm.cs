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
using System.Threading;
using Mirle.Agv.INX.Configs;

namespace Mirle.Agv.INX.View
{
    public partial class MIPCClassificationForm : UserControl
    {
        private List<MIPCData> mipcDataList;
        private List<MIPCInfo> mipcInfoList = new List<MIPCInfo>();
        private MIPCControlHandler mipcControl;

        public MIPCClassificationForm(MIPCControlHandler mipcControl, List<MIPCData> mipcDataList)
        {
            this.mipcControl = mipcControl;
            this.mipcDataList = mipcDataList;
            InitializeComponent();
            InitialUserControl();
        }

        private void InitialUserControl()
        {
            try
            {
                MIPCInfo mipcInfo;

                int y = 10;

                for (int i = 0; i < mipcDataList.Count; i++)
                {
                    mipcInfo = new MIPCInfo(mipcControl, mipcDataList[i]);
                    mipcInfo.Location = new Point(50, y);

                    this.panel1.Controls.Add(mipcInfo);
                    mipcInfoList.Add(mipcInfo);
                    y += 30;
                }

                panel1.AutoScroll = true;
            }
            catch { }
        }

        public void UpdateData()
        {
            for (int i = 0; i < mipcInfoList.Count; i++)
                mipcInfoList[i].UpdateData();
        }
    }
}
