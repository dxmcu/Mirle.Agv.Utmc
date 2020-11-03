using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Mirle.Agv.INX.Controller;
using System.IO.MemoryMappedFiles;
using System.IO;
using Mirle.Agv.INX.Controller.Tools;
using Mirle.Agv.INX.Model.Configs;

namespace Mirle.Agv.INX.View
{
    public partial class InitialForm : Form
    {
        private ManualResetEvent ShutdownEvent = new ManualResetEvent(false);
        private ManualResetEvent PauseEvent = new ManualResetEvent(true);

        private MainFlowHandler mainFlowHander;

        private MainForm mainForm;

        private Thread initailThread = null;

        public InitialForm()
        {
            InitializeComponent();

            mainFlowHander = new MainFlowHandler();

            mainFlowHander.OnComponentIntialDoneEvent += MainFlowHandler_OnComponentIntialDoneEvent;

            initailThread = new Thread(new ThreadStart(InitailMainFlowThread));
            initailThread.Start();
        }

        private void InitailMainFlowThread()
        {
            mainFlowHander.InitialMainFlowHander();
        }

        private void MainFlowHandler_OnComponentIntialDoneEvent(object sender, InitialEventArgs e)
        {
            ListBoxAppend(lst_StartUpMsg, String.Concat(DateTime.Now.ToString("[yyyy-MM-dd HH-mm-ss]"), e.Message));

            if (e.IsEnd)
            {
                if (e.Scuess)
                {
                    GoNextForm();
                }
                else
                {
                    Thread.Sleep(5000);
                    ThisClose();
                }
            }
        }

        private void GoNextForm()
        {
            if (InvokeRequired)
            {
                Action del = new Action(GoNextForm);
                Invoke(del);
            }
            else
            {
                this.Hide();
                mainForm = new MainForm(mainFlowHander);
                mainForm.Show();
            }
        }

        public delegate void ListBoxAppendCallback(ListBox listBox, string msg);
        private void ListBoxAppend(ListBox listBox, string msg)
        {
            if (listBox.InvokeRequired)
            {
                ListBoxAppendCallback mydel = new ListBoxAppendCallback(ListBoxAppend);
                this.Invoke(mydel, new object[] { listBox, msg });
            }
            else
            {
                listBox.Items.Add(msg);
            }
        }

        private void ThisClose()
        {
            ShutdownEvent.Set();
            PauseEvent.Set();

            Application.Exit();
            Environment.Exit(Environment.ExitCode);
        }

        private void cmd_Close_Click(object sender, EventArgs e)
        {
            ThisClose();
        }

        private void InitialForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ThisClose();
        }
    }
}
