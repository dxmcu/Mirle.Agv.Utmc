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
    public partial class LabelNameAndValue : UserControl
    {
        public LabelNameAndValue(string name, bool hideBorderStyle = false, float labelSize = 14)
        {
            InitializeComponent();

            this.label_Name.Font = new System.Drawing.Font("新細明體", labelSize);

            label_Name.Text = String.Concat(name, " : ");

            if (label_Name.Size.Width > label_Value.Location.X)
            {
                this.Size = new Size(this.Size.Width + (label_Name.Size.Width - label_Value.Location.X), this.Size.Height);
                label_Value.Location = new Point(label_Name.Size.Width, label_Value.Location.Y);
            }

            if (hideBorderStyle)
            {
                label_Value.BorderStyle = BorderStyle.None;
                label_Value.AutoSize = true;
                label_Value.Location = new Point(label_Value.Location.X, label_Name.Location.Y);
            }
        }

        public void SetLabelValueBigerr(int extraLength)
        {
            label_Value.Size = new Size(label_Value.Size.Width + extraLength, label_Value.Size.Height);

            this.Size = new Size(this.Size.Width + extraLength, this.Size.Height);
        }

        public void SetValueAndColor(string value, int enumInt = 0)
        {
            label_Value.Text = value;

            if (label_Value.Location.X + label_Value.Size.Width > this.Size.Width)
                this.Size = new Size(label_Value.Location.X + label_Value.Size.Width, this.Size.Height);

            if (enumInt < 100)
                label_Value.ForeColor = Color.Black;
            else if (enumInt < 200)
                label_Value.ForeColor = Color.Green;
            else if (enumInt < 300)
                label_Value.ForeColor = Color.Yellow;
            else if (enumInt < 400)
                label_Value.ForeColor = Color.Red;
            else
                label_Value.ForeColor = Color.DarkRed;
        }

        public void SetReady(bool ready)
        {
            if (ready)
            {
                label_Value.Text = "Ready";
                label_Value.ForeColor = Color.Green;
            }
            else
            {
                label_Value.Text = "NotReady";
                label_Value.ForeColor = Color.Red;
            }
        }

        public void SetError(bool error)
        {
            if (error)
            {
                label_Value.Text = "Error";
                label_Value.ForeColor = Color.Red;
            }
            else
            {
                label_Value.Text = "Normal";
                label_Value.ForeColor = Color.Green;
            }
        }
    }
}
