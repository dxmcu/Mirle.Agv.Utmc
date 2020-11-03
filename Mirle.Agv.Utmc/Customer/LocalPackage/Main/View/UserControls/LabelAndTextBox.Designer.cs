namespace Mirle.Agv.INX.View
{
    partial class LabelAndTextBox
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
            this.label_Name = new System.Windows.Forms.Label();
            this.tB_Value = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label_Name
            // 
            this.label_Name.Font = new System.Drawing.Font("新細明體", 14F);
            this.label_Name.Location = new System.Drawing.Point(2, 0);
            this.label_Name.Name = "label_Name";
            this.label_Name.Size = new System.Drawing.Size(140, 30);
            this.label_Name.TabIndex = 0;
            this.label_Name.Text = "label1";
            this.label_Name.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tB_Value
            // 
            this.tB_Value.Font = new System.Drawing.Font("新細明體", 14F);
            this.tB_Value.Location = new System.Drawing.Point(140, 0);
            this.tB_Value.Name = "tB_Value";
            this.tB_Value.Size = new System.Drawing.Size(140, 30);
            this.tB_Value.TabIndex = 1;
            this.tB_Value.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // LabelAndTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tB_Value);
            this.Controls.Add(this.label_Name);
            this.Name = "LabelAndTextBox";
            this.Size = new System.Drawing.Size(280, 30);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_Name;
        private System.Windows.Forms.TextBox tB_Value;
    }
}
