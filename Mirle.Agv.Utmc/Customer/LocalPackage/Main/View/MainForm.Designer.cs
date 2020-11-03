namespace Mirle.Agv.INX.View
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuStrip_MainForm = new System.Windows.Forms.MenuStrip();
            this.系統ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.關閉ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.模式ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MIPC = new System.Windows.Forms.ToolStripMenuItem();
            this.MoveControl = new System.Windows.Forms.ToolStripMenuItem();
            this.人機畫面ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel_Map = new System.Windows.Forms.Panel();
            this.pB_Map = new System.Windows.Forms.PictureBox();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.MovingAddresList = new System.Windows.Forms.ListBox();
            this.button_SendToMoveControlDebug = new System.Windows.Forms.Button();
            this.button_ClearAddressList = new System.Windows.Forms.Button();
            this.button_Login = new System.Windows.Forms.Button();
            this.button_LogOut = new System.Windows.Forms.Button();
            this.tC_Alarm = new System.Windows.Forms.TabControl();
            this.pP_Alarm = new System.Windows.Forms.TabPage();
            this.tbxNowAlarm = new System.Windows.Forms.TextBox();
            this.tP_AlarmHistory = new System.Windows.Forms.TabPage();
            this.tbxNowAlarmHistory = new System.Windows.Forms.TextBox();
            this.button_Alarm = new System.Windows.Forms.Button();
            this.button_BuzzOff = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbxDebugLogMsg = new System.Windows.Forms.TextBox();
            this.button_AutoManual = new System.Windows.Forms.Button();
            this.label_Alarm = new System.Windows.Forms.Label();
            this.label_Warn = new System.Windows.Forms.Label();
            this.pB_Warn = new System.Windows.Forms.PictureBox();
            this.pB_Alarm = new System.Windows.Forms.PictureBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button_SetPosition = new System.Windows.Forms.Button();
            this.label_LocateStatus = new System.Windows.Forms.Label();
            this.menuStrip_MainForm.SuspendLayout();
            this.panel_Map.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pB_Map)).BeginInit();
            this.tC_Alarm.SuspendLayout();
            this.pP_Alarm.SuspendLayout();
            this.tP_AlarmHistory.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pB_Warn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pB_Alarm)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip_MainForm
            // 
            this.menuStrip_MainForm.Font = new System.Drawing.Font("Microsoft JhengHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.menuStrip_MainForm.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip_MainForm.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.系統ToolStripMenuItem,
            this.模式ToolStripMenuItem});
            this.menuStrip_MainForm.Location = new System.Drawing.Point(0, 0);
            this.menuStrip_MainForm.Name = "menuStrip_MainForm";
            this.menuStrip_MainForm.Size = new System.Drawing.Size(1674, 28);
            this.menuStrip_MainForm.TabIndex = 1;
            this.menuStrip_MainForm.Text = "menuStrip1";
            // 
            // 系統ToolStripMenuItem
            // 
            this.系統ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.關閉ToolStripMenuItem});
            this.系統ToolStripMenuItem.Name = "系統ToolStripMenuItem";
            this.系統ToolStripMenuItem.Size = new System.Drawing.Size(53, 24);
            this.系統ToolStripMenuItem.Text = "系統";
            // 
            // 關閉ToolStripMenuItem
            // 
            this.關閉ToolStripMenuItem.Name = "關閉ToolStripMenuItem";
            this.關閉ToolStripMenuItem.Size = new System.Drawing.Size(110, 24);
            this.關閉ToolStripMenuItem.Text = "關閉";
            this.關閉ToolStripMenuItem.Click += new System.EventHandler(this.關閉ToolStripMenuItem_Click);
            // 
            // 模式ToolStripMenuItem
            // 
            this.模式ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MIPC,
            this.MoveControl,
            this.人機畫面ToolStripMenuItem});
            this.模式ToolStripMenuItem.Name = "模式ToolStripMenuItem";
            this.模式ToolStripMenuItem.Size = new System.Drawing.Size(53, 24);
            this.模式ToolStripMenuItem.Text = "模式";
            // 
            // MIPC
            // 
            this.MIPC.Name = "MIPC";
            this.MIPC.Size = new System.Drawing.Size(177, 24);
            this.MIPC.Text = "MIPC";
            this.MIPC.Click += new System.EventHandler(this.MIPC_Click);
            // 
            // MoveControl
            // 
            this.MoveControl.Name = "MoveControl";
            this.MoveControl.Size = new System.Drawing.Size(177, 24);
            this.MoveControl.Text = "MoveControl";
            this.MoveControl.Click += new System.EventHandler(this.MoveControl_Click);
            // 
            // 人機畫面ToolStripMenuItem
            // 
            this.人機畫面ToolStripMenuItem.Name = "人機畫面ToolStripMenuItem";
            this.人機畫面ToolStripMenuItem.Size = new System.Drawing.Size(177, 24);
            this.人機畫面ToolStripMenuItem.Text = "人機畫面";
            this.人機畫面ToolStripMenuItem.Click += new System.EventHandler(this.人機畫面ToolStripMenuItem_Click);
            // 
            // panel_Map
            // 
            this.panel_Map.Controls.Add(this.pB_Map);
            this.panel_Map.Location = new System.Drawing.Point(10, 70);
            this.panel_Map.Name = "panel_Map";
            this.panel_Map.Size = new System.Drawing.Size(1070, 580);
            this.panel_Map.TabIndex = 2;
            // 
            // pB_Map
            // 
            this.pB_Map.Location = new System.Drawing.Point(0, 0);
            this.pB_Map.Name = "pB_Map";
            this.pB_Map.Size = new System.Drawing.Size(223, 70);
            this.pB_Map.TabIndex = 0;
            this.pB_Map.TabStop = false;
            // 
            // timer
            // 
            this.timer.Enabled = true;
            this.timer.Interval = 200;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // MovingAddresList
            // 
            this.MovingAddresList.Font = new System.Drawing.Font("新細明體", 16F);
            this.MovingAddresList.FormattingEnabled = true;
            this.MovingAddresList.ItemHeight = 21;
            this.MovingAddresList.Location = new System.Drawing.Point(1085, 70);
            this.MovingAddresList.Name = "MovingAddresList";
            this.MovingAddresList.ScrollAlwaysVisible = true;
            this.MovingAddresList.Size = new System.Drawing.Size(125, 424);
            this.MovingAddresList.TabIndex = 62;
            // 
            // button_SendToMoveControlDebug
            // 
            this.button_SendToMoveControlDebug.Font = new System.Drawing.Font("新細明體", 14F);
            this.button_SendToMoveControlDebug.Location = new System.Drawing.Point(1085, 510);
            this.button_SendToMoveControlDebug.Name = "button_SendToMoveControlDebug";
            this.button_SendToMoveControlDebug.Size = new System.Drawing.Size(125, 30);
            this.button_SendToMoveControlDebug.TabIndex = 63;
            this.button_SendToMoveControlDebug.Text = "SendList";
            this.button_SendToMoveControlDebug.UseVisualStyleBackColor = true;
            this.button_SendToMoveControlDebug.Click += new System.EventHandler(this.button_SendToMoveControlDebug_Click);
            // 
            // button_ClearAddressList
            // 
            this.button_ClearAddressList.Font = new System.Drawing.Font("新細明體", 14F);
            this.button_ClearAddressList.Location = new System.Drawing.Point(1085, 546);
            this.button_ClearAddressList.Name = "button_ClearAddressList";
            this.button_ClearAddressList.Size = new System.Drawing.Size(125, 30);
            this.button_ClearAddressList.TabIndex = 64;
            this.button_ClearAddressList.Text = "Clear";
            this.button_ClearAddressList.UseVisualStyleBackColor = true;
            this.button_ClearAddressList.Click += new System.EventHandler(this.button_ClearAddressList_Click);
            // 
            // button_Login
            // 
            this.button_Login.Font = new System.Drawing.Font("新細明體", 14F);
            this.button_Login.Location = new System.Drawing.Point(850, 35);
            this.button_Login.Name = "button_Login";
            this.button_Login.Size = new System.Drawing.Size(69, 30);
            this.button_Login.TabIndex = 65;
            this.button_Login.Text = "登入";
            this.button_Login.UseVisualStyleBackColor = true;
            this.button_Login.Click += new System.EventHandler(this.button_Login_Click);
            // 
            // button_LogOut
            // 
            this.button_LogOut.Font = new System.Drawing.Font("新細明體", 14F);
            this.button_LogOut.Location = new System.Drawing.Point(925, 35);
            this.button_LogOut.Name = "button_LogOut";
            this.button_LogOut.Size = new System.Drawing.Size(69, 30);
            this.button_LogOut.TabIndex = 66;
            this.button_LogOut.Text = "登出";
            this.button_LogOut.UseVisualStyleBackColor = true;
            this.button_LogOut.Click += new System.EventHandler(this.button_LogOut_Click);
            // 
            // tC_Alarm
            // 
            this.tC_Alarm.Controls.Add(this.pP_Alarm);
            this.tC_Alarm.Controls.Add(this.tP_AlarmHistory);
            this.tC_Alarm.Font = new System.Drawing.Font("新細明體", 16F);
            this.tC_Alarm.Location = new System.Drawing.Point(1216, 528);
            this.tC_Alarm.Name = "tC_Alarm";
            this.tC_Alarm.SelectedIndex = 0;
            this.tC_Alarm.Size = new System.Drawing.Size(450, 260);
            this.tC_Alarm.TabIndex = 67;
            // 
            // pP_Alarm
            // 
            this.pP_Alarm.Controls.Add(this.tbxNowAlarm);
            this.pP_Alarm.Location = new System.Drawing.Point(4, 31);
            this.pP_Alarm.Name = "pP_Alarm";
            this.pP_Alarm.Padding = new System.Windows.Forms.Padding(3);
            this.pP_Alarm.Size = new System.Drawing.Size(442, 225);
            this.pP_Alarm.TabIndex = 0;
            this.pP_Alarm.Text = "Alarm";
            this.pP_Alarm.UseVisualStyleBackColor = true;
            // 
            // tbxNowAlarm
            // 
            this.tbxNowAlarm.Font = new System.Drawing.Font("新細明體", 11F);
            this.tbxNowAlarm.Location = new System.Drawing.Point(0, 0);
            this.tbxNowAlarm.MaxLength = 65550;
            this.tbxNowAlarm.Multiline = true;
            this.tbxNowAlarm.Name = "tbxNowAlarm";
            this.tbxNowAlarm.ReadOnly = true;
            this.tbxNowAlarm.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbxNowAlarm.Size = new System.Drawing.Size(442, 220);
            this.tbxNowAlarm.TabIndex = 138;
            // 
            // tP_AlarmHistory
            // 
            this.tP_AlarmHistory.Controls.Add(this.tbxNowAlarmHistory);
            this.tP_AlarmHistory.Location = new System.Drawing.Point(4, 31);
            this.tP_AlarmHistory.Name = "tP_AlarmHistory";
            this.tP_AlarmHistory.Padding = new System.Windows.Forms.Padding(3);
            this.tP_AlarmHistory.Size = new System.Drawing.Size(442, 225);
            this.tP_AlarmHistory.TabIndex = 1;
            this.tP_AlarmHistory.Text = "AlarmHistory";
            this.tP_AlarmHistory.UseVisualStyleBackColor = true;
            // 
            // tbxNowAlarmHistory
            // 
            this.tbxNowAlarmHistory.Font = new System.Drawing.Font("新細明體", 11F);
            this.tbxNowAlarmHistory.Location = new System.Drawing.Point(0, 0);
            this.tbxNowAlarmHistory.MaxLength = 65550;
            this.tbxNowAlarmHistory.Multiline = true;
            this.tbxNowAlarmHistory.Name = "tbxNowAlarmHistory";
            this.tbxNowAlarmHistory.ReadOnly = true;
            this.tbxNowAlarmHistory.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbxNowAlarmHistory.Size = new System.Drawing.Size(442, 220);
            this.tbxNowAlarmHistory.TabIndex = 139;
            // 
            // button_Alarm
            // 
            this.button_Alarm.Font = new System.Drawing.Font("Times New Roman", 14F);
            this.button_Alarm.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.button_Alarm.Location = new System.Drawing.Point(1541, 34);
            this.button_Alarm.Name = "button_Alarm";
            this.button_Alarm.Size = new System.Drawing.Size(125, 56);
            this.button_Alarm.TabIndex = 68;
            this.button_Alarm.Text = "Reset Alarm";
            this.button_Alarm.UseVisualStyleBackColor = true;
            this.button_Alarm.Click += new System.EventHandler(this.button_Alarm_Click);
            // 
            // button_BuzzOff
            // 
            this.button_BuzzOff.Font = new System.Drawing.Font("Times New Roman", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_BuzzOff.Location = new System.Drawing.Point(1541, 96);
            this.button_BuzzOff.Name = "button_BuzzOff";
            this.button_BuzzOff.Size = new System.Drawing.Size(125, 54);
            this.button_BuzzOff.TabIndex = 69;
            this.button_BuzzOff.Text = "Buzz Off";
            this.button_BuzzOff.UseVisualStyleBackColor = true;
            this.button_BuzzOff.Click += new System.EventHandler(this.button_BuzzOff_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(1363, 34);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(172, 41);
            this.groupBox1.TabIndex = 75;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "AGVC Connection";
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label4.Location = new System.Drawing.Point(6, 12);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(149, 24);
            this.label4.TabIndex = 2;
            this.label4.Text = "Connection";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbxDebugLogMsg
            // 
            this.tbxDebugLogMsg.Font = new System.Drawing.Font("新細明體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbxDebugLogMsg.Location = new System.Drawing.Point(10, 655);
            this.tbxDebugLogMsg.Multiline = true;
            this.tbxDebugLogMsg.Name = "tbxDebugLogMsg";
            this.tbxDebugLogMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbxDebugLogMsg.Size = new System.Drawing.Size(1200, 133);
            this.tbxDebugLogMsg.TabIndex = 76;
            // 
            // button_AutoManual
            // 
            this.button_AutoManual.BackColor = System.Drawing.Color.Red;
            this.button_AutoManual.Font = new System.Drawing.Font("Times New Roman", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_AutoManual.Location = new System.Drawing.Point(1216, 34);
            this.button_AutoManual.Name = "button_AutoManual";
            this.button_AutoManual.Size = new System.Drawing.Size(141, 116);
            this.button_AutoManual.TabIndex = 77;
            this.button_AutoManual.Text = "Manual";
            this.button_AutoManual.UseVisualStyleBackColor = false;
            this.button_AutoManual.Click += new System.EventHandler(this.btnAutoManual_Click);
            // 
            // label_Alarm
            // 
            this.label_Alarm.AutoSize = true;
            this.label_Alarm.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Alarm.Location = new System.Drawing.Point(1103, 40);
            this.label_Alarm.Name = "label_Alarm";
            this.label_Alarm.Size = new System.Drawing.Size(55, 21);
            this.label_Alarm.TabIndex = 80;
            this.label_Alarm.Text = "Alarm";
            // 
            // label_Warn
            // 
            this.label_Warn.AutoSize = true;
            this.label_Warn.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Warn.Location = new System.Drawing.Point(1002, 40);
            this.label_Warn.Name = "label_Warn";
            this.label_Warn.Size = new System.Drawing.Size(49, 21);
            this.label_Warn.TabIndex = 82;
            this.label_Warn.Text = "Warn";
            // 
            // pB_Warn
            // 
            this.pB_Warn.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pB_Warn.Location = new System.Drawing.Point(1053, 34);
            this.pB_Warn.Name = "pB_Warn";
            this.pB_Warn.Size = new System.Drawing.Size(49, 30);
            this.pB_Warn.TabIndex = 81;
            this.pB_Warn.TabStop = false;
            // 
            // pB_Alarm
            // 
            this.pB_Alarm.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pB_Alarm.Location = new System.Drawing.Point(1161, 34);
            this.pB_Alarm.Name = "pB_Alarm";
            this.pB_Alarm.Size = new System.Drawing.Size(49, 30);
            this.pB_Alarm.TabIndex = 79;
            this.pB_Alarm.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(1363, 101);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(172, 49);
            this.groupBox2.TabIndex = 76;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "AGVC Connection";
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label5.Location = new System.Drawing.Point(6, 18);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(149, 24);
            this.label5.TabIndex = 2;
            this.label5.Text = "Connection";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button_SetPosition
            // 
            this.button_SetPosition.Font = new System.Drawing.Font("新細明體", 14F);
            this.button_SetPosition.Location = new System.Drawing.Point(1085, 616);
            this.button_SetPosition.Name = "button_SetPosition";
            this.button_SetPosition.Size = new System.Drawing.Size(125, 30);
            this.button_SetPosition.TabIndex = 83;
            this.button_SetPosition.Text = "SetPosition";
            this.button_SetPosition.UseVisualStyleBackColor = true;
            this.button_SetPosition.Click += new System.EventHandler(this.button_SetPosition_Click);
            // 
            // label_LocateStatus
            // 
            this.label_LocateStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_LocateStatus.Font = new System.Drawing.Font("新細明體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_LocateStatus.ForeColor = System.Drawing.Color.Red;
            this.label_LocateStatus.Location = new System.Drawing.Point(1086, 583);
            this.label_LocateStatus.Name = "label_LocateStatus";
            this.label_LocateStatus.Size = new System.Drawing.Size(124, 26);
            this.label_LocateStatus.TabIndex = 84;
            this.label_LocateStatus.Text = "定位NG";
            this.label_LocateStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1674, 791);
            this.ControlBox = false;
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.label_Warn);
            this.Controls.Add(this.pB_Warn);
            this.Controls.Add(this.label_Alarm);
            this.Controls.Add(this.pB_Alarm);
            this.Controls.Add(this.button_AutoManual);
            this.Controls.Add(this.panel_Map);
            this.Controls.Add(this.MovingAddresList);
            this.Controls.Add(this.button_SendToMoveControlDebug);
            this.Controls.Add(this.button_ClearAddressList);
            this.Controls.Add(this.tbxDebugLogMsg);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button_BuzzOff);
            this.Controls.Add(this.button_Alarm);
            this.Controls.Add(this.tC_Alarm);
            this.Controls.Add(this.button_LogOut);
            this.Controls.Add(this.button_Login);
            this.Controls.Add(this.menuStrip_MainForm);
            this.Controls.Add(this.label_LocateStatus);
            this.Controls.Add(this.button_SetPosition);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.menuStrip_MainForm.ResumeLayout(false);
            this.menuStrip_MainForm.PerformLayout();
            this.panel_Map.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pB_Map)).EndInit();
            this.tC_Alarm.ResumeLayout(false);
            this.pP_Alarm.ResumeLayout(false);
            this.pP_Alarm.PerformLayout();
            this.tP_AlarmHistory.ResumeLayout(false);
            this.tP_AlarmHistory.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pB_Warn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pB_Alarm)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip_MainForm;
        private System.Windows.Forms.ToolStripMenuItem 系統ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 關閉ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 模式ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem MoveControl;
        private System.Windows.Forms.Panel panel_Map;
        private System.Windows.Forms.PictureBox pB_Map;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.ListBox MovingAddresList;
        private System.Windows.Forms.Button button_SendToMoveControlDebug;
        private System.Windows.Forms.Button button_ClearAddressList;
        private System.Windows.Forms.Button button_Login;
        private System.Windows.Forms.Button button_LogOut;
        private System.Windows.Forms.TabControl tC_Alarm;
        private System.Windows.Forms.TabPage pP_Alarm;
        private System.Windows.Forms.TabPage tP_AlarmHistory;
        private System.Windows.Forms.Button button_Alarm;
        private System.Windows.Forms.Button button_BuzzOff;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbxDebugLogMsg;
        private System.Windows.Forms.Button button_AutoManual;
        private System.Windows.Forms.PictureBox pB_Alarm;
        private System.Windows.Forms.Label label_Alarm;
        private System.Windows.Forms.Label label_Warn;
        private System.Windows.Forms.PictureBox pB_Warn;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolStripMenuItem MIPC;
        private System.Windows.Forms.Button button_SetPosition;
        private System.Windows.Forms.Label label_LocateStatus;
        private System.Windows.Forms.TextBox tbxNowAlarm;
        private System.Windows.Forms.TextBox tbxNowAlarmHistory;
        private System.Windows.Forms.ToolStripMenuItem 人機畫面ToolStripMenuItem;
    }
}