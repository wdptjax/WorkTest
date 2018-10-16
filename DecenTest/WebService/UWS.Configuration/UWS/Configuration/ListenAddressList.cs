namespace UWS.Configuration
{
    using HttpConfig;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(false)]
    public class ListenAddressList : List<ListenAddress>
    {
        public void Add(string address)
        {
            this.Add(new ListenAddress(address));
        }

        public void Add(ListenAddress address)
        {
            if (!this.Contains(address.RawAddress))
            {
                if (address.GetPort() == 0)
                {
                    foreach (ListenAddress address2 in this)
                    {
                        if (address2.EndpointUsesSystemAssignedPort())
                        {
                            return;
                        }
                    }
                    address.AssignSystemPort();
                }
                base.Add(address);
            }
        }

        public void AddAddresses(params string[] addresses)
        {
            foreach (string str in addresses)
            {
                this.Add(str);
            }
        }

        public void AddAddresses(params ListenAddress[] addresses)
        {
            foreach (ListenAddress address in addresses)
            {
                this.Add(address);
            }
        }

        internal void AddByPort(ushort port, bool ssl)
        {
            string address = ListenAddress.PortToListenAddress(port, ssl);
            this.Add(address);
        }

        public bool Contains(string address)
        {
            return (this.IndexOf(address) >= 0);
        }

        internal List<string> FilterOutTakenListenUrls(string vDir, ICollection<string> listenUrlsToExcludeFromTest)
        {
            List<string> list = this.FindTakenListenUrls(vDir, listenUrlsToExcludeFromTest);
            if (list != null)
            {
                foreach (string str in list)
                {
                    string str2;
                    string address = ListenAddress.FormatUrl(ListenAddress.ValidateUrlWithWildcard(str, out str2), str2, string.Empty);
                    if (!this.Remove(address))
                    {
                        throw new Exception(string.Format("Failed to remove taken URL \"{0}\" (\"{1}\") from the list desired listen-to addresses.", str, address));
                    }
                }
            }
            return list;
        }

        internal List<string> FindTakenListenUrls(string vDir, ICollection<string> listenUrlsToExcludeFromTest)
        {
            return FindTakenListenUrls(this.GetHttpListenerUrls(vDir), listenUrlsToExcludeFromTest);
        }

        private static List<string> FindTakenListenUrls(string[] endpointsToTest, ICollection<string> listenUrlsToExcludeFromTest)
        {
            List<string> list = new List<string>();
            foreach (string str in endpointsToTest)
            {
                if (((listenUrlsToExcludeFromTest == null) || !listenUrlsToExcludeFromTest.Contains(str)) && !ListenAddress.IsListenUrlFree(str))
                {
                    list.Add(str);
                }
            }
            if (list.Count == 0)
            {
                return null;
            }
            return list;
        }

        public string[] GetHttpListenerUrls(string vDir)
        {
            string[] strArray = new string[base.Count];
            int num = 0;
            foreach (ListenAddress address in this)
            {
                strArray[num++] = address.ToListenUrl(vDir);
            }
            return strArray;
        }

        public List<SslConfigItem> GetSslConfigList(Guid appID, ref List<SslConfigItem> persistedSslItems)
        {
            List<SslConfigItem> list = new List<SslConfigItem>();
            foreach (ListenAddress address in this)
            {
                if (address.sslRegistration != null)
                {
                    list.Add(address.sslRegistration);
                }
                else if (address.IsSSL)
                {
                    list.Add(address.ToSslConfigItem(appID, ref persistedSslItems, true));
                }
            }
            return list;
        }

        internal bool HasListenAddress(string address, string vDir)
        {
            foreach (string str in this.GetHttpListenerUrls(vDir))
            {
                if (str.ToLowerInvariant() == address.ToLowerInvariant())
                {
                    return true;
                }
            }
            return false;
        }

        public int IndexOf(string addressString)
        {
            int num = 0;
            foreach (ListenAddress address in this)
            {
                if (address.RawAddress == addressString)
                {
                    return num;
                }
                num++;
            }
            return -1;
        }

        public bool Remove(string address)
        {
            int index = this.IndexOf(address);
            if (index < 0)
            {
                return false;
            }
            base.RemoveAt(index);
            return true;
        }

        private bool RemoveEndpointsWithTakenSystemAssignedPort(string vDir)
        {
            List<string> listenUrlsToExcludeFromTest = new List<string>();
            foreach (ListenAddress address in this)
            {
                if (address.EndpointUsesSystemAssignedPort())
                {
                    listenUrlsToExcludeFromTest.Add(address.RawAddress);
                }
            }
            return (this.FilterOutTakenListenUrls(vDir, listenUrlsToExcludeFromTest) != null);
        }
    }
}

