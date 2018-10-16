namespace UWS.Configuration.COM
{
    using System;
    using System.Runtime.InteropServices;

    [InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("C95709E3-28F9-41E5-8329-D4B3726BB7B5"), ComVisible(true)]
    public interface IListenEndpoint1
    {
        string ID { get; set; }
        string VirtualDirectory { get; set; }
        IComCompatibleCollection ListenAddresses { get; }
        string UIApplicationName { get; }
    }
}

