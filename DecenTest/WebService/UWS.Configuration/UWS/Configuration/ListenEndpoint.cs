namespace UWS.Configuration
{
    using HttpConfig;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml.Serialization;
    using UWS.Configuration.COM;
    using UWS.Framework;

    [Serializable, ComVisible(false)]
    public abstract class ListenEndpoint
    {
        private static readonly ushort[] defaultPorts = new ushort[] { 80, 0x1f90, 0x1e4c, 0 };
        private readonly ListenAddressList listenAddresses;
        protected string virtualDirectory;

        protected ListenEndpoint()
        {
            this.listenAddresses = new ListenAddressList();
            this.ChangeID = Guid.NewGuid();
            this.Stopped = false;
        }

        internal ListenEndpoint(IListenEndpoint1 ep)
        {
            this.listenAddresses = new ListenAddressList();
            this.ID = new Guid(ep.ID);
            this.VirtualDirectory = ep.VirtualDirectory;
            this.ListenAddresses.Clear();
            foreach (string str in ep.ListenAddresses)
            {
                this.ListenAddresses.Add(str);
            }
        }

        internal virtual void ApplyFinalDefaultsAndValidateBeforeSaving()
        {
            if (this.ID.Equals(Guid.Empty))
            {
                throw new ApplicationException("ID can't be blank when registering application or redirection. Be sure to use Metabase.GetWebAppEntry(Guid applicationID) instead of WebAppConfigEntry() constructor when creating new WebAppConfigEntry instance for application registration.");
            }
            if (this.ListenAddresses.Count == 0)
            {
                this.SetDefaultListenAddresses();
            }
            this.FilterOutListenUrlsTakenByOthers();
            this.ChangeID = Guid.NewGuid();
        }

        public Guid EnsureID()
        {
            if (this.ID == Guid.Empty)
            {
                this.ID = Guid.NewGuid();
            }
            return this.ID;
        }

        private void FilterOutListenUrlsTakenByOthers()
        {
            ICollection<string> currentListenUrlsBeforeReRegistering = this.GetCurrentListenUrlsBeforeReRegistering();
            List<string> list = this.ListenAddresses.FilterOutTakenListenUrls(this.VirtualDirectory, currentListenUrlsBeforeReRegistering);
            if (list != null)
            {
                StringBuilder builder = new StringBuilder();
                foreach (string str in list)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(", ");
                    }
                    builder.Append(str);
                }
                Trace.TraceWarning("Following endpoints will not be registered because they are used by some other application: {0}.", new object[] { builder });
            }
        }

        protected abstract ListenEndpoint FindExistingEndpoint(Guid id);
        private string FormatRedirUrl(Uri requestUrl, string host, string queryString)
        {
            return this.FormatRedirUrl(requestUrl, host, null, queryString);
        }

        private string FormatRedirUrl(Uri requestUrl, string host, string startUrl, string queryString)
        {
            string str = (this.VirtualDirectory + "/").TrimStart(new char[] { '/' });
            if (startUrl == null)
            {
                startUrl = this.GetDefaultDocForExplorerRedirection();
            }
            if (startUrl != null)
            {
                startUrl = startUrl.TrimStart(new char[] { '/' });
            }
            return string.Format("{0}://{1}:{2}/{3}{4}{5}", new object[] { requestUrl.Scheme, host, requestUrl.Port, str, startUrl, queryString });
        }

        public static string FormatVDir(string rawVDir)
        {
            if (string.IsNullOrEmpty(rawVDir))
            {
                return "/";
            }
            return ("/" + rawVDir.Trim(new char[] { '/' }));
        }

        public string[] GetAllExplorerRedirectionUrls(string requestHostOrIP, string startUrl, string requestQueryString)
        {
            SortedDictionary<int, string> dictionary = this.GetAllExplorerRedirectionUrlsInternal(requestHostOrIP, startUrl, requestQueryString);
            if (dictionary == null)
            {
                return new string[0];
            }
            string[] strArray = new string[dictionary.Count];
            int num = 0;
            foreach (KeyValuePair<int, string> pair in dictionary)
            {
                strArray[num++] = pair.Value;
            }
            return strArray;
        }

        private SortedDictionary<int, string> GetAllExplorerRedirectionUrlsInternal(string requestHostOrIP, string startUrl, string requestQueryString)
        {
            SortedDictionary<int, string> dictionary = new SortedDictionary<int, string>();
            foreach (ListenAddress address in this.ListenAddresses)
            {
                Uri requestUrl = address.GetUri(requestHostOrIP);
                string str = this.FormatRedirUrl(requestUrl, requestUrl.Host, startUrl, requestQueryString);
                dictionary.Add(requestUrl.Port, str);
            }
            return dictionary;
        }

        internal List<ushort> GetAllListenPorts()
        {
            List<ushort> list = new List<ushort>();
            foreach (ListenAddress address in this.listenAddresses)
            {
                ushort port = (ushort) address.GetPort();
                list.Add(port);
            }
            return list;
        }

        public string[] GetBrowseUrls(params string[] hostsIfWildCard)
        {
            List<string> list = new List<string>();
            foreach (ListenAddress address in this.ListenAddresses)
            {
                if (!address.IsWildcardAddress())
                {
                    string uriString = address.ToListenUrl(this.VirtualDirectory);
                    string str2 = new Uri(uriString).ToString();
                    list.Add((uriString.Length < str2.Length) ? uriString : str2);
                }
                else
                {
                    foreach (string str3 in hostsIfWildCard)
                    {
                        string str4 = address.ToUIUrl(str3, this.VirtualDirectory);
                        string str5 = new Uri(str4).ToString();
                        list.Add((str4.Length < str5.Length) ? str4 : str5);
                    }
                }
            }
            return list.ToArray();
        }

        public Dictionary<string, List<string>> GetBrowseUrlsGroupedByHost(params string[] hostsIfWildCard)
        {
            Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
            foreach (string str in this.GetBrowseUrls(hostsIfWildCard))
            {
                Uri uri = new Uri(str);
                string key = SystemUtilites.MassageHostName(uri.Host.ToLowerInvariant());
                if (!dictionary.ContainsKey(key))
                {
                    dictionary[key] = new List<string>();
                }
                dictionary[key].Add(str);
            }
            return dictionary;
        }

        protected ICollection<string> GetCurrentListenUrlsBeforeReRegistering()
        {
            ListenEndpoint endpoint = this.FindExistingEndpoint(this.ID);
            if (endpoint == null)
            {
                return null;
            }
            return endpoint.ListenAddresses.GetHttpListenerUrls(endpoint.VirtualDirectory);
        }

        protected virtual string GetDefaultDocForExplorerRedirection()
        {
            return string.Empty;
        }

        public string GetExplorerDefaultRedirectionUrl(string hostOrIP, int port, string startUrl, string queryString, int[] portPreferences, bool returnAnyIfNoGoodPort)
        {
            SortedDictionary<int, string> dictionary = this.GetAllExplorerRedirectionUrlsInternal(hostOrIP, startUrl, queryString);
            if ((portPreferences == null) || (portPreferences.Length == 0))
            {
                returnAnyIfNoGoodPort = true;
            }
            else
            {
                List<int> list = new List<int>(portPreferences) {
                    port
                };
                foreach (int num in list)
                {
                    string str;
                    if (dictionary.TryGetValue(num, out str))
                    {
                        return str;
                    }
                }
            }
            if ((dictionary.Count == 1) || returnAnyIfNoGoodPort)
            {
                foreach (KeyValuePair<int, string> pair in dictionary)
                {
                    return pair.Value;
                }
            }
            return null;
        }

        public string[] GetHttpListenerUrls()
        {
            return this.GetMainListenUrls();
        }

        public string GetHttpListenerUrlsUI()
        {
            StringBuilder builder = new StringBuilder();
            string[] mainListenUrls = this.GetMainListenUrls();
            for (int i = 0; i < mainListenUrls.Length; i++)
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }
                if (i == 4)
                {
                    builder.Append("...");
                    break;
                }
                string str = mainListenUrls[i];
                builder.Append(str);
            }
            return builder.ToString();
        }

        public string[] GetMainListenUrls()
        {
            string virtualDirectory = this.GetVirtualDirectory();
            return this.ListenAddresses.GetHttpListenerUrls(virtualDirectory);
        }

        public List<SslConfigItem> GetSslConfigItemsToUpdate(ref List<SslConfigItem> persistedAppSsl, out List<SslConfigItem> itemsToDelete)
        {
            List<SslConfigItem> sslConfigList = this.ListenAddresses.GetSslConfigList(this.ID, ref persistedAppSsl);
            itemsToDelete = SetOperations<SslConfigItem>.Difference(persistedAppSsl, sslConfigList);
            return sslConfigList;
        }

        protected string GetVirtualDirectory()
        {
            return ((string.IsNullOrEmpty(this.VirtualDirectory) || (this.VirtualDirectory == "/")) ? string.Empty : (this.VirtualDirectory.Trim(new char[] { '/' }) + "/"));
        }

        public bool HasWildcardListenAddress()
        {
            foreach (ListenAddress address in this.ListenAddresses)
            {
                if (address.IsWildcardAddress())
                {
                    return true;
                }
            }
            return false;
        }

        protected string ListenAddressToHost(string listenAddress)
        {
            return ListenAddress.ToListenUrl(this.GetVirtualDirectory(), listenAddress);
        }

        private void SetDefaultListenAddresses()
        {
            if (string.IsNullOrEmpty(this.VirtualDirectory))
            {
                throw new ApplicationException("ListenEndpoint.VirtualDirectory property must be set before calling SetDefaultListenAddresses().");
            }
            if (this.VirtualDirectory == "/")
            {
                this.ListenAddresses.AddByPort(0, false);
            }
            else
            {
                foreach (ushort num in defaultPorts)
                {
                    this.ListenAddresses.AddByPort(num, num == 0x1bb);
                }
            }
        }

        [XmlAttribute(AttributeName="Version")]
        public Guid ChangeID { get; set; }

        [XmlAttribute]
        public Guid ID { get; set; }

        public ListenAddressList ListenAddresses
        {
            get
            {
                return this.listenAddresses;
            }
        }

        [XmlAttribute, DefaultValue(false)]
        public bool Stopped { get; set; }

        [XmlIgnore]
        public abstract string UIApplicationName { get; }

        public virtual string VirtualDirectory
        {
            get
            {
                return FormatVDir(this.virtualDirectory);
            }
            set
            {
                this.virtualDirectory = FormatVDir(value);
            }
        }
    }
}

