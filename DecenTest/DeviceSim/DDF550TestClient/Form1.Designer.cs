namespace DDF550TestClient
{
    partial class Form1
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.cboCmd = new System.Windows.Forms.ComboBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.btnDisConnect = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.numPort = new System.Windows.Forms.NumericUpDown();
            this.txtIp = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.txtRecvHex = new System.Windows.Forms.TextBox();
            this.txtRecvStr = new System.Windows.Forms.TextBox();
            this.txtSendHex = new System.Windows.Forms.TextBox();
            this.txtSendStr = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.cboCmd);
            this.splitContainer1.Panel1.Controls.Add(this.btnSend);
            this.splitContainer1.Panel1.Controls.Add(this.button4);
            this.splitContainer1.Panel1.Controls.Add(this.button3);
            this.splitContainer1.Panel1.Controls.Add(this.btnDisConnect);
            this.splitContainer1.Panel1.Controls.Add(this.btnConnect);
            this.splitContainer1.Panel1.Controls.Add(this.numPort);
            this.splitContainer1.Panel1.Controls.Add(this.txtIp);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel1);
            this.splitContainer1.Size = new System.Drawing.Size(793, 677);
            this.splitContainer1.SplitterDistance = 31;
            this.splitContainer1.TabIndex = 9;
            // 
            // cboCmd
            // 
            this.cboCmd.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCmd.FormattingEnabled = true;
            this.cboCmd.Location = new System.Drawing.Point(667, 5);
            this.cboCmd.Name = "cboCmd";
            this.cboCmd.Size = new System.Drawing.Size(121, 20);
            this.cboCmd.TabIndex = 18;
            this.cboCmd.SelectedIndexChanged += new System.EventHandler(this.cboCmd_SelectedIndexChanged);
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(424, 4);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(75, 23);
            this.btnSend.TabIndex = 17;
            this.btnSend.Text = "发送";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(586, 4);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 16;
            this.button4.Text = "AsciiToHex";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(505, 4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 15;
            this.button3.Text = "HexToAscii";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // btnDisConnect
            // 
            this.btnDisConnect.Location = new System.Drawing.Point(343, 4);
            this.btnDisConnect.Name = "btnDisConnect";
            this.btnDisConnect.Size = new System.Drawing.Size(75, 23);
            this.btnDisConnect.TabIndex = 14;
            this.btnDisConnect.Text = "断开";
            this.btnDisConnect.UseVisualStyleBackColor = true;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(262, 4);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 13;
            this.btnConnect.Text = "连接";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // numPort
            // 
            this.numPort.Location = new System.Drawing.Point(207, 6);
            this.numPort.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numPort.Name = "numPort";
            this.numPort.Size = new System.Drawing.Size(49, 21);
            this.numPort.TabIndex = 12;
            this.numPort.Value = new decimal(new int[] {
            5555,
            0,
            0,
            0});
            // 
            // txtIp
            // 
            this.txtIp.Location = new System.Drawing.Point(41, 6);
            this.txtIp.Name = "txtIp";
            this.txtIp.Size = new System.Drawing.Size(119, 21);
            this.txtIp.TabIndex = 11;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(166, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 10;
            this.label2.Text = "Port:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 12);
            this.label1.TabIndex = 9;
            this.label1.Text = "IP:";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.txtRecvHex, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtRecvStr, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtSendHex, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtSendStr, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(793, 642);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // txtRecvHex
            // 
            this.txtRecvHex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtRecvHex.Location = new System.Drawing.Point(399, 324);
            this.txtRecvHex.Multiline = true;
            this.txtRecvHex.Name = "txtRecvHex";
            this.txtRecvHex.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtRecvHex.Size = new System.Drawing.Size(391, 315);
            this.txtRecvHex.TabIndex = 3;
            // 
            // txtRecvStr
            // 
            this.txtRecvStr.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtRecvStr.Location = new System.Drawing.Point(3, 324);
            this.txtRecvStr.Multiline = true;
            this.txtRecvStr.Name = "txtRecvStr";
            this.txtRecvStr.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtRecvStr.Size = new System.Drawing.Size(390, 315);
            this.txtRecvStr.TabIndex = 2;
            // 
            // txtSendHex
            // 
            this.txtSendHex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSendHex.Location = new System.Drawing.Point(399, 3);
            this.txtSendHex.Multiline = true;
            this.txtSendHex.Name = "txtSendHex";
            this.txtSendHex.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSendHex.Size = new System.Drawing.Size(391, 315);
            this.txtSendHex.TabIndex = 1;
            // 
            // txtSendStr
            // 
            this.txtSendStr.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSendStr.Location = new System.Drawing.Point(3, 3);
            this.txtSendStr.Multiline = true;
            this.txtSendStr.Name = "txtSendStr";
            this.txtSendStr.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSendStr.Size = new System.Drawing.Size(390, 315);
            this.txtSendStr.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(793, 677);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button btnDisConnect;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.TextBox txtIp;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox txtRecvHex;
        private System.Windows.Forms.TextBox txtRecvStr;
        private System.Windows.Forms.TextBox txtSendHex;
        private System.Windows.Forms.TextBox txtSendStr;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.ComboBox cboCmd;
    }
}