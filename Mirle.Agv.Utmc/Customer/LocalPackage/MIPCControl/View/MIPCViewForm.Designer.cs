namespace Mirle.Agv.INX.View
{
    partial class MIPCViewForm
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
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.button_Hide = new System.Windows.Forms.Button();
            this.tC_View = new System.Windows.Forms.TabControl();
            this.label_Status = new System.Windows.Forms.Label();
            this.label_StatusValue = new System.Windows.Forms.Label();
            this.label_ReadyValue = new System.Windows.Forms.Label();
            this.label_Ready = new System.Windows.Forms.Label();
            this.label_ErrorBitValue = new System.Windows.Forms.Label();
            this.label_ErrorBit = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // timer
            // 
            this.timer.Interval = 200;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // button_Hide
            // 
            this.button_Hide.Font = new System.Drawing.Font("標楷體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_Hide.Location = new System.Drawing.Point(849, -1);
            this.button_Hide.Name = "button_Hide";
            this.button_Hide.Size = new System.Drawing.Size(36, 23);
            this.button_Hide.TabIndex = 137;
            this.button_Hide.Text = "X";
            this.button_Hide.UseVisualStyleBackColor = true;
            this.button_Hide.Click += new System.EventHandler(this.button_Hide_Click);
            // 
            // tC_View
            // 
            this.tC_View.Font = new System.Drawing.Font("新細明體", 9F);
            this.tC_View.Location = new System.Drawing.Point(2, 28);
            this.tC_View.Name = "tC_View";
            this.tC_View.SelectedIndex = 0;
            this.tC_View.Size = new System.Drawing.Size(883, 731);
            this.tC_View.TabIndex = 138;
            // 
            // label_Status
            // 
            this.label_Status.AutoSize = true;
            this.label_Status.Font = new System.Drawing.Font("新細明體", 12F);
            this.label_Status.Location = new System.Drawing.Point(12, 5);
            this.label_Status.Name = "label_Status";
            this.label_Status.Size = new System.Drawing.Size(57, 16);
            this.label_Status.TabIndex = 139;
            this.label_Status.Text = "Status : ";
            // 
            // label_StatusValue
            // 
            this.label_StatusValue.AutoSize = true;
            this.label_StatusValue.Font = new System.Drawing.Font("新細明體", 12F);
            this.label_StatusValue.Location = new System.Drawing.Point(92, 5);
            this.label_StatusValue.Name = "label_StatusValue";
            this.label_StatusValue.Size = new System.Drawing.Size(45, 16);
            this.label_StatusValue.TabIndex = 140;
            this.label_StatusValue.Text = "Status";
            // 
            // label_ReadyValue
            // 
            this.label_ReadyValue.AutoSize = true;
            this.label_ReadyValue.Font = new System.Drawing.Font("新細明體", 12F);
            this.label_ReadyValue.Location = new System.Drawing.Point(310, 5);
            this.label_ReadyValue.Name = "label_ReadyValue";
            this.label_ReadyValue.Size = new System.Drawing.Size(45, 16);
            this.label_ReadyValue.TabIndex = 142;
            this.label_ReadyValue.Text = "Status";
            // 
            // label_Ready
            // 
            this.label_Ready.AutoSize = true;
            this.label_Ready.Font = new System.Drawing.Font("新細明體", 12F);
            this.label_Ready.Location = new System.Drawing.Point(230, 5);
            this.label_Ready.Name = "label_Ready";
            this.label_Ready.Size = new System.Drawing.Size(60, 16);
            this.label_Ready.TabIndex = 141;
            this.label_Ready.Text = "Ready : ";
            // 
            // label_ErrorBitValue
            // 
            this.label_ErrorBitValue.AutoSize = true;
            this.label_ErrorBitValue.Font = new System.Drawing.Font("新細明體", 12F);
            this.label_ErrorBitValue.Location = new System.Drawing.Point(528, 5);
            this.label_ErrorBitValue.Name = "label_ErrorBitValue";
            this.label_ErrorBitValue.Size = new System.Drawing.Size(45, 16);
            this.label_ErrorBitValue.TabIndex = 144;
            this.label_ErrorBitValue.Text = "Status";
            // 
            // label_ErrorBit
            // 
            this.label_ErrorBit.AutoSize = true;
            this.label_ErrorBit.Font = new System.Drawing.Font("新細明體", 12F);
            this.label_ErrorBit.Location = new System.Drawing.Point(448, 5);
            this.label_ErrorBit.Name = "label_ErrorBit";
            this.label_ErrorBit.Size = new System.Drawing.Size(48, 16);
            this.label_ErrorBit.TabIndex = 143;
            this.label_ErrorBit.Text = "Error :";
            // 
            // MIPCViewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 761);
            this.ControlBox = false;
            this.Controls.Add(this.label_ErrorBitValue);
            this.Controls.Add(this.label_ErrorBit);
            this.Controls.Add(this.label_ReadyValue);
            this.Controls.Add(this.label_Ready);
            this.Controls.Add(this.label_StatusValue);
            this.Controls.Add(this.label_Status);
            this.Controls.Add(this.tC_View);
            this.Controls.Add(this.button_Hide);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MIPCViewForm";
            this.Text = "MIPCViewForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.Button button_Hide;
        private System.Windows.Forms.TabControl tC_View;
        private System.Windows.Forms.Label label_Status;
        private System.Windows.Forms.Label label_StatusValue;
        private System.Windows.Forms.Label label_ReadyValue;
        private System.Windows.Forms.Label label_Ready;
        private System.Windows.Forms.Label label_ErrorBitValue;
        private System.Windows.Forms.Label label_ErrorBit;
    }
}