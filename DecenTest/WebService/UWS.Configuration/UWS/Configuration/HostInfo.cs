namespace UWS.Configuration
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public class HostInfo
    {
        private string Host;
        private int Port;
        private string Scheme;

        internal HostInfo(Uri uri)
        {
            this.Scheme = uri.Scheme.ToLowerInvariant();
            this.Host = uri.Host.ToLowerInvariant();
            this.Port = uri.Port;
        }

        internal UriMatchType Match(Uri uri)
        {
            if (this.Port == uri.Port)
            {
                if (this.Scheme != uri.Scheme)
                {
                    return UriMatchType.None;
                }
                if (this.IsSSL)
                {
                    return UriMatchType.Exact;
                }
                if (this.Host == uri.Host.ToLowerInvariant())
                {
                    return UriMatchType.Exact;
                }
                if (this.IsWildcard)
                {
                    return UriMatchType.Wildcard;
                }
            }
            return UriMatchType.None;
        }

        internal bool Matches(Uri uri)
        {
            return (this.Match(uri) != UriMatchType.None);
        }

        internal string Authority
        {
            get
            {
                return string.Format("{0}:{1}", this.Host, this.Port);
            }
        }

        internal string BaseUrl
        {
            get
            {
                return string.Format("{0}://{1}:{2}", this.Scheme, this.Host, this.Port);
            }
        }

        private bool IsSSL
        {
            get
            {
                return (this.Scheme == "https");
            }
        }

        private bool IsWildcard
        {
            get
            {
                return (this.Host == VDirHostMap.wildcardHost);
            }
        }
    }
}

