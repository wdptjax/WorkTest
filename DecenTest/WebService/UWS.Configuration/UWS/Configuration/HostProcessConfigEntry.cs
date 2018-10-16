namespace UWS.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using UWS.Configuration.Properties;
    using UWS.Framework;

    [Serializable, ComVisible(false)]
    public class HostProcessConfigEntry
    {
        private Dictionary<Guid, ListenEndpoint> listeners;

        public HostProcessConfigEntry()
        {
            this.listeners = new Dictionary<Guid, ListenEndpoint>();
            this.VersionID = Guid.NewGuid();
            this.RecycleAfterMemoryUsageExceedsByte = 0L;
            this.RecycleAfterMinutes = 0L;
        }

        public HostProcessConfigEntry(HostProcessClrAndBitness template) : this(template, true)
        {
        }

        public HostProcessConfigEntry(HostProcessClrAndBitness template, bool useDefaultHostProcess) : this(useDefaultHostProcess ? AppPoolIDs.DefaultHostName : null, useDefaultHostProcess ? new Guid?(AppPoolIDs.DefaultNetworkServiceHostIDs[(int) template]) : null, template, ProcessIdentity.NetworkService)
        {
            if (useDefaultHostProcess)
            {
                this.VersionID = this.HostID;
            }
        }

        public HostProcessConfigEntry(HostProcessClrAndBitness template, ProcessIdentity userAccount) : this(null, null, template, userAccount)
        {
        }

        public HostProcessConfigEntry(string appPoolName, Guid? appPoolID, HostProcessClrAndBitness template) : this(appPoolName, appPoolID, template, ProcessIdentity.NetworkService)
        {
        }

        public HostProcessConfigEntry(string appPoolName, Guid? appPoolID, HostProcessClrAndBitness template, ProcessIdentity userAccount) : this()
        {
            this.Name = appPoolName;
            this.HostID = appPoolID.HasValue ? appPoolID.Value : Guid.NewGuid();
            this.HostType = template;
            this.Identity = userAccount;
        }

        public bool CanRemoveFromMetabaseExplicitly()
        {
            return (this.Applications.Count == 0);
        }

        public bool CanRemoveHostFromMetabaseDuringAppUnreg(Guid? privateAppID)
        {
            if (this.Applications.Count != 0)
            {
                return false;
            }
            return ((privateAppID.HasValue && (privateAppID.Value == this.HostID)) || this.IsDefaultSharedHost);
        }

        public static HostProcessConfigEntry CreateDefaultHostConfigObject(Guid defaultHostID)
        {
            if (!AppPoolIDs.IsDefaultHostID(defaultHostID))
            {
                throw new Exception(string.Format("Application host ID {0:B} is not ID of a default host process.", defaultHostID));
            }
            foreach (HostProcessClrAndBitness bitness in Enum.GetValues(typeof(HostProcessClrAndBitness)))
            {
                foreach (ProcessIdentity identity in Enum.GetValues(typeof(ProcessIdentity)))
                {
                    if (defaultHostID == AppPoolRegistrationHelper.GetDefaultHostID(bitness, identity))
                    {
                        return CreateDefaultHostConfigObject(bitness, identity);
                    }
                }
            }
            throw new Exception("Should never get here");
        }

        internal static HostProcessConfigEntry CreateDefaultHostConfigObject(HostProcessClrAndBitness template, ProcessIdentity processIdentity)
        {
            Guid defaultHostID = AppPoolRegistrationHelper.GetDefaultHostID(template, processIdentity);
            return new HostProcessConfigEntry(AppPoolIDs.DefaultHostName, new Guid?(defaultHostID), template, processIdentity) { VersionID = defaultHostID };
        }

        public bool EndpointExists(Guid appOrRedirID)
        {
            return (this.GetEndpoint(appOrRedirID) != null);
        }

        public override bool Equals(object obj)
        {
            HostProcessConfigEntry entry = obj as HostProcessConfigEntry;
            if (entry == null)
            {
                return base.Equals(obj);
            }
            return (((entry.HostID == this.HostID) && (entry.HostType == this.HostType)) && (entry.Identity == this.Identity));
        }

        public WebAppConfigEntry GetApplication(Guid appID)
        {
            return (this.GetEndpoint(appID) as WebAppConfigEntry);
        }

        public string GetClrBitnessAndUserUI()
        {
            string str2;
            bool flag;
            string str = this.Identity.ToString();
            RuntimeVersion version = AppPoolRegistrationHelper.ToClrAndBitness(this.HostType, out flag);
            if (!flag && !SystemUtilites.Is64BitOperatingSystem)
            {
                flag = true;
            }
            switch (version)
            {
                case RuntimeVersion.AspNet_1_2or3x:
                    str2 = flag ? Resources.Clr_1_2_3 : Resources.Clr_2_3;
                    break;

                case RuntimeVersion.AspNet_4:
                    str2 = Resources.Clr_4;
                    break;

                default:
                    str2 = version.ToString();
                    break;
            }
            if (!SystemUtilites.Is64BitOperatingSystem)
            {
                return string.Format("{0}, {1}", str2, str);
            }
            string str3 = flag ? Resources.Bitness32 : Resources.Bitness64;
            return string.Format("{0}, {1}, {2}", str2, str3, str);
        }

        public ListenEndpoint GetEndpoint(Guid appOrRedirID)
        {
            ListenEndpoint endpoint;
            if (!this.Applications.TryGetValue(appOrRedirID, out endpoint))
            {
                return null;
            }
            return endpoint;
        }

        public bool IsCompatibleSharedHost(HostProcessConfigEntry otherHostConfig)
        {
            return this.IsCompatibleSharedHost(otherHostConfig.HostType, otherHostConfig.Identity, otherHostConfig.IsLegacyCassiniHost);
        }

        public bool IsCompatibleSharedHost(HostProcessClrAndBitness otherHostType, ProcessIdentity otherProcessIdentity, bool otherProcessIsLegacyCassini)
        {
            if (this.IsPrivateHost || (this.IsLegacyCassiniHost != otherProcessIsLegacyCassini))
            {
                return false;
            }
            return ((this.HostType == otherHostType) && (this.Identity == otherProcessIdentity));
        }

        public override string ToString()
        {
            return string.Format("ID={0:B}, Name=\"{1}\", CLR & Bitness={2}, Original security context user={3}, Number of applications={4}", new object[] { this.HostID, this.Name, this.HostType, this.Identity, this.Applications.Count });
        }

        [XmlIgnore]
        public Dictionary<Guid, ListenEndpoint> Applications
        {
            get
            {
                return this.listeners;
            }
        }

        [XmlElement(ElementName="Applications")]
        public ApplicationCollection apps
        {
            get
            {
                ApplicationCollection applications = new ApplicationCollection();
                foreach (WebAppConfigEntry entry in this.listeners.Values)
                {
                    applications.apps.Add(entry);
                }
                return applications;
            }
            set
            {
                this.Applications.Clear();
                foreach (WebAppConfigEntry entry in value.apps)
                {
                    this.Applications.Add(entry.ID, entry);
                }
            }
        }

        public bool CanAutoRecycle
        {
            get
            {
                if (this.RecycleAfterMemoryUsageExceedsByte <= 0L)
                {
                    return (this.RecycleAfterMinutes > 0L);
                }
                return true;
            }
        }

        public static string DefaultHostName
        {
            get
            {
                return Resources.DefaultSharedHostName;
            }
        }

        [XmlAttribute(AttributeName="ID")]
        public Guid HostID { get; set; }

        [XmlAttribute]
        public HostProcessClrAndBitness HostType { get; set; }

        [XmlAttribute]
        public ProcessIdentity Identity { get; set; }

        public bool IsDefaultSharedHost
        {
            get
            {
                return AppPoolIDs.IsDefaultHostID(this.HostID);
            }
        }

        public bool IsLegacyCassiniHost
        {
            get
            {
                foreach (Guid guid in AppPoolIDs.LegacyCassiniHostIDs)
                {
                    if (this.HostID == guid)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool IsPrivateHost
        {
            get
            {
                return ((this.Applications.Count == 1) && this.Applications.ContainsKey(this.HostID));
            }
        }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public long RecycleAfterMemoryUsageExceedsByte { get; set; }

        [XmlAttribute]
        public long RecycleAfterMinutes { get; set; }

        public string UIName
        {
            get
            {
                if (AppPoolIDs.IsDefaultHostID(this.HostID))
                {
                    return DefaultHostName;
                }
                string name = null;
                if (!string.IsNullOrEmpty(this.Name))
                {
                    name = this.Name;
                }
                else if (this.IsPrivateHost)
                {
                    foreach (ListenEndpoint endpoint in this.Applications.Values)
                    {
                        WebAppConfigEntry entry = endpoint as WebAppConfigEntry;
                        if (entry != null)
                        {
                            name = entry.UIApplicationName;
                            break;
                        }
                    }
                }
                if (name == null)
                {
                    name = this.HostID.ToString("B");
                }
                if (this.IsPrivateHost)
                {
                    name = name + Resources.PrivateHostSuffix;
                }
                return name;
            }
        }

        [XmlAttribute(AttributeName="Version")]
        public Guid VersionID { get; set; }
    }
}

