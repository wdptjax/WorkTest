namespace UWS.Configuration.Properties
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.CompilerServices;

    [CompilerGenerated, GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"), DebuggerNonUserCode]
    internal class Resources
    {
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceMan;

        internal Resources()
        {
        }

        internal static string AnySslIP
        {
            get
            {
                return ResourceManager.GetString("AnySslIP", resourceCulture);
            }
        }

        internal static string Bitness32
        {
            get
            {
                return ResourceManager.GetString("Bitness32", resourceCulture);
            }
        }

        internal static string Bitness64
        {
            get
            {
                return ResourceManager.GetString("Bitness64", resourceCulture);
            }
        }

        internal static string Clr_1_2_3
        {
            get
            {
                return ResourceManager.GetString("Clr_1_2_3", resourceCulture);
            }
        }

        internal static string Clr_2_3
        {
            get
            {
                return ResourceManager.GetString("Clr_2_3", resourceCulture);
            }
        }

        internal static string Clr_4
        {
            get
            {
                return ResourceManager.GetString("Clr_4", resourceCulture);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }

        internal static string DefaultSharedHostName
        {
            get
            {
                return ResourceManager.GetString("DefaultSharedHostName", resourceCulture);
            }
        }

        internal static string HttpListenerPlusWildcardUI
        {
            get
            {
                return ResourceManager.GetString("HttpListenerPlusWildcardUI", resourceCulture);
            }
        }

        internal static string HttpListenerStarWildcardUI
        {
            get
            {
                return ResourceManager.GetString("HttpListenerStarWildcardUI", resourceCulture);
            }
        }

        internal static string LegacyCassini1AppPoolName
        {
            get
            {
                return ResourceManager.GetString("LegacyCassini1AppPoolName", resourceCulture);
            }
        }

        internal static string LegacyCassini2AppPoolName
        {
            get
            {
                return ResourceManager.GetString("LegacyCassini2AppPoolName", resourceCulture);
            }
        }

        internal static string PrivateHostSuffix
        {
            get
            {
                return ResourceManager.GetString("PrivateHostSuffix", resourceCulture);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    System.Resources.ResourceManager manager = new System.Resources.ResourceManager("UWS.Configuration.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = manager;
                }
                return resourceMan;
            }
        }
    }
}

