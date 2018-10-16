namespace CassiniConfiguration
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using System.Xml.Serialization;

    [Serializable, XmlRoot("CassiningMetabase")]
    public class Metabase
    {
        private ApplicationEntry[] applicationArray = new ApplicationEntry[0];
        public static readonly Guid cassiniConfirationAppID = new Guid("4fd8b3f7-bc73-4583-95fe-e7b69b10a3ae");
        private static FileSystemWatcher configFileWather = null;
        internal const string dataFolder = @"\UltiDev\Cassini";
        private static bool firstNotification = true;
        private static Mutex lastCallTimeMutex = new Mutex();
        private static DateTime lastTimeEventHandlerCalled = DateTime.Now;
        private const int metabaseChangeNotificationDelayMillisecond = 0x3e8;

        public static event MetabaseUpdatedHandler MeatabaseFileUpdated;

        static Metabase()
        {
            if (!Directory.Exists(ConfigFileFolder))
            {
                Directory.CreateDirectory(ConfigFileFolder);
            }
            configFileWather = new FileSystemWatcher(ConfigFileFolder);
            configFileWather.Filter = ConfigFileName;
            configFileWather.NotifyFilter = NotifyFilters.LastWrite;
            configFileWather.Changed -= new FileSystemEventHandler(Metabase.OnMetabaseFileUpdated);
            configFileWather.Changed += new FileSystemEventHandler(Metabase.OnMetabaseFileUpdated);
            configFileWather.EnableRaisingEvents = true;
        }

        public bool CanUsePort(int port)
        {
            if (this.HasApplication(port))
            {
                return false;
            }
            if (!IsPortFree(port))
            {
                return false;
            }
            return true;
        }

        public ApplicationEntry FindApplication(Guid appID)
        {
            foreach (ApplicationEntry entry in this.Applications)
            {
                if (entry.ApplicationID == appID)
                {
                    return entry;
                }
            }
            return null;
        }

        private ApplicationEntry FindApplication(int port)
        {
            int index = Array.BinarySearch(this.Applications, port, new AppComparer());
            if (index >= 0)
            {
                return this.Applications[index];
            }
            return null;
        }

        public int FindApplicationIndex(ApplicationEntry app)
        {
            int num = Array.BinarySearch(this.applicationArray, app.Port, new AppComparer());
            if (num >= 0)
            {
                return num;
            }
            ApplicationEntry entry = this.FindApplication(app.ApplicationID);
            if (entry != null)
            {
                return Array.BinarySearch(this.applicationArray, entry.Port, new AppComparer());
            }
            return -1;
        }

        private int FindApplicationIndex(string appName, string physicalAddress)
        {
            appName = appName.ToLower();
            physicalAddress = physicalAddress.ToLower();
            for (int i = 0; i < this.Applications.Length; i++)
            {
                ApplicationEntry entry = this.Applications[i];
                if ((entry.Name.ToLower() == appName) || (entry.PhysicalPath.ToLower() == physicalAddress))
                {
                    return i;
                }
            }
            return -1;
        }

        public static int FindFreeEphemeralPort()
        {
            IPEndPoint localEP = new IPEndPoint(IPAddress.Loopback, 0);
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP))
            {
                socket.Bind(localEP);
                localEP = (IPEndPoint)socket.LocalEndPoint;
                return localEP.Port;
            }
        }

        public int FindFreeTcpPort()
        {
            int num;
            do
            {
                num = FindFreeEphemeralPort();
            }
            while (this.HasApplication(num));
            return num;
        }

        public bool HasApplication(int port)
        {
            return (this[port] != null);
        }

        public static bool IsPortFree(int port)
        {
            bool flag;
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Loopback, port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(remoteEP);
                socket.Close();
                flag = false;
            }
            catch
            {
                flag = true;
            }
            finally
            {
                if (socket != null)
                {
#warning 原本的代码是socket.Dispose();
                    socket.Close();
                }
            }
            return flag;
        }

        public static Metabase Load()
        {
            Metabase metabase2;
            if (!System.IO.File.Exists(ConfigFileLocation))
            {
                return new Metabase();
            }
            XmlTextReader xmlReader = null;
            try
            {
                xmlReader = new XmlTextReader(ConfigFileLocation);
                XmlSerializer serializer = new XmlSerializer(typeof(Metabase));
                metabase2 = (Metabase)serializer.Deserialize(xmlReader);
            }
            finally
            {
                if (xmlReader != null)
                {
                    xmlReader.Close();
                }
            }
            return metabase2;
        }

        private static void MetabaseLoadAttempt(object unused)
        {
            Metabase metabase = null;
            try
            {
                metabase = Load();
            }
            catch
            {
                new System.Threading.Timer(new TimerCallback(Metabase.MetabaseLoadAttempt), null, 100, -1);
                return;
            }
            try
            {
                MeatabaseFileUpdated(metabase);
            }
            catch
            {
            }
            finally
            {
                configFileWather.Changed += new FileSystemEventHandler(Metabase.OnMetabaseFileUpdated);
            }
        }

        private static void OnMetabaseFileUpdated(object srouce, FileSystemEventArgs args)
        {
            if (MeatabaseFileUpdated != null)
            {
                if (firstNotification)
                {
                    firstNotification = false;
                }
                else
                {
                    firstNotification = true;
                    configFileWather.Changed -= new FileSystemEventHandler(Metabase.OnMetabaseFileUpdated);
                    MetabaseLoadAttempt(null);
                }
            }
        }

        public int RegisterApplication(ApplicationEntry appEntry)
        {
            return this.RegisterApplication(appEntry, false);
        }

        public static ApplicationEntry RegisterApplication(string physicalPath)
        {
            ApplicationEntry app = new ApplicationEntry(physicalPath);
            RegisterApplicationInternal(app);
            return app;
        }

        public int RegisterApplication(ApplicationEntry appEntry, bool autoSave)
        {
            int index = this.FindApplicationIndex(appEntry);
            if (index >= 0)
            {
                if (appEntry.Port == 0)
                {
                    appEntry.Port = this.Applications[index].Port;
                }
                this.Applications[index] = appEntry;
                this.SortApplicationCollection();
            }
            else
            {
                if (appEntry.Port == 0)
                {
                    appEntry.Port = this.FindFreeTcpPort();
                }
                else if (!this.CanUsePort(appEntry.Port))
                {
                    throw new Exception(string.Format("Cannon assign port {0} to application \"{1}\". Port is already in use.", appEntry.Port, appEntry.GetFriendlyName()));
                }
                ArrayList list = new ArrayList(this.Applications) {
                    appEntry
                };
                this.Applications = (ApplicationEntry[])list.ToArray(typeof(ApplicationEntry));
            }
            if (autoSave)
            {
                this.Save();
            }
            return appEntry.Port;
        }

        public static ApplicationEntry RegisterApplication(Guid applicationID, string name, string description, string physicalPath, string defaultDocument, bool keepRunning)
        {
            return RegisterApplication(applicationID, name, 0, description, physicalPath, defaultDocument, keepRunning);
        }

        public static ApplicationEntry RegisterApplication(Guid applicationID, string name, int port, string description, string physicalPath, string defaultDocument, bool keepRunning)
        {
            ApplicationEntry app = new ApplicationEntry(applicationID, name, description, port, physicalPath, defaultDocument, keepRunning);
            RegisterApplicationInternal(app);
            return app;
        }

        private static void RegisterApplicationInternal(ApplicationEntry app)
        {
            Load().RegisterApplication(app, true);
        }

        public void Save()
        {
            XmlTextWriter writer = null;
            try
            {
                writer = new XmlTextWriter(ConfigFileLocation, Encoding.UTF8)
                {
                    Formatting = Formatting.Indented
                };
                new XmlSerializer(typeof(Metabase)).Serialize((XmlWriter)writer, this);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
        }

        private void SortApplicationCollection()
        {
            Array.Sort(this.applicationArray, new AppComparer());
        }

        public static ApplicationEntry UnregisterApplication(Guid appID)
        {
            if (appID == cassiniConfirationAppID)
            {
                throw new ApplicationException("Cassini configuration application cannot be unregistered.");
            }
            return Load().UnregisterApplication(appID, true);
        }

        public void UnregisterApplication(ApplicationEntry app, bool autoSave)
        {
            int length = this.FindApplicationIndex(app);
            if (length < 0)
            {
                throw new ApplicationException(string.Format("Cannot unregister application \"{0}\". No application is assigned to port {1}.", app.GetFriendlyName(), app.Port));
            }
            ApplicationEntry[] destinationArray = new ApplicationEntry[this.Applications.Length - 1];
            Array.Copy(this.Applications, 0, destinationArray, 0, length);
            Array.Copy(this.Applications, length + 1, destinationArray, length, destinationArray.Length - length);
            this.applicationArray = destinationArray;
            if (autoSave)
            {
                this.Save();
            }
        }

        public ApplicationEntry UnregisterApplication(Guid appID, bool autoSave)
        {
            ApplicationEntry app = this.FindApplication(appID);
            if (app != null)
            {
                this.UnregisterApplication(app, autoSave);
            }
            return app;
        }

        [XmlElement("Application")]
        public ApplicationEntry[] Applications
        {
            get
            {
                return this.applicationArray;
            }
            set
            {
                this.applicationArray = (value == null) ? new ApplicationEntry[0] : value;
                this.SortApplicationCollection();
            }
        }

        public static string ConfigFileFolder
        {
            get
            {
                return (Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\UltiDev\Cassini");
            }
        }

        public static string ConfigFileLocation
        {
            get
            {
                return Path.Combine(ConfigFileFolder, ConfigFileName);
            }
        }

        public static string ConfigFileName
        {
            get
            {
                return "CassiniMetabase.xml";
            }
        }

        public ApplicationEntry this[int port]
        {
            get
            {
                return this.FindApplication(port);
            }
        }

        private class AppComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                int num = (x is int) ? ((int)x) : ((ApplicationEntry)x).Port;
                int num2 = (y is int) ? ((int)y) : ((ApplicationEntry)y).Port;
                return (num - num2);
            }
        }

        public delegate void MetabaseUpdatedHandler(Metabase metabase);
    }
}

