using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mirle.Agv.INX.View
{
    public partial class LabelList : UserControl
    {
        public LabelList()
        {
            InitializeComponent();
        }

        public void SetLabelByStringList(List<string> stringList)
        {
            for (int i = 0; i < stringList.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        label_Name.Text = stringList[i];
                        break;
                    case 1:
                        label1.Text = stringList[i];
                        break;
                    case 2:
                        label2.Text = stringList[i];
                        break;
                    case 3:
                        label3.Text = stringList[i];
                        break;
                    case 4:
                        label4.Text = stringList[i];
                        break;
                    case 5:
                        label5.Text = stringList[i];
                        break;
                    case 6:
                        label6.Text = stringList[i];
                        break;
                    case 7:
                        label7.Text = stringList[i];
                        break;
                    case 8:
                        label8.Text = stringList[i];
                        break;
                    case 9:
                        label9.Text = stringList[i];
                        break;
                    default:
                        break;
                }
            }
        }

        public void SetLabelBackColorByUint(uint status)
        {
            for (int i = 0; i < 9; i++)
            {
                switch (i)
                {
                    case 0:
                        label9.BackColor = (((status >> i) & 1) == 1) ? Color.Green : Color.White;
                        break;
                    case 1:
                        label8.BackColor = (((status >> i) & 1) == 1) ? Color.Yellow : Color.White;
                        break;
                    case 2:
                        label7.BackColor = (((status >> i) & 1) == 1) ? Color.Yellow : Color.White;
                        break;
                    case 3:
                        label6.BackColor = (((status >> i) & 1) == 1) ? Color.Red : Color.White;
                        break;
                    case 4:
                        label5.BackColor = (((status >> i) & 1) == 1) ? Color.Red : Color.White;
                        break;
                    case 5:
                        label4.BackColor = (((status >> i) & 1) == 1) ? Color.DarkRed : Color.White;
                        break;
                    case 6:
                        label3.BackColor = (((status >> i) & 1) == 1) ? Color.DarkRed : Color.White;
                        break;
                    case 7:
                        label2.BackColor = (((status >> i) & 1) == 1) ? Color.Red : Color.White;
                        break;
                    case 8:
                        label1.BackColor = (((status >> i) & 1) == 1) ? Color.DarkRed : Color.White;
                        break;
                    default:
                        break;
                }

            }
        }
    }
}
