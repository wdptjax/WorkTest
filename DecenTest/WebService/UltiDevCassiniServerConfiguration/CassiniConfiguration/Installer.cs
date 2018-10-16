namespace CassiniConfiguration
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration.Install;

    [RunInstaller(true)]
    public class Installer : System.Configuration.Install.Installer
    {
        private Container components;

        public Installer()
        {
            this.InitializeComponent();
        }

        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
            this.RegisterApplication(savedState);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
        }

        private void RegisterApplication(IDictionary stateSaver)
        {
            string str = string.Empty;
            try
            {
                str = "converting appID parameter to GUID";
                string g = base.Context.Parameters["appid"];
                Guid applicationID = new Guid(g);
                str = "getting appLocation parameter value";
                string physicalPath = base.Context.Parameters["applocation"];
                if ((physicalPath == null) || (physicalPath.Length == 0))
                {
                    throw new ApplicationException("Unable to register application. AppLocation parameter is not specified.");
                }
                str = "getting appName parameter value";
                string name = base.Context.Parameters["appname"];
                str = "getting appDescription parameter value";
                string description = base.Context.Parameters["appdescription"];
                str = "getting appPort parameter value";
                int port = 0;
                string s = base.Context.Parameters["appport"];
                if ((s != null) && (s.Length > 0))
                {
                    port = int.Parse(s);
                }
                str = "getting appDefaultDoc parameter value";
                string defaultDocument = base.Context.Parameters["appdefaultdoc"];
                str = "getting appKeepRunning parameter value";
                string str8 = base.Context.Parameters["appkeeprunning"];
                bool keepRunning = true;
                if ((str8 != null) && (str8.Length > 0))
                {
                    keepRunning = bool.Parse(str8);
                }
                str = "calling CassiniConfiguration.Metabase.RegisterApplication() method";
                Metabase.RegisterApplication(applicationID, name, port, description, physicalPath, defaultDocument, keepRunning);
                stateSaver["AppID"] = applicationID.ToString();
            }
            catch (Exception exception)
            {
                throw new Exception(string.Format("Failed to register an application with Cassini while {0}.", str), exception);
            }
        }

        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
            this.UnregisterApp();
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
            this.UnregisterApp();
        }

        private void UnregisterApp()
        {
            string str = string.Empty;
            try
            {
                str = "converting appID parameter to GUID";
                string g = base.Context.Parameters["appid"];
                Guid appID = new Guid(g);
                str = "calling CassiniConfiguration.Metabase.UnregisterApplication(appID) method";
                Metabase.UnregisterApplication(appID);
            }
            catch (Exception exception)
            {
                throw new Exception(string.Format("Failed to unregister Cassini application while {0}.", str), exception);
            }
        }
    }
}

