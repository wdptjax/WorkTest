namespace UltiDev.WebServer.Core
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), Guid("2BE82EA0-0DD1-4563-AEA6-DA2F4666F39C")]
    public enum ApplicationStatus
    {
        Running,
        Suspended,
        NotFound
    }
}

