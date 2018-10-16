namespace UltiDev.WebServer.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Security.Principal;
    using System.Threading;
    using UWS.Configuration;

    public class AppPoolController : IAppPoolController
    {
        private readonly Dictionary<Guid, IListenerMonitor> applications = new Dictionary<Guid, IListenerMonitor>();
        public static readonly string EventSource = "UWS.RequestProcessor";
        private readonly string exeFolderPath = null;
        private volatile bool processRetired = false;
        private readonly TraceListener[] tracers;

        public AppPoolController(TraceListener[] traceListeners, string exeFolderPath)
        {
            this.tracers = traceListeners;
            this.exeFolderPath = exeFolderPath;
        }

        private IListenerMonitor GetApplicationMonitor(Guid appID)
        {
            IListenerMonitor monitor = null;
            lock (this.applications)
            {
                this.applications.TryGetValue(appID, out monitor);
            }
            return monitor;
        }

        public ListenEndpoint[] GetApplications()
        {
            lock (this.applications)
            {
                ListenEndpoint[] endpointArray = new ListenEndpoint[this.applications.Count];
                int num = 0;
                foreach (IListenerMonitor monitor in this.applications.Values)
                {
                    endpointArray[num++] = monitor.Configuration;
                }
                return endpointArray;
            }
        }

        public ApplicationStatus[] GetApplicationsStatuses(params Guid[] appIDs)
        {
            if (appIDs == null)
            {
                throw new ArgumentNullException("appIDs");
            }
            ApplicationStatus[] statusArray = new ApplicationStatus[appIDs.Length];
            for (int i = 0; i < appIDs.Length; i++)
            {
                IListenerMonitor applicationMonitor = this.GetApplicationMonitor(appIDs[i]);
                if (applicationMonitor == null)
                {
                    statusArray[i] = ApplicationStatus.NotFound;
                }
                else
                {
                    statusArray[i] = applicationMonitor.GetApplicationStatus();
                }
            }
            return statusArray;
        }

        public ApplicationStatus GetApplicationStatus(Guid appID)
        {
            IListenerMonitor applicationMonitor = this.GetApplicationMonitor(appID);
            if (applicationMonitor == null)
            {
                return ApplicationStatus.NotFound;
            }
            return applicationMonitor.GetApplicationStatus();
        }

        public static ProcessEnvironment GetEnvironmentData()
        {
            int num;
            int num2;
            Process currentProcess = Process.GetCurrentProcess();
            ProcessEnvironment environment = new ProcessEnvironment {
                ClrVersion = Environment.Version.ToString(),
                CommandLine = Environment.CommandLine,
                ProcessBitness = IntPtr.Size * 8,
                ProcessID = currentProcess.Id,
                ProcessUserName = WindowsIdentity.GetCurrent().Name,
                ProcessStartTime = new DateTime?(currentProcess.StartTime)
            };
            ThreadPool.GetMaxThreads(out environment.MaxRequestThreads, out environment.MaxIoThreads);
            ThreadPool.GetAvailableThreads(out num, out num2);
            environment.UsedRequestThreads = environment.MaxRequestThreads - num;
            environment.UsedIoThreads = environment.MaxIoThreads - num2;
            return environment;
        }

        public ProcessEnvironment GetEnvironmentInfo()
        {
            ProcessEnvironment environmentData = GetEnvironmentData();
            if (this.processRetired)
            {
                environmentData.HostProcessRetired = true;
            }
            return environmentData;
        }

        public void InitiateAppUnload(Guid appID)
        {
            lock (this.applications)
            {
                IListenerMonitor applicationMonitor = this.GetApplicationMonitor(appID);
                if (applicationMonitor != null)
                {
                    applicationMonitor.InitiateShutdown();
                    this.applications.Remove(appID);
                }
            }
        }

        public bool IsReadyToExit()
        {
            if (!this.processRetired)
            {
                return false;
            }
            lock (this.applications)
            {
                foreach (IListenerMonitor monitor in this.applications.Values)
                {
                    if (!monitor.IsApplicationExplicitlyUnloaded)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void PrepareForUnload()
        {
            lock (this.applications)
            {
                List<Guid> list = new List<Guid>(this.applications.Keys);
                foreach (Guid guid in list)
                {
                    this.InitiateAppUnload(guid);
                }
                this.processRetired = true;
            }
        }

        public bool RestartApplication(Guid appID)
        {
            IListenerMonitor applicationMonitor = this.GetApplicationMonitor(appID);
            if (applicationMonitor == null)
            {
                return false;
            }
            return this.RestartApplicationInternal(applicationMonitor.Configuration, applicationMonitor, !applicationMonitor.IsHttpListenerSuspended);
        }

        private bool RestartApplicationInternal(ListenEndpoint newListenerEntry, IListenerMonitor appMonintor, bool startHttpListener)
        {
            lock (this.applications)
            {
                if (appMonintor == null)
                {
                    appMonintor = this.GetApplicationMonitor(newListenerEntry.ID);
                }
                if (appMonintor == null)
                {
                    return false;
                }
                appMonintor.InitiateShutdown();
                appMonintor = ListenerMonitorFactory.CreateListenerMonitor(newListenerEntry, this.tracers, this.exeFolderPath);
                this.applications[newListenerEntry.ID] = appMonintor;
            }
            appMonintor.Start(startHttpListener);
            return true;
        }

        public bool ResumeApplication(Guid appID)
        {
            IListenerMonitor applicationMonitor = this.GetApplicationMonitor(appID);
            if (applicationMonitor == null)
            {
                return false;
            }
            return applicationMonitor.Resume();
        }

        public void StartHttpListeners()
        {
            lock (this.applications)
            {
                foreach (IListenerMonitor monitor in this.applications.Values)
                {
                    monitor.Resume();
                }
            }
        }

        public void StartNewAppsAndRestartChanged(ListenEndpoint[] newAppsConfig, bool startHttpListener)
        {
            lock (this.applications)
            {
                foreach (ListenEndpoint endpoint in newAppsConfig)
                {
                    IListenerMonitor applicationMonitor = this.GetApplicationMonitor(endpoint.ID);
                    if (applicationMonitor == null)
                    {
                        applicationMonitor = ListenerMonitorFactory.CreateListenerMonitor(endpoint, this.tracers, this.exeFolderPath);
                        this.applications.Add(endpoint.ID, applicationMonitor);
                        applicationMonitor.Start(startHttpListener);
                    }
                    else if (endpoint.ChangeID != applicationMonitor.Configuration.ChangeID)
                    {
                        this.RestartApplicationInternal(endpoint, applicationMonitor, startHttpListener);
                    }
                }
            }
        }

        public void StopHttpListerners()
        {
            lock (this.applications)
            {
                foreach (IListenerMonitor monitor in this.applications.Values)
                {
                    monitor.Suspend();
                }
            }
        }

        public bool SuspendApplication(Guid appID)
        {
            IListenerMonitor applicationMonitor = this.GetApplicationMonitor(appID);
            if (applicationMonitor == null)
            {
                return false;
            }
            return applicationMonitor.Suspend();
        }

        public void UnloadApplicationImmediately(Guid appID)
        {
            IListenerMonitor applicationMonitor = this.GetApplicationMonitor(appID);
            if (applicationMonitor != null)
            {
                applicationMonitor.ForceStop();
            }
        }

        public void UnloadRemovedApps(ListenEndpoint[] newAppsConfig)
        {
            List<ListenEndpoint> list = new List<ListenEndpoint>();
            lock (this.applications)
            {
                foreach (Guid guid in this.applications.Keys)
                {
                    bool flag = false;
                    foreach (ListenEndpoint endpoint in newAppsConfig)
                    {
                        if (endpoint.ID == guid)
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        list.Add(this.GetApplicationMonitor(guid).Configuration);
                    }
                }
                foreach (ListenEndpoint endpoint2 in list)
                {
                    this.InitiateAppUnload(endpoint2.ID);
                }
            }
        }
    }
}

