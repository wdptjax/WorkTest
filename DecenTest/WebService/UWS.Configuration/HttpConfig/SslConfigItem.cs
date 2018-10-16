namespace HttpConfig
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text;
    using UWS.Configuration;
    using UWS.Configuration.Properties;
    using UWS.Framework;

    public class SslConfigItem
    {
        private IPAddress _address;
        private Guid _appId;
        private HttpConfig.HttpApi.ClientCertCheckMode _certCheckMode;
        private string _certStoreName;
        private HttpConfig.HttpApi.SslConfigFlag _flags;
        private byte[] _hash;
        private ushort _port;
        private int _revocationFreshnessTime;
        private int _revocationUrlRetrievalTimeout;
        private string _sslCtlIdentifier;
        private string _sslCtlStoreName;
        private const string defaultCertStoreName = "MY";
        private bool needUpdate;
        private bool presentInHttpCfg;

        public SslConfigItem()
        {
            this.needUpdate = true;
            this._address = new IPAddress(0L);
            this._appId = Guid.Empty;
            this._certStoreName = "MY";
        }

        public SslConfigItem(SslConfigItem toClone, bool copyPersistenceInfo = false)
        {
            this.needUpdate = true;
            this._address = new IPAddress(0L);
            this._appId = Guid.Empty;
            this._certStoreName = "MY";
            if (toClone == null)
            {
                throw new ArgumentNullException("toClone");
            }
            if (copyPersistenceInfo)
            {
                this.presentInHttpCfg = toClone.presentInHttpCfg;
                this.needUpdate = toClone.needUpdate;
            }
            this._address = toClone._address;
            this._port = toClone._port;
            this._hash = toClone._hash;
            this._appId = toClone._appId;
            this._certStoreName = toClone._certStoreName;
            this._revocationFreshnessTime = toClone._revocationFreshnessTime;
            this._revocationUrlRetrievalTimeout = toClone._revocationUrlRetrievalTimeout;
            this._sslCtlIdentifier = toClone._sslCtlIdentifier;
            this._sslCtlStoreName = toClone._sslCtlStoreName;
            this._certCheckMode = toClone._certCheckMode;
            this._flags = toClone._flags;
        }

        internal void ApplyConfig(ConfigItemAction action)
        {
            IntPtr zero = IntPtr.Zero;
            HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_SSL_SET structure = this.ToStruct();
            try
            {
                zero = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
                Marshal.StructureToPtr(structure, zero, false);
                if (this.presentInHttpCfg)
                {
                    HttpConfig.HttpApi.HttpDeleteServiceConfiguration(IntPtr.Zero, HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_ID.HttpServiceConfigSSLCertInfo, zero, Marshal.SizeOf(structure), IntPtr.Zero);
                }
                if ((action == ConfigItemAction.Update) || (action == ConfigItemAction.Create))
                {
                    ErrorCheck.VerifySuccess(HttpConfig.HttpApi.HttpSetServiceConfiguration(IntPtr.Zero, HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_ID.HttpServiceConfigSSLCertInfo, zero, Marshal.SizeOf(structure), IntPtr.Zero), "HttpSetServiceConfiguration (SSL) failed.", new object[0]);
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.DestroyStructure(zero, typeof(HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_SSL_SET));
                    Marshal.FreeHGlobal(zero);
                }
                FreeStruct(structure);
            }
        }

        private static SslConfigItem Deserialize(IntPtr pSslConfigSetStruct)
        {
            SslConfigItem item = new SslConfigItem();
            HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_SSL_SET http_service_config_ssl_set = (HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_SSL_SET) Marshal.PtrToStructure(pSslConfigSetStruct, typeof(HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_SSL_SET));
            item._port = (ushort) IPAddress.NetworkToHostOrder(Marshal.ReadInt16(http_service_config_ssl_set.KeyDesc.pIpPort, 2));
            item._address = new IPAddress(Marshal.ReadInt32(http_service_config_ssl_set.KeyDesc.pIpPort, 4) & ((long) 0xffffffffL));
            item._appId = http_service_config_ssl_set.ParamDesc.AppId;
            item._certStoreName = http_service_config_ssl_set.ParamDesc.pSslCertStoreName ?? "MY";
            item._certCheckMode = http_service_config_ssl_set.ParamDesc.CertCheckMode;
            item._revocationFreshnessTime = http_service_config_ssl_set.ParamDesc.RevocationFreshnessTime;
            item._revocationUrlRetrievalTimeout = http_service_config_ssl_set.ParamDesc.RevocationUrlRetrievalTimeout;
            item._sslCtlIdentifier = http_service_config_ssl_set.ParamDesc.pSslCtlIdentifier;
            item._sslCtlStoreName = http_service_config_ssl_set.ParamDesc.pSslCtlStoreName;
            item._flags = http_service_config_ssl_set.ParamDesc.Flags;
            item._hash = new byte[http_service_config_ssl_set.ParamDesc.SslHashLength];
            Marshal.Copy(http_service_config_ssl_set.ParamDesc.pSslHash, item._hash, 0, item._hash.Length);
            item.presentInHttpCfg = true;
            item.needUpdate = false;
            return item;
        }

        public override bool Equals(object obj)
        {
            SslConfigItem item = obj as SslConfigItem;
            if (item == null)
            {
                return false;
            }
            return (((((this.Port == item.Port) && this.Address.Equals(item.Address)) && ((this.CertStoreName == item.CertStoreName) && (this.AppId == item.AppId))) && ((this.HashString == item.HashString) && (this.CertCheckMode == item.CertCheckMode))) && (this.Flags == item.Flags));
        }

        private static void FreeStruct(HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_SSL_SET sslStruct)
        {
            Marshal.FreeHGlobal(sslStruct.KeyDesc.pIpPort);
            Marshal.FreeHGlobal(sslStruct.ParamDesc.pSslHash);
        }

        public static List<SslConfigItem> GetApplicationSslItems(Guid appID, Dictionary<string, SslConfigItem> sslConfig = null)
        {
            if (sslConfig == null)
            {
                sslConfig = LoadHttpSysSSLInfo();
            }
            List<SslConfigItem> list = new List<SslConfigItem>();
            foreach (SslConfigItem item in sslConfig.Values)
            {
                if (item.AppId == appID)
                {
                    list.Add(item);
                }
            }
            return list;
        }

        public static SslConfigItem LoadConfigItem(IPAddress address, ushort port, ref Dictionary<string, SslConfigItem> sslConfig)
        {
            SslConfigItem item;
            if (sslConfig == null)
            {
                sslConfig = LoadHttpSysSSLInfo();
            }
            if (!sslConfig.TryGetValue(MakeSearchKey(address, port), out item))
            {
                return null;
            }
            return item;
        }

        public static Dictionary<string, SslConfigItem> LoadHttpSysSSLInfo()
        {
            Dictionary<string, SslConfigItem> dictionary = new Dictionary<string, SslConfigItem>();
            try
            {
                using (new HttpConfig.HttpApi())
                {
                    HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_SSL_QUERY structure = new HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_SSL_QUERY {
                        QueryDesc = HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_QUERY_TYPE.HttpServiceConfigQueryNext
                    };
                    IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_SSL_QUERY)));
                    try
                    {
                        structure.dwToken = 0;
                        while (true)
                        {
                            Marshal.StructureToPtr(structure, ptr, false);
                            int requiredBufferLength = 0;
                            HttpConfig.HttpApi.Error error = QueryServiceConfig(ptr, IntPtr.Zero, 0, out requiredBufferLength);
                            if (error == HttpConfig.HttpApi.Error.ERROR_NO_MORE_ITEMS)
                            {
                                return dictionary;
                            }
                            if (error != HttpConfig.HttpApi.Error.ERROR_INSUFFICIENT_BUFFER)
                            {
                                ErrorCheck.VerifySuccess(error, "HttpQueryServiceConfiguration (SSL) failed.", new object[0]);
                            }
                            IntPtr dest = Marshal.AllocHGlobal(requiredBufferLength);
                            try
                            {
                                HttpConfig.HttpApi.ZeroMemory(dest, requiredBufferLength);
                                ErrorCheck.VerifySuccess(QueryServiceConfig(ptr, dest, requiredBufferLength, out requiredBufferLength), "HttpQueryServiceConfiguration (SSL) failed.", new object[0]);
                                SslConfigItem item = Deserialize(dest);
                                dictionary[item.Key] = item;
                            }
                            finally
                            {
                                Marshal.FreeHGlobal(dest);
                            }
                            structure.dwToken++;
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                    return dictionary;
                }
            }
            catch (Exception exception)
            {
                Trace.TraceError("Failed to load HTTP.SYS SSL configuration due to: {0}", new object[] { exception });
            }
            return dictionary;
        }

        public static string MakeSearchKey(IPAddress addreess, ushort port)
        {
            if (addreess == null)
            {
                addreess = IPAddress.Any;
            }
            return string.Format("{0}:{1}", addreess, port);
        }

        private static HttpConfig.HttpApi.Error QueryServiceConfig(IntPtr pInput, IntPtr pOutput, int outputLength, out int requiredBufferLength)
        {
            return HttpConfig.HttpApi.HttpQueryServiceConfiguration(IntPtr.Zero, HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_ID.HttpServiceConfigSSLCertInfo, pInput, Marshal.SizeOf(typeof(HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_SSL_QUERY)), pOutput, outputLength, out requiredBufferLength, IntPtr.Zero);
        }

        public ListenAddress ToListenAddress()
        {
            return new ListenAddress(this.Port, this.Address);
        }

        private HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_SSL_SET ToStruct()
        {
            HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_SSL_SET http_service_config_ssl_set = new HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_SSL_SET {
                KeyDesc = { pIpPort = HttpConfig.HttpApi.BuildSockaddr(2, this._port, this._address) }
            };
            if (this._hash != null)
            {
                http_service_config_ssl_set.ParamDesc.pSslHash = Marshal.AllocHGlobal(this._hash.Length);
                Marshal.Copy(this._hash, 0, http_service_config_ssl_set.ParamDesc.pSslHash, this._hash.Length);
                http_service_config_ssl_set.ParamDesc.SslHashLength = this._hash.Length;
            }
            http_service_config_ssl_set.ParamDesc.AppId = this._appId;
            http_service_config_ssl_set.ParamDesc.pSslCertStoreName = this._certStoreName;
            http_service_config_ssl_set.ParamDesc.RevocationFreshnessTime = this._revocationFreshnessTime;
            http_service_config_ssl_set.ParamDesc.RevocationUrlRetrievalTimeout = this._revocationUrlRetrievalTimeout;
            http_service_config_ssl_set.ParamDesc.pSslCtlIdentifier = this._sslCtlIdentifier;
            http_service_config_ssl_set.ParamDesc.pSslCtlStoreName = this._sslCtlStoreName;
            http_service_config_ssl_set.ParamDesc.Flags = this._flags;
            http_service_config_ssl_set.ParamDesc.CertCheckMode = this._certCheckMode;
            return http_service_config_ssl_set;
        }

        public IPAddress Address
        {
            get
            {
                return this._address;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Address", "Address cannot be null");
                }
                this._address = value;
            }
        }

        public string AddressString
        {
            get
            {
                if (this.Address == null)
                {
                    return string.Empty;
                }
                if (!this.Address.Equals(IPAddress.Any) && !this.Address.Equals(IPAddress.IPv6Any))
                {
                    return SystemUtilites.IpAddressToString(this.Address);
                }
                return Resources.AnySslIP;
            }
        }

        public Guid AppId
        {
            get
            {
                return this._appId;
            }
            set
            {
                this._appId = value;
            }
        }

        internal HttpConfig.HttpApi.ClientCertCheckMode CertCheckMode
        {
            get
            {
                return this._certCheckMode;
            }
            set
            {
                this._certCheckMode = value;
            }
        }

        public string CertStoreName
        {
            get
            {
                return this._certStoreName;
            }
            set
            {
                this._certStoreName = value;
            }
        }

        public bool ClientCertificate
        {
            get
            {
                return ((this.Flags & HttpConfig.HttpApi.SslConfigFlag.NegotiateClientCertificates) == HttpConfig.HttpApi.SslConfigFlag.NegotiateClientCertificates);
            }
            set
            {
                if (value)
                {
                    this.Flags |= HttpConfig.HttpApi.SslConfigFlag.NegotiateClientCertificates;
                }
                else
                {
                    this.Flags &= ~HttpConfig.HttpApi.SslConfigFlag.NegotiateClientCertificates;
                }
            }
        }

        internal HttpConfig.HttpApi.SslConfigFlag Flags
        {
            get
            {
                return this._flags;
            }
            set
            {
                this._flags = value;
            }
        }

        public byte[] Hash
        {
            get
            {
                return this._hash;
            }
            set
            {
                this._hash = value;
            }
        }

        public string HashString
        {
            get
            {
                if (this._hash == null)
                {
                    return null;
                }
                StringBuilder builder = new StringBuilder(this._hash.Length * 2);
                foreach (byte num in this._hash)
                {
                    builder.AppendFormat("{0:X2}", num);
                }
                return builder.ToString();
            }
            set
            {
                if (value == null)
                {
                    this._hash = null;
                }
                else
                {
                    this._hash = new byte[value.Length / 2];
                    for (int i = 0; i < value.Length; i += 2)
                    {
                        string s = value.Substring(i, 2);
                        this._hash[i / 2] = byte.Parse(s, NumberStyles.HexNumber);
                    }
                }
            }
        }

        public string Key
        {
            get
            {
                return MakeSearchKey(this._address, this._port);
            }
        }

        public bool NeedUpdate
        {
            get
            {
                return this.needUpdate;
            }
            set
            {
                this.needUpdate = value;
            }
        }

        public bool Persisted
        {
            get
            {
                return !this.NeedUpdate;
            }
            set
            {
                this.needUpdate = false;
            }
        }

        public ushort Port
        {
            get
            {
                return this._port;
            }
            set
            {
                this._port = value;
            }
        }

        public int RevocationFreshnessTime
        {
            get
            {
                return this._revocationFreshnessTime;
            }
            set
            {
                this._revocationFreshnessTime = value;
            }
        }

        public int RevocationUrlRetrievalTimeout
        {
            get
            {
                return this._revocationUrlRetrievalTimeout;
            }
            set
            {
                this._revocationUrlRetrievalTimeout = value;
            }
        }

        public string SslCtlIdentifier
        {
            get
            {
                return this._sslCtlIdentifier;
            }
            set
            {
                this._sslCtlIdentifier = value;
            }
        }

        public string SslCtlStoreName
        {
            get
            {
                return this._sslCtlStoreName;
            }
            set
            {
                this._sslCtlStoreName = value;
            }
        }
    }
}

