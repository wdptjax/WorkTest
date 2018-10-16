namespace UltiDev.WebServer.Core
{
    using System;
    using UWS.Configuration;

    public interface IListenerMonitor
    {
        void ForceStop();
        ApplicationStatus GetApplicationStatus();
        void InitiateShutdown();
        bool Resume();
        bool Start(bool startHttpListener);
        bool Suspend();

        ListenEndpoint Configuration { get; }

        bool IsApplicationExplicitlyUnloaded { get; }

        bool IsHttpListenerSuspended { get; }
    }
}

