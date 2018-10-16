namespace UWS.Configuration.COM
{
    using System;
    using System.Runtime.InteropServices;
    using UWS.Configuration;
    using UWS.Framework;

    [ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("00454F15-7D7C-4FB6-A703-E8F063F80484")]
    public interface IMetabaseComServer1
    {
        IWebAppConfigEntry1 GetWebAppEntry(string applicationGUID, bool includeUnisntalledInSearch);
        bool IsApplicationInstalled(string applicationGUID);
        bool IsApplicationUninstalled(string applicationGUID);
        void UnregisterApplication(string applicationGUID);
        void RegisterApplication(RuntimeVersion aspNetVersion, bool run32bitOnx64, bool useDefaultSharedHostProcess, ProcessIdentity processIdentity, IWebAppConfigEntry1 application);
        void RegisterApplicationDisp(RuntimeVersion aspNetVersion, bool run32bitOnx64, bool useDefaultSharedHostProcess, ProcessIdentity processIdentity, object application);
        void RegisterApplication1(RuntimeVersion aspNetVersion, bool run32bitOnx64, ProcessIdentity hostUserIdentity, IWebAppConfigEntry1 application);
        void RegisterApplication1Disp(RuntimeVersion aspNetVersion, bool run32bitOnx64, ProcessIdentity hostUserIdentity, object application);
        void RegisterApplication2(RuntimeVersion aspNetVersion, bool run32bitOnx64, IWebAppConfigEntry1 application);
        void RegisterApplication2Disp(RuntimeVersion aspNetVersion, bool run32bitOnx64, object application);
        IRedirectionConfigEntry1 GetRedirectionEntry(string redirectionGUID);
        void RegisterRedirection(IRedirectionConfigEntry1 redirection);
        void RegisterRedirectionDisp(object redirection);
        void UnregisterRedirection(string redirectionGUID);
        int GetRegisteredApplicationCount();
        bool HasRegisteredApplications();
    }
}

