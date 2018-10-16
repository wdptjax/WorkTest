namespace UWS.Configuration.COM
{
    using System;
    using System.Runtime.InteropServices;
    using UWS.Configuration;
    using UWS.Framework;

    [ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("ADE7C3AA-0C44-4B3D-BA54-1270C0330AE0")]
    public interface IMetabaseComServer2 : IMetabaseComServer1
    {
        IAppShortcut1 GetShortcut(IWebAppConfigEntry1 app, string optionalShortcutLocation = null, string optionalUrlHost = null, string optionalPathAndQueryString = null);
        IAppShortcut1 GetShortcutDisp(object app, string optionalShortcutLocation = null, string optionalUrlHost = null, string optionalPathAndQueryString = null);
        IMetabaseComServer1 Base { get; }
        void RegisterApplicationWithShortcuts(RuntimeVersion aspNetVersion, bool run32bitOnx64, bool useDefaultSharedHostProcess, ProcessIdentity processIdentity, IWebAppConfigEntry1 application, IAppShortcut1 shortcut1, IAppShortcut1 shortcut2, IAppShortcut1 shortcut3);
        void RegisterApplicationWithShortcutsDisp(RuntimeVersion aspNetVersion, bool run32bitOnx64, bool useDefaultSharedHostProcess, ProcessIdentity processIdentity, object application, object shortcut1, object shortcut2, object shortcut3);
        void RegisterApplicationWithShortcuts1(RuntimeVersion aspNetVersion, bool run32bitOnx64, ProcessIdentity hostUserIdentity, IWebAppConfigEntry1 application, IAppShortcut1 shortcut1, IAppShortcut1 shortcut2, IAppShortcut1 shortcut3);
        void RegisterApplicationWithShortcuts1Disp(RuntimeVersion aspNetVersion, bool run32bitOnx64, ProcessIdentity hostUserIdentity, object application, object shortcut1, object shortcut2, object shortcut3);
        void RegisterApplicationWithShortcuts2(RuntimeVersion aspNetVersion, bool run32bitOnx64, IWebAppConfigEntry1 application, IAppShortcut1 shortcut1, IAppShortcut1 shortcut2, IAppShortcut1 shortcut3);
        void RegisterApplicationWithShortcuts2Disp(RuntimeVersion aspNetVersion, bool run32bitOnx64, object application, object shortcut1, object shortcut2, object shortcut3);
        string GetDefaultHost(bool forIOS);
        string CreateAppRedirectLink(string appID, string host = null, string optionalPathAndQueryString = null, params ushort[] portOrder);
        bool WaitForAppToStart(string appID, int timeoutMilliseconds = -1);
        bool LaunchTheAppInTheBrowser(Guid appID, string optionalPathAndQueryString = null, int timeoutMilliseconds = -1);
    }
}

