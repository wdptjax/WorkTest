namespace IISDev
{
    partial class FrmMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPhyscialPath = new System.Windows.Forms.TextBox();
            this.btnSelectPhyscialDir = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtVirualPath = new System.Windows.Forms.TextBox();
            this.numUDPort = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.pnlShowUrl = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.numUDPort)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnStart.Location = new System.Drawing.Point(12, 310);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "启动";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStop.Location = new System.Drawing.Point(213, 310);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "停止";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "设置应用程序位置";
            // 
            // txtPhyscialPath
            // 
            this.txtPhyscialPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPhyscialPath.Location = new System.Drawing.Point(12, 24);
            this.txtPhyscialPath.Name = "txtPhyscialPath";
            this.txtPhyscialPath.Size = new System.Drawing.Size(238, 21);
            this.txtPhyscialPath.TabIndex = 3;
            // 
            // btnSelectPhyscialDir
            // 
            this.btnSelectPhyscialDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectPhyscialDir.Location = new System.Drawing.Point(256, 22);
            this.btnSelectPhyscialDir.Name = "btnSelectPhyscialDir";
            this.btnSelectPhyscialDir.Size = new System.Drawing.Size(32, 23);
            this.btnSelectPhyscialDir.TabIndex = 4;
            this.btnSelectPhyscialDir.Text = "...";
            this.btnSelectPhyscialDir.UseVisualStyleBackColor = true;
            this.btnSelectPhyscialDir.Click += new System.EventHandler(this.btnSelectPhyscialDir_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "虚拟路径:";
            // 
            // txtVirualPath
            // 
            this.txtVirualPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtVirualPath.Location = new System.Drawing.Point(77, 56);
            this.txtVirualPath.Name = "txtVirualPath";
            this.txtVirualPath.Size = new System.Drawing.Size(211, 21);
            this.txtVirualPath.TabIndex = 6;
            // 
            // numUDPort
            // 
            this.numUDPort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.numUDPort.Location = new System.Drawing.Point(77, 83);
            this.numUDPort.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.numUDPort.Name = "numUDPort";
            this.numUDPort.Size = new System.Drawing.Size(90, 21);
            this.numUDPort.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "端    口:";
            // 
            // pnlShowUrl
            // 
            this.pnlShowUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlShowUrl.AutoScroll = true;
            this.pnlShowUrl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlShowUrl.Location = new System.Drawing.Point(12, 110);
            this.pnlShowUrl.Name = "pnlShowUrl";
            this.pnlShowUrl.Size = new System.Drawing.Size(276, 194);
            this.pnlShowUrl.TabIndex = 9;
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 345);
            this.Controls.Add(this.pnlShowUrl);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numUDPort);
            this.Controls.Add(this.txtVirualPath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnSelectPhyscialDir);
            this.Controls.Add(this.txtPhyscialPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WebService承载程序";
            ((System.ComponentModel.ISupportInitialize)(this.numUDPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPhyscialPath;
        private System.Windows.Forms.Button btnSelectPhyscialDir;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtVirualPath;
        private System.Windows.Forms.NumericUpDown numUDPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel pnlShowUrl;
    }
}

