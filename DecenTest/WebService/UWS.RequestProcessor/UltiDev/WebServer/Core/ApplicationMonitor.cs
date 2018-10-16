namespace UltiDev.WebServer.Core
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;
    using UWS.Configuration;

    public class ApplicationMonitor : MarshalByRefObject, IListenerMonitor
    {
        private WebAppDomain application = null;
        internal string exeFolderPath = null;
        private ManualResetEvent reloadingApp = new ManualResetEvent(false);
        private volatile bool restartAppDomainWhenUnloaded = true;
        internal readonly TraceListener[] tracers = null;

        internal ApplicationMonitor(WebAppConfigEntry appSettings, TraceListener[] traceListeners, string exeFolderPath)
        {
            this.AppSettings = appSettings;
            this.tracers = traceListeners;
            this.exeFolderPath = exeFolderPath;
        }

        private WebAppDomain CreateWorkerAppDomainWithHost()
        {
            string appId = "/UWS/" + this.AppSettings.ID.ToString("B").ToUpperInvariant();
            Type type = typeof(WebAppDomain);
            //return (WebAppDomain)ApplicationManager.GetApplicationManager().CreateObject(appId, type, this.AppSettings.VirtualDirectory, this.AppSettings.PhysicalDirectory, false);
            // 将依赖项加入应用程序域
            Type type1 = typeof(WebAppConfigEntry);
            CreateWorkerAppDomainWithHost(appId, this.AppSettings.VirtualDirectory, this.AppSettings.PhysicalDirectory, type1);
            // 反射得到WebAppDomain应用程序
            return (WebAppDomain)CreateWorkerAppDomainWithHost(appId, this.AppSettings.VirtualDirectory, this.AppSettings.PhysicalDirectory, type, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="virtualPath"></param>
        /// <param name="physicalPath"></param>
        /// <param name="hostType"></param>
        /// <param name="sign">true:需要返回实例对象;false:只需要加入应用程序域,不需要实例化对象</param>
        /// <returns></returns>
        static object CreateWorkerAppDomainWithHost(string appId, string virtualPath, string physicalPath, Type hostType, bool sign = false)
        {

            // 通过反射使用私有的 BuildManagerHost 类型
            // 这种方式创建 Web 应用程序域
            // 不需要程序集注册到 GAC 或者放置到 bin 文件夹

            //// 唯一的应用程序名
            //string uniqueAppString
            //    = string.Concat(virtualPath, physicalPath)
            //        .ToLowerInvariant();
            //// 获取唯一的 Id
            //string appId
            //    = (uniqueAppString.GetHashCode())
            //        .ToString("x", CultureInfo.InvariantCulture);
            // 在 Web 应用程序域中创建 BuildManagerHost
            var appManager = ApplicationManager.GetApplicationManager();
            // System.Web.Compilation.BuildManagerHost 是一个内部类
            // 不能在 MSDN 中查到
            var buildManagerHostType = typeof(HttpRuntime)
                .Assembly
                .GetType("System.Web.Compilation.BuildManagerHost");

            // 为应用程序域创建对象
            var buildManagerHost
                = appManager.CreateObject(
                    appId,
                    buildManagerHostType,
                    virtualPath,
                    physicalPath,
                    false
                    );
            // 调用 BuildManagerHost.RegisterAssembly 方法将类型注册到应用程序域
            buildManagerHostType.InvokeMember(
                "RegisterAssembly",
                BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic,
                null,
                buildManagerHost,
                    new object[2] {
             hostType.Assembly.FullName,
             hostType.Assembly.Location }
               );
            // 如果只是将依赖项加入应用程序域,不需要创建对象实例,则直接返回null即可
            if (!sign)
                return null;
            // 现在可以使用类型创建对象实例
            return appManager.CreateObject(
              appId,
              hostType,
              virtualPath,
              physicalPath,
              false);
        }
        public void ForceStop()
        {
            this.restartAppDomainWhenUnloaded = false;
            lock (this)
            {
                if (this.application != null)
                {
                    this.application.ForceStop();
                    this.application = null;
                }
            }
        }

        public ApplicationStatus GetApplicationStatus()
        {
            ApplicationStatus notFound;
            try
            {
                lock (this)
                {
                    if ((this.application == null) || this.application.IsSelfUnloaded())
                    {
                        return ApplicationStatus.NotFound;
                    }
                    notFound = this.application.IsSuspended ? ApplicationStatus.Suspended : ApplicationStatus.Running;
                }
            }
            catch
            {
                notFound = ApplicationStatus.NotFound;
            }
            return notFound;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void InitiateShutdown()
        {
            this.restartAppDomainWhenUnloaded = false;
            if (this.GetApplicationStatus() != ApplicationStatus.NotFound)
            {
                this.application.InitiateShutdown();
            }
        }

        public bool IsApplicationRunning()
        {
            return (this.GetApplicationStatus() == ApplicationStatus.Running);
        }

        public void OnAppDomainUnloaded(WebAppDomain unloadedApplication, ApplicationShutdownReason unloadReason, bool startHttpListener)
        {
            ApplicationMonitor monitor;
            UltiDev.WebServer.Core.Trace.TraceInformation("Application \"{0}\" (ID={1:B}, AppDomain ID={2:B}) is winding down due to {3}.", new object[] { this.AppSettings.ApplicationName, this.AppSettings.ID, unloadedApplication.AppDomainInstanceID, unloadReason });
            lock ((monitor = this))
            {
                if ((this.application == null) || (unloadedApplication.AppDomainInstanceID != this.application.AppDomainInstanceID))
                {
                    return;
                }
            }
            if (!this.restartAppDomainWhenUnloaded)
            {
                lock ((monitor = this))
                {
                    this.application = null;
                }
            }
            else
            {
                WebAppDomain application = null;
                lock ((monitor = this))
                {
                    UltiDev.WebServer.Core.Trace.TraceInformation("Restarting application \"{0}\" due to {1}, replacing AppDomain ID={2:B}.", new object[] { this.AppSettings.ApplicationName, unloadReason, unloadedApplication.AppDomainInstanceID });
                    application = this.application;
                    this.Start(false);
                    if (application != null)
                    {
                        application.Suspend();
                    }
                    if (startHttpListener)
                    {
                        this.Resume();
                    }
                }
                UltiDev.WebServer.Core.Trace.TraceInformation("Application \"{0}\" (ID={1:B}) has been restarted, replacing AppDomain ID={2:B}.", new object[] { this.AppSettings.ApplicationName, this.AppSettings.ID, unloadedApplication.AppDomainInstanceID });
                if ((application != null) && startHttpListener)
                {
                    application.WaitForRequestsInProgress();
                }
            }
        }

        public bool Resume()
        {
            if (this.GetApplicationStatus() != ApplicationStatus.Suspended)
            {
                return false;
            }
            try
            {
                this.application.Resume();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool Start(bool startHttpListener)
        {
            lock (this)
            {
                bool flag;
                this.restartAppDomainWhenUnloaded = true;
                WebAppDomain domain = null;
                int num = 1;
                do
                {
                    string str;
                    flag = false;
                    try
                    {
                        domain = this.CreateWorkerAppDomainWithHost();
                        try
                        {
                            str = string.Format("Created ASP.NET AppDomain (AppDomain ID={3:B}) for application \"{0}\" (Application ID={1:B}).\r\nEnvironment info: {2}", new object[] { this.AppSettings.ApplicationName, this.Configuration.ID, AppPoolController.GetEnvironmentData(), domain.AppDomainInstanceID });
                            UltiDev.WebServer.Core.Trace.TraceInformation(str, new object[0]);
                            if (!Environment.UserInteractive)
                            {
                                EventLog.WriteEntry(AppPoolController.EventSource, str);
                            }
                        }
                        catch
                        {
                        }
                        if (!domain.IsSelfUnloaded())
                        {
                            domain.Init(this, this.AppSettings, this.tracers, this.exeFolderPath, startHttpListener);
                        }
                    }
                    catch (Exception exception)
                    {
                        try
                        {
                            if (domain == null)
                            {
                                str = string.Format("Attempt {4} of {5} has failed to create ASP.NET AppDomain for application \"{0}\" (ID={1:B}).\r\nEnvironment info:\r\n{2}\r\n\r\nError:\r\n{3}", new object[] { this.AppSettings.ApplicationName, this.Configuration.ID, AppPoolController.GetEnvironmentData(), ListenerMonitorFactory.GetExceptionMessage(exception), num, 3 });
                            }
                            else
                            {
                                str = string.Format("Attempt {5} of {6} has failed to initialize ASP.NET AppDomain for application \"{0}\" (ID={1:B}, AppDomain ID={4:B}).\r\nEnvironment info:\r\n{2}\r\n\r\nError:\r\n{3}", new object[] { this.AppSettings.ApplicationName, this.Configuration.ID, AppPoolController.GetEnvironmentData(), ListenerMonitorFactory.GetExceptionMessage(exception), domain.AppDomainInstanceID, num, 3 });
                                if (!domain.IsSelfUnloaded())
                                {
                                    try
                                    {
                                        domain.ForceStop();
                                    }
                                    catch
                                    {
                                    }
                                    domain = null;
                                }
                            }
                            flag = num++ < 3;
                            UltiDev.WebServer.Core.Trace.TraceError(str, new object[0]);
                            if (!Environment.UserInteractive)
                            {
                                EventLog.WriteEntry(AppPoolController.EventSource, str, EventLogEntryType.Error);
                            }
                        }
                        catch
                        {
                        }
                    }
                    if (!flag)
                    {
                        flag = (domain != null) && domain.IsSelfUnloaded();
                    }
                }
                while (flag);
                this.application = domain;
                return (domain != null);
            }
        }

        public bool Suspend()
        {
            if (this.GetApplicationStatus() != ApplicationStatus.Running)
            {
                return false;
            }
            try
            {
                this.application.Suspend();
            }
            catch
            {
                return false;
            }
            return true;
        }

        internal WebAppConfigEntry AppSettings { get; set; }

        public ListenEndpoint Configuration
        {
            get
            {
                return this.AppSettings;
            }
        }

        public bool IsApplicationExplicitlyUnloaded
        {
            get
            {
                return (!this.restartAppDomainWhenUnloaded && (this.application == null));
            }
        }

        public bool IsHttpListenerSuspended
        {
            get
            {
                return this.application.IsSuspended;
            }
        }
    }
}

