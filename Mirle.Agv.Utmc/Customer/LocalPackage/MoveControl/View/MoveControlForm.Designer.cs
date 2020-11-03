namespace Mirle.Agv.INX.View
{
    partial class MoveControlForm
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
            this.tC_MoveControl = new System.Windows.Forms.TabControl();
            this.tP_CreateMoveCommand = new System.Windows.Forms.TabPage();
            this.panel_AdminAdressIDAdd = new System.Windows.Forms.Panel();
            this.button_AdmiinAddressIDAdd = new System.Windows.Forms.Button();
            this.tB_AdmiinAddressID = new System.Windows.Forms.TextBox();
            this.button_DebugModeSend = new System.Windows.Forms.Button();
            this.btnClearMoveCmdInfo = new System.Windows.Forms.Button();
            this.button_AlarmReset = new System.Windows.Forms.Button();
            this.button_ReveseAddressList = new System.Windows.Forms.Button();
            this.button_RunTimesOdd = new System.Windows.Forms.Button();
            this.button_RunTimesEven = new System.Windows.Forms.Button();
            this.tB_RunTimes = new System.Windows.Forms.TextBox();
            this.button_CreateCommandList_Stop = new System.Windows.Forms.Button();
            this.label_RunTimes = new System.Windows.Forms.Label();
            this.panel_CreateCommand_Hide = new System.Windows.Forms.Panel();
            this.button_AutoCycleRun = new System.Windows.Forms.Button();
            this.button_ReadOrWriteSend = new System.Windows.Forms.Button();
            this.cB_ReadFileName = new System.Windows.Forms.ComboBox();
            this.button_ReadOrWrite = new System.Windows.Forms.Button();
            this.label_CreateCommand_Angle = new System.Windows.Forms.Label();
            this.label_CreateCommand_Y = new System.Windows.Forms.Label();
            this.label_CreateCommand_X = new System.Windows.Forms.Label();
            this.button_SetRealPosition = new System.Windows.Forms.Button();
            this.tB_CreateCommand_Angle = new System.Windows.Forms.TextBox();
            this.tB_CreateCommand_Y = new System.Windows.Forms.TextBox();
            this.tB_CreateCommand_X = new System.Windows.Forms.TextBox();
            this.tB_WriteFileName = new System.Windows.Forms.TextBox();
            this.gB_SpeedSelect = new System.Windows.Forms.GroupBox();
            this.rB_SecrtionSpeed = new System.Windows.Forms.RadioButton();
            this.rB_SettingSpeed = new System.Windows.Forms.RadioButton();
            this.button_CheckAddress = new System.Windows.Forms.Button();
            this.label_AddressList = new System.Windows.Forms.Label();
            this.AddressList = new System.Windows.Forms.ListBox();
            this.label_AddressFromMainForm = new System.Windows.Forms.Label();
            this.MainForm_AddressList = new System.Windows.Forms.ListBox();
            this.tP_MoveCommand = new System.Windows.Forms.TabPage();
            this.label_LoopTime_Label = new System.Windows.Forms.Label();
            this.label_LoopTime = new System.Windows.Forms.Label();
            this.button_ResetAlarm = new System.Windows.Forms.Button();
            this.button_SendList = new System.Windows.Forms.Button();
            this.button_StopMove = new System.Windows.Forms.Button();
            this.label_StopReasonValue = new System.Windows.Forms.Label();
            this.label_StopReason = new System.Windows.Forms.Label();
            this.label_MoveCommandID = new System.Windows.Forms.Label();
            this.label_MoveCommandIDLabel = new System.Windows.Forms.Label();
            this.cB_GetAllReserve = new System.Windows.Forms.CheckBox();
            this.label_ReserveList = new System.Windows.Forms.Label();
            this.ReserveList = new System.Windows.Forms.ListBox();
            this.CommandList = new System.Windows.Forms.ListBox();
            this.tP_MoveControlCommand = new System.Windows.Forms.TabPage();
            this.tP_StraightCorrection = new System.Windows.Forms.TabPage();
            this.tP_LocateControlSetOffset = new System.Windows.Forms.TabPage();
            this.tP_WallSetting = new System.Windows.Forms.TabPage();
            this.tP_ConfigSetting = new System.Windows.Forms.TabPage();
            this.button_Hide = new System.Windows.Forms.Button();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.tbxLogView = new System.Windows.Forms.TextBox();
            this.tB_TargetSection = new System.Windows.Forms.TextBox();
            this.button_TargetButton = new System.Windows.Forms.Button();
            this.label_TargetSection = new System.Windows.Forms.Label();
            this.tC_MoveControl.SuspendLayout();
            this.tP_CreateMoveCommand.SuspendLayout();
            this.panel_AdminAdressIDAdd.SuspendLayout();
            this.panel_CreateCommand_Hide.SuspendLayout();
            this.gB_SpeedSelect.SuspendLayout();
            this.tP_MoveCommand.SuspendLayout();
            this.SuspendLayout();
            // 
            // tC_MoveControl
            // 
            this.tC_MoveControl.Controls.Add(this.tP_CreateMoveCommand);
            this.tC_MoveControl.Controls.Add(this.tP_MoveCommand);
            this.tC_MoveControl.Controls.Add(this.tP_MoveControlCommand);
            this.tC_MoveControl.Controls.Add(this.tP_StraightCorrection);
            this.tC_MoveControl.Controls.Add(this.tP_LocateControlSetOffset);
            this.tC_MoveControl.Controls.Add(this.tP_WallSetting);
            this.tC_MoveControl.Controls.Add(this.tP_ConfigSetting);
            this.tC_MoveControl.Font = new System.Drawing.Font("新細明體", 16F);
            this.tC_MoveControl.Location = new System.Drawing.Point(0, 0);
            this.tC_MoveControl.Name = "tC_MoveControl";
            this.tC_MoveControl.SelectedIndex = 0;
            this.tC_MoveControl.Size = new System.Drawing.Size(1182, 520);
            this.tC_MoveControl.TabIndex = 0;
            // 
            // tP_CreateMoveCommand
            // 
            this.tP_CreateMoveCommand.Controls.Add(this.label_TargetSection);
            this.tP_CreateMoveCommand.Controls.Add(this.button_TargetButton);
            this.tP_CreateMoveCommand.Controls.Add(this.tB_TargetSection);
            this.tP_CreateMoveCommand.Controls.Add(this.panel_AdminAdressIDAdd);
            this.tP_CreateMoveCommand.Controls.Add(this.button_DebugModeSend);
            this.tP_CreateMoveCommand.Controls.Add(this.btnClearMoveCmdInfo);
            this.tP_CreateMoveCommand.Controls.Add(this.button_AlarmReset);
            this.tP_CreateMoveCommand.Controls.Add(this.button_ReveseAddressList);
            this.tP_CreateMoveCommand.Controls.Add(this.button_RunTimesOdd);
            this.tP_CreateMoveCommand.Controls.Add(this.button_RunTimesEven);
            this.tP_CreateMoveCommand.Controls.Add(this.tB_RunTimes);
            this.tP_CreateMoveCommand.Controls.Add(this.button_CreateCommandList_Stop);
            this.tP_CreateMoveCommand.Controls.Add(this.label_RunTimes);
            this.tP_CreateMoveCommand.Controls.Add(this.panel_CreateCommand_Hide);
            this.tP_CreateMoveCommand.Controls.Add(this.gB_SpeedSelect);
            this.tP_CreateMoveCommand.Controls.Add(this.button_CheckAddress);
            this.tP_CreateMoveCommand.Controls.Add(this.label_AddressList);
            this.tP_CreateMoveCommand.Controls.Add(this.AddressList);
            this.tP_CreateMoveCommand.Controls.Add(this.label_AddressFromMainForm);
            this.tP_CreateMoveCommand.Controls.Add(this.MainForm_AddressList);
            this.tP_CreateMoveCommand.Font = new System.Drawing.Font("新細明體", 9F);
            this.tP_CreateMoveCommand.Location = new System.Drawing.Point(4, 31);
            this.tP_CreateMoveCommand.Name = "tP_CreateMoveCommand";
            this.tP_CreateMoveCommand.Padding = new System.Windows.Forms.Padding(3);
            this.tP_CreateMoveCommand.Size = new System.Drawing.Size(1174, 485);
            this.tP_CreateMoveCommand.TabIndex = 1;
            this.tP_CreateMoveCommand.Text = "產生命令";
            this.tP_CreateMoveCommand.UseVisualStyleBackColor = true;
            // 
            // panel_AdminAdressIDAdd
            // 
            this.panel_AdminAdressIDAdd.Controls.Add(this.button_AdmiinAddressIDAdd);
            this.panel_AdminAdressIDAdd.Controls.Add(this.tB_AdmiinAddressID);
            this.panel_AdminAdressIDAdd.Location = new System.Drawing.Point(330, 182);
            this.panel_AdminAdressIDAdd.Name = "panel_AdminAdressIDAdd";
            this.panel_AdminAdressIDAdd.Size = new System.Drawing.Size(225, 67);
            this.panel_AdminAdressIDAdd.TabIndex = 122;
            // 
            // button_AdmiinAddressIDAdd
            // 
            this.button_AdmiinAddressIDAdd.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_AdmiinAddressIDAdd.Location = new System.Drawing.Point(128, 19);
            this.button_AdmiinAddressIDAdd.Name = "button_AdmiinAddressIDAdd";
            this.button_AdmiinAddressIDAdd.Size = new System.Drawing.Size(68, 30);
            this.button_AdmiinAddressIDAdd.TabIndex = 123;
            this.button_AdmiinAddressIDAdd.Text = "Add";
            this.button_AdmiinAddressIDAdd.UseVisualStyleBackColor = true;
            this.button_AdmiinAddressIDAdd.Click += new System.EventHandler(this.button_AdmiinAddressIDAdd_Click);
            // 
            // tB_AdmiinAddressID
            // 
            this.tB_AdmiinAddressID.Font = new System.Drawing.Font("新細明體", 12F);
            this.tB_AdmiinAddressID.Location = new System.Drawing.Point(21, 19);
            this.tB_AdmiinAddressID.Name = "tB_AdmiinAddressID";
            this.tB_AdmiinAddressID.Size = new System.Drawing.Size(88, 27);
            this.tB_AdmiinAddressID.TabIndex = 0;
            // 
            // button_DebugModeSend
            // 
            this.button_DebugModeSend.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_DebugModeSend.Location = new System.Drawing.Point(934, 440);
            this.button_DebugModeSend.Name = "button_DebugModeSend";
            this.button_DebugModeSend.Size = new System.Drawing.Size(225, 40);
            this.button_DebugModeSend.TabIndex = 121;
            this.button_DebugModeSend.Text = "產生移動命令";
            this.button_DebugModeSend.UseVisualStyleBackColor = true;
            this.button_DebugModeSend.Click += new System.EventHandler(this.button_DebugModeSend_Click);
            // 
            // btnClearMoveCmdInfo
            // 
            this.btnClearMoveCmdInfo.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnClearMoveCmdInfo.Location = new System.Drawing.Point(330, 380);
            this.btnClearMoveCmdInfo.Name = "btnClearMoveCmdInfo";
            this.btnClearMoveCmdInfo.Size = new System.Drawing.Size(150, 40);
            this.btnClearMoveCmdInfo.TabIndex = 120;
            this.btnClearMoveCmdInfo.Text = "Clear";
            this.btnClearMoveCmdInfo.UseVisualStyleBackColor = true;
            this.btnClearMoveCmdInfo.Click += new System.EventHandler(this.btnClearMoveCmdInfo_Click);
            // 
            // button_AlarmReset
            // 
            this.button_AlarmReset.Font = new System.Drawing.Font("新細明體", 12F);
            this.button_AlarmReset.Location = new System.Drawing.Point(1070, 59);
            this.button_AlarmReset.Name = "button_AlarmReset";
            this.button_AlarmReset.Size = new System.Drawing.Size(70, 70);
            this.button_AlarmReset.TabIndex = 119;
            this.button_AlarmReset.Text = "Alarm\r\nReset\r\n";
            this.button_AlarmReset.UseVisualStyleBackColor = true;
            // 
            // button_ReveseAddressList
            // 
            this.button_ReveseAddressList.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_ReveseAddressList.Location = new System.Drawing.Point(330, 316);
            this.button_ReveseAddressList.Name = "button_ReveseAddressList";
            this.button_ReveseAddressList.Size = new System.Drawing.Size(150, 30);
            this.button_ReveseAddressList.TabIndex = 113;
            this.button_ReveseAddressList.Text = " 路徑全部顛倒";
            this.button_ReveseAddressList.UseVisualStyleBackColor = true;
            // 
            // button_RunTimesOdd
            // 
            this.button_RunTimesOdd.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_RunTimesOdd.Location = new System.Drawing.Point(585, 266);
            this.button_RunTimesOdd.Name = "button_RunTimesOdd";
            this.button_RunTimesOdd.Size = new System.Drawing.Size(68, 30);
            this.button_RunTimesOdd.TabIndex = 112;
            this.button_RunTimesOdd.Text = "次";
            this.button_RunTimesOdd.UseVisualStyleBackColor = true;
            // 
            // button_RunTimesEven
            // 
            this.button_RunTimesEven.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_RunTimesEven.Location = new System.Drawing.Point(503, 266);
            this.button_RunTimesEven.Name = "button_RunTimesEven";
            this.button_RunTimesEven.Size = new System.Drawing.Size(68, 30);
            this.button_RunTimesEven.TabIndex = 111;
            this.button_RunTimesEven.Text = "趟";
            this.button_RunTimesEven.UseVisualStyleBackColor = true;
            // 
            // tB_RunTimes
            // 
            this.tB_RunTimes.Location = new System.Drawing.Point(421, 266);
            this.tB_RunTimes.Name = "tB_RunTimes";
            this.tB_RunTimes.Size = new System.Drawing.Size(72, 22);
            this.tB_RunTimes.TabIndex = 110;
            this.tB_RunTimes.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // button_CreateCommandList_Stop
            // 
            this.button_CreateCommandList_Stop.Font = new System.Drawing.Font("新細明體", 16F);
            this.button_CreateCommandList_Stop.Location = new System.Drawing.Point(793, 438);
            this.button_CreateCommandList_Stop.Name = "button_CreateCommandList_Stop";
            this.button_CreateCommandList_Stop.Size = new System.Drawing.Size(135, 41);
            this.button_CreateCommandList_Stop.TabIndex = 116;
            this.button_CreateCommandList_Stop.Text = "Stop";
            this.button_CreateCommandList_Stop.UseVisualStyleBackColor = true;
            this.button_CreateCommandList_Stop.Click += new System.EventHandler(this.button_CreateCommandList_Stop_Click);
            // 
            // label_RunTimes
            // 
            this.label_RunTimes.AutoSize = true;
            this.label_RunTimes.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_RunTimes.Location = new System.Drawing.Point(340, 273);
            this.label_RunTimes.Name = "label_RunTimes";
            this.label_RunTimes.Size = new System.Drawing.Size(76, 19);
            this.label_RunTimes.TabIndex = 109;
            this.label_RunTimes.Text = "來回跑 :";
            // 
            // panel_CreateCommand_Hide
            // 
            this.panel_CreateCommand_Hide.Controls.Add(this.button_AutoCycleRun);
            this.panel_CreateCommand_Hide.Controls.Add(this.button_ReadOrWriteSend);
            this.panel_CreateCommand_Hide.Controls.Add(this.cB_ReadFileName);
            this.panel_CreateCommand_Hide.Controls.Add(this.button_ReadOrWrite);
            this.panel_CreateCommand_Hide.Controls.Add(this.label_CreateCommand_Angle);
            this.panel_CreateCommand_Hide.Controls.Add(this.label_CreateCommand_Y);
            this.panel_CreateCommand_Hide.Controls.Add(this.label_CreateCommand_X);
            this.panel_CreateCommand_Hide.Controls.Add(this.button_SetRealPosition);
            this.panel_CreateCommand_Hide.Controls.Add(this.tB_CreateCommand_Angle);
            this.panel_CreateCommand_Hide.Controls.Add(this.tB_CreateCommand_Y);
            this.panel_CreateCommand_Hide.Controls.Add(this.tB_CreateCommand_X);
            this.panel_CreateCommand_Hide.Controls.Add(this.tB_WriteFileName);
            this.panel_CreateCommand_Hide.Location = new System.Drawing.Point(509, 307);
            this.panel_CreateCommand_Hide.Name = "panel_CreateCommand_Hide";
            this.panel_CreateCommand_Hide.Size = new System.Drawing.Size(258, 172);
            this.panel_CreateCommand_Hide.TabIndex = 118;
            // 
            // button_AutoCycleRun
            // 
            this.button_AutoCycleRun.Location = new System.Drawing.Point(4, 77);
            this.button_AutoCycleRun.Name = "button_AutoCycleRun";
            this.button_AutoCycleRun.Size = new System.Drawing.Size(132, 29);
            this.button_AutoCycleRun.TabIndex = 112;
            this.button_AutoCycleRun.Text = "AutoCycleRun";
            this.button_AutoCycleRun.UseVisualStyleBackColor = true;
            this.button_AutoCycleRun.Click += new System.EventHandler(this.button_AutoCycleRun_Click);
            // 
            // button_ReadOrWriteSend
            // 
            this.button_ReadOrWriteSend.Font = new System.Drawing.Font("新細明體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_ReadOrWriteSend.Location = new System.Drawing.Point(181, 21);
            this.button_ReadOrWriteSend.Name = "button_ReadOrWriteSend";
            this.button_ReadOrWriteSend.Size = new System.Drawing.Size(70, 50);
            this.button_ReadOrWriteSend.TabIndex = 110;
            this.button_ReadOrWriteSend.Text = "確定";
            this.button_ReadOrWriteSend.UseVisualStyleBackColor = true;
            // 
            // cB_ReadFileName
            // 
            this.cB_ReadFileName.Font = new System.Drawing.Font("新細明體", 16F);
            this.cB_ReadFileName.FormattingEnabled = true;
            this.cB_ReadFileName.Location = new System.Drawing.Point(76, 33);
            this.cB_ReadFileName.Name = "cB_ReadFileName";
            this.cB_ReadFileName.Size = new System.Drawing.Size(100, 29);
            this.cB_ReadFileName.TabIndex = 108;
            this.cB_ReadFileName.Visible = false;
            // 
            // button_ReadOrWrite
            // 
            this.button_ReadOrWrite.Font = new System.Drawing.Font("新細明體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_ReadOrWrite.Location = new System.Drawing.Point(4, 21);
            this.button_ReadOrWrite.Name = "button_ReadOrWrite";
            this.button_ReadOrWrite.Size = new System.Drawing.Size(70, 50);
            this.button_ReadOrWrite.TabIndex = 107;
            this.button_ReadOrWrite.Text = "匯出";
            this.button_ReadOrWrite.UseVisualStyleBackColor = true;
            // 
            // label_CreateCommand_Angle
            // 
            this.label_CreateCommand_Angle.AutoSize = true;
            this.label_CreateCommand_Angle.Location = new System.Drawing.Point(178, 115);
            this.label_CreateCommand_Angle.Name = "label_CreateCommand_Angle";
            this.label_CreateCommand_Angle.Size = new System.Drawing.Size(33, 12);
            this.label_CreateCommand_Angle.TabIndex = 106;
            this.label_CreateCommand_Angle.Text = "Angle";
            // 
            // label_CreateCommand_Y
            // 
            this.label_CreateCommand_Y.AutoSize = true;
            this.label_CreateCommand_Y.Location = new System.Drawing.Point(114, 115);
            this.label_CreateCommand_Y.Name = "label_CreateCommand_Y";
            this.label_CreateCommand_Y.Size = new System.Drawing.Size(13, 12);
            this.label_CreateCommand_Y.TabIndex = 105;
            this.label_CreateCommand_Y.Text = "Y";
            // 
            // label_CreateCommand_X
            // 
            this.label_CreateCommand_X.AutoSize = true;
            this.label_CreateCommand_X.Location = new System.Drawing.Point(33, 115);
            this.label_CreateCommand_X.Name = "label_CreateCommand_X";
            this.label_CreateCommand_X.Size = new System.Drawing.Size(13, 12);
            this.label_CreateCommand_X.TabIndex = 104;
            this.label_CreateCommand_X.Text = "X";
            // 
            // button_SetRealPosition
            // 
            this.button_SetRealPosition.Font = new System.Drawing.Font("新細明體", 12F);
            this.button_SetRealPosition.Location = new System.Drawing.Point(164, 74);
            this.button_SetRealPosition.Name = "button_SetRealPosition";
            this.button_SetRealPosition.Size = new System.Drawing.Size(87, 32);
            this.button_SetRealPosition.TabIndex = 103;
            this.button_SetRealPosition.Text = "設定位置";
            this.button_SetRealPosition.UseVisualStyleBackColor = true;
            this.button_SetRealPosition.Click += new System.EventHandler(this.button_SetRealPosition_Click);
            // 
            // tB_CreateCommand_Angle
            // 
            this.tB_CreateCommand_Angle.Location = new System.Drawing.Point(168, 136);
            this.tB_CreateCommand_Angle.Name = "tB_CreateCommand_Angle";
            this.tB_CreateCommand_Angle.Size = new System.Drawing.Size(83, 22);
            this.tB_CreateCommand_Angle.TabIndex = 102;
            // 
            // tB_CreateCommand_Y
            // 
            this.tB_CreateCommand_Y.Location = new System.Drawing.Point(86, 136);
            this.tB_CreateCommand_Y.Name = "tB_CreateCommand_Y";
            this.tB_CreateCommand_Y.Size = new System.Drawing.Size(83, 22);
            this.tB_CreateCommand_Y.TabIndex = 101;
            // 
            // tB_CreateCommand_X
            // 
            this.tB_CreateCommand_X.Location = new System.Drawing.Point(4, 136);
            this.tB_CreateCommand_X.Name = "tB_CreateCommand_X";
            this.tB_CreateCommand_X.Size = new System.Drawing.Size(83, 22);
            this.tB_CreateCommand_X.TabIndex = 75;
            // 
            // tB_WriteFileName
            // 
            this.tB_WriteFileName.Location = new System.Drawing.Point(76, 40);
            this.tB_WriteFileName.Name = "tB_WriteFileName";
            this.tB_WriteFileName.Size = new System.Drawing.Size(100, 22);
            this.tB_WriteFileName.TabIndex = 109;
            // 
            // gB_SpeedSelect
            // 
            this.gB_SpeedSelect.Controls.Add(this.rB_SecrtionSpeed);
            this.gB_SpeedSelect.Controls.Add(this.rB_SettingSpeed);
            this.gB_SpeedSelect.Font = new System.Drawing.Font("新細明體", 16F);
            this.gB_SpeedSelect.Location = new System.Drawing.Point(330, 34);
            this.gB_SpeedSelect.Name = "gB_SpeedSelect";
            this.gB_SpeedSelect.Size = new System.Drawing.Size(167, 129);
            this.gB_SpeedSelect.TabIndex = 74;
            this.gB_SpeedSelect.TabStop = false;
            this.gB_SpeedSelect.Text = "速度選擇";
            // 
            // rB_SecrtionSpeed
            // 
            this.rB_SecrtionSpeed.AutoSize = true;
            this.rB_SecrtionSpeed.Location = new System.Drawing.Point(21, 79);
            this.rB_SecrtionSpeed.Name = "rB_SecrtionSpeed";
            this.rB_SecrtionSpeed.Size = new System.Drawing.Size(133, 26);
            this.rB_SecrtionSpeed.TabIndex = 76;
            this.rB_SecrtionSpeed.TabStop = true;
            this.rB_SecrtionSpeed.Text = "Section速度";
            this.rB_SecrtionSpeed.UseVisualStyleBackColor = true;
            // 
            // rB_SettingSpeed
            // 
            this.rB_SettingSpeed.AutoSize = true;
            this.rB_SettingSpeed.Location = new System.Drawing.Point(21, 32);
            this.rB_SettingSpeed.Name = "rB_SettingSpeed";
            this.rB_SettingSpeed.Size = new System.Drawing.Size(116, 26);
            this.rB_SettingSpeed.TabIndex = 75;
            this.rB_SettingSpeed.TabStop = true;
            this.rB_SettingSpeed.Text = "設定速度";
            this.rB_SettingSpeed.UseVisualStyleBackColor = true;
            // 
            // button_CheckAddress
            // 
            this.button_CheckAddress.Font = new System.Drawing.Font("新細明體", 16F);
            this.button_CheckAddress.Location = new System.Drawing.Point(330, 440);
            this.button_CheckAddress.Name = "button_CheckAddress";
            this.button_CheckAddress.Size = new System.Drawing.Size(150, 40);
            this.button_CheckAddress.TabIndex = 71;
            this.button_CheckAddress.Text = "補Address";
            this.button_CheckAddress.UseVisualStyleBackColor = true;
            this.button_CheckAddress.Click += new System.EventHandler(this.button_CheckAddress_Click);
            // 
            // label_AddressList
            // 
            this.label_AddressList.AutoSize = true;
            this.label_AddressList.Font = new System.Drawing.Font("新細明體", 14F);
            this.label_AddressList.Location = new System.Drawing.Point(188, 12);
            this.label_AddressList.Name = "label_AddressList";
            this.label_AddressList.Size = new System.Drawing.Size(101, 19);
            this.label_AddressList.TabIndex = 70;
            this.label_AddressList.Text = "Address List";
            // 
            // AddressList
            // 
            this.AddressList.Font = new System.Drawing.Font("新細明體", 16F);
            this.AddressList.FormattingEnabled = true;
            this.AddressList.ItemHeight = 21;
            this.AddressList.Location = new System.Drawing.Point(171, 34);
            this.AddressList.Name = "AddressList";
            this.AddressList.ScrollAlwaysVisible = true;
            this.AddressList.Size = new System.Drawing.Size(150, 445);
            this.AddressList.TabIndex = 69;
            // 
            // label_AddressFromMainForm
            // 
            this.label_AddressFromMainForm.AutoSize = true;
            this.label_AddressFromMainForm.Font = new System.Drawing.Font("新細明體", 12F);
            this.label_AddressFromMainForm.Location = new System.Drawing.Point(5, 12);
            this.label_AddressFromMainForm.Name = "label_AddressFromMainForm";
            this.label_AddressFromMainForm.Size = new System.Drawing.Size(155, 16);
            this.label_AddressFromMainForm.TabIndex = 68;
            this.label_AddressFromMainForm.Text = "MainForm Address List";
            // 
            // MainForm_AddressList
            // 
            this.MainForm_AddressList.Font = new System.Drawing.Font("新細明體", 16F);
            this.MainForm_AddressList.FormattingEnabled = true;
            this.MainForm_AddressList.ItemHeight = 21;
            this.MainForm_AddressList.Location = new System.Drawing.Point(8, 34);
            this.MainForm_AddressList.Name = "MainForm_AddressList";
            this.MainForm_AddressList.ScrollAlwaysVisible = true;
            this.MainForm_AddressList.Size = new System.Drawing.Size(150, 445);
            this.MainForm_AddressList.TabIndex = 67;
            // 
            // tP_MoveCommand
            // 
            this.tP_MoveCommand.Controls.Add(this.label_LoopTime_Label);
            this.tP_MoveCommand.Controls.Add(this.label_LoopTime);
            this.tP_MoveCommand.Controls.Add(this.button_ResetAlarm);
            this.tP_MoveCommand.Controls.Add(this.button_SendList);
            this.tP_MoveCommand.Controls.Add(this.button_StopMove);
            this.tP_MoveCommand.Controls.Add(this.label_StopReasonValue);
            this.tP_MoveCommand.Controls.Add(this.label_StopReason);
            this.tP_MoveCommand.Controls.Add(this.label_MoveCommandID);
            this.tP_MoveCommand.Controls.Add(this.label_MoveCommandIDLabel);
            this.tP_MoveCommand.Controls.Add(this.cB_GetAllReserve);
            this.tP_MoveCommand.Controls.Add(this.label_ReserveList);
            this.tP_MoveCommand.Controls.Add(this.ReserveList);
            this.tP_MoveCommand.Controls.Add(this.CommandList);
            this.tP_MoveCommand.Font = new System.Drawing.Font("新細明體", 9F);
            this.tP_MoveCommand.Location = new System.Drawing.Point(4, 31);
            this.tP_MoveCommand.Name = "tP_MoveCommand";
            this.tP_MoveCommand.Padding = new System.Windows.Forms.Padding(3);
            this.tP_MoveCommand.Size = new System.Drawing.Size(1174, 485);
            this.tP_MoveCommand.TabIndex = 0;
            this.tP_MoveCommand.Text = "移動資料";
            this.tP_MoveCommand.UseVisualStyleBackColor = true;
            // 
            // label_LoopTime_Label
            // 
            this.label_LoopTime_Label.AutoSize = true;
            this.label_LoopTime_Label.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_LoopTime_Label.ForeColor = System.Drawing.Color.Black;
            this.label_LoopTime_Label.Location = new System.Drawing.Point(1022, 414);
            this.label_LoopTime_Label.Name = "label_LoopTime_Label";
            this.label_LoopTime_Label.Size = new System.Drawing.Size(84, 19);
            this.label_LoopTime_Label.TabIndex = 158;
            this.label_LoopTime_Label.Text = "loopTime:";
            // 
            // label_LoopTime
            // 
            this.label_LoopTime.AutoSize = true;
            this.label_LoopTime.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_LoopTime.ForeColor = System.Drawing.Color.Red;
            this.label_LoopTime.Location = new System.Drawing.Point(1112, 414);
            this.label_LoopTime.Name = "label_LoopTime";
            this.label_LoopTime.Size = new System.Drawing.Size(56, 19);
            this.label_LoopTime.TabIndex = 157;
            this.label_LoopTime.Text = "XXms";
            // 
            // button_ResetAlarm
            // 
            this.button_ResetAlarm.Location = new System.Drawing.Point(884, 443);
            this.button_ResetAlarm.Name = "button_ResetAlarm";
            this.button_ResetAlarm.Size = new System.Drawing.Size(125, 34);
            this.button_ResetAlarm.TabIndex = 156;
            this.button_ResetAlarm.Text = "ResetAlarm";
            this.button_ResetAlarm.UseVisualStyleBackColor = true;
            this.button_ResetAlarm.Click += new System.EventHandler(this.button_ResetAlarm_Click);
            // 
            // button_SendList
            // 
            this.button_SendList.Location = new System.Drawing.Point(1015, 443);
            this.button_SendList.Name = "button_SendList";
            this.button_SendList.Size = new System.Drawing.Size(153, 34);
            this.button_SendList.TabIndex = 154;
            this.button_SendList.Text = "執行移動命令";
            this.button_SendList.UseVisualStyleBackColor = true;
            this.button_SendList.Click += new System.EventHandler(this.button_SendList_Click);
            // 
            // button_StopMove
            // 
            this.button_StopMove.Location = new System.Drawing.Point(783, 443);
            this.button_StopMove.Name = "button_StopMove";
            this.button_StopMove.Size = new System.Drawing.Size(95, 34);
            this.button_StopMove.TabIndex = 155;
            this.button_StopMove.Text = "Stop";
            this.button_StopMove.UseVisualStyleBackColor = true;
            this.button_StopMove.Click += new System.EventHandler(this.button_StopMove_Click);
            // 
            // label_StopReasonValue
            // 
            this.label_StopReasonValue.AutoSize = true;
            this.label_StopReasonValue.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_StopReasonValue.ForeColor = System.Drawing.Color.Red;
            this.label_StopReasonValue.Location = new System.Drawing.Point(450, 5);
            this.label_StopReasonValue.Name = "label_StopReasonValue";
            this.label_StopReasonValue.Size = new System.Drawing.Size(110, 19);
            this.label_StopReasonValue.TabIndex = 153;
            this.label_StopReasonValue.Text = "ErrorMessage";
            // 
            // label_StopReason
            // 
            this.label_StopReason.AutoSize = true;
            this.label_StopReason.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_StopReason.Location = new System.Drawing.Point(334, 5);
            this.label_StopReason.Name = "label_StopReason";
            this.label_StopReason.Size = new System.Drawing.Size(110, 19);
            this.label_StopReason.TabIndex = 152;
            this.label_StopReason.Text = "Stop Reason :";
            // 
            // label_MoveCommandID
            // 
            this.label_MoveCommandID.AutoSize = true;
            this.label_MoveCommandID.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_MoveCommandID.Location = new System.Drawing.Point(178, 5);
            this.label_MoveCommandID.Name = "label_MoveCommandID";
            this.label_MoveCommandID.Size = new System.Drawing.Size(57, 19);
            this.label_MoveCommandID.TabIndex = 151;
            this.label_MoveCommandID.Text = "Empty";
            // 
            // label_MoveCommandIDLabel
            // 
            this.label_MoveCommandIDLabel.AutoSize = true;
            this.label_MoveCommandIDLabel.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_MoveCommandIDLabel.Location = new System.Drawing.Point(7, 6);
            this.label_MoveCommandIDLabel.Name = "label_MoveCommandIDLabel";
            this.label_MoveCommandIDLabel.Size = new System.Drawing.Size(165, 19);
            this.label_MoveCommandIDLabel.TabIndex = 150;
            this.label_MoveCommandIDLabel.Text = "Move Command ID :";
            // 
            // cB_GetAllReserve
            // 
            this.cB_GetAllReserve.AutoSize = true;
            this.cB_GetAllReserve.Checked = true;
            this.cB_GetAllReserve.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cB_GetAllReserve.Font = new System.Drawing.Font("新細明體", 12F);
            this.cB_GetAllReserve.Location = new System.Drawing.Point(1061, 5);
            this.cB_GetAllReserve.Name = "cB_GetAllReserve";
            this.cB_GetAllReserve.Size = new System.Drawing.Size(107, 20);
            this.cB_GetAllReserve.TabIndex = 149;
            this.cB_GetAllReserve.Text = "取得所有點";
            this.cB_GetAllReserve.UseVisualStyleBackColor = true;
            // 
            // label_ReserveList
            // 
            this.label_ReserveList.AutoSize = true;
            this.label_ReserveList.Font = new System.Drawing.Font("新細明體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_ReserveList.Location = new System.Drawing.Point(933, 6);
            this.label_ReserveList.Name = "label_ReserveList";
            this.label_ReserveList.Size = new System.Drawing.Size(110, 19);
            this.label_ReserveList.TabIndex = 148;
            this.label_ReserveList.Text = "Reserve List :";
            // 
            // ReserveList
            // 
            this.ReserveList.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.ReserveList.FormattingEnabled = true;
            this.ReserveList.HorizontalScrollbar = true;
            this.ReserveList.ItemHeight = 16;
            this.ReserveList.Location = new System.Drawing.Point(937, 27);
            this.ReserveList.Name = "ReserveList";
            this.ReserveList.ScrollAlwaysVisible = true;
            this.ReserveList.Size = new System.Drawing.Size(231, 372);
            this.ReserveList.TabIndex = 146;
            this.ReserveList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ReserveList_MouseDoubleClick);
            // 
            // CommandList
            // 
            this.CommandList.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.CommandList.FormattingEnabled = true;
            this.CommandList.HorizontalScrollbar = true;
            this.CommandList.ItemHeight = 16;
            this.CommandList.Location = new System.Drawing.Point(3, 27);
            this.CommandList.Name = "CommandList";
            this.CommandList.ScrollAlwaysVisible = true;
            this.CommandList.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.CommandList.Size = new System.Drawing.Size(928, 372);
            this.CommandList.TabIndex = 145;
            // 
            // tP_MoveControlCommand
            // 
            this.tP_MoveControlCommand.Location = new System.Drawing.Point(4, 31);
            this.tP_MoveControlCommand.Name = "tP_MoveControlCommand";
            this.tP_MoveControlCommand.Size = new System.Drawing.Size(1174, 485);
            this.tP_MoveControlCommand.TabIndex = 2;
            this.tP_MoveControlCommand.Text = "指定命令";
            this.tP_MoveControlCommand.UseVisualStyleBackColor = true;
            // 
            // tP_StraightCorrection
            // 
            this.tP_StraightCorrection.Location = new System.Drawing.Point(4, 31);
            this.tP_StraightCorrection.Name = "tP_StraightCorrection";
            this.tP_StraightCorrection.Size = new System.Drawing.Size(1174, 485);
            this.tP_StraightCorrection.TabIndex = 3;
            this.tP_StraightCorrection.Text = "直線校正";
            this.tP_StraightCorrection.UseVisualStyleBackColor = true;
            // 
            // tP_LocateControlSetOffset
            // 
            this.tP_LocateControlSetOffset.Location = new System.Drawing.Point(4, 31);
            this.tP_LocateControlSetOffset.Name = "tP_LocateControlSetOffset";
            this.tP_LocateControlSetOffset.Size = new System.Drawing.Size(1174, 485);
            this.tP_LocateControlSetOffset.TabIndex = 4;
            this.tP_LocateControlSetOffset.Text = "定位校正";
            this.tP_LocateControlSetOffset.UseVisualStyleBackColor = true;
            // 
            // tP_WallSetting
            // 
            this.tP_WallSetting.Location = new System.Drawing.Point(4, 31);
            this.tP_WallSetting.Name = "tP_WallSetting";
            this.tP_WallSetting.Size = new System.Drawing.Size(1174, 485);
            this.tP_WallSetting.TabIndex = 5;
            this.tP_WallSetting.Text = "牆壁設定";
            this.tP_WallSetting.UseVisualStyleBackColor = true;
            // 
            // tP_ConfigSetting
            // 
            this.tP_ConfigSetting.Location = new System.Drawing.Point(4, 31);
            this.tP_ConfigSetting.Name = "tP_ConfigSetting";
            this.tP_ConfigSetting.Size = new System.Drawing.Size(1174, 485);
            this.tP_ConfigSetting.TabIndex = 6;
            this.tP_ConfigSetting.Text = "參數修改";
            this.tP_ConfigSetting.UseVisualStyleBackColor = true;
            // 
            // button_Hide
            // 
            this.button_Hide.Font = new System.Drawing.Font("標楷體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_Hide.Location = new System.Drawing.Point(1149, -1);
            this.button_Hide.Name = "button_Hide";
            this.button_Hide.Size = new System.Drawing.Size(36, 23);
            this.button_Hide.TabIndex = 136;
            this.button_Hide.Text = "X";
            this.button_Hide.UseVisualStyleBackColor = true;
            this.button_Hide.Click += new System.EventHandler(this.button_Hide_Click);
            // 
            // timer
            // 
            this.timer.Interval = 200;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // tbxLogView
            // 
            this.tbxLogView.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tbxLogView.Location = new System.Drawing.Point(0, 520);
            this.tbxLogView.MaxLength = 65550;
            this.tbxLogView.Multiline = true;
            this.tbxLogView.Name = "tbxLogView";
            this.tbxLogView.ReadOnly = true;
            this.tbxLogView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbxLogView.Size = new System.Drawing.Size(1185, 240);
            this.tbxLogView.TabIndex = 137;
            // 
            // tB_TargetSection
            // 
            this.tB_TargetSection.Location = new System.Drawing.Point(977, 179);
            this.tB_TargetSection.Name = "tB_TargetSection";
            this.tB_TargetSection.Size = new System.Drawing.Size(126, 22);
            this.tB_TargetSection.TabIndex = 123;
            // 
            // button_TargetButton
            // 
            this.button_TargetButton.Font = new System.Drawing.Font("新細明體", 14F);
            this.button_TargetButton.Location = new System.Drawing.Point(977, 210);
            this.button_TargetButton.Name = "button_TargetButton";
            this.button_TargetButton.Size = new System.Drawing.Size(126, 34);
            this.button_TargetButton.TabIndex = 124;
            this.button_TargetButton.Text = "Set";
            this.button_TargetButton.UseVisualStyleBackColor = true;
            this.button_TargetButton.Click += new System.EventHandler(this.button_TargetButton_Click);
            // 
            // label_TargetSection
            // 
            this.label_TargetSection.AutoSize = true;
            this.label_TargetSection.Font = new System.Drawing.Font("新細明體", 14F);
            this.label_TargetSection.Location = new System.Drawing.Point(981, 155);
            this.label_TargetSection.Name = "label_TargetSection";
            this.label_TargetSection.Size = new System.Drawing.Size(53, 19);
            this.label_TargetSection.TabIndex = 125;
            this.label_TargetSection.Text = "label1";
            // 
            // MoveControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1184, 761);
            this.ControlBox = false;
            this.Controls.Add(this.tbxLogView);
            this.Controls.Add(this.button_Hide);
            this.Controls.Add(this.tC_MoveControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MoveControlForm";
            this.Text = "MoveControlForm";
            this.tC_MoveControl.ResumeLayout(false);
            this.tP_CreateMoveCommand.ResumeLayout(false);
            this.tP_CreateMoveCommand.PerformLayout();
            this.panel_AdminAdressIDAdd.ResumeLayout(false);
            this.panel_AdminAdressIDAdd.PerformLayout();
            this.panel_CreateCommand_Hide.ResumeLayout(false);
            this.panel_CreateCommand_Hide.PerformLayout();
            this.gB_SpeedSelect.ResumeLayout(false);
            this.gB_SpeedSelect.PerformLayout();
            this.tP_MoveCommand.ResumeLayout(false);
            this.tP_MoveCommand.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tC_MoveControl;
        private System.Windows.Forms.TabPage tP_CreateMoveCommand;
        private System.Windows.Forms.TabPage tP_MoveCommand;
        private System.Windows.Forms.Button button_Hide;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.Label label_AddressList;
        private System.Windows.Forms.Label label_AddressFromMainForm;
        private System.Windows.Forms.ListBox MainForm_AddressList;
        private System.Windows.Forms.TabPage tP_MoveControlCommand;
        private System.Windows.Forms.TabPage tP_StraightCorrection;
        private System.Windows.Forms.TabPage tP_LocateControlSetOffset;
        private System.Windows.Forms.TabPage tP_WallSetting;
        private System.Windows.Forms.TabPage tP_ConfigSetting;
        private System.Windows.Forms.TextBox tbxLogView;
        private System.Windows.Forms.Button button_CheckAddress;
        private System.Windows.Forms.GroupBox gB_SpeedSelect;
        private System.Windows.Forms.ListBox AddressList;
        private System.Windows.Forms.Button button_DebugModeSend;
        private System.Windows.Forms.Button btnClearMoveCmdInfo;
        private System.Windows.Forms.Button button_AlarmReset;
        private System.Windows.Forms.Button button_ReveseAddressList;
        private System.Windows.Forms.Button button_RunTimesOdd;
        private System.Windows.Forms.Button button_RunTimesEven;
        private System.Windows.Forms.TextBox tB_RunTimes;
        private System.Windows.Forms.Button button_CreateCommandList_Stop;
        private System.Windows.Forms.Label label_RunTimes;
        private System.Windows.Forms.Panel panel_CreateCommand_Hide;
        private System.Windows.Forms.Button button_AutoCycleRun;
        private System.Windows.Forms.Button button_ReadOrWriteSend;
        private System.Windows.Forms.ComboBox cB_ReadFileName;
        private System.Windows.Forms.Button button_ReadOrWrite;
        private System.Windows.Forms.Label label_CreateCommand_Angle;
        private System.Windows.Forms.Label label_CreateCommand_Y;
        private System.Windows.Forms.Label label_CreateCommand_X;
        private System.Windows.Forms.Button button_SetRealPosition;
        private System.Windows.Forms.TextBox tB_CreateCommand_Angle;
        private System.Windows.Forms.TextBox tB_CreateCommand_Y;
        private System.Windows.Forms.TextBox tB_CreateCommand_X;
        private System.Windows.Forms.TextBox tB_WriteFileName;
        private System.Windows.Forms.Label label_LoopTime_Label;
        private System.Windows.Forms.Label label_LoopTime;
        private System.Windows.Forms.Button button_ResetAlarm;
        private System.Windows.Forms.Button button_SendList;
        private System.Windows.Forms.Button button_StopMove;
        private System.Windows.Forms.Label label_StopReasonValue;
        private System.Windows.Forms.Label label_StopReason;
        private System.Windows.Forms.Label label_MoveCommandID;
        private System.Windows.Forms.Label label_MoveCommandIDLabel;
        private System.Windows.Forms.CheckBox cB_GetAllReserve;
        private System.Windows.Forms.Label label_ReserveList;
        private System.Windows.Forms.ListBox ReserveList;
        private System.Windows.Forms.ListBox CommandList;
        private System.Windows.Forms.RadioButton rB_SecrtionSpeed;
        private System.Windows.Forms.RadioButton rB_SettingSpeed;
        private System.Windows.Forms.Panel panel_AdminAdressIDAdd;
        private System.Windows.Forms.Button button_AdmiinAddressIDAdd;
        private System.Windows.Forms.TextBox tB_AdmiinAddressID;
        private System.Windows.Forms.Label label_TargetSection;
        private System.Windows.Forms.Button button_TargetButton;
        private System.Windows.Forms.TextBox tB_TargetSection;
    }
}