namespace HttpConfig
{
    using System;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class HttpApi : IDisposable
    {
        private static volatile bool initialized = false;
        private static object locker = new object();
        private static volatile int refCount = 0;

        public HttpApi()
        {
            lock (locker)
            {
                if (!initialized)
                {
                    ErrorCheck.VerifySuccess(HttpInitialize(new HTTPAPI_VERSION(1, 0), InitFlag.HTTP_INITIALIZE_CONFIG, IntPtr.Zero), "HttpAPI Initialization", new object[0]);
                    initialized = true;
                }
                refCount++;
            }
        }

        internal static IntPtr BuildSockaddr(short family, ushort port, IPAddress address)
        {
            int cb = Marshal.SizeOf(typeof(sockaddr));
            IntPtr dest = Marshal.AllocHGlobal(cb);
            ZeroMemory(dest, cb);
            Marshal.WriteInt16(dest, family);
            ushort num2 = (ushort) IPAddress.HostToNetworkOrder((short) port);
            Marshal.WriteInt16(dest, 2, (short) num2);
            byte[] addressBytes = address.GetAddressBytes();
            IntPtr destination = IncIntPtr(dest, 4);
            Marshal.Copy(addressBytes, 0, destination, addressBytes.Length);
            return dest;
        }

        public void Dispose()
        {
            lock (locker)
            {
                refCount--;
                if (((refCount == 0) && initialized) && (HttpTerminate(InitFlag.HTTP_INITIALIZE_CONFIG, IntPtr.Zero) == Error.NO_ERROR))
                {
                    initialized = false;
                }
            }
        }

        [DllImport("Httpapi.dll")]
        internal static extern Error HttpDeleteServiceConfiguration(IntPtr ServiceHandle, HTTP_SERVICE_CONFIG_ID ConfigId, IntPtr pConfigInformation, int ConfigInformationLength, IntPtr pOverlapped);
        [DllImport("Httpapi.dll")]
        internal static extern Error HttpInitialize(HTTPAPI_VERSION version, InitFlag flags, IntPtr reserved);
        [DllImport("Httpapi.dll")]
        internal static extern Error HttpQueryServiceConfiguration(IntPtr ServiceHandle, HTTP_SERVICE_CONFIG_ID ConfigId, IntPtr pInputConfigInfo, int InputConfigInfoLength, IntPtr pOutputConfigInfo, int OutputConfigInfoLength, out int pReturnLength, IntPtr pOverlapped);
        [DllImport("Httpapi.dll")]
        internal static extern Error HttpSetServiceConfiguration(IntPtr ServiceHandle, HTTP_SERVICE_CONFIG_ID ConfigId, IntPtr pConfigInformation, int ConfigInformationLength, IntPtr pOverlapped);
        [DllImport("Httpapi.dll")]
        internal static extern Error HttpTerminate(InitFlag flags, IntPtr reserved);
        internal static IntPtr IncIntPtr(IntPtr ptr, int count)
        {
            return (IntPtr) (((int) ptr) + count);
        }

        [DllImport("Kernel32.dll", EntryPoint="RtlZeroMemory")]
        internal static extern void ZeroMemory(IntPtr dest, int size);

        internal enum ClientCertCheckMode
        {
            CachedRevocationOnly = 2,
            EnableRevocation = 0,
            NoUsageCheck = 0x10000,
            NoVerifyRevocation = 1,
            UseRevocationFreshnessTime = 4
        }

        internal enum Error
        {
            ERROR_ALREADY_EXISTS = 0xb7,
            ERROR_FILE_NOT_FOUND = 2,
            ERROR_HANDLE_EOF = 0x26,
            ERROR_INSUFFICIENT_BUFFER = 0x7a,
            ERROR_INVALID_DATA = 13,
            ERROR_INVALID_DLL = 0x482,
            ERROR_INVALID_HANDLE = 6,
            ERROR_INVALID_PARAMETER = 0x57,
            ERROR_NO_MORE_ITEMS = 0x103,
            ERROR_NOT_FOUND = 0x490,
            NO_ERROR = 0
        }

        internal enum HTTP_SERVICE_CONFIG_ID
        {
            HttpServiceConfigIPListenList,
            HttpServiceConfigSSLCertInfo,
            HttpServiceConfigUrlAclInfo
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_SERVICE_CONFIG_IP_LISTEN_PARAM
        {
            internal short AddrLength;
            internal IntPtr pAddress;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_SERVICE_CONFIG_IP_LISTEN_QUERY
        {
            internal int AddrCount;
            internal HttpConfig.HttpApi.SOCKADDR_STORAGE AddrList;
        }

        internal enum HTTP_SERVICE_CONFIG_QUERY_TYPE
        {
            HttpServiceConfigQueryExact,
            HttpServiceConfigQueryNext
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_SERVICE_CONFIG_SSL_KEY
        {
            internal IntPtr pIpPort;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_SERVICE_CONFIG_SSL_PARAM
        {
            internal int SslHashLength;
            internal IntPtr pSslHash;
            internal Guid AppId;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pSslCertStoreName;
            internal HttpConfig.HttpApi.ClientCertCheckMode CertCheckMode;
            internal int RevocationFreshnessTime;
            internal int RevocationUrlRetrievalTimeout;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pSslCtlIdentifier;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pSslCtlStoreName;
            internal HttpConfig.HttpApi.SslConfigFlag Flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_SERVICE_CONFIG_SSL_QUERY
        {
            internal HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_QUERY_TYPE QueryDesc;
            internal HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_SSL_KEY KeyDesc;
            internal int dwToken;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_SERVICE_CONFIG_SSL_SET
        {
            internal HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_SSL_KEY KeyDesc;
            internal HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_SSL_PARAM ParamDesc;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_SERVICE_CONFIG_URLACL_KEY
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pUrlPrefix;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_SERVICE_CONFIG_URLACL_PARAM
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pStringSecurityDescriptor;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_SERVICE_CONFIG_URLACL_QUERY
        {
            internal HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_QUERY_TYPE QueryDesc;
            internal HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_URLACL_KEY KeyDesc;
            internal int dwToken;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_SERVICE_CONFIG_URLACL_SET
        {
            internal HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_URLACL_KEY KeyDesc;
            internal HttpConfig.HttpApi.HTTP_SERVICE_CONFIG_URLACL_PARAM ParamDesc;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTPAPI_VERSION
        {
            internal ushort major;
            internal ushort minor;
            internal HTTPAPI_VERSION(ushort majorVersion, ushort minorVersion)
            {
                this.major = majorVersion;
                this.minor = minorVersion;
            }
        }

        internal enum InitFlag
        {
            HTTP_INITIALIZE_CONFIG = 2,
            HTTP_INITIALIZE_SERVER = 1
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct sockaddr
        {
            internal short sa_family;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=14, ArraySubType=UnmanagedType.U1)]
            private char[] sa_data;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SOCKADDR_STORAGE
        {
            internal short sa_family;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=6, ArraySubType=UnmanagedType.U1)]
            internal byte[] __ss_pad1;
            internal long __ss_align;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x70, ArraySubType=UnmanagedType.U1)]
            internal byte[] __ss_pad2;
        }

        [Flags]
        internal enum SslConfigFlag
        {
            DoNotRouteToRawIsapiFilters = 4,
            NegotiateClientCertificates = 2,
            None = 0,
            UseDSMapper = 1
        }
    }
}

