namespace UWS.Configuration.COM
{
    using System;
    using UWS.Configuration;

    [Serializable]
    public class RedirectionConfigEntryComServer : ListenEndpointComServer<RedirectionConfigEntry>, IRedirectionConfigEntry1, IListenEndpoint1
    {
        internal RedirectionConfigEntryComServer() : this(new RedirectionConfigEntry())
        {
        }

        internal RedirectionConfigEntryComServer(RedirectionConfigEntry entry) : base(entry)
        {
        }

        public string GetRedirectionUrlFor(string sourceUrl)
        {
            return base.Entry.GetRedirectionUrlFor(new Uri(sourceUrl));
        }

        public IListenEndpoint1 Endpoint
        {
            get
            {
                return this;
            }
        }

        public bool RedirectAllToOne
        {
            get
            {
                return base.Entry.RedirectAllToOne;
            }
            set
            {
                base.Entry.RedirectAllToOne = value;
            }
        }

        public RedirectCode RedirectionCode
        {
            get
            {
                return base.Entry.RedirectionCode;
            }
            set
            {
                base.Entry.RedirectionCode = value;
            }
        }

        public string RedirectToUrl
        {
            get
            {
                return base.Entry.RedirectToUrl;
            }
            set
            {
                base.Entry.RedirectToUrl = value;
            }
        }
    }
}

