namespace UWS.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class HostProcessCollection
    {
        [XmlElement(ElementName="HostProcess")]
        public List<HostProcessConfigEntry> appPools;

        public HostProcessCollection()
        {
            this.appPools = new List<HostProcessConfigEntry>();
        }

        public HostProcessCollection(ICollection<HostProcessConfigEntry> appPools)
        {
            this.appPools = new List<HostProcessConfigEntry>();
            this.appPools.AddRange(appPools);
        }
    }
}

