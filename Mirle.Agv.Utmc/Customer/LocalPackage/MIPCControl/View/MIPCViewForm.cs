using Mirle.Agv.INX.Controller;
using Mirle.Agv.INX.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mirle.Agv.INX.View
{
    public partial class MIPCViewForm : Form
    {
        private MIPCControlHandler mipcControl;
        private LocalData localData = LocalData.Instance;

        private List<TabPage> tabPageList = new List<TabPage>();

        private List<MIPCClassificationForm> mipcClassificationFormList = new List<MIPCClassificationForm>();
        private MIPCMotionCommandForm commandPage = null;

        public MIPCViewForm(MIPCControlHandler mipcControl)
        {
            this.mipcControl = mipcControl;
            InitializeComponent();
            InitialPageForm();
        }

        #region Initial-PageForm.
        private void InitialPageForm()
        {
            try
            {
                TabPage tabPage;
                MIPCClassificationForm temp;

                foreach (string className in mipcControl.AllDataByClassification.Keys)
                {
                    tabPage = new TabPage();
                    tabPage.Text = className;

                    temp = new MIPCClassificationForm(mipcControl, mipcControl.AllDataByClassification[className]);
                    temp.Location = new Point(0, 10);
                    temp.Size = new Size(900, 700);
                    tabPage.Controls.Add(temp);
                    mipcClassificationFormList.Add(temp);

                    tabPageList.Add(tabPage);
                    tC_View.TabPages.Add(tabPage);
                }


                tabPage = new TabPage();
                tabPage.Text = "MotionCommand";
                commandPage = new MIPCMotionCommandForm(mipcControl);
                commandPage.Location = new Point(0, 0);
                commandPage.Size = new Size(800, 600);
                tabPage.Controls.Add(commandPage);

                tabPageList.Add(tabPage);
                tC_View.TabPages.Add(tabPage);

            }
            catch { }
        }
        #endregion

        #region ShowAndHide.
        public void ShowForm()
        {
            if (!timer.Enabled)
            {
                this.Show();
                timer.Enabled = true;
            }

            this.BringToFront();
        }

        private void button_Hide_Click(object sender, EventArgs e)
        {
            timer.Enabled = false;
            this.Hide();
        }
        #endregion

        private void timer_Tick(object sender, EventArgs e)
        {
            label_StatusValue.Text = mipcControl.Status.ToString();

            try
            {
                int index = tC_View.SelectedIndex;
               
                if (index >= 0)
                {
                    if (index < mipcClassificationFormList.Count)
                        mipcClassificationFormList[index].UpdateData();
                    else
                        commandPage.UpdateData();
                }
            }
            catch { }
        }
    }
}
