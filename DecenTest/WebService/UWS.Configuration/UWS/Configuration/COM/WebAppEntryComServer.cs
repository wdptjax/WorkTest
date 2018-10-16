namespace UWS.Configuration.COM
{
    using System;
    using System.Net;
    using UWS.Configuration;

    [Serializable]
    public class WebAppEntryComServer : ListenEndpointComServer<WebAppConfigEntry>, IWebAppConfigEntry1, IListenEndpoint1
    {
        internal WebAppEntryComServer() : this(new WebAppConfigEntry())
        {
        }

        internal WebAppEntryComServer(WebAppConfigEntry entry) : base(entry)
        {
        }

        public string VerifyCanRegister()
        {
            return base.Entry.VerifyCanRegister();
        }

        public bool AllowDirectoryListing
        {
            get
            {
                return base.Entry.AllowDirectoryListing;
            }
            set
            {
                base.Entry.AllowDirectoryListing = value;
            }
        }

        public bool AllowSavingToUninstalledAppSection
        {
            get
            {
                return base.Entry.AllowSavingToUninstalledAppSection;
            }
            set
            {
                base.Entry.AllowSavingToUninstalledAppSection = value;
            }
        }

        public string ApplicationDescription
        {
            get
            {
                return base.Entry.ApplicationDescription;
            }
            set
            {
                base.Entry.ApplicationDescription = value;
            }
        }

        public string ApplicationName
        {
            get
            {
                return base.Entry.ApplicationName;
            }
            set
            {
                base.Entry.ApplicationName = value;
            }
        }

        public ApplicationType AppType
        {
            get
            {
                return base.Entry.AppType;
            }
            set
            {
                base.Entry.AppType = value;
            }
        }

        public AuthSchemes AuthenicationMode
        {
            get
            {
                return (AuthSchemes) base.Entry.AuthenicationMode;
            }
            set
            {
                base.Entry.AuthenicationMode = (AuthenticationSchemes) value;
            }
        }

        public bool AuthenticationAgainstWindows
        {
            get
            {
                return base.Entry.AuthenticationAgainstWindows;
            }
        }

        public string BasicAndDigestRealm
        {
            get
            {
                return base.Entry.BasicAndDigestRealm;
            }
            set
            {
                base.Entry.BasicAndDigestRealm = value;
            }
        }

        public bool BasicAuthAgainstWindows
        {
            get
            {
                return base.Entry.BasicAuthAgainstWindows;
            }
            set
            {
                base.Entry.BasicAuthAgainstWindows = value;
            }
        }

        public bool BypassAppServerForStaticContent
        {
            get
            {
                return base.Entry.BypassAppServerForStaticContent;
            }
            set
            {
                base.Entry.BypassAppServerForStaticContent = value;
            }
        }

        public bool CompressResponseIfPossible
        {
            get
            {
                return base.Entry.CompressResponseIfPossible;
            }
            set
            {
                base.Entry.CompressResponseIfPossible = value;
            }
        }

        public string DefaultDocument
        {
            get
            {
                return base.Entry.DefaultDocument;
            }
            set
            {
                base.Entry.DefaultDocument = value;
            }
        }

        public IComCompatibleCollection DynamicContentFileExtensions
        {
            get
            {
                return new ComCompatibleCollection(base.Entry.DynamicContentFileExtensions);
            }
        }

        public IListenEndpoint1 Endpoint
        {
            get
            {
                return this;
            }
        }

        public string HandlerPath
        {
            get
            {
                return base.Entry.HandlerPath;
            }
            set
            {
                base.Entry.HandlerPath = value;
            }
        }

        public bool ImpersonateWindowsIdentityForStaticContent
        {
            get
            {
                return base.Entry.ImpersonateWindowsIdentityForStaticContent;
            }
            set
            {
                base.Entry.ImpersonateWindowsIdentityForStaticContent = value;
            }
        }

        public string PhysicalDirectory
        {
            get
            {
                return base.Entry.PhysicalDirectory;
            }
            set
            {
                base.Entry.PhysicalDirectory = value;
            }
        }

        public IComCompatibleCollection ProhibitedAspNetFolders
        {
            get
            {
                return new ComCompatibleCollection(base.Entry.ProhibitedAspNetFolders);
            }
        }

        public IComCompatibleCollection RegisteredShortcuts
        {
            get
            {
                return new ComCompatibleCollection(base.Entry.RegisteredShortcuts);
            }
        }
    }
}

