namespace Mirle.Agv.INX.View
{
    partial class MIPCInfo
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 元件設計工具產生的程式碼

        /// <summary> 
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label_MIPCName = new System.Windows.Forms.Label();
            this.label_IPCName = new System.Windows.Forms.Label();
            this.label_WriteRead = new System.Windows.Forms.Label();
            this.tB_Write = new System.Windows.Forms.TextBox();
            this.button_Send = new System.Windows.Forms.Button();
            this.label_Value = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label_MIPCName
            // 
            this.label_MIPCName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_MIPCName.Font = new System.Drawing.Font("新細明體", 14F);
            this.label_MIPCName.Location = new System.Drawing.Point(0, 0);
            this.label_MIPCName.Name = "label_MIPCName";
            this.label_MIPCName.Size = new System.Drawing.Size(180, 30);
            this.label_MIPCName.TabIndex = 0;
            this.label_MIPCName.Text = "MIPC_Name";
            this.label_MIPCName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_IPCName
            // 
            this.label_IPCName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_IPCName.Font = new System.Drawing.Font("新細明體", 14F);
            this.label_IPCName.Location = new System.Drawing.Point(179, 0);
            this.label_IPCName.Name = "label_IPCName";
            this.label_IPCName.Size = new System.Drawing.Size(180, 30);
            this.label_IPCName.TabIndex = 1;
            this.label_IPCName.Text = "IPC_Name";
            this.label_IPCName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_WriteRead
            // 
            this.label_WriteRead.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_WriteRead.Font = new System.Drawing.Font("新細明體", 14F);
            this.label_WriteRead.Location = new System.Drawing.Point(477, 0);
            this.label_WriteRead.Name = "label_WriteRead";
            this.label_WriteRead.Size = new System.Drawing.Size(50, 30);
            this.label_WriteRead.TabIndex = 2;
            this.label_WriteRead.Text = "R/W";
            this.label_WriteRead.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tB_Write
            // 
            this.tB_Write.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tB_Write.Font = new System.Drawing.Font("新細明體", 14F);
            this.tB_Write.Location = new System.Drawing.Point(527, 0);
            this.tB_Write.Name = "tB_Write";
            this.tB_Write.Size = new System.Drawing.Size(116, 30);
            this.tB_Write.TabIndex = 3;
            this.tB_Write.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // button_Send
            // 
            this.button_Send.Font = new System.Drawing.Font("新細明體", 14F);
            this.button_Send.Location = new System.Drawing.Point(643, 0);
            this.button_Send.Name = "button_Send";
            this.button_Send.Size = new System.Drawing.Size(87, 30);
            this.button_Send.TabIndex = 4;
            this.button_Send.Text = "Write";
            this.button_Send.UseVisualStyleBackColor = true;
            this.button_Send.Click += new System.EventHandler(this.button_Send_Click);
            // 
            // label_Value
            // 
            this.label_Value.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_Value.Font = new System.Drawing.Font("新細明體", 14F);
            this.label_Value.Location = new System.Drawing.Point(358, 0);
            this.label_Value.Name = "label_Value";
            this.label_Value.Size = new System.Drawing.Size(120, 30);
            this.label_Value.TabIndex = 5;
            this.label_Value.Text = "Value";
            this.label_Value.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MIPCInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label_Value);
            this.Controls.Add(this.button_Send);
            this.Controls.Add(this.tB_Write);
            this.Controls.Add(this.label_WriteRead);
            this.Controls.Add(this.label_IPCName);
            this.Controls.Add(this.label_MIPCName);
            this.Name = "MIPCInfo";
            this.Size = new System.Drawing.Size(730, 30);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_MIPCName;
        private System.Windows.Forms.Label label_IPCName;
        private System.Windows.Forms.Label label_WriteRead;
        private System.Windows.Forms.TextBox tB_Write;
        private System.Windows.Forms.Button button_Send;
        private System.Windows.Forms.Label label_Value;
    }
}
