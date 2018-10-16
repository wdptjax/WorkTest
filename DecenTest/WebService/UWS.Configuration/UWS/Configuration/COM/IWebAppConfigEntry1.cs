namespace UWS.Configuration.COM
{
    using System;
    using System.Runtime.InteropServices;
    using UWS.Configuration;

    [Guid("F110BDBD-6F99-41E0-900F-C63FB720D32C"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IWebAppConfigEntry1 : IListenEndpoint1
    {
        string ApplicationName { get; set; }
        string ApplicationDescription { get; set; }
        string PhysicalDirectory { get; set; }
        string DefaultDocument { get; set; }
        IComCompatibleCollection DynamicContentFileExtensions { get; }
        bool AllowDirectoryListing { get; set; }
        bool BypassAppServerForStaticContent { get; set; }
        AuthSchemes AuthenicationMode { get; set; }
        bool AuthenticationAgainstWindows { get; }
        bool BasicAuthAgainstWindows { get; set; }
        string BasicAndDigestRealm { get; set; }
        bool ImpersonateWindowsIdentityForStaticContent { get; set; }
        bool CompressResponseIfPossible { get; set; }
        IComCompatibleCollection ProhibitedAspNetFolders { get; }
        bool AllowSavingToUninstalledAppSection { get; set; }
        ApplicationType AppType { get; set; }
        string VerifyCanRegister();
        string HandlerPath { get; set; }
        IListenEndpoint1 Endpoint { get; }
        IComCompatibleCollection RegisteredShortcuts { get; }
    }
}

