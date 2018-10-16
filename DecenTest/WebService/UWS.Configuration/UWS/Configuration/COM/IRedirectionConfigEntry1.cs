namespace UWS.Configuration.COM
{
    using System;
    using System.Runtime.InteropServices;
    using UWS.Configuration;

    [Guid("8994E0C7-9B48-4E01-9B18-1A7F51EDEEA6"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IRedirectionConfigEntry1 : IListenEndpoint1
    {
        string RedirectToUrl { get; set; }
        RedirectCode RedirectionCode { get; set; }
        bool RedirectAllToOne { get; set; }
        string GetRedirectionUrlFor(string sourceUrl);
        IListenEndpoint1 Endpoint { get; }
    }
}

