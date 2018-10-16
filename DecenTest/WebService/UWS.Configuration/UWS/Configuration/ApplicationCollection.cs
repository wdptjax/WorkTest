namespace UWS.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class ApplicationCollection
    {
        [XmlElement(ElementName="Application")]
        public List<WebAppConfigEntry> apps;

        public ApplicationCollection()
        {
            this.apps = new List<WebAppConfigEntry>();
        }

        public ApplicationCollection(ICollection<WebAppConfigEntry> applications)
        {
            this.apps = new List<WebAppConfigEntry>();
            this.apps.AddRange(applications);
        }
    }
}

