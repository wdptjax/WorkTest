namespace UltiDev.WebServer.Core
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using UWS.Configuration;

    public class RedirectorMonitor : IListenerMonitor
    {
        private volatile bool explicitlyUnloaded = false;
        private RedirectRequestDispatcher processor = null;

        public RedirectorMonitor(RedirectionConfigEntry config)
        {
            this.processor = new RedirectRequestDispatcher(config);
        }

        public void ForceStop()
        {
            if (!this.explicitlyUnloaded)
            {
                lock (this)
                {
                    this.explicitlyUnloaded = true;
                    this.processor.Stop();
                }
                try
                {
                    string format = string.Format("Stopped redirector to \"{0}\" (Configuration ID={1:B}).", this.processor.RedirectionConfig.RedirectToUrl, this.Configuration.ID);
                    UltiDev.WebServer.Core.Trace.TraceInformation(format, new object[0]);
                    if (!Environment.UserInteractive)
                    {
                        EventLog.WriteEntry(AppPoolController.EventSource, format);
                    }
                }
                catch
                {
                }
            }
        }

        public ApplicationStatus GetApplicationStatus()
        {
            return (this.IsHttpListenerSuspended ? ApplicationStatus.Suspended : ApplicationStatus.Running);
        }

        public void InitiateShutdown()
        {
            this.ForceStop();
        }

        public bool Resume()
        {
            if (!this.IsHttpListenerSuspended)
            {
                return false;
            }
            try
            {
                this.processor.Resume();
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
                this.explicitlyUnloaded = false;
                if (startHttpListener)
                {
                    string str;
                    try
                    {
                        this.Resume();
                        try
                        {
                            str = string.Format("Started redirector to \"{0}\" (Configuration ID={1:B}).\r\nEnvironment info: {2}", this.processor.RedirectionConfig.RedirectToUrl, this.Configuration.ID, AppPoolController.GetEnvironmentData());
                            UltiDev.WebServer.Core.Trace.TraceInformation(str, new object[0]);
                            if (!Environment.UserInteractive)
                            {
                                EventLog.WriteEntry(AppPoolController.EventSource, str);
                            }
                        }
                        catch
                        {
                        }
                    }
                    catch (Exception exception)
                    {
                        try
                        {
                            str = string.Format("Failed to start redirector to \"{0}\" (ID={1:B}).\r\nEnvironment info:\r\n{2}\r\n\r\nError:\r\n{3}", new object[] { this.processor.RedirectionConfig.RedirectToUrl, this.Configuration.ID, AppPoolController.GetEnvironmentData(), ListenerMonitorFactory.GetExceptionMessage(exception) });
                            UltiDev.WebServer.Core.Trace.TraceError(str, new object[0]);
                            if (!Environment.UserInteractive)
                            {
                                EventLog.WriteEntry(AppPoolController.EventSource, str, EventLogEntryType.Error);
                            }
                        }
                        catch
                        {
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        public bool Suspend()
        {
            if (this.IsHttpListenerSuspended)
            {
                return false;
            }
            try
            {
                this.processor.Suspend();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public ListenEndpoint Configuration
        {
            get
            {
                lock (this)
                {
                    return this.processor.RedirectionConfig;
                }
            }
        }

        public bool IsApplicationExplicitlyUnloaded
        {
            get
            {
                return this.explicitlyUnloaded;
            }
        }

        public bool IsHttpListenerSuspended
        {
            get
            {
                return this.processor.IsSuspended;
            }
        }
    }
}

