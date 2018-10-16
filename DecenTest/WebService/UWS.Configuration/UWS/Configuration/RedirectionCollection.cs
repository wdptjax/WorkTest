namespace UWS.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class RedirectionCollection
    {
        [XmlElement(ElementName="Redirection")]
        public List<RedirectionConfigEntry> redirections;

        public RedirectionCollection()
        {
            this.redirections = new List<RedirectionConfigEntry>();
        }

        public RedirectionCollection(ICollection<RedirectionConfigEntry> redirs)
        {
            this.redirections = new List<RedirectionConfigEntry>();
            this.redirections.AddRange(redirs);
        }
    }
}

