namespace UWS.Configuration.COM
{
    using System;
    using System.Runtime.InteropServices;
    using UWS.Configuration;
    using UWS.Framework;

    [Guid("DE85E3D5-C67E-4586-9E69-F4D418170CCD"), ComVisible(true), ComDefaultInterface(typeof(IMetabaseComServer1)), ClassInterface(ClassInterfaceType.AutoDual), ProgId("UltiDev.WebServer.MetabaseComServer.1")]
    public class MetabaseComServer : IMetabaseComServer1
    {
        public IRedirectionConfigEntry1 GetRedirectionEntry(string redirectionGUID)
        {
            Guid redirectionID = new Guid(redirectionGUID);
            return new RedirectionConfigEntryComServer(Metabase.GetRedirectionEntry(redirectionID));
        }

        public int GetRegisteredApplicationCount()
        {
            return Metabase.GetRegisteredApplicationCount();
        }

        public IWebAppConfigEntry1 GetWebAppEntry(string applicationGUID, bool includeUninstalledInSearch)
        {
            Guid applicationID = new Guid(applicationGUID);
            return new WebAppEntryComServer(Metabase.GetWebAppEntry(applicationID, includeUninstalledInSearch));
        }

        public bool HasRegisteredApplications()
        {
            return Metabase.HasRegisteredApplications();
        }

        public bool IsApplicationInstalled(string applicationGUID)
        {
            Guid appID = new Guid(applicationGUID);
            return (Metabase.FindApplication(appID) != null);
        }

        public bool IsApplicationUninstalled(string applicationGUID)
        {
            if (this.IsApplicationInstalled(applicationGUID))
            {
                return false;
            }
            Guid applicationID = new Guid(applicationGUID);
            return (Metabase.GetUninstalledAppEntry(applicationID) != null);
        }

        public void RegisterApplication(RuntimeVersion aspNetVersion, bool run32bitOnx64, bool useDefaultSharedHostProcess, ProcessIdentity hostUserIdentity, IWebAppConfigEntry1 application)
        {
            WebAppConfigEntry entry = WebAppConfigEntry.FromComConfigEntry(application);
            Metabase.RegisterApplication(aspNetVersion, run32bitOnx64, useDefaultSharedHostProcess, hostUserIdentity, entry);
        }

        public void RegisterApplication1(RuntimeVersion aspNetVersion, bool run32bitOnx64, ProcessIdentity hostUserIdentity, IWebAppConfigEntry1 application)
        {
            WebAppConfigEntry entry = WebAppConfigEntry.FromComConfigEntry(application);
            Metabase.RegisterApplication(aspNetVersion, run32bitOnx64, hostUserIdentity, entry);
        }

        public void RegisterApplication1Disp(RuntimeVersion aspNetVersion, bool run32bitOnx64, ProcessIdentity hostUserIdentity, object application)
        {
            this.RegisterApplication1(aspNetVersion, run32bitOnx64, hostUserIdentity, (IWebAppConfigEntry1) application);
        }

        public void RegisterApplication2(RuntimeVersion aspNetVersion, bool run32bitOnx64, IWebAppConfigEntry1 application)
        {
            WebAppConfigEntry entry = WebAppConfigEntry.FromComConfigEntry(application);
            Metabase.RegisterApplication(aspNetVersion, run32bitOnx64, entry);
        }

        public void RegisterApplication2Disp(RuntimeVersion aspNetVersion, bool run32bitOnx64, object application)
        {
            this.RegisterApplication2(aspNetVersion, run32bitOnx64, (IWebAppConfigEntry1) application);
        }

        public void RegisterApplicationDisp(RuntimeVersion aspNetVersion, bool run32bitOnx64, bool useDefaultSharedHostProcess, ProcessIdentity hostUserIdentity, object application)
        {
            this.RegisterApplication(aspNetVersion, run32bitOnx64, useDefaultSharedHostProcess, hostUserIdentity, (IWebAppConfigEntry1) application);
        }

        public void RegisterRedirection(IRedirectionConfigEntry1 redirection)
        {
            Metabase.RegisterRedirector(RedirectionConfigEntry.FromComConfigEntry(redirection));
        }

        public void RegisterRedirectionDisp(object redirection)
        {
            this.RegisterRedirection((IRedirectionConfigEntry1) redirection);
        }

        public void UnregisterApplication(string applicationGUID)
        {
            Guid appID = new Guid(applicationGUID);
            Metabase.UnregisterApplication(appID);
        }

        public void UnregisterRedirection(string redirectionGUID)
        {
            Guid redirectiorID = new Guid(redirectionGUID);
            Metabase.UnregisterRedirector(redirectiorID);
        }

        public static Guid ComClsID
        {
            get
            {
                return new Guid((typeof(MetabaseComServer).GetCustomAttributes(typeof(GuidAttribute), false)[0] as GuidAttribute).Value);
            }
        }
    }
}

