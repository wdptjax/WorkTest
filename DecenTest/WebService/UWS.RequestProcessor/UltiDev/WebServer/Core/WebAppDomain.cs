namespace UltiDev.WebServer.Core
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;
    using UWS.Configuration;

    public class WebAppDomain : MarshalByRefObject, IRegisteredObject
    {
        private WebAppConfigEntry appConfigSettings = null;
        public readonly Guid AppDomainInstanceID = Guid.NewGuid();
        private const int AppDomainUnloadFinalRequestTimeoutSecond = 120;
        private volatile ApplicationMonitor appMonitor = null;
        private readonly string appVirtualDir;
        internal string exeFolderPath = null;
        private volatile bool forceStopped = false;
        private Thread keepRunningThread;
        private readonly string lowerCasedClientScriptPathWithTrailingSlash;
        private readonly string lowerCasedVirtualPath;
        private readonly string lowerCasedVirtualPathWithTrailingSlash;
        private readonly string physicalClientScriptPath;
        private readonly string physicalRootNoSlash;
        private ApplicationRequestDispatcher processor = null;
        private volatile bool selfUnloaded = false;
        private volatile bool shutdownInitiated = false;
        private ManualResetEvent stopKeepAliveThread = new ManualResetEvent(true);

        public WebAppDomain()
        {
            HostingEnvironment.RegisterObject(this);
            this.appVirtualDir = AspNetRequest.UrlDecodeNoQueryString(HttpRuntime.AppDomainAppVirtualPath);
            this.lowerCasedVirtualPath = this.appVirtualDir.ToLowerInvariant().TrimEnd(new char[] { '/' });
            this.lowerCasedVirtualPathWithTrailingSlash = this.lowerCasedVirtualPath + '/';
            this.physicalRootNoSlash = HttpRuntime.AppDomainAppPath.TrimEnd(new char[] { '\\' });
            this.lowerCasedClientScriptPathWithTrailingSlash = (HttpRuntime.AspClientScriptVirtualPath + "/").ToLowerInvariant();
            this.physicalClientScriptPath = HttpRuntime.AspClientScriptPhysicalPath + @"\";
            if (!Directory.Exists(this.physicalClientScriptPath))
            {
                this.physicalClientScriptPath = this.physicalClientScriptPath.Replace(@"64\", @"\");
                if (!Directory.Exists(this.physicalClientScriptPath))
                {
                    this.physicalClientScriptPath = HttpRuntime.AspClientScriptPhysicalPath + @"\";
                    UltiDev.WebServer.Core.Trace.TraceWarning("Folder \"{0}\" not found. Some applications (mostly relying on Crystal Reports) may not work properly.", new object[] { this.physicalClientScriptPath });
                }
            }
            this.keepRunningThread = new Thread(new ThreadStart(this.PingTheAppLoop));
            this.keepRunningThread.IsBackground = true;
            this.keepRunningThread.Priority = ThreadPriority.BelowNormal;
        }

        public void ForceStop()
        {
            if (!this.forceStopped)
            {
                this.forceStopped = true;
                this.InitiateShutdown();
                HostingEnvironment.UnregisterObject(this);
            }
        }

        public DateTime Heartbeat()
        {
            return DateTime.UtcNow;
        }

        public void Init(ApplicationMonitor monitor, WebAppConfigEntry appConfiguration, TraceListener[] traceListeners, string exeFolderPath, bool startHttpListener)
        {
            lock (this)
            {
                this.appMonitor = monitor;
                this.appConfigSettings = appConfiguration;
                this.exeFolderPath = exeFolderPath;
                if (traceListeners != null)
                {
                    UltiDev.WebServer.Core.Trace.Listeners.AddRange(traceListeners);
                }
                if (this.selfUnloaded)
                {
                    UltiDev.WebServer.Core.Trace.TraceWarning("\"{0}\" (AppDomain instanceID={1:B}) has been unloaded before it was started.", new object[] { AppDomain.CurrentDomain.BaseDirectory, this.AppDomainInstanceID });
                    monitor.OnAppDomainUnloaded(this, ApplicationShutdownReason.None, startHttpListener);
                }
                else
                {
                    this.processor = new ApplicationRequestDispatcher(this);
                    UltiDev.WebServer.Core.Trace.TraceInformation("Starting accepting requests for \"{0}\" (AppDomain ID={1:B}).", new object[] { AppDomain.CurrentDomain.BaseDirectory, this.AppDomainInstanceID });
                    if (startHttpListener)
                    {
                        this.Resume();
                    }
                }
                this.AppInstanceID = this.AppSettings.ID.ToString().ToUpper();
                this.AppInstanceMetaPath = "UWS/" + this.AppInstanceID;
                this.AppMdPath = string.Format("{0}/ROOT/{1}", this.AppInstanceMetaPath, this.AppSettings.VirtualDirectory.Trim(new char[] { '/' }));
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void InitiateShutdown()
        {
            this.stopKeepAliveThread.Set();
            if (this.appMonitor != null)
            {
                this.appMonitor = null;
            }
            if (this.processor != null)
            {
                this.processor.Stop();
                this.processor = null;
            }
            if (!this.shutdownInitiated)
            {
                this.shutdownInitiated = true;
                HostingEnvironment.InitiateShutdown();
            }
        }

        public bool IsAspNetClientScriptRequest(string path)
        {
            return path.StartsWith(this.lowerCasedClientScriptPathWithTrailingSlash, StringComparison.Ordinal);
        }

        public bool IsSelfUnloaded()
        {
            return this.selfUnloaded;
        }

        public bool IsVirtualPathAppPath(string path)
        {
            if (path == null)
            {
                return false;
            }
            path = CultureInfo.InvariantCulture.TextInfo.ToLower(path);
            return ((path == this.lowerCasedVirtualPath) || (path == this.lowerCasedVirtualPathWithTrailingSlash));
        }

        public bool IsVirtualPathInApp(string path)
        {
            bool flag;
            return this.IsVirtualPathInApp(path, out flag);
        }

        public bool IsVirtualPathInApp(string path, out bool isClientScriptPath)
        {
            isClientScriptPath = false;
            if (path != null)
            {
                if ((this.appVirtualDir == "/") && path.StartsWith("/", StringComparison.Ordinal))
                {
                    isClientScriptPath = this.IsAspNetClientScriptRequest(path);
                    return true;
                }
                path = CultureInfo.InvariantCulture.TextInfo.ToLower(path);
                if (path.StartsWith(this.lowerCasedVirtualPathWithTrailingSlash, StringComparison.Ordinal))
                {
                    return true;
                }
                if (path == this.lowerCasedVirtualPath)
                {
                    return true;
                }
                if (path.StartsWith(this.lowerCasedClientScriptPathWithTrailingSlash, StringComparison.Ordinal))
                {
                    isClientScriptPath = true;
                    return true;
                }
            }
            return false;
        }

        public string MapPath(string path)
        {
            string physicalRootNoSlash;
            path = AspNetRequest.UrlDecodeNoQueryString(path);
            if (string.IsNullOrEmpty(path) || path.Equals("/"))
            {
                if (this.appVirtualDir == "/")
                {
                    physicalRootNoSlash = this.physicalRootNoSlash;
                }
                else
                {
                    physicalRootNoSlash = Environment.SystemDirectory;
                }
            }
            else if (this.IsVirtualPathAppPath(path))
            {
                physicalRootNoSlash = this.physicalRootNoSlash;
            }
            else
            {
                bool flag;
                if (this.IsVirtualPathInApp(path, out flag))
                {
                    if (flag)
                    {
                        physicalRootNoSlash = Path.Combine(this.PhysicalClientScriptPath, path.Substring(this.NormalizedClientScriptPath.Length));
                    }
                    else
                    {
                        physicalRootNoSlash = Path.Combine(this.PhysicalPath, path.Substring(this.NormalizedVirtualPath.Length));
                    }
                }
                else if (path.StartsWith("/", StringComparison.Ordinal))
                {
                    physicalRootNoSlash = Path.Combine(this.PhysicalPath, path.Substring(1));
                }
                else
                {
                    physicalRootNoSlash = Path.Combine(this.PhysicalPath, path);
                }
            }
            physicalRootNoSlash = physicalRootNoSlash.Replace('/', '\\');
            if (!(!physicalRootNoSlash.EndsWith(@"\", StringComparison.Ordinal) || physicalRootNoSlash.EndsWith(@":\", StringComparison.Ordinal)))
            {
                physicalRootNoSlash = physicalRootNoSlash.Substring(0, physicalRootNoSlash.Length - 1);
            }
            return physicalRootNoSlash;
        }

        private void PingTheAppLoop()
        {
            this.stopKeepAliveThread.Reset();
            do
            {
                WebAppConfigEntry appSettings = this.AppSettings;
                if ((appSettings != null) && !string.IsNullOrEmpty(appSettings.RelativePingPath))
                {
                    string pingUrl = appSettings.GetPingUrl(false);
                    if (pingUrl != null)
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(WebAppDomain.SendPingRequest), pingUrl);
                    }
                }
            }
            while (!this.stopKeepAliveThread.WaitOne(0xea60, false));
        }

        public void Resume()
        {
            this.processor.Resume();
            this.StartKeepAliveThreadIfNecessary();
        }

        private static void SendPingRequest(object urlObj)
        {
            string requestUriString = (string) urlObj;
            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(requestUriString);
                request.KeepAlive = false;
                request.CookieContainer = new CookieContainer();
                request.Timeout = 0x2710;
                request.UserAgent = "UltiDev Web Server application wake-up caller 2.0";
                UltiDev.WebServer.Core.Trace.TraceInformation("Sending \"application wakup-up\" request to \"{0}\"...", new object[] { requestUriString });
                using (request.GetResponse())
                {
                    UltiDev.WebServer.Core.Trace.TraceInformation("Received response for {0}", new object[] { request.RequestUri });
                }
            }
            catch (Exception exception)
            {
                UltiDev.WebServer.Core.Trace.TraceWarning("Failed while trying to send wake-up request to \"{0}\" due to \"{1}\".", new object[] { requestUriString, exception.GetBaseException().Message });
            }
        }

        private void StartKeepAliveThreadIfNecessary()
        {
            WebAppConfigEntry appSettings = this.AppSettings;
            if (((appSettings != null) && !string.IsNullOrEmpty(appSettings.RelativePingPath)) && !string.IsNullOrEmpty(appSettings.RelativePingPath))
            {
                this.keepRunningThread.Name = string.Format("App \"Keep Running\" ping thread for \"{0}\", ID={1:B}", appSettings.ApplicationName, appSettings.ID);
                this.keepRunningThread.Start();
            }
        }

        public void Suspend()
        {
            this.stopKeepAliveThread.Set();
            if (this.processor != null)
            {
                this.processor.Suspend();
            }
        }

        void IRegisteredObject.Stop(bool immediate)
        {
            lock (this)
            {
                this.selfUnloaded = true;
                this.stopKeepAliveThread.Set();
                bool startHttpListener = !this.IsSuspended;
                if (!this.shutdownInitiated)
                {
                    this.shutdownInitiated = true;
                    if (this.appMonitor != null)
                    {
                        this.appMonitor.OnAppDomainUnloaded(this, HostingEnvironment.ShutdownReason, startHttpListener);
                    }
                    else
                    {
                        this.Suspend();
                    }
                }
                if (immediate)
                {
                    Debug.Assert(this.selfUnloaded);
                    UltiDev.WebServer.Core.Trace.TraceInformation(string.Format("ASP.NET AppDomain for \"{0}\" (AppDomain ID={1:B}) is getting shut down because of {2}.", AppDomain.CurrentDomain.BaseDirectory, this.AppDomainInstanceID, HostingEnvironment.ShutdownReason), new object[0]);
                    this.ForceStop();
                }
            }
        }

        public bool WaitForRequestsInProgress()
        {
            return ((this.processor == null) || this.processor.noRequestsInProgress.WaitOne(0x1d4c0, false));
        }

        public string AppInstanceID { get; set; }

        public string AppInstanceMetaPath { get; set; }

        public string AppMdPath { get; set; }

        internal WebAppConfigEntry AppSettings
        {
            get
            {
                return this.appConfigSettings;
            }
        }

        public Guid ChangeID
        {
            get
            {
                return this.appConfigSettings.ChangeID;
            }
        }

        public bool IsSuspended
        {
            get
            {
                return ((this.processor == null) || this.processor.IsSuspended);
            }
        }

        public string NormalizedClientScriptPath
        {
            get
            {
                return this.lowerCasedClientScriptPathWithTrailingSlash;
            }
        }

        public string NormalizedVirtualPath
        {
            get
            {
                return this.lowerCasedVirtualPathWithTrailingSlash;
            }
        }

        public string PhysicalClientScriptPath
        {
            get
            {
                return this.physicalClientScriptPath;
            }
        }

        public string PhysicalPath
        {
            get
            {
                return this.physicalRootNoSlash;
            }
        }

        public string VirtualPath
        {
            get
            {
                return this.appVirtualDir;
            }
        }
    }
}

