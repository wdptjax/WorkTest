namespace UWS.Configuration
{
    using CassiniConfiguration;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml.Serialization;
    using UWS.Configuration.COM;

    [Serializable, ComVisible(false)]
    public sealed class WebAppConfigEntry : ListenEndpoint
    {
        private bool allowSavingToUninstalledAppSection;
        private AuthenticationSchemes auth;
        private string defaultDoc;
        public static readonly string[] defaultDynamicExtensions = new string[] { 
            "aspx", "asmx", "svc", "ashx", "axd", "xamlx", "asax", "cshtml", "vbhtml", "php", "jsp", "do", "pl", "cgi", "py", "asp",
            "asa", "cfm"
        };
        private static readonly string[] defaultFileNames = new string[] { "default", "index" };
        public static readonly string[] defaultRestrictedAspNetDirs = new string[] { "bin", "obj", "app_browsers", "app_code", "app_data", "app_localresources", "app_globalresources", "app_webreferences" };
        private readonly UniqueList<string> dynamicContentFileExtensions;
        private string physicalDirectory;
        private readonly UniqueList<string> prohibitedAspNetFolders;
        private readonly UniqueList<string> registeredShortcuts;
        private string relativePingPath;
        private static readonly string[] staticHtmlExtensions = new string[] { "htm", "html" };
        private static readonly string[] DefaultDefautlDocs = InitDefaultDocList().ToArray();

        internal WebAppConfigEntry()
        {
            this.dynamicContentFileExtensions = new UniqueList<string>();
            this.allowSavingToUninstalledAppSection = true;
            this.prohibitedAspNetFolders = new UniqueList<string>();
            this.auth = AuthenticationSchemes.Anonymous;
            this.registeredShortcuts = new UniqueList<string>();
            this.AuthenicationMode = AuthenticationSchemes.Anonymous;
            this.CompressResponseIfPossible = true;
            this.AddDynamicExtensions(defaultDynamicExtensions);
            this.BypassAppServerForStaticContent = true;
            this.BasicAuthAgainstWindows = true;
            this.ImpersonateWindowsIdentityForStaticContent = true;
            this.AppType = ApplicationType.AspNetOrStaticHtml;
            this.ProhibitedAspNetFolders.AddRange(defaultRestrictedAspNetDirs);
        }

        internal WebAppConfigEntry(IWebAppConfigEntry1 app) : base(app)
        {
            this.dynamicContentFileExtensions = new UniqueList<string>();
            this.allowSavingToUninstalledAppSection = true;
            this.prohibitedAspNetFolders = new UniqueList<string>();
            this.auth = AuthenticationSchemes.Anonymous;
            this.registeredShortcuts = new UniqueList<string>();
            this.ApplicationName = app.ApplicationName;
            this.ApplicationDescription = app.ApplicationDescription;
            this.PhysicalDirectory = app.PhysicalDirectory;
            this.AllowSavingToUninstalledAppSection = app.AllowSavingToUninstalledAppSection;
            this.DefaultDocument = app.DefaultDocument;
            this.AllowDirectoryListing = app.AllowDirectoryListing;
            this.BypassAppServerForStaticContent = app.BypassAppServerForStaticContent;
            this.AuthenicationMode = (AuthenticationSchemes) app.AuthenicationMode;
            this.BasicAuthAgainstWindows = app.BasicAuthAgainstWindows;
            this.BasicAndDigestRealm = app.BasicAndDigestRealm;
            this.ImpersonateWindowsIdentityForStaticContent = app.ImpersonateWindowsIdentityForStaticContent;
            this.CompressResponseIfPossible = app.CompressResponseIfPossible;
            this.AppType = app.AppType;
            this.DynamicContentFileExtensions.Clear();
            foreach (string str in app.DynamicContentFileExtensions)
            {
                this.AddDynamicExtension(str);
            }
            this.ProhibitedAspNetFolders.Clear();
            foreach (string str2 in app.ProhibitedAspNetFolders)
            {
                this.ProhibitedAspNetFolders.Add(str2);
            }
            this.HandlerPath = app.HandlerPath;
        }

        internal WebAppConfigEntry(ApplicationEntry legacyApp, Guid version) : this()
        {
            base.ChangeID = version;
            base.ID = legacyApp.ApplicationID;
            this.PhysicalDirectory = legacyApp.PhysicalPath;
            this.VirtualDirectory = "/";
            base.ListenAddresses.Add(string.Format("http://*:{0}/", legacyApp.Port));
            this.ApplicationName = string.IsNullOrEmpty(legacyApp.Name) ? null : legacyApp.Name.Trim(new char[] { '/' });
            this.ApplicationDescription = legacyApp.Description;
            if (!string.IsNullOrEmpty(legacyApp.DefaultDocument))
            {
                this.DefaultDocument = legacyApp.DefaultDocument;
            }
            if (legacyApp.KeepRunning)
            {
                this.RelativePingPath = string.IsNullOrEmpty(legacyApp.DefaultDocument) ? string.Empty : legacyApp.DefaultDocument;
            }
            this.BypassAppServerForStaticContent = false;
            this.AllowDirectoryListing = false;
            this.AllowSavingToUninstalledAppSection = true;
        }

        public void AddDynamicExtension(string ext)
        {
            ext = MassageExtension(ext);
            if (ext != null)
            {
                this.DynamicContentFileExtensions.Add(ext);
            }
        }

        public void AddDynamicExtensions(ICollection<string> extensions)
        {
            if ((extensions != null) && (extensions.Count != 0))
            {
                foreach (string str in extensions)
                {
                    this.AddDynamicExtension(str);
                }
            }
        }

        internal override void ApplyFinalDefaultsAndValidateBeforeSaving()
        {
            if (string.IsNullOrEmpty(base.virtualDirectory))
            {
                this.VirtualDirectory = this.PhysicalFolderToVirtualDir();
            }
            if (string.IsNullOrEmpty(this.ApplicationName) && !string.IsNullOrEmpty(this.VirtualDirectory))
            {
                this.ApplicationName = this.VirtualDirectory.Trim(new char[] { '/' });
            }
            base.ApplyFinalDefaultsAndValidateBeforeSaving();
            string str = this.VerifyCanRegister();
            if (!string.IsNullOrEmpty(str))
            {
                throw new ApplicationException(string.Format("Unable to register application because not all required application settings are specified:\r\n{0}", str));
            }
        }

        protected override ListenEndpoint FindExistingEndpoint(Guid id)
        {
            return UWS.Configuration.Metabase.FindApplication(id);
        }

        internal static WebAppConfigEntry FromComConfigEntry(IWebAppConfigEntry1 app)
        {
            if (app is WebAppEntryComServer)
            {
                return ((WebAppEntryComServer) app).Entry;
            }
            return new WebAppConfigEntry(app);
        }

        public string GetPingUrl(bool canUseNonPingPath)
        {
            string str = (this.RelativePingPath == null) ? string.Empty : this.RelativePingPath;
            if (!canUseNonPingPath && (str.Length == 0))
            {
                return null;
            }
            string[] httpListenerUrls = base.GetHttpListenerUrls();
            if ((httpListenerUrls == null) || (httpListenerUrls.Length == 0))
            {
                return null;
            }
            string str2 = null;
            foreach (string str3 in httpListenerUrls)
            {
                if (!string.IsNullOrEmpty(str3))
                {
                    string uriString = str3.ToLowerInvariant().Replace("://+:", "://localhost:").Replace("://*:", "://localhost:");
                    try
                    {
                        Uri uri = new Uri(uriString);
                        if (((uri.Host == "localhost") || (uri.Host == "127.0.0.1")) || (uri.Host == "[::1]"))
                        {
                            str2 = uriString;
                            break;
                        }
                    }
                    catch
                    {
                    }
                }
            }
            if (str2 == null)
            {
                str2 = httpListenerUrls[0];
            }
            return string.Format("{0}/{1}", str2.TrimEnd(new char[] { '/' }), str.TrimStart(new char[] { '/' }));
        }

        private static List<string> InitDefaultDocList()
        {
            List<string> list = new List<string>(staticHtmlExtensions.Length + defaultDynamicExtensions.Length);
            list.AddRange(staticHtmlExtensions);
            list.AddRange(defaultDynamicExtensions);
            List<string> list2 = new List<string>(list.Count * defaultFileNames.Length);
            foreach (string str in list)
            {
                foreach (string str2 in defaultFileNames)
                {
                    list2.Add(string.Format("{0}.{1}", str2, str));
                }
            }
            return list2;
        }

        internal void InitDefaultsFromUninstalledApp(WebAppConfigEntry uninstalledApp)
        {
            if (uninstalledApp != null)
            {
                if (base.ID != uninstalledApp.ID)
                {
                    throw new ArgumentException("Application's runtime settings can only be imported from WebAppConfigEntry with the ApplicationID.");
                }
                if (base.ListenAddresses.Count == 0)
                {
                    if (string.IsNullOrEmpty(this.VirtualDirectory) || (this.VirtualDirectory == "/"))
                    {
                        this.VirtualDirectory = uninstalledApp.VirtualDirectory;
                    }
                    base.ListenAddresses.AddRange(uninstalledApp.ListenAddresses);
                    base.ListenAddresses.FilterOutTakenListenUrls(this.VirtualDirectory, null);
                }
            }
        }

        public bool IsDynamicContentExtension(string loweredFileExtNoDot)
        {
            foreach (string str in this.DynamicContentFileExtensions)
            {
                if (str.ToLowerInvariant().TrimStart(new char[] { '.' }) == loweredFileExtNoDot)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsValidPingPath(string pingPath)
        {
            if (string.IsNullOrEmpty(pingPath))
            {
                return true;
            }
            try
            {
                new Uri(string.Format("http://hkw/{0}", pingPath.TrimStart(new char[] { '/', '\\' }))).ToString();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string MassageExtension(string ext)
        {
            if (ext == null)
            {
                return null;
            }
            ext = ext.ToLowerInvariant().Trim(new char[] { '*', ' ', '\t', '?', '.' });
            if (ext.Length == 0)
            {
                return null;
            }
            return ext;
        }

        private string PhysicalFolderToVirtualDir()
        {
            return PhysicalFolderToVirtualDir(this.PhysicalDirectory);
        }

        public static string PhysicalFolderToVirtualDir(DirectoryInfo physicalFolder)
        {
            return (physicalFolder.Name.Trim(Path.GetInvalidPathChars()) + "/");
        }

        public static string PhysicalFolderToVirtualDir(string folderPath)
        {
            char[] invalidPathChars = Path.GetInvalidPathChars();
            if (string.IsNullOrEmpty(folderPath) || (folderPath.IndexOfAny(invalidPathChars) >= 0))
            {
                throw new ArgumentException(string.Format("Argument \"folderPath\" has invalid value \"{0}\"", folderPath));
            }
            DirectoryInfo physicalFolder = new DirectoryInfo(folderPath);
            return PhysicalFolderToVirtualDir(physicalFolder);
        }

        private bool PurifyAuthValue(AuthenticationSchemes mixedval, AuthenticationSchemes desired)
        {
            if ((mixedval & desired) != AuthenticationSchemes.None)
            {
                this.auth = desired;
                return true;
            }
            return false;
        }

        public WebAppConfigEntry[] ToCollection()
        {
            return new WebAppConfigEntry[] { this };
        }

        internal string VerifyCanRegister()
        {
            StringBuilder builder = new StringBuilder();
            if (string.IsNullOrEmpty(this.PhysicalDirectory))
            {
                builder.AppendLine("Physical directory is not specified");
            }
            if (base.ListenAddresses.Count == 0)
            {
                builder.AppendLine("Listen endpoints are either not specified or are already taken by other applications.");
            }
            if (builder.Length == 0)
            {
                return null;
            }
            return builder.ToString();
        }

        public bool AllowDirectoryListing { get; set; }

        public bool AllowSavingToUninstalledAppSection
        {
            get
            {
                return this.allowSavingToUninstalledAppSection;
            }
            set
            {
                this.allowSavingToUninstalledAppSection = value;
            }
        }

        [XmlElement(ElementName="Description")]
        public string ApplicationDescription { get; set; }

        [XmlAttribute(AttributeName="Name")]
        public string ApplicationName { get; set; }

        public ApplicationType AppType { get; set; }

        public AuthenticationSchemes AuthenicationMode
        {
            get
            {
                return this.auth;
            }
            set
            {
                if (value != AuthenticationSchemes.None)
                {
                    if (this.PurifyAuthValue(value, AuthenticationSchemes.Anonymous))
                    {
                        return;
                    }
                    if (this.PurifyAuthValue(value, AuthenticationSchemes.IntegratedWindowsAuthentication))
                    {
                        return;
                    }
                    if (this.PurifyAuthValue(value, AuthenticationSchemes.Digest))
                    {
                        return;
                    }
                    if (this.PurifyAuthValue(value, AuthenticationSchemes.Basic))
                    {
                        return;
                    }
                }
                this.auth = AuthenticationSchemes.Anonymous;
            }
        }

        public bool AuthenticationAgainstWindows
        {
            get
            {
                if (this.AuthenicationMode == AuthenticationSchemes.Anonymous)
                {
                    return false;
                }
                return (((((this.AuthenicationMode & AuthenticationSchemes.Negotiate) != AuthenticationSchemes.None) || ((this.AuthenicationMode & AuthenticationSchemes.Ntlm) != AuthenticationSchemes.None)) || ((this.AuthenicationMode & AuthenticationSchemes.Digest) != AuthenticationSchemes.None)) || (((this.AuthenicationMode & AuthenticationSchemes.Basic) != AuthenticationSchemes.None) && this.BasicAuthAgainstWindows));
            }
        }

        public string BasicAndDigestRealm { get; set; }

        public bool BasicAuthAgainstWindows { get; set; }

        public bool BypassAppServerForStaticContent { get; set; }

        public bool CompressResponseIfPossible { get; set; }

        public string DefaultDocument
        {
            get
            {
                return this.defaultDoc;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.defaultDoc = null;
                }
                else
                {
                    int num = value.IndexOfAny(Path.GetInvalidFileNameChars());
                    if (num >= 0)
                    {
                        throw new Exception(string.Format("Default document name \"{0}\" contains illegal character '{1}'.", value, value[num]));
                    }
                    this.defaultDoc = value;
                }
            }
        }

        public string[] DefaultDocuments
        {
            get
            {
                if (this.DefaultDocument == null)
                {
                    return DefaultDefautlDocs;
                }
                return new string[] { this.DefaultDocument };
            }
        }

        public UniqueList<string> DynamicContentFileExtensions
        {
            get
            {
                return this.dynamicContentFileExtensions;
            }
        }

        [DefaultValue((string) null)]
        public string HandlerPath { get; set; }

        public bool ImpersonateWindowsIdentityForStaticContent { get; set; }

        public bool IsCassiniExplorerApp
        {
            get
            {
                return (base.ID == AppPoolRegistrationHelper.LegacyCassiniExplorerAppID);
            }
        }

        public string PhysicalDirectory
        {
            get
            {
                return this.physicalDirectory;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                int num = value.IndexOfAny(Path.GetInvalidPathChars());
                if (num >= 0)
                {
                    throw new ArgumentException(string.Format("Path \"{0}\" is invalid because it contains one of the following illegal character: '{1}'.", value, value[num]));
                }
                DirectoryInfo info = new DirectoryInfo(value);
                if (!info.Exists)
                {
                    try
                    {
                        Trace.TraceWarning("Application physical folder does not exist: \"{0}\".", new object[] { info.FullName });
                    }
                    catch
                    {
                    }
                }
                this.physicalDirectory = info.FullName;
            }
        }

        public UniqueList<string> ProhibitedAspNetFolders
        {
            get
            {
                return this.prohibitedAspNetFolders;
            }
        }

        public UniqueList<string> RegisteredShortcuts
        {
            get
            {
                return this.registeredShortcuts;
            }
        }

        public string RelativePingPath
        {
            get
            {
                return this.relativePingPath;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.relativePingPath = null;
                }
                else
                {
                    if (!IsValidPingPath(value))
                    {
                        throw new Exception(string.Format("Relative site path \"{0}\" is invalid. It should look like \"/pages/pingpage.html\".", value));
                    }
                    this.relativePingPath = value;
                }
            }
        }

        public override string UIApplicationName
        {
            get
            {
                if (!string.IsNullOrEmpty(this.ApplicationName))
                {
                    return this.ApplicationName;
                }
                if (!string.IsNullOrEmpty(this.PhysicalDirectory))
                {
                    DirectoryInfo info = new DirectoryInfo(this.PhysicalDirectory);
                    if (!string.IsNullOrEmpty(info.Name))
                    {
                        return info.Name;
                    }
                }
                if (this.VirtualDirectory != "/")
                {
                    return this.VirtualDirectory;
                }
                return base.ID.ToString("B").ToUpperInvariant();
            }
        }

        public override string VirtualDirectory
        {
            get
            {
                return base.VirtualDirectory;
            }
            set
            {
                value = ListenEndpoint.FormatVDir(value);
                string str = value.Trim(new char[] { '/' });
                int num = str.IndexOfAny(Path.GetInvalidFileNameChars());
                if (num >= 0)
                {
                    throw new ArgumentException(string.Format("Virtual directory name \"{0}\" is invalid because it contains illegal character: \"{1}\".", str, str[num]));
                }
                base.VirtualDirectory = value;
            }
        }
    }
}

