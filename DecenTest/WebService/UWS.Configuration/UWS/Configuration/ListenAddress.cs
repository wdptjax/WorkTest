namespace UWS.Configuration
{
    using HttpConfig;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;
    using UWS.Configuration.Properties;
    using UWS.Framework;

    [Serializable, ComVisible(false)]
    public class ListenAddress
    {
        protected const string fakePlusHost = "pluscaaaaannnnnttttthhhhaaaaappppppppppeeeeeeennnnn";
        protected const string fakeStarHost = "starcaaaaannnnnttttthhhhaaaaappppppppppeeeeeeennnnn";
        private string rawAddress;
        [NonSerialized, DefaultValue((string) null), XmlIgnore]
        public SslConfigItem sslRegistration;

        public ListenAddress()
        {
        }

        public ListenAddress(string address) : this(address, null)
        {
        }

        public ListenAddress(ushort port) : this(port, (IPAddress) null)
        {
        }

        public ListenAddress(ListenAddress addressToClone)
        {
            this.RawAddress = addressToClone.RawAddress;
            this.SystemAssignedPort = addressToClone.SystemAssignedPort;
            this.SslIpAddress = addressToClone.SslIpAddress;
        }

        public ListenAddress(string address, IPAddress sslIpAddress)
        {
            if (sslIpAddress != null)
            {
                if (!address.ToLowerInvariant().StartsWith("https://"))
                {
                    throw new ArgumentException(string.Format("Listen address \"{0}\" is invalid. When sslIpAddress address is specified, listen adress protocol must be \"https://\".", address));
                }
            }
            else if (!address.ToLowerInvariant().StartsWith("http://"))
            {
                throw new ArgumentException(string.Format("Listen address \"{0}\" is invalid. When sslIpAddress address is not specified, listen adress can not be \"https://\".\r\n\r\nPlease note that registration of https/SSL endpoints programmatically or via command-line interface is not allowed because that would require redistributing server certificates, including private keys, which would be a security breach.\r\n\r\n", address));
            }
            this.RawAddress = address;
            this.SslIpAddress = sslIpAddress;
        }

        [Obsolete("Starting with build 15, another constructor, ListenAddress(ushort port, IPAddress sslIpAddress), should be used instead.")]
        public ListenAddress(ushort port, bool isSsl) : this(PortToListenAddress(port, isSsl))
        {
        }

        public ListenAddress(ushort port, IPAddress sslIpAddress) : this(PortToListenAddress(port, sslIpAddress != null), sslIpAddress)
        {
        }

        public bool AssignSystemPort()
        {
            if (this.GetPort() != 0)
            {
                return false;
            }
            this.rawAddress = this.MassageEndpointAddress(this.RawAddress, true);
            return true;
        }

        protected virtual int ConvertPortZero()
        {
            if (this.SystemAssignedPort < 1)
            {
                this.SystemAssignedPort = SystemUtilites.FindFreeTcpPort();
            }
            return this.SystemAssignedPort;
        }

        internal bool EndpointUsesSystemAssignedPort()
        {
            return this.EndpointUsesSystemAssignedPort(this.RawAddress);
        }

        internal bool EndpointUsesSystemAssignedPort(string endpoint)
        {
            string str;
            if (this.SystemAssignedPort < 1)
            {
                return false;
            }
            return (ValidateUrlWithWildcard(endpoint, out str).Port == this.SystemAssignedPort);
        }

        public override bool Equals(object obj)
        {
            ListenAddress address = obj as ListenAddress;
            if (address == null)
            {
                return false;
            }
            return (this.RawAddress == address.RawAddress);
        }

        internal static string FormatUrl(Uri uri, string host, string vDir)
        {
            return string.Format("{0}://{1}:{2}/{3}", new object[] { uri.Scheme, host, uri.Port, vDir });
        }

        public int GetPort()
        {
            return this.GetUriWithBogusHost().Port;
        }

        public Uri GetUri(out string host)
        {
            Uri uri = ValidateUrlWithWildcard(this.RawAddress, out host);
            switch (uri.Host)
            {
                case "pluscaaaaannnnnttttthhhhaaaaappppppppppeeeeeeennnnn":
                    host = "+";
                    return uri;

                case "starcaaaaannnnnttttthhhhaaaaappppppppppeeeeeeennnnn":
                    host = "*";
                    return uri;
            }
            host = uri.Host;
            return uri;
        }

        public Uri GetUri(string actualHostForWildcardAddress)
        {
            return new Uri(ReplaceEndpontAddressWildcard(this.RawAddress, actualHostForWildcardAddress, actualHostForWildcardAddress));
        }

        internal Uri GetUriWithBogusHost()
        {
            string str;
            return ValidateUrlWithWildcard(this.RawAddress, out str);
        }

        public static bool IsListenUrlFree(string listenUrl)
        {
            bool flag;
            HttpListener listener = new HttpListener {
                Prefixes = { listenUrl }
            };
            try
            {
                listener.Start();
                listener.Stop();
                flag = true;
            }
            catch (HttpListenerException exception)
            {
                if (exception.ErrorCode == 5)
                {
                    throw new Exception(string.Format("Need to run as administrator/elevated to test whether HttpListener can listen to \"{0}\".", listenUrl), exception);
                }
                flag = false;
            }
            catch (Exception)
            {
                flag = false;
            }
            finally
            {
                listener.Close();
            }
            return flag;
        }

        public bool IsUrlFree(string vDir)
        {
            return IsListenUrlFree(this.ToListenUrl(vDir));
        }

        public bool IsWildcardAddress()
        {
            string str;
            this.GetUri(out str);
            if (str != "*")
            {
                return (str == "+");
            }
            return true;
        }

        private static string MakeUrl(string vDir, string listenAddress, string hostIfWildcard)
        {
            string str;
            if (string.IsNullOrEmpty(vDir) || (vDir == "/"))
            {
                vDir = string.Empty;
            }
            else
            {
                vDir = vDir.Trim(new char[] { '/' }) + "/";
            }
            Uri uri = ValidateUrlWithWildcard(listenAddress, out str);
            if (!string.IsNullOrEmpty(hostIfWildcard) && ((str == "*") || (str == "+")))
            {
                str = hostIfWildcard;
            }
            return FormatUrl(uri, str, vDir);
        }

        private string MassageEndpointAddress(string endpoint, bool convertPortZero)
        {
            string str;
            Uri uri = ValidateUrlWithWildcard(endpoint, out str);
            if (uri == null)
            {
                throw new ApplicationException(string.Format("\"{0}\" is not a valid listen endpoint address.", endpoint));
            }
            if (!string.IsNullOrEmpty(uri.PathAndQuery) && (uri.PathAndQuery != "/"))
            {
                throw new ApplicationException(string.Format("Listen endpoint \"{0}\" contains invalid part \"{1}\". It can contain only protocol, address and port, like http://contoso.com:8080/. Virtual directory is specified as a separate parameter \"vdir\" and query string cannot be used at all.", endpoint, uri.PathAndQuery));
            }
            int port = uri.Port;
            if ((port == 0) && convertPortZero)
            {
                port = this.ConvertPortZero();
            }
            return string.Format("{0}://{1}:{2}/", uri.Scheme, str, port);
        }

        internal static string PortToListenAddress(ushort port, bool ssl)
        {
            return string.Format("http{1}://*:{0}/", port, ssl ? "s" : string.Empty);
        }

        protected static string ReplaceEndpontAddressWildcard(string endpointAddress, string replacePlusWith, string replaceStarWith)
        {
            endpointAddress = endpointAddress.Replace("://+", "://" + replacePlusWith).Replace("://*", "://" + replaceStarWith);
            return endpointAddress;
        }

        public string ToListenUrl(string vDir)
        {
            return ToListenUrl(vDir, this.RawAddress);
        }

        internal static string ToListenUrl(string vDir, string listenAddress)
        {
            return MakeUrl(vDir, listenAddress, null);
        }

        public SslConfigItem ToSslConfigItem(Guid appID, bool createNewIfNoPersisted)
        {
            List<SslConfigItem> persistedAppSsl = null;
            return this.ToSslConfigItem(appID, ref persistedAppSsl, createNewIfNoPersisted);
        }

        public SslConfigItem ToSslConfigItem(Guid appID, ref List<SslConfigItem> persistedAppSsl, bool createNewIfNoPersisted)
        {
            if (this.SslIpAddress == null)
            {
                return null;
            }
            if (persistedAppSsl == null)
            {
                persistedAppSsl = SslConfigItem.GetApplicationSslItems(appID, null);
            }
            ushort port = (ushort) this.GetPort();
            foreach (SslConfigItem item in persistedAppSsl)
            {
                if ((item.AppId != appID) && (appID != Guid.Empty))
                {
                    throw new Exception("Trying to convert ListenAddress object to SslConfigItem belonging to different applications.");
                }
                if ((item.Port == port) && this.SslIpAddress.Equals(item.Address))
                {
                    return item;
                }
            }
            if (!createNewIfNoPersisted)
            {
                return null;
            }
            return new SslConfigItem { 
                Address = this.SslIpAddress,
                AppId = appID,
                Port = (ushort) this.GetPort(),
                CertStoreName = "MY"
            };
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.RawAddress))
            {
                return this.RawAddress;
            }
            return ReplaceEndpontAddressWildcard(this.RawAddress, Resources.HttpListenerPlusWildcardUI, Resources.HttpListenerStarWildcardUI);
        }

        public string ToUIUrl(string hostIfWildcard, string vDir)
        {
            return MakeUrl(vDir, this.RawAddress, hostIfWildcard);
        }

        protected static Uri ValidateUrlWithoutWildcard(string urlWithoutWildcard, out string host)
        {
            IPAddress address;
            host = null;
            Uri uri = null;
            try
            {
                uri = new Uri(urlWithoutWildcard);
            }
            catch
            {
                return null;
            }
            if (IPAddress.TryParse(uri.Host, out address))
            {
                host = address.ToString();
                if (address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    host = string.Format("[{0}]", host);
                }
                return uri;
            }
            host = uri.Host.Replace("pluscaaaaannnnnttttthhhhaaaaappppppppppeeeeeeennnnn", "+").Replace("starcaaaaannnnnttttthhhhaaaaappppppppppeeeeeeennnnn", "*");
            return uri;
        }

        public static Uri ValidateUrlWithWildcard(string urlWithWildcard, out string host)
        {
            return ValidateUrlWithoutWildcard(ReplaceEndpontAddressWildcard(urlWithWildcard, "pluscaaaaannnnnttttthhhhaaaaappppppppppeeeeeeennnnn", "starcaaaaannnnnttttthhhhaaaaappppppppppeeeeeeennnnn"), out host);
        }

        [XmlIgnore]
        public bool IsSSL
        {
            get
            {
                return (this.SslIpAddress != null);
            }
        }

        [XmlAttribute("Address")]
        public string RawAddress
        {
            get
            {
                return this.rawAddress;
            }
            set
            {
                this.rawAddress = this.MassageEndpointAddress(value, false);
            }
        }

        [XmlAttribute, DefaultValue((string) null)]
        public string SslAddress
        {
            get
            {
                if (this.SslIpAddress != null)
                {
                    return SystemUtilites.IpAddressToString(this.SslIpAddress);
                }
                return null;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.SslIpAddress = null;
                }
                else
                {
                    this.SslIpAddress = IPAddress.Parse(value);
                }
            }
        }

        [XmlIgnore]
        public IPAddress SslIpAddress { get; set; }

        [XmlAttribute, DefaultValue(0)]
        public ushort SystemAssignedPort { get; set; }
    }
}

