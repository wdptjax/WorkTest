namespace UltiDev.WebServer.Core
{
    using System;
    using System.Diagnostics;
    using UWS.Configuration;

    public static class ListenerMonitorFactory
    {
        public static IListenerMonitor CreateListenerMonitor(ListenEndpoint config, TraceListener[] traceListeners, string exeFolderPath)
        {
            if (config == null)
            {
                throw new ArgumentNullException("ListenEndpoint config");
            }
            if (config is WebAppConfigEntry)
            {
                return new ApplicationMonitor((WebAppConfigEntry) config, traceListeners, exeFolderPath);
            }
            if (!(config is RedirectionConfigEntry))
            {
                throw new Exception(string.Format("Type \"{0}\" is unknown to ListenerMonitorFactory.", config.GetType().Name));
            }
            return new RedirectorMonitor((RedirectionConfigEntry) config);
        }

        public static string GetExceptionMessage(Exception ex)
        {
            return ex.ToString();
        }
    }
}

