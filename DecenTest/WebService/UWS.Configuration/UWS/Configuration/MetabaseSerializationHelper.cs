namespace UWS.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;

    [Serializable, XmlRoot("ApplicationConfiguration")]
    public class MetabaseSerializationHelper
    {
        [XmlIgnore]
        public Dictionary<Guid, HostProcessConfigEntry> Hosts = new Dictionary<Guid, HostProcessConfigEntry>();
        [XmlIgnore]
        public Dictionary<Guid, RedirectionConfigEntry> Redirections = new Dictionary<Guid, RedirectionConfigEntry>();
        [XmlIgnore]
        public Dictionary<Guid, WebAppConfigEntry> UninstalledApps = new Dictionary<Guid, WebAppConfigEntry>();

        internal WebAppConfigEntry FindAppEntry(Guid appID)
        {
            HostProcessConfigEntry entry;
            return this.FindAppEntry(appID, out entry);
        }

        internal WebAppConfigEntry FindAppEntry(Guid appID, out HostProcessConfigEntry appPool)
        {
            WebAppConfigEntry application;
            appPool = this.GetHostConfig(appID);
            if (appPool != null)
            {
                application = appPool.GetApplication(appID);
                if (application != null)
                {
                    if (appID != application.ID)
                    {
                        throw new ApplicationException(string.Format("Metabase contains wrong key ({0:B}) for application with ID={1:B}.", appID, application.ID));
                    }
                    return application;
                }
            }
            foreach (HostProcessConfigEntry entry2 in this.Hosts.Values)
            {
                application = entry2.GetApplication(appID);
                if (application != null)
                {
                    appPool = entry2;
                    return application;
                }
            }
            return null;
        }

        public HostProcessConfigEntry GetHostConfig(Guid poolID)
        {
            HostProcessConfigEntry entry;
            if (this.Hosts.TryGetValue(poolID, out entry))
            {
                return entry;
            }
            return null;
        }

        internal RedirectionConfigEntry GetRedirection(Guid redirectionID)
        {
            RedirectionConfigEntry entry;
            if (this.Redirections.TryGetValue(redirectionID, out entry))
            {
                return entry;
            }
            return null;
        }

        [XmlElement(ElementName="HostProcesses")]
        public HostProcessCollection hostList
        {
            get
            {
                return new HostProcessCollection(this.Hosts.Values);
            }
            set
            {
                this.Hosts.Clear();
                foreach (HostProcessConfigEntry entry in value.appPools)
                {
                    this.Hosts.Add(entry.HostID, entry);
                }
            }
        }

        [XmlElement(ElementName="Redirections")]
        public RedirectionCollection redirections
        {
            get
            {
                return new RedirectionCollection(this.Redirections.Values);
            }
            set
            {
                this.Redirections.Clear();
                foreach (RedirectionConfigEntry entry in value.redirections)
                {
                    this.Redirections.Add(entry.ID, entry);
                }
            }
        }

        [XmlElement(ElementName="UninstalledApplications")]
        public ApplicationCollection uninstalledApps
        {
            get
            {
                return new ApplicationCollection(this.UninstalledApps.Values);
            }
            set
            {
                this.UninstalledApps.Clear();
                foreach (WebAppConfigEntry entry in value.apps)
                {
                    this.UninstalledApps.Add(entry.ID, entry);
                }
            }
        }
    }
}

