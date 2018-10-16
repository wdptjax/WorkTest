namespace CassiniConfiguration
{
    using System;
    using System.IO;
    using System.Net;
    using System.Xml.Serialization;

    [Serializable]
    public class ApplicationEntry
    {
        [XmlElement]
        public Guid ApplicationID;
        [XmlElement]
        public string DefaultDocument;
        [XmlElement]
        public string Description;
        [XmlAttribute]
        public string FrameworkVersion;
        [XmlAttribute]
        public bool KeepRunning;
        [XmlElement]
        public string Name;
        [XmlElement]
        public string PhysicalPath;
        [XmlElement]
        public int Port;

        public ApplicationEntry()
        {
            this.Name = string.Empty;
            this.Description = string.Empty;
            this.DefaultDocument = string.Empty;
            this.FrameworkVersion = Environment.Version.ToString(2);
            this.KeepRunning = true;
            this.ApplicationID = Guid.NewGuid();
        }

        public ApplicationEntry(string physicalPath) : this(0, physicalPath)
        {
        }

        public ApplicationEntry(int port, string physicalPath)
        {
            this.Name = string.Empty;
            this.Description = string.Empty;
            this.DefaultDocument = string.Empty;
            this.FrameworkVersion = Environment.Version.ToString(2);
            this.KeepRunning = true;
            this.ApplicationID = Guid.NewGuid();
            if (!Directory.Exists(physicalPath))
            {
                throw new Exception(string.Format("Cannot create Cassini Aplication entry. Directory \"{0}\" does not exist.", physicalPath));
            }
            this.Port = port;
            this.PhysicalPath = physicalPath;
        }

        public ApplicationEntry(Guid appID, string name, string description, string physicalPath, string defaultDocument, bool keepRunning) : this(appID, name, description, 0, physicalPath, defaultDocument, keepRunning)
        {
        }

        public ApplicationEntry(Guid appID, string name, string description, int port, string physicalPath, string defaultDocument, bool keepRunning) : this(port, physicalPath)
        {
            this.ApplicationID = appID;
            this.Name = name;
            this.Description = description;
            this.DefaultDocument = defaultDocument;
            this.KeepRunning = keepRunning;
        }

        public string GetFriendlyName()
        {
            if ((this.Name != null) && (this.Name.Length > 0))
            {
                return this.Name;
            }
            string[] strArray = this.PhysicalPath.Split(new char[] { '\\', '/' });
            for (int i = strArray.Length - 1; i >= 0; i--)
            {
                if (strArray[i].Length > 0)
                {
                    return strArray[i];
                }
            }
            return this.GetLocalhostUrl();
        }

        public string GetLANUrl()
        {
            return this.GetUrl("http", Environment.MachineName);
        }

        public string GetLocalhostUrl()
        {
            return this.GetUrl("http", IPAddress.Loopback.ToString());
        }

        public string GetUrl(string scheme, string hostname)
        {
            return string.Format("{0}://{1}:{2}/{3}", new object[] { scheme, hostname, this.Port, this.DefaultDocument });
        }

        public bool IsRunning()
        {
            return !Metabase.IsPortFree(this.Port);
        }
    }
}

