using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestTDOA
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private bool _isSending = false;
        private int _sendSpan = 10;

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtSendSpan.Text, out _sendSpan))
            {
                MessageBox.Show("错误的发送间隔");
                return;
            }

            btnStart.Enabled = false;
            btnStop.Enabled = true;
            txtSendSpan.Enabled = false;

            _isSending = true;


            Task.Factory.StartNew(() =>
            {
                try
                {
                    using (Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 5555);
                        server.Bind(ipEndPoint);
                        server.Listen(1);
                        using (Socket server1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                        {
                            IPEndPoint ipEndPoint1 = new IPEndPoint(IPAddress.Any, 5565);
                            server1.Bind(ipEndPoint1);
                            server1.Listen(1);
                            bool result = true;
                            while (result)
                            {
                                try
                                {
                                    using (Socket socket = server1.Accept())
                                    {
                                        while (_isSending)
                                        {
                                            //例子 设置频率
                                            //Data.SetFrequency(101700000);
                                            byte[] buffer = Data.packet_bytes;
                                            socket.Send(buffer);

                                            Thread.Sleep(_sendSpan);
                                        }
                                    }
                                }
                                catch (Exception exx)
                                {
                                    result = MessageBox.Show("发生如下错误，是否继续监听？\r\n" + exx.ToString(), "错误", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1) == DialogResult.Yes;
                                }
                            }
                            this.Invoke(new Action(() => btnStop_Click(null, null)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Invoke(new Action(() => { MessageBox.Show(this, ex.ToString()); btnStop_Click(null, null); }));
                }
            });
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _isSending = false;
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            txtSendSpan.Enabled = true;
        }
    }
}
