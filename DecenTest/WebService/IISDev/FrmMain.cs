using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using UltiDev.WebServer.Core;
using UWS.Configuration;
using UWS.Framework;

namespace IISDev
{
    public partial class FrmMain : Form
    {
        private static readonly string[] LOCALHOSTS = new string[] { "localhost", "[::1]", "127.0.0.1" };
        private string _dynamicFileExtensions = "aspx, asmx, svc, ashx, axd, xamlx, asax, cshtml, vbhtml, php, jsp, do, pl, cgi, py, asp, asa, cfm";
        private AppPoolController _theAppPool;
        private Guid _appId = Guid.Empty;

        private string _physicalPath = string.Empty;
        private string _virualPath = string.Empty;
        private ushort _listenPort = 0;

        public FrmMain()
        {
            InitializeComponent();
            string fullName = new FileInfo(typeof(FrmMain).Assembly.Location).Directory.FullName;
            if (!File.Exists(Path.Combine(fullName, "SystemFileExtMimeTypeMap.txt")))
            {
                fullName = SystemUtilites.UwsRoot;
            }
            TraceListener[] traceListeners = new TraceListener[] { new ConsoleTraceListener() };
            this._theAppPool = new AppPoolController(traceListeners, fullName);

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            txtPhyscialPath.Text = Properties.Settings.Default.PhysicalPath;
            txtVirualPath.Text = Properties.Settings.Default.VirtualPath;
            numUDPort.Value = Properties.Settings.Default.AppPort;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            _listenPort = (ushort)numUDPort.Value;
            List<int> usingPort = GetActivePorts();
            if(usingPort.Contains(_listenPort))
            {
                MessageBox.Show("端口:"+_listenPort+"已被使用,请更换端口重试!");
                return;
            }

            _physicalPath = txtPhyscialPath.Text;

            if (!txtVirualPath.Text.StartsWith("/"))
                txtVirualPath.Text = "/" + txtVirualPath.Text;

            _virualPath = txtVirualPath.Text;
            WebAppConfigEntry configItem = this.GetConfigItem(Guid.NewGuid());
            this._appId = configItem.ID;
            this._theAppPool.StartNewAppsAndRestartChanged(configItem.ToCollection(), true);
            if(_theAppPool.GetApplicationStatus(_appId) != ApplicationStatus.Running)
            {
                MessageBox.Show("应用程序启动失败!");
                return;
            }

            btnStart.Enabled = false;
            btnStop.Enabled = true;

            Properties.Settings.Default.PhysicalPath = _physicalPath;
            Properties.Settings.Default.VirtualPath = _virualPath;
            Properties.Settings.Default.AppPort = _listenPort;
            Properties.Settings.Default.Save();
            int index = 0;
            foreach (string url in configItem.GetMainListenUrls())
            {
                LinkLabel lbl = new LinkLabel();
                lbl.Text = url;
                lbl.Parent = pnlShowUrl;
                pnlShowUrl.Controls.Add(lbl);
                lbl.Left = 0;
                lbl.Top = lbl.Height * index + 2;
                lbl.Width = pnlShowUrl.Width - 10;
                lbl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                lbl.LinkClicked += Lbl_LinkClicked;
                index++;
            }
        }

        private void Lbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel lbl = sender as LinkLabel;
            Process.Start(lbl.Text);
            e.Link.Visited = true;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            this._theAppPool.UnloadApplicationImmediately(_appId);
            btnStart.Enabled = true;
            foreach (var control in pnlShowUrl.Controls)
            {
                if (control is LinkLabel)
                {
                    LinkLabel lbl = control as LinkLabel;
                    lbl.LinkClicked -= Lbl_LinkClicked;
                }
            }
            pnlShowUrl.Controls.Clear();
        }

        private void btnSelectPhyscialDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "选择WebService所在文件夹位置";
            if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                return;
            string str = folderBrowserDialog.SelectedPath;
            if (string.IsNullOrEmpty(str))
                return;
            txtPhyscialPath.Text = str;
        }

        #region 辅助方法
        /// <summary>
        /// 查询系统中正在使用的端口
        /// </summary>
        /// <returns></returns>
        private List<int> GetActivePorts()
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();
            if (ipEndPoints.Count() == 0) return new List<int>();
            return ipEndPoints.Select(i => i.Port).ToList();
        }

        private static void AddListenUrl(WebAppConfigEntry cfg, string hostOrIP, ushort portNumber)
        {
            string address = string.Format("http://{0}:{1}", hostOrIP, portNumber);
            cfg.ListenAddresses.Add(address);
        }

        private void SetHosts(WebAppConfigEntry cfg, ushort portNumber)
        {
            cfg.ListenAddresses.Clear();
            IPAddress[] hostAddresses = null;
            if (UltiDev.Framework.Interop.UacHelper.IsProcessElevated)
            {
                string defaultDomainName = SystemUtilites.GetDefaultDomainName();
                if (defaultDomainName != null)
                {
                    AddListenUrl(cfg, defaultDomainName, portNumber);
                    hostAddresses = Dns.GetHostAddresses(defaultDomainName);
                }
            }
            foreach (string str2 in LOCALHOSTS)
            {
                AddListenUrl(cfg, str2, portNumber);
            }
            if (hostAddresses != null)
            {
                foreach (IPAddress address in hostAddresses)
                {
                    AddListenUrl(cfg, SystemUtilites.IpAddressToString(address), portNumber);
                }
            }
        }
        public WebAppConfigEntry GetConfigItem(Guid? currentApp = new Guid?())
        {
            WebAppConfigEntry webAppEntry = UWS.Configuration.Metabase.GetWebAppEntry(Guid.Empty);
            if (currentApp.HasValue)
            {
                webAppEntry.ID = currentApp.Value;
            }
            webAppEntry.PhysicalDirectory = _physicalPath;
            webAppEntry.DefaultDocument = string.Empty;
            webAppEntry.VirtualDirectory = _virualPath;

            this.SetHosts(webAppEntry, _listenPort);

            webAppEntry.CompressResponseIfPossible = true;
            webAppEntry.AllowDirectoryListing = true;
            webAppEntry.BypassAppServerForStaticContent = true;
            webAppEntry.AuthenicationMode = AuthenticationSchemes.Anonymous;
            webAppEntry.BasicAuthAgainstWindows = false;
            webAppEntry.BasicAndDigestRealm = string.Empty;
            webAppEntry.ImpersonateWindowsIdentityForStaticContent = true;
            webAppEntry.DynamicContentFileExtensions.AddRange(this.DynamicFileExtensions);
            this.DynamicFileExtensions = webAppEntry.DynamicContentFileExtensions.ToArray();
            return webAppEntry;
        }

        [Browsable(false)]
        public string[] DynamicFileExtensions
        {
            get
            {
                List<string> list = new List<string>();
                foreach (string str in _dynamicFileExtensions.Split(new char[] { ',' }))
                {
                    string item = WebAppConfigEntry.MassageExtension(str);
                    if (item != null)
                    {
                        list.Add(item);
                    }
                }
                return list.ToArray();
            }
            set
            {
                UniqueList<string> list = new UniqueList<string>();
                list.AddRange(WebAppConfigEntry.defaultDynamicExtensions);
                if (value != null)
                {
                    list.AddRange(value);
                }
                StringBuilder builder = new StringBuilder();
                foreach (string str in list)
                {
                    string str2 = WebAppConfigEntry.MassageExtension(str);
                    if (str2 != null)
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append(", ");
                        }
                        builder.Append(str2);
                    }
                }
                _dynamicFileExtensions = builder.ToString();
            }
        }

        #endregion 辅助方法

    }
}
