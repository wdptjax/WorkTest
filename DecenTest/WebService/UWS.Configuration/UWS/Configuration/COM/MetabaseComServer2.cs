namespace UWS.Configuration.COM
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UWS.Configuration;
    using UWS.Framework;

    [Guid("77BED825-5825-415F-ADE2-3D8625716FA7"), ClassInterface(ClassInterfaceType.AutoDual), ComDefaultInterface(typeof(IMetabaseComServer2)), ProgId("UltiDev.WebServer.MetabaseComServer.2"), ComVisible(true)]
    public class MetabaseComServer2 : MetabaseComServer, IMetabaseComServer2, IMetabaseComServer1
    {
        public string CreateAppRedirectLink(string appID, string host = null, string optionalPathAndQueryString = null, params ushort[] portOrder)
        {
            return Metabase.CreateAppRedirectLink(new Guid(appID), host, optionalPathAndQueryString, portOrder);
        }

        public string GetDefaultHost(bool forIOS)
        {
            return Metabase.GetDefaultHost(forIOS);
        }

        public IAppShortcut1 GetShortcut(IWebAppConfigEntry1 app, string optionalShortcutLocation, string optionalUrlHost, string optionalPathAndQueryString)
        {
            WebAppEntryComServer server = (WebAppEntryComServer) app;
            return AppShortcut.Instantiate(server.Entry, optionalShortcutLocation, optionalUrlHost, optionalPathAndQueryString, new ushort[0]);
        }

        public IAppShortcut1 GetShortcutDisp(object app, string optionalShortcutLocation = null, string optionalUrlHost = null, string optionalPathAndQueryString = null)
        {
            return this.GetShortcut((IWebAppConfigEntry1) app, optionalShortcutLocation, optionalUrlHost, optionalPathAndQueryString);
        }

        public bool LaunchTheAppInTheBrowser(Guid appID, string optionalPathAndQueryString = null, int timeoutMilliseconds = -1)
        {
            return Metabase.LaunchTheAppInTheBrowser(appID, optionalPathAndQueryString, timeoutMilliseconds);
        }

        public void RegisterApplicationWithShortcuts(RuntimeVersion aspNetVersion, bool run32bitOnx64, bool useDefaultSharedHostProcess, ProcessIdentity processIdentity, IWebAppConfigEntry1 application, IAppShortcut1 shortcut1, IAppShortcut1 shortcut2, IAppShortcut1 shortcut3)
        {
            WebAppConfigEntry entry = WebAppConfigEntry.FromComConfigEntry(application);
            Metabase.RegisterApplication(aspNetVersion, run32bitOnx64, useDefaultSharedHostProcess, processIdentity, entry, ToAppShortCollection(new IAppShortcut1[] { shortcut1, shortcut2, shortcut3 }));
        }

        public void RegisterApplicationWithShortcuts1(RuntimeVersion aspNetVersion, bool run32bitOnx64, ProcessIdentity hostUserIdentity, IWebAppConfigEntry1 application, IAppShortcut1 shortcut1, IAppShortcut1 shortcut2, IAppShortcut1 shortcut3)
        {
            WebAppConfigEntry entry = WebAppConfigEntry.FromComConfigEntry(application);
            Metabase.RegisterApplication(aspNetVersion, run32bitOnx64, hostUserIdentity, entry, ToAppShortCollection(new IAppShortcut1[] { shortcut1, shortcut2, shortcut3 }));
        }

        public void RegisterApplicationWithShortcuts1Disp(RuntimeVersion aspNetVersion, bool run32bitOnx64, ProcessIdentity hostUserIdentity, object application, object shortcut1, object shortcut2, object shortcut3)
        {
            this.RegisterApplicationWithShortcuts1(aspNetVersion, run32bitOnx64, hostUserIdentity, (IWebAppConfigEntry1) application, (IAppShortcut1) shortcut1, (IAppShortcut1) shortcut2, (IAppShortcut1) shortcut3);
        }

        public void RegisterApplicationWithShortcuts2(RuntimeVersion aspNetVersion, bool run32bitOnx64, IWebAppConfigEntry1 application, IAppShortcut1 shortcut1, IAppShortcut1 shortcut2, IAppShortcut1 shortcut3)
        {
            WebAppConfigEntry entry = WebAppConfigEntry.FromComConfigEntry(application);
            Metabase.RegisterApplication(aspNetVersion, run32bitOnx64, entry, ToAppShortCollection(new IAppShortcut1[] { shortcut1, shortcut2, shortcut3 }));
        }

        public void RegisterApplicationWithShortcuts2Disp(RuntimeVersion aspNetVersion, bool run32bitOnx64, object application, object shortcut1, object shortcut2, object shortcut3)
        {
            this.RegisterApplicationWithShortcuts2(aspNetVersion, run32bitOnx64, (IWebAppConfigEntry1) application, (IAppShortcut1) shortcut1, (IAppShortcut1) shortcut2, (IAppShortcut1) shortcut3);
        }

        public void RegisterApplicationWithShortcutsDisp(RuntimeVersion aspNetVersion, bool run32bitOnx64, bool useDefaultSharedHostProcess, ProcessIdentity processIdentity, object application, object shortcut1, object shortcut2, object shortcut3)
        {
            this.RegisterApplicationWithShortcuts(aspNetVersion, run32bitOnx64, useDefaultSharedHostProcess, processIdentity, (IWebAppConfigEntry1) application, (IAppShortcut1) shortcut1, (IAppShortcut1) shortcut2, (IAppShortcut1) shortcut3);
        }

        protected static List<AppShortcut> ToAppShortCollection(params IAppShortcut1[] shortcuts)
        {
            if (shortcuts != null)
            {
                List<AppShortcut> list = new List<AppShortcut>();
                foreach (IAppShortcut1 shortcut in shortcuts)
                {
                    if (shortcut != null)
                    {
                        list.Add((AppShortcut) shortcut);
                    }
                }
                if (list.Count != 0)
                {
                    return list;
                }
            }
            return null;
        }

        public bool WaitForAppToStart(string appID, int timeoutMilliseconds = -1)
        {
            return Metabase.WaitForAppToStart(new Guid(appID), timeoutMilliseconds);
        }

        public IMetabaseComServer1 Base
        {
            get
            {
                return this;
            }
        }
    }
}

