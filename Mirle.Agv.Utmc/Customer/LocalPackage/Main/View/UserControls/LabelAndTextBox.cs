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
    public partial class LabelAndTextBox : UserControl
    {
        public LabelAndTextBox(string name, bool passwordMode = false)
        {
            InitializeComponent();

            label_Name.Text = String.Concat(name, " :");
            tB_Value.Text = "";

            if (passwordMode)
            {
                tB_Value.PasswordChar = '*';
            }
        }

        public string ValueString
        {
            get
            {
                return tB_Value.Text;
            }

            set
            {
                tB_Value.Text = value;
            }
        }
    }
}
