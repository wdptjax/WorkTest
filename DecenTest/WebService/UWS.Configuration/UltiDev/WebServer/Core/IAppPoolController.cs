namespace UltiDev.WebServer.Core
{
    using System;
    using System.Runtime.InteropServices;
    using UWS.Configuration;

    [ComVisible(true), Guid("24953601-9480-4cd4-ae45-0212836cc7fc")]
    public interface IAppPoolController
    {
        void PrepareForUnload();
        void UnloadRemovedApps(ListenEndpoint[] appConfig);
        ListenEndpoint[] GetApplications();
        void StartNewAppsAndRestartChanged(ListenEndpoint[] appConfig, bool startHttpListener);
        void InitiateAppUnload(Guid appID);
        void UnloadApplicationImmediately(Guid appID);
        bool SuspendApplication(Guid appID);
        void StopHttpListerners();
        void StartHttpListeners();
        ApplicationStatus GetApplicationStatus(Guid appID);
        ApplicationStatus[] GetApplicationsStatuses(params Guid[] appIDs);
        bool ResumeApplication(Guid appID);
        bool RestartApplication(Guid appID);
        ProcessEnvironment GetEnvironmentInfo();
    }
}

