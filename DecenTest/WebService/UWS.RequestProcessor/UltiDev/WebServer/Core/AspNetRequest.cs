namespace UltiDev.WebServer.Core
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;
    using UWS.Configuration;
    using UWS.Framework;

    public class AspNetRequest : SimpleWorkerRequest
    {
        private WebAppDomain appDomain;
        private string appPathTranslated;
        private WindowsIdentity basicWindowsAuthIdentity;
        private int byteWritten;
        private bool chunkedCompression;
        private bool clientAcceptsGZip;
        private X509Certificate2 clientCert;
        private bool clientCertAttempted;
        internal ManualResetEvent completionSignal;
        private MemoryStream compressionBuffer;
        private static readonly Regex compressionHeaderValues = new Regex("(?:gzip)|(?:deflate)|(?:bzip2)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private ChunkedTransferDecoder dechunker;
        private byte[] deferred404Response;
        private WindowsImpersonationContext impersonationContextForStaticContent;
        private bool incorrectByteRangeHeader;
        private bool? isStaticContent;
        private const int MaxChunkLength = 0x10000;
        private const int MaxUnchunkedStaticResponse = 0x200000;
        private bool outOfBusiness;
        private Stream outputStream;
        private string pathInfo;
        private string processUserName;
        private HttpListenerContext requestContext;
        private string requestPath;
        private static readonly PropertyInfo sentHeadersPi = typeof(HttpListenerResponse).GetProperty("SentHeaders", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly List<string> staticHtmlFileExtensions = new List<string>(new string[] { "htm", "html" });
        private string translatedFileSystemPath;
        private AspNetHostingPermissionLevel? trustLevel;
        private static readonly string uniq = Guid.NewGuid().ToString();
        private IntPtr? WindowsIdentityToken;

        public AspNetRequest(WebAppDomain appDomain, HttpListenerContext requestContext, string processUserName) : base(GetRelativeUrl(requestContext.Request.Url.AbsolutePath), GetQueryString(requestContext), null)
        {
            this.basicWindowsAuthIdentity = null;
            this.impersonationContextForStaticContent = null;
            this.appDomain = null;
            this.requestContext = null;
            this.processUserName = null;
            this.requestPath = null;
            this.translatedFileSystemPath = null;
            this.pathInfo = string.Empty;
            this.appPathTranslated = null;
            this.incorrectByteRangeHeader = false;
            this.outOfBusiness = false;
            this.clientAcceptsGZip = false;
            this.byteWritten = 0;
            this.isStaticContent = null;
            this.dechunker = null;
            this.deferred404Response = null;
            this.compressionBuffer = null;
            this.outputStream = null;
            this.chunkedCompression = false;
            this.completionSignal = null;
            this.clientCert = null;
            this.clientCertAttempted = false;
            this.trustLevel = null;
            this.WindowsIdentityToken = null;
            using (new RequestMethodTracer(requestContext, "AspNetRequest() CONSTRUCTOR", new object[0]))
            {
                this.requestContext = requestContext;
                RequestDispatcher.LifecycleTrace(this.requestContext, "Entered AspNetRequest() constructor.", new object[0]);
                this.processUserName = processUserName;
                this.appDomain = appDomain;
                this.CheckClientForGZip();
            }
        }

        private void AddCompressedResponseHeader()
        {
            this.Response.Headers.Add("Content-Encoding", "gzip");
        }

        private void CheckClientForGZip()
        {
            string str = this.Request.Headers["Accept-Encoding"];
            if (!string.IsNullOrEmpty(str))
            {
                foreach (string str2 in str.ToLowerInvariant().Split(new char[] { ',', ' ', ';' }))
                {
                    if (str2 == "gzip")
                    {
                        this.clientAcceptsGZip = true;
                        break;
                    }
                }
            }
        }

        private bool CheckIfStaticContent()
        {
            string str3;
            if (this.Request.HttpMethod != "GET")
            {
                return false;
            }
            if (!string.IsNullOrEmpty(this.Request.Url.Query))
            {
                return false;
            }
            string fileName = this.MapPath(this.GetFilePath());
            if (fileName.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                return false;
            }
            FileInfo info = new FileInfo(fileName);
            if (info.Name.Length == 0)
            {
                return false;
            }
            string loweredFileExtNoDot = info.Extension.TrimStart(new char[] { '.' }).ToLowerInvariant();
            if (this.AppSettings.IsDynamicContentExtension(loweredFileExtNoDot))
            {
                return false;
            }
            if (!Extensions.IsStaticContent(loweredFileExtNoDot, out str3, this.appDomain.exeFolderPath))
            {
                return false;
            }
            if (!info.Exists)
            {
                return false;
            }
            this.Response.ContentType = str3;
            return true;
        }

        public override void CloseConnection()
        {
            base.CloseConnection();
        }

        public override void EndOfRequest()
        {
            using (new RequestMethodTracer(this.requestContext, "EndOfRequest()", new object[0]))
            {
                RequestDispatcher.LifecycleTrace(this.requestContext, "Entered AspNetRequest.EndOfRequest(). this.responseClosed = {0}", new object[] { this.outOfBusiness });
                this.GoOutOfBusiness();
            }
        }

        public override void FlushResponse(bool finalFlush)
        {
            using (new RequestMethodTracer(this.requestContext, "FlushResponse()", new object[0]))
            {
                if (this.outputStream != null)
                {
                    if (finalFlush && (this.deferred404Response != null))
                    {
                        if (this.Response.StatusCode == 0x194)
                        {
                            this.SendResponseFromMemory(this.deferred404Response, this.deferred404Response.Length);
                        }
                        this.deferred404Response = null;
                    }
                    this.outputStream.Flush();
                    if (finalFlush)
                    {
                        Trace.TraceInformation("Written {0} bytes (before compression, if any) in responding to \"{1}\".", new object[] { this.byteWritten, this.Request.Url });
                        if (this.IsBufferedCompression)
                        {
                            this.SendCompressionBuffer();
                        }
                    }
                }
            }
        }

        public override string GetAppPath()
        {
            return base.GetAppPath();
        }

        public override string GetAppPathTranslated()
        {
            using (new RequestMethodTracer(this.requestContext, "GetAppPathTranslated()", new object[0]))
            {
                if (this.appPathTranslated == null)
                {
                    this.appPathTranslated = this.MapPath(this.appDomain.VirtualPath).TrimEnd(new char[] { '\\' }) + @"\";
                }
                return this.appPathTranslated;
            }
        }

        public HttpListenerBasicIdentity GetBasicAuthIdentity()
        {
            if (this.requestContext.User != null)
            {
                return (this.requestContext.User.Identity as HttpListenerBasicIdentity);
            }
            return null;
        }

        private string GetBasicAuthPassword()
        {
            HttpListenerBasicIdentity basicAuthIdentity = this.GetBasicAuthIdentity();
            if (basicAuthIdentity != null)
            {
                return basicAuthIdentity.Password;
            }
            return null;
        }

        public override byte[] GetClientCertificate()
        {
            using (new RequestMethodTracer(this.requestContext, "GetClientCertificate()", new object[0]))
            {
                if (this.ClientCert == null)
                {
                    return base.GetClientCertificate();
                }
                return this.ClientCert.RawData;
            }
        }

        public override byte[] GetClientCertificatePublicKey()
        {
            using (new RequestMethodTracer(this.requestContext, "GetClientCertificatePublicKey()", new object[0]))
            {
                if (this.ClientCert == null)
                {
                    return base.GetClientCertificatePublicKey();
                }
                return this.ClientCert.PublicKey.EncodedKeyValue.RawData;
            }
        }

        public override DateTime GetClientCertificateValidFrom()
        {
            using (new RequestMethodTracer(this.requestContext, "GetClientCertificateValidFrom()", new object[0]))
            {
                if (this.ClientCert == null)
                {
                    return base.GetClientCertificateValidFrom();
                }
                return this.ClientCert.NotBefore;
            }
        }

        public override DateTime GetClientCertificateValidUntil()
        {
            using (new RequestMethodTracer(this.requestContext, "GetClientCertificateValidUntil()", new object[0]))
            {
                if (this.ClientCert == null)
                {
                    return base.GetClientCertificateValidUntil();
                }
                return this.ClientCert.NotAfter;
            }
        }

        private IntPtr GetClientOrProcessUserToken()
        {
            WindowsIdentity clientUserWindowsIdentity = this.GetClientUserWindowsIdentity();
            if (clientUserWindowsIdentity == null)
            {
                return RequestDispatcher.processUserToken;
            }
            if (this.trustLevel.HasValue && (((AspNetHostingPermissionLevel) this.trustLevel.Value) < AspNetHostingPermissionLevel.Unrestricted))
            {
                return RequestDispatcher.processUserToken;
            }
            try
            {
                return clientUserWindowsIdentity.Token;
            }
            catch (SecurityException exception)
            {
                this.trustLevel = new AspNetHostingPermissionLevel?(GetCurrentTrustLevel());
                string str = exception.GetBaseException().ToString();
                Trace.TraceWarning(string.Format(Resources.SecurityExceptionWhileGettingWinUserID, this.trustLevel, str), new object[0]);
                return RequestDispatcher.processUserToken;
            }
        }

        private string GetClientUserAuthenticationType()
        {
            IIdentity clientUserIdentity = this.GetClientUserIdentity();
            if (clientUserIdentity == null)
            {
                return string.Empty;
            }
            return clientUserIdentity.AuthenticationType;
        }

        private IIdentity GetClientUserIdentity()
        {
            return ((this.requestContext.User == null) ? null : this.requestContext.User.Identity);
        }

        private string GetClientUserName()
        {
            IIdentity clientUserIdentity = this.GetClientUserIdentity();
            return ((clientUserIdentity == null) ? string.Empty : clientUserIdentity.Name);
        }

        private WindowsIdentity GetClientUserWindowsIdentity()
        {
            using (new RequestMethodTracer(this.requestContext, "GetClientUserWindowsIdentity()", new object[0]))
            {
                IIdentity clientUserIdentity = this.GetClientUserIdentity();
                if ((clientUserIdentity != null) && (clientUserIdentity is WindowsIdentity))
                {
                    return (clientUserIdentity as WindowsIdentity);
                }
                if (this.basicWindowsAuthIdentity != null)
                {
                    return this.basicWindowsAuthIdentity;
                }
                return null;
            }
        }

        private static AspNetHostingPermissionLevel GetCurrentTrustLevel()
        {
            foreach (AspNetHostingPermissionLevel level in new AspNetHostingPermissionLevel[] { AspNetHostingPermissionLevel.Unrestricted, AspNetHostingPermissionLevel.High, AspNetHostingPermissionLevel.Medium, AspNetHostingPermissionLevel.Low, AspNetHostingPermissionLevel.Minimal })
            {
                try
                {
                    new AspNetHostingPermission(level).Demand();
                }
                catch (SecurityException)
                {
                    continue;
                }
                return level;
            }
            return AspNetHostingPermissionLevel.None;
        }

        private string GetDirecotryDefaultFile(DirectoryInfo di)
        {
            foreach (string str in this.AppSettings.DefaultDocuments)
            {
                FileInfo[] files = di.GetFiles(str);
                if ((files != null) && (files.Length > 0))
                {
                    return files[0].Name;
                }
            }
            return null;
        }

        private string GetErrorResponseBody(HttpStatusCode statusCode, string message)
        {
            string str = Messages.FormatErrorMessageBody(statusCode, this.appDomain.VirtualPath);
            if (!string.IsNullOrEmpty(message))
            {
                str = string.Format("{0}\r\n<!--\r\n{1}\r\n-->", str, message);
            }
            return str;
        }

        public override string GetFilePath()
        {
            using (new RequestMethodTracer(this.requestContext, "GetFilePath()", new object[0]))
            {
                return this.requestPath;
            }
        }

        public override string GetFilePathTranslated()
        {
            using (new RequestMethodTracer(this.requestContext, "GetFilePathTranslated()", new object[0]))
            {
                return this.translatedFileSystemPath;
            }
        }

        public override string GetHttpVerbName()
        {
            using (RequestMethodTracer tracer = new RequestMethodTracer(this.requestContext, "GetHttpVerbName()", new object[0]))
            {
                tracer.retVal = this.Request.HttpMethod;
                return this.Request.HttpMethod;
            }
        }

        public override string GetHttpVersion()
        {
            using (new RequestMethodTracer(this.requestContext, "GetHttpVersion()", new object[0]))
            {
                return this.Protocol;
            }
        }

        public override string GetKnownRequestHeader(int index)
        {
            using (RequestMethodTracer tracer = new RequestMethodTracer(this.requestContext, "GetKnownRequestHeader({0})", new object[] { index }))
            {
                string userAgent = null;
                if (this.Request != null)
                {
                    if (index == 0x27)
                    {
                        userAgent = this.Request.UserAgent;
                    }
                    else
                    {
                        userAgent = this.Request.Headers[HttpWorkerRequest.GetKnownRequestHeaderName(index)];
                    }
                }
                tracer.retVal = userAgent;
                return userAgent;
            }
        }

        public override string GetLocalAddress()
        {
            using (new RequestMethodTracer(this.requestContext, "GetLocalAddress()", new object[0]))
            {
                if (this.Request == null)
                {
                    return null;
                }
                return this.Request.LocalEndPoint.Address.ToString();
            }
        }

        public override int GetLocalPort()
        {
            using (new RequestMethodTracer(this.requestContext, "GetLocalPort()", new object[0]))
            {
                if (this.Request == null)
                {
                    return -1;
                }
                return this.Request.LocalEndPoint.Port;
            }
        }

        public override string GetPathInfo()
        {
            using (new RequestMethodTracer(this.requestContext, "GetPathInfo()", new object[0]))
            {
                return this.pathInfo;
            }
        }

        public override string GetProtocol()
        {
            using (new RequestMethodTracer(this.requestContext, "GetProtocol()", new object[0]))
            {
                return base.GetProtocol();
            }
        }

        private static string GetQueryString(HttpListenerContext requestContext)
        {
            return requestContext.Request.Url.Query.TrimStart(new char[] { '?' });
        }

        public override string GetRawUrl()
        {
            using (RequestMethodTracer tracer = new RequestMethodTracer(this.requestContext, "GetRawUrl()", new object[0]))
            {
                tracer.retVal = this.Request.RawUrl;
                return this.Request.RawUrl;
            }
        }

        private static string GetRelativeUrl(string absUrl)
        {
            string appDomainAppVirtualPath = HttpRuntime.AppDomainAppVirtualPath;
            if (appDomainAppVirtualPath != "/")
            {
                string str2 = appDomainAppVirtualPath;
                if (absUrl == str2)
                {
                    return string.Empty;
                }
                if (absUrl.StartsWith(str2))
                {
                    absUrl = absUrl.Substring(str2.Length + 1);
                }
                if (absUrl.StartsWith("/"))
                {
                    absUrl = absUrl.Substring(1);
                }
            }
            return absUrl;
        }

        public override string GetRemoteAddress()
        {
            using (new RequestMethodTracer(this.requestContext, "GetRemoteAddress()", new object[0]))
            {
                if (this.Request == null)
                {
                    return null;
                }
                return this.Request.RemoteEndPoint.Address.ToString();
            }
        }

        public override int GetRemotePort()
        {
            using (new RequestMethodTracer(this.requestContext, "GetRemotePort()", new object[0]))
            {
                if (this.Request == null)
                {
                    return -1;
                }
                return this.Request.RemoteEndPoint.Port;
            }
        }

        public override string GetServerName()
        {
            using (new RequestMethodTracer(this.requestContext, "GetServerName()", new object[0]))
            {
                return this.Request.Url.Host;
            }
        }

        public override string GetServerVariable(string serverVarName)
        {
            using (RequestMethodTracer tracer = new RequestMethodTracer(this.requestContext, "GetServerVariable(\"{0}\")", new object[] { serverVarName }))
            {
                if (this.Request == null)
                {
                    return null;
                }
                string clientUserAuthenticationType = string.Empty;
                switch (serverVarName)
                {
                    case "ALL_RAW":
                        clientUserAuthenticationType = Extensions.ToHeaderString(this.Request.Headers);
                        break;

                    case "AUTH_TYPE":
                        clientUserAuthenticationType = this.GetClientUserAuthenticationType();
                        break;

                    case "AUTH_PASSWORD":
                        clientUserAuthenticationType = this.GetBasicAuthPassword();
                        break;

                    case "AUTH_USER":
                    case "REMOTE_USER":
                    case "UNMAPPED_REMOTE_USER":
                        clientUserAuthenticationType = this.GetClientUserName();
                        break;

                    case "LOGON_USER":
                        clientUserAuthenticationType = this.GetWindowsUserName();
                        break;

                    case "SERVER_PROTOCOL":
                        clientUserAuthenticationType = this.Protocol;
                        break;

                    case "SERVER_SOFTWARE":
                        clientUserAuthenticationType = RequestDispatcher.ServerName;
                        break;

                    case "CERT_FLAGS":
                        if (this.ClientCert != null)
                        {
                            clientUserAuthenticationType = "1";
                        }
                        break;

                    case "CERT_ISSUER":
                        if (this.ClientCert != null)
                        {
                            clientUserAuthenticationType = this.ClientCert.Issuer;
                        }
                        break;

                    case "CERT_SUBJECT":
                        if (this.ClientCert != null)
                        {
                            clientUserAuthenticationType = this.ClientCert.Subject;
                        }
                        break;

                    case "CERT_SERIALNUMBER":
                        if (this.ClientCert != null)
                        {
                            clientUserAuthenticationType = this.ClientCert.SerialNumber;
                        }
                        break;

                    case "CERT_COOKIE":
                        if (this.ClientCert != null)
                        {
                            clientUserAuthenticationType = this.ClientCert.GetCertHashString();
                        }
                        break;

                    case "HTTPS":
                        clientUserAuthenticationType = this.Request.IsSecureConnection ? "on" : "off";
                        break;

                    case "INSTANCE_ID":
                        clientUserAuthenticationType = this.appDomain.AppInstanceID;
                        break;

                    case "INSTANCE_META_PATH":
                        clientUserAuthenticationType = this.appDomain.AppInstanceMetaPath;
                        break;

                    case "APPL_MD_PATH":
                        clientUserAuthenticationType = this.appDomain.AppMdPath;
                        break;
                }
                if (clientUserAuthenticationType == null)
                {
                    clientUserAuthenticationType = string.Empty;
                }
                tracer.retVal = clientUserAuthenticationType;
                return clientUserAuthenticationType;
            }
        }

        public override string GetUnknownRequestHeader(string name)
        {
            using (RequestMethodTracer tracer = new RequestMethodTracer(this.requestContext, "GetUnknownRequestHeader(\"{0}\")", new object[] { name }))
            {
                string str = null;
                if (this.Request == null)
                {
                    str = this.Request.Headers[name];
                }
                tracer.retVal = str;
                return str;
            }
        }

        public override string[][] GetUnknownRequestHeaders()
        {
            using (new RequestMethodTracer(this.requestContext, "GetUnknownRequestHeaders()", new object[0]))
            {
                if (this.Request == null)
                {
                    return null;
                }
                NameValueCollection headers = this.Request.Headers;
                int count = headers.Count;
                List<string[]> list = new List<string[]>(count);
                for (int i = 0; i < count; i++)
                {
                    string key = headers.GetKey(i);
                    if (HttpWorkerRequest.GetKnownRequestHeaderIndex(key) == -1)
                    {
                        string str2 = headers.Get(i);
                        list.Add(new string[] { key, str2 });
                    }
                }
                return list.ToArray();
            }
        }

        public override string GetUriPath()
        {
            using (new RequestMethodTracer(this.requestContext, "GetUriPath()", new object[0]))
            {
                return (this.requestPath + this.pathInfo);
            }
        }

        public override IntPtr GetUserToken()
        {
            using (RequestMethodTracer tracer = new RequestMethodTracer(this.requestContext, "GetUserToken()", new object[0]))
            {
                if (!this.WindowsIdentityToken.HasValue)
                {
                    this.WindowsIdentityToken = new IntPtr?(this.GetClientOrProcessUserToken());
                }
                tracer.retVal = this.WindowsIdentityToken.Value.ToInt64();
                return this.WindowsIdentityToken.Value;
            }
        }

        private string GetWindowsUserName()
        {
            WindowsIdentity clientUserWindowsIdentity = this.GetClientUserWindowsIdentity();
            return ((clientUserWindowsIdentity == null) ? string.Empty : clientUserWindowsIdentity.Name);
        }

        private void GoOutOfBusiness()
        {
            using (new RequestMethodTracer(this.requestContext, "GoOutOfBusiness()", new object[0]))
            {
                RequestDispatcher.LifecycleTrace(this.requestContext, "Entered AspNetRequest.GoOutOfBusiness().", new object[0]);
                if (this.outOfBusiness)
                {
                    RequestDispatcher.LifecycleTrace(this.requestContext, "Repeated call of AspNetRequest.GoOutOfBusiness(). Do nothing.", new object[0]);
                }
                else
                {
                    this.outOfBusiness = true;
                    RequestDispatcher.LifecycleTrace(this.requestContext, "Starting AspNetRequest.GoOutOfBusiness() cleanup.", new object[0]);
                    if (this.IsBufferedCompression)
                    {
                        this.FlushResponse(true);
                    }
                    if (this.outputStream != null)
                    {
                        if (this.outputStream != this.Response.OutputStream)
                        {
                            this.outputStream.Dispose();
                        }
                        this.outputStream = null;
                    }
                    if (this.compressionBuffer != null)
                    {
                        this.compressionBuffer.Dispose();
                        this.compressionBuffer = null;
                    }
                    if (this.impersonationContextForStaticContent != null)
                    {
                        this.impersonationContextForStaticContent.Undo();
                        this.impersonationContextForStaticContent = null;
                    }
                    if (this.basicWindowsAuthIdentity != null)
                    {
                        WindowsAuthHelper.CloseHandle(this.basicWindowsAuthIdentity.Token);
                        this.basicWindowsAuthIdentity = null;
                    }
                    this.outputStream = null;
                    this.compressionBuffer = null;
                    this.appDomain = null;
                    this.requestPath = null;
                    this.translatedFileSystemPath = null;
                    this.pathInfo = null;
                    this.processUserName = null;
                    if (this.completionSignal != null)
                    {
                        this.completionSignal.Set();
                        this.completionSignal = null;
                    }
                    RequestDispatcher.LifecycleTrace(this.requestContext, "Leaving AspNetRequest.GoOutOfBusiness().", new object[0]);
                    this.requestContext = null;
                }
            }
        }

        public override bool HeadersSent()
        {
            if (sentHeadersPi != null)
            {
                object obj2 = sentHeadersPi.GetValue(this.Response, null);
                if (obj2 is bool)
                {
                    return (bool) obj2;
                }
            }
            return base.HeadersSent();
        }

        private bool ImpersonateClientWinIdentityForStaticContent()
        {
            using (new RequestMethodTracer(this.requestContext, "ImpersonateClientWinIdentityForStaticContent()", new object[0]))
            {
                if (!(((this.AppSettings.ImpersonateWindowsIdentityForStaticContent && this.AppSettings.AuthenticationAgainstWindows) && this.AppSettings.BypassAppServerForStaticContent) && this.IsStaticContent()))
                {
                    return false;
                }
                WindowsIdentity clientUserWindowsIdentity = this.GetClientUserWindowsIdentity();
                if (clientUserWindowsIdentity == null)
                {
                    return false;
                }
                if (WindowsIdentity.GetCurrent().User.Value == clientUserWindowsIdentity.User.Value)
                {
                    return false;
                }
                this.impersonationContextForStaticContent = clientUserWindowsIdentity.Impersonate();
                return true;
            }
        }

        private void InitOutputMode(int firstResponseFragmentLength)
        {
            if (this.outputStream == null)
            {
                long uncompressedSize = (this.Response.ContentLength64 < 0L) ? ((long) firstResponseFragmentLength) : this.Response.ContentLength64;
                if (!this.IsCompressableContent(uncompressedSize))
                {
                    this.outputStream = this.Response.OutputStream;
                }
                else
                {
                    this.AddCompressedResponseHeader();
                    if (this.chunkedCompression || (this.Response.ContentLength64 < 0L))
                    {
                        this.Response.SendChunked = true;
                        this.chunkedCompression = true;
                        this.outputStream = new GZipStream(this.Response.OutputStream, CompressionMode.Compress, true);
                    }
                    else
                    {
                        this.compressionBuffer = new MemoryStream();
                        this.outputStream = new GZipStream(this.compressionBuffer, CompressionMode.Compress, true);
                    }
                }
            }
        }

        public bool InitRequest()
        {
            using (new RequestMethodTracer(this.requestContext, "InitRequest()", new object[0]))
            {
                this.requestPath = this.MassageRequestPath(this.Request.Url.AbsolutePath, out this.pathInfo);
                this.translatedFileSystemPath = this.MapPath(this.requestPath);
                Trace.TraceInformation("Handling request for \"{0}\" mapped to \"{1}\".", new object[] { this.requestPath, this.translatedFileSystemPath });
                if (this.translatedFileSystemPath.IndexOfAny(Path.GetInvalidPathChars()) > 0)
                {
                    this.ReturnBadRequestError("URL contains illegal characters", new object[0]);
                    return false;
                }
                if (this.IsFailedBasicAuthAgainstWindows())
                {
                    return false;
                }
                this.ImpersonateClientWinIdentityForStaticContent();
                if (this.IsRestrictedFolderAccess())
                {
                    this.SendSimpleResponse(HttpStatusCode.Forbidden);
                    return false;
                }
                if (this.appDomain.IsAspNetClientScriptRequest(this.requestPath))
                {
                    string str = this.requestPath.Substring(this.appDomain.NormalizedClientScriptPath.Length).Replace('/', '\\');
                    this.translatedFileSystemPath = Path.Combine(this.appDomain.PhysicalClientScriptPath, str);
                    this.IsStaticContent();
                    if (!Directory.Exists(this.translatedFileSystemPath))
                    {
                        this.SendResponseFromFile(this.translatedFileSystemPath, 0L, -1L);
                        return false;
                    }
                }
                FileInfo info = new FileInfo(this.translatedFileSystemPath);
                if (!info.Exists)
                {
                    DirectoryInfo di = new DirectoryInfo(this.translatedFileSystemPath.TrimEnd(new char[] { '\\' }));
                    if (!di.Exists)
                    {
                        return true;
                    }
                    if (!this.requestPath.EndsWith("/"))
                    {
                        string url = string.Format("{0}/{1}", this.requestPath, this.Request.Url.Query);
                        this.Response.Redirect(url);
                        return false;
                    }
                    string direcotryDefaultFile = this.GetDirecotryDefaultFile(di);
                    if (string.IsNullOrEmpty(direcotryDefaultFile))
                    {
                        if (this.AppSettings.AllowDirectoryListing && !this.requestPath.EndsWith("/"))
                        {
                            this.requestPath = this.requestPath + "/";
                        }
                        return true;
                    }
                    this.requestPath = string.Format("{0}/{1}", this.requestPath.TrimEnd(new char[] { '/' }), direcotryDefaultFile);
                    this.translatedFileSystemPath = Path.Combine(di.FullName, direcotryDefaultFile);
                    Trace.TraceInformation("Request path modified to \"{0}\" mapped to \"{1}\".", new object[] { this.requestPath, this.translatedFileSystemPath });
                }
                return true;
            }
        }

        private bool IsCompressableContent(long uncompressedSize)
        {
            if (!this.AppSettings.CompressResponseIfPossible)
            {
                return false;
            }
            if (!this.clientAcceptsGZip)
            {
                return false;
            }
            if (this.Response.SendChunked)
            {
                return false;
            }
            string str = this.Response.Headers["Content-Encoding"];
            if (!string.IsNullOrEmpty(str) && compressionHeaderValues.IsMatch(str))
            {
                return false;
            }
            if (string.IsNullOrEmpty(this.Response.ContentType))
            {
                this.IsStaticContent();
                if (string.IsNullOrEmpty(this.Response.ContentType))
                {
                    return false;
                }
            }
            CompressionStats.CompressionAction action = CompressionStats.ShouldCompress(this.Response.ContentType, uncompressedSize);
            if (action == CompressionStats.CompressionAction.CompressChunked)
            {
                this.chunkedCompression = true;
            }
            return (action != CompressionStats.CompressionAction.DontCompress);
        }

        private bool IsFailedBasicAuthAgainstWindows()
        {
            using (new RequestMethodTracer(this.requestContext, "IsFailedBasicAuthAgainstWindows()", new object[0]))
            {
                this.basicWindowsAuthIdentity = null;
                HttpListenerBasicIdentity basicAuthIdentity = this.GetBasicAuthIdentity();
                if (basicAuthIdentity == null)
                {
                    return false;
                }
                if (!this.AppSettings.BasicAuthAgainstWindows)
                {
                    return false;
                }
                string usernameWithDomain = basicAuthIdentity.Name.Replace('/', '\\');
                if (usernameWithDomain.Contains(@"\"))
                {
                    this.basicWindowsAuthIdentity = WindowsAuthHelper.GetWindowsIdentity(usernameWithDomain, basicAuthIdentity.Password);
                }
                else
                {
                    string tcpMachineName;
                    if (string.IsNullOrEmpty(this.AppSettings.BasicAndDigestRealm))
                    {
                        tcpMachineName = SystemUtilites.TcpMachineName;
                    }
                    else
                    {
                        tcpMachineName = this.AppSettings.BasicAndDigestRealm;
                    }
                    this.basicWindowsAuthIdentity = WindowsAuthHelper.GetWindowsIdentity(tcpMachineName, usernameWithDomain, basicAuthIdentity.Password);
                }
                if ((this.basicWindowsAuthIdentity != null) && this.basicWindowsAuthIdentity.IsAuthenticated)
                {
                    return false;
                }
                this.SendSimpleResponse(HttpStatusCode.Unauthorized, "Basic Authentication credentials failed Windows authentication.", new object[0]);
                return true;
            }
        }

        private bool IsRestrictedFolderAccess()
        {
            string str2 = this.translatedFileSystemPath.ToLowerInvariant();
            foreach (string str3 in this.AppSettings.ProhibitedAspNetFolders)
            {
                string str = this.MapPath(this.appDomain.NormalizedVirtualPath + str3).ToLowerInvariant().TrimEnd(new char[] { '\\', '/' }) + @"\";
                if (str2.StartsWith(str))
                {
                    return true;
                }
            }
            return false;
        }

        public override bool IsSecure()
        {
            using (new RequestMethodTracer(this.requestContext, "IsSecure()", new object[0]))
            {
                return this.Request.IsSecureConnection;
            }
        }

        internal bool IsStaticContent()
        {
            if (!this.isStaticContent.HasValue)
            {
                this.isStaticContent = new bool?(this.CheckIfStaticContent());
            }
            return this.isStaticContent.Value;
        }

        public override string MapPath(string path)
        {
            using (RequestMethodTracer tracer = new RequestMethodTracer(this.requestContext, "MapPath(\"{0}\")", new object[] { path }))
            {
                string str = this.appDomain.MapPath(path);
                tracer.retVal = str;
                return str;
            }
        }

        private string MassageRequestPath(string absPath, out string pathInfo)
        {
            absPath = UrlDecodeNoQueryString(absPath);
            pathInfo = string.Empty;
            string str = absPath.ToLowerInvariant();
            List<string> list = new List<string>(this.AppSettings.DynamicContentFileExtensions.Count + staticHtmlFileExtensions.Count);
            list.AddRange(this.AppSettings.DynamicContentFileExtensions);
            list.AddRange(staticHtmlFileExtensions);
            foreach (string str2 in list)
            {
                string str3 = "." + str2.ToLowerInvariant().TrimStart(new char[] { '.' });
                int index = str.IndexOf(str3);
                if (index >= 0)
                {
                    int startIndex = index + str3.Length;
                    if (startIndex == absPath.Length)
                    {
                        return absPath;
                    }
                    if (absPath[index + str3.Length] == '/')
                    {
                        pathInfo = absPath.Substring(startIndex);
                        absPath = absPath.Substring(0, startIndex);
                        return absPath;
                    }
                }
            }
            return absPath;
        }

        private FileStream OpenFile(string fileName)
        {
            try
            {
                return new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (UnauthorizedAccessException)
            {
                string name = new FileInfo(fileName).Name;
                this.SendSimpleResponse(HttpStatusCode.Forbidden, "Access denied to file \"{0}\"", new object[] { name });
                return null;
            }
        }

        private bool ProcessDirectoryListingRequest()
        {
            if (this.AppSettings.AllowDirectoryListing)
            {
                if (this.Request.HttpMethod != "GET")
                {
                    return false;
                }
                string translatedFileSystemPath = this.translatedFileSystemPath;
                if (!Directory.Exists(translatedFileSystemPath))
                {
                    return false;
                }
                FileSystemInfo[] elements = null;
                try
                {
                    elements = new DirectoryInfo(translatedFileSystemPath).GetFileSystemInfos();
                }
                catch
                {
                }
                string path = null;
                string requestPath = this.requestPath;
                if (requestPath.Length > 1)
                {
                    int length = requestPath.LastIndexOf('/', requestPath.Length - 2);
                    path = (length > 0) ? requestPath.Substring(0, length) : "/";
                    if (!this.appDomain.IsVirtualPathInApp(path))
                    {
                        path = null;
                    }
                }
                string format = Messages.FormatDirectoryListing(requestPath, path, elements);
                this.SendSimpleHtmlResponse(HttpStatusCode.OK, format, new object[0]);
            }
            return false;
        }

        public override int ReadEntityBody(byte[] buffer, int size)
        {
            using (RequestMethodTracer tracer = new RequestMethodTracer(this.requestContext, "ReadEntityBody()", new object[0]))
            {
                int num = this.ReadEntityBody(buffer, 0, size);
                tracer.retVal = num;
                return num;
            }
        }

        public override int ReadEntityBody(byte[] buffer, int offset, int size)
        {
            int num;
            using (new RequestMethodTracer(this.requestContext, "ReadEntityBody()", new object[0]))
            {
                if (buffer == null)
                {
                    return -1;
                }
                if ((offset < 0) || (size < 0))
                {
                    return -1;
                }
                if (size > (offset + buffer.Length))
                {
                    return -1;
                }
                if (offset > size)
                {
                    return -1;
                }
                if (size > buffer.Length)
                {
                    return -1;
                }
                if ((size - offset) > buffer.Length)
                {
                    num = -1;
                }
                else
                {
                    try
                    {
                        num = this.Request.InputStream.Read(buffer, offset, size);
                    }
                    catch (Exception exception)
                    {
                        Trace.TraceWarning("AspNetRequest.ReadEntityBody() failed to call Request.InputStream.Read() due to \"{0}\".", new object[] { exception });
                        num = -1;
                    }
                }
            }
            return num;
        }

        private void ReturnAccessDenied(string format, params object[] args)
        {
            this.SendSimpleResponse(HttpStatusCode.Forbidden, format, args);
        }

        private void ReturnBadRequestError(string format, params object[] args)
        {
            this.SendSimpleResponse(HttpStatusCode.BadRequest, format, args);
        }

        public override void SendCalculatedContentLength(int contentLength)
        {
            using (new RequestMethodTracer(this.requestContext, "SendCalculatedContentLength(int: {0})", new object[] { contentLength }))
            {
                if (this.Response != null)
                {
                    this.Response.ContentLength64 = contentLength;
                }
            }
        }

        public override void SendCalculatedContentLength(long contentLength)
        {
            using (new RequestMethodTracer(this.requestContext, "SendCalculatedContentLength(long: {0})", new object[] { contentLength }))
            {
                if (this.Response != null)
                {
                    this.Response.ContentLength64 = contentLength;
                }
            }
        }

        private void SendCompressionBuffer()
        {
            this.outputStream.Dispose();
            this.outputStream = null;
            byte[] buffer = this.compressionBuffer.ToArray();
            if (buffer == null)
            {
                this.Response.ContentLength64 = 0L;
            }
            else
            {
                CompressionStats.RecordSample(this.Response.ContentType, this.Response.ContentLength64, buffer.LongLength);
                if (GC.GetGeneration(buffer) > 1)
                {
                    Trace.TraceWarning("GZipped response buffer landed in LOH for response type \"{0}\", uncompressed content size {1} and compressed size {2}. This can cause running out of memory as LOH is not compacted during garbage collection.", new object[] { this.Response.ContentType, this.Response.ContentLength64, buffer.LongLength });
                }
                this.Response.ContentLength64 = buffer.LongLength;
                try
                {
                    this.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    this.Response.OutputStream.Flush();
                }
                catch (Exception exception)
                {
                    Trace.TraceWarning("Failed while writing final {0} bytes of compression buffer for \"{1}\" due to the client having terminated connection.\r\n{2}", new object[] { buffer.Length, this.Request.Url, exception.Message });
                }
                finally
                {
                    this.compressionBuffer.Dispose();
                    this.compressionBuffer = null;
                }
            }
        }

        public override void SendKnownResponseHeader(int index, string value)
        {
            using (new RequestMethodTracer(this.requestContext, "SendKnownResponseHeader({0}, \"{1}\")", new object[] { index, value }))
            {
                string str;
                if (this.Response != null)
                {
                    switch (index)
                    {
                        case 0x12:
                        case 0x13:
                            if (!this.incorrectByteRangeHeader)
                            {
                                goto Label_0182;
                            }
                            return;

                        case 20:
                            if (value != "bytes")
                            {
                                goto Label_0182;
                            }
                            this.incorrectByteRangeHeader = true;
                            return;

                        case 0x1a:
                        case 1:
                        case 2:
                            return;

                        case 3:
                            bool flag;
                            if (!bool.TryParse(value, out flag))
                            {
                                goto Label_0182;
                            }
                            this.Response.KeepAlive = flag;
                            return;

                        case 4:
                        case 5:
                            goto Label_0182;

                        case 6:
                            if (value != "chunked")
                            {
                                goto Label_0182;
                            }
                            this.Response.SendChunked = true;
                            return;

                        case 11:
                            long num;
                            if (!long.TryParse(value, out num))
                            {
                                break;
                            }
                            this.Response.ContentLength64 = num;
                            return;

                        case 12:
                            this.Response.ContentType = value;
                            return;

                        default:
                            goto Label_0182;
                    }
                    Trace.TraceError("Invalid value of the Content-Length response header: \"{0}\".", new object[] { value });
                }
                return;
            Label_0182:
                str = HttpWorkerRequest.GetKnownResponseHeaderName(index);
                this.SendUnknownResponseHeader(str, value);
            }
        }

        public override void SendResponseFromFile(IntPtr handle, long offset, long length)
        {
            using (new RequestMethodTracer(this.requestContext, "SendResponseFromFile()", new object[0]))
            {
                if (length != 0L)
                {
                    using (SafeFileHandle handle2 = new SafeFileHandle(handle, false))
                    {
                        using (FileStream stream = new FileStream(handle2, FileAccess.Read))
                        {
                            this.SendResponseFromFileStream(stream, offset, length);
                        }
                    }
                }
            }
        }

        public override void SendResponseFromFile(string filename, long offset, long length)
        {
            using (new RequestMethodTracer(this.requestContext, "SendResponseFromFile()", new object[0]))
            {
                if (length != 0L)
                {
                    FileInfo info = new FileInfo(filename);
                    string str = string.Format("{0}${1}", info.Length, info.LastWriteTimeUtc.Ticks);
                    this.Response.Headers["ETag"] = str;
                    string str2 = this.Request.Headers["If-None-Match"];
                    if (str2 == str)
                    {
                        this.Response.StatusCode = 0x130;
                        this.Response.ContentLength64 = 0L;
                        Trace.TraceInformation("Sending HTTP 304 (use cached file) for \"{0}\".", new object[] { this.GetFilePath() });
                    }
                    else
                    {
                        using (FileStream stream = this.OpenFile(filename))
                        {
                            this.SendResponseFromFileStream(stream, offset, length);
                        }
                    }
                }
            }
        }

        private void SendResponseFromFileStream(FileStream f, long offset, long length)
        {
            using (new RequestMethodTracer(this.requestContext, "SendResponseFromFileStream()", new object[0]))
            {
                if (f != null)
                {
                    long num = f.Length;
                    if (length == -1L)
                    {
                        length = num - offset;
                    }
                    if (((length >= 1L) && (offset >= 0L)) && ((offset + length) <= num))
                    {
                        if ((offset == 0L) && (length == num))
                        {
                            string str = this.Request.Headers["If-None-Match"];
                            if (!(string.IsNullOrEmpty(str) || (str != this.Response.Headers["ETag"])))
                            {
                                this.Response.StatusCode = 0x130;
                                this.Response.ContentLength64 = 0L;
                                return;
                            }
                        }
                        if (offset > 0L)
                        {
                            f.Seek(offset, SeekOrigin.Begin);
                        }
                        int count = (int) Math.Min(length, 0x10000L);
                        byte[] buffer = new byte[count];
                        int num3 = 0;
                        for (long i = length; i > 0L; i -= num3)
                        {
                            num3 = f.Read(buffer, 0, count);
                            if (num3 == 0)
                            {
                                return;
                            }
                            this.SendResponseFromMemory(buffer, num3);
                            if (length > 0x200000L)
                            {
                                this.FlushResponse(false);
                            }
                        }
                    }
                }
            }
        }

        public override void SendResponseFromMemory(byte[] data, int length)
        {
            using (new RequestMethodTracer(this.requestContext, "SendResponseFromMemory(byte[], length: {0})", new object[] { length }))
            {
                if (this.Response != null)
                {
                    if (length < 0)
                    {
                        length = data.Length;
                    }
                    if ((((this.Response.StatusCode == 0x194) && (this.deferred404Response == null)) && (length == this.Response.ContentLength64)) && !this.HeadersSent())
                    {
                        this.deferred404Response = data;
                        try
                        {
                            if (this.ProcessDirectoryListingRequest())
                            {
                                return;
                            }
                        }
                        finally
                        {
                            this.deferred404Response = null;
                        }
                    }
                    if (this.outputStream == null)
                    {
                        this.InitOutputMode(length);
                        Trace.TraceInformation("SendResponseFromMemory(): Starting serving  \"{0}\" as{1}{2} {3}. HTTP result {4}. Data length: {5}", new object[] { this.GetFilePath(), this.Response.SendChunked ? " chunked," : string.Empty, (this.compressionBuffer != null) ? " compressed" : " uncompressed (or compressed by application)", this.Response.ContentType, this.Response.StatusCode, length });
                    }
                    if ((this.Response.SendChunked && !this.chunkedCompression) && ((this.byteWritten == 0) || (this.dechunker != null)))
                    {
                        if (length > 0)
                        {
                            if (this.dechunker == null)
                            {
                                this.dechunker = new ChunkedTransferDecoder(this.outputStream);
                            }
                            this.byteWritten += this.dechunker.Write(data, length);
                        }
                    }
                    else
                    {
                        try
                        {
                            this.outputStream.Write(data, 0, length);
                            this.byteWritten += length;
                        }
                        catch (Exception exception)
                        {
                            Trace.TraceWarning("SendResponseFromMemory() failed to call outputStream.Write() due to \"{0}\".", new object[] { exception });
                        }
                    }
                }
            }
        }

        private void SendSimpleHtmlResponse(HttpStatusCode statusCode, string format, params object[] args)
        {
            this.SendSimpleResponse(statusCode, "text/html; charset=utf-8", false, format, args);
        }

        private void SendSimpleResponse(HttpStatusCode statusCode)
        {
            this.SendSimpleResponse(statusCode, null, false, null, null);
        }

        private void SendSimpleResponse(HttpStatusCode statusCode, string format, params object[] args)
        {
            this.SendSimpleResponse(statusCode, null, false, format, args);
        }

        private void SendSimpleResponse(HttpStatusCode statusCode, string contentType, bool keepAlive, string format, params object[] args)
        {
            using (new RequestMethodTracer(this.requestContext, "SendSimpleResponse()", new object[0]))
            {
                string message = null;
                if (format != null)
                {
                    if ((args == null) || (args.Length == 0))
                    {
                        message = format;
                    }
                    else
                    {
                        message = string.Format(format, args);
                    }
                }
                this.Response.StatusCode = (int) statusCode;
                this.Response.KeepAlive = keepAlive;
                if (!string.IsNullOrEmpty(contentType))
                {
                    this.Response.ContentType = contentType;
                }
                else if (statusCode != HttpStatusCode.OK)
                {
                    message = this.GetErrorResponseBody(statusCode, message);
                    this.Response.ContentType = "text/html; charset=utf-8";
                }
                if (string.IsNullOrEmpty(message))
                {
                    this.Response.ContentLength64 = 0L;
                }
                else
                {
                    if (string.IsNullOrEmpty(this.Response.ContentType))
                    {
                        this.Response.ContentType = "text/plain; charset=utf-8";
                    }
                    byte[] bytes = Encoding.UTF8.GetBytes(message);
                    this.Response.ContentLength64 = bytes.Length;
                    this.SendResponseFromMemory(bytes, -1);
                }
                this.FlushResponse(true);
                this.GoOutOfBusiness();
            }
        }

        internal void SendStaticFile()
        {
            string fileName = this.MapPath(this.GetFilePath());
            FileInfo info = new FileInfo(fileName);
            string str2 = this.Request.Headers["Range"];
            if (!((str2 != null) && str2.ToLowerInvariant().StartsWith("bytes")))
            {
                this.Response.ContentLength64 = info.Length;
                this.SendResponseFromFile(fileName, 0L, info.Length);
            }
            else
            {
                List<ByteRange> list = new List<ByteRange>();
                long num = 0L;
                string[] strArray = str2.Split(new char[] { '=' });
                if (strArray.Length == 2)
                {
                    string[] strArray2 = strArray[1].Split(new char[] { ',' });
                    foreach (string str3 in strArray2)
                    {
                        ByteRange item = new ByteRange(str3, info.Length);
                        num += item.Length;
                        list.Add(item);
                    }
                }
                this.Response.ContentLength64 = num;
                if (num > 0L)
                {
                    using (FileStream stream = this.OpenFile(fileName))
                    {
                        foreach (ByteRange range2 in list)
                        {
                            this.SendResponseFromFileStream(stream, range2.StartIndex, range2.Length);
                        }
                    }
                }
            }
            this.FlushResponse(true);
            this.GoOutOfBusiness();
        }

        public override void SendStatus(int statusCode, string statusDescription)
        {
            using (new RequestMethodTracer(this.requestContext, "SendStatus({0}, \"{1}\")", new object[] { statusCode, statusDescription }))
            {
                if (this.Response != null)
                {
                    this.Response.StatusCode = statusCode;
                    this.Response.StatusDescription = statusDescription;
                }
            }
        }

        public override void SendUnknownResponseHeader(string name, string value)
        {
            using (new RequestMethodTracer(this.requestContext, "SendUnknownResponseHeader(\"{0}\", \"{1}\")", new object[] { name, value }))
            {
                if (this.Response != null)
                {
                    this.Response.Headers.Add(name, value);
                }
            }
        }

        public static string UrlDecodeNoQueryString(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                url = url.Replace("+", uniq);
                url = HttpUtility.UrlDecode(url, Encoding.UTF8);
                url = url.Replace(uniq, "+");
            }
            return url;
        }

        private WebAppConfigEntry AppSettings
        {
            get
            {
                return this.appDomain.AppSettings;
            }
        }

        internal X509Certificate2 ClientCert
        {
            get
            {
                if (!this.IsSecure())
                {
                    return null;
                }
                if (!this.clientCertAttempted)
                {
                    this.clientCertAttempted = true;
                    this.clientCert = this.Request.GetClientCertificate();
                }
                return this.clientCert;
            }
        }

        private bool IsBufferedCompression
        {
            get
            {
                return (this.OutputMode == OutputModeEnum.BufferredCompression);
            }
        }

        private OutputModeEnum OutputMode
        {
            get
            {
                if ((this.outputStream == null) || (this.outputStream == this.Response.OutputStream))
                {
                    return OutputModeEnum.Passthrough;
                }
                if (this.compressionBuffer == null)
                {
                    return OutputModeEnum.StreamCompression;
                }
                return OutputModeEnum.BufferredCompression;
            }
        }

        private string Protocol
        {
            get
            {
                if (this.Request == null)
                {
                    return null;
                }
                return ("HTTP/" + this.Request.ProtocolVersion.ToString(2));
            }
        }

        private HttpListenerRequest Request
        {
            get
            {
                if (this.requestContext == null)
                {
                    return null;
                }
                return this.requestContext.Request;
            }
        }

        public override Guid RequestTraceIdentifier
        {
            get
            {
                using (new RequestMethodTracer(this.requestContext, "RequestTraceIdentifier", new object[0]))
                {
                    return this.requestContext.Request.RequestTraceIdentifier;
                }
            }
        }

        private HttpListenerResponse Response
        {
            get
            {
                if (this.requestContext == null)
                {
                    return null;
                }
                return this.requestContext.Response;
            }
        }

        public class ByteRange
        {
            public ByteRange(long startIndex, long endIndex)
            {
                if (startIndex < 0L)
                {
                    throw new ArgumentException("StartIndex should be 0 or greater.");
                }
                if (endIndex < startIndex)
                {
                    throw new ArgumentException("EndIndex is less than start index.");
                }
                this.StartIndex = startIndex;
                this.EndIndex = endIndex;
            }

            public ByteRange(string range, long fileLength)
            {
                string[] strArray = range.Split(new char[] { '-' });
                long result = 0L;
                if (!long.TryParse(strArray[0].Trim(), out result))
                {
                    this.StartIndex = 0L;
                }
                else
                {
                    this.StartIndex = result;
                    if (this.StartIndex < 0L)
                    {
                        throw new ArgumentException("StartIndex should be 0 or greater.");
                    }
                }
                long num2 = 0L;
                if ((strArray.Length == 2) && long.TryParse(strArray[1], out num2))
                {
                    this.EndIndex = num2;
                }
                else
                {
                    this.EndIndex = fileLength - 1L;
                }
                if (this.EndIndex < this.StartIndex)
                {
                    throw new ArgumentException("EndIndex is less than start index.");
                }
            }

            public long EndIndex { get; set; }

            public long Length
            {
                get
                {
                    return ((this.EndIndex - this.StartIndex) + 1L);
                }
            }

            public long StartIndex { get; set; }
        }

        public enum OutputModeEnum
        {
            Passthrough,
            BufferredCompression,
            StreamCompression
        }
    }
}

