namespace NIMDemo
{
    partial class RtsForm
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
            if (disposing)
            {
                _peerDrawingGraphics.Dispose();
                _myDrawingGraphics.Dispose();
                _myPen.Dispose();
                _peerPen.Dispose();
                _sendDataTimer.Dispose();
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.promptLabel = new System.Windows.Forms.Label();
            this.promptLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.audioCtrlBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(260, 475);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "上一步";
            this.button1.UseVisualStyleBackColor = true;

            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(175, 475);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 0;
            this.button2.Text = "全部清空";
            this.button2.UseVisualStyleBackColor = true;

            // 
            // promptLabel
            // 
            this.promptLabel.Name = "promptLabel";
            this.promptLabel.Size = new System.Drawing.Size(400, 30);
            this.promptLabel.TabIndex = 0;
            this.promptLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.promptLabel.Text = "开始会话";
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(37, 34);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(433, 401);
            this.panel1.TabIndex = 1;

            // 
            // audioBtn
            // 
            this.audioCtrlBtn.Location = new System.Drawing.Point(80, 475);
            this.audioCtrlBtn.Name = "audioCtrlBtn";
            this.audioCtrlBtn.Size = new System.Drawing.Size(80, 23);
            this.audioCtrlBtn.TabIndex = 0;
            this.audioCtrlBtn.Text = "开启麦克风";
            this.audioCtrlBtn.UseVisualStyleBackColor = true;
            this.audioCtrlBtn.Tag = 0;

            // 
            // RtsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 510);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.promptLabel);
            this.Controls.Add(this.audioCtrlBtn);
            this.Name = "RtsForm";
            this.Text = "白板演示";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label promptLabel;
        private System.Windows.Forms.Button audioCtrlBtn;
    }
}