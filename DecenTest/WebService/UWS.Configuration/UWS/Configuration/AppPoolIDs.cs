namespace UWS.Configuration
{
    using System;
    using System.Runtime.InteropServices;
    using UWS.Configuration.Properties;

    [ComVisible(false)]
    public static class AppPoolIDs
    {
        internal static string DefaultHostName = Resources.DefaultSharedHostName;
        public static readonly Guid[] DefaultLocalSystemHostIDs = new Guid[] { DefaultPoolLsClr2AnyCPU, DefaultPoolLsClr2x86, DefaultPoolLsClr4AnyCPU, DefaultPoolLsClr4x86 };
        public static readonly Guid[] DefaultNetworkServiceHostIDs = new Guid[] { DefaultPoolNsClr2AnyCPU, DefaultPoolNsClr2x86, DefaultPoolNsClr4AnyCPU, DefaultPoolNsClr4x86 };
        public static readonly Guid DefaultPoolLsClr2AnyCPU = new Guid("{3A7DE9F6-1E51-4E27-ACB5-AC8906444562}");
        public static readonly Guid DefaultPoolLsClr2x86 = new Guid("{2366B999-DB02-4AC3-9476-5CBC8A2404A6}");
        public static readonly Guid DefaultPoolLsClr4AnyCPU = new Guid("{501E5447-A262-4A5C-834A-9B7B1B6F956D}");
        public static readonly Guid DefaultPoolLsClr4x86 = new Guid("{090D16C1-9563-4A25-9040-15776ADD6675}");
        public static readonly Guid DefaultPoolNsClr2AnyCPU = new Guid("{457E3DE5-EF7B-4d8a-8D07-D5B99F96A608}");
        public static readonly Guid DefaultPoolNsClr2x86 = new Guid("{A0CBED78-595B-4769-A18A-E67599EE7F69}");
        public static readonly Guid DefaultPoolNsClr4AnyCPU = new Guid("{9000B55F-4CAB-4292-8C90-81D3D162BB18}");
        public static readonly Guid DefaultPoolNsClr4x86 = new Guid("{DEC48E15-D524-4685-9A53-76C4175FF004}");
        internal static string legacyCassini1AppPoolName = Resources.LegacyCassini1AppPoolName;
        internal static string legacyCassini2AppPoolName = Resources.LegacyCassini2AppPoolName;
        public static readonly Guid LegacyCassiniClr1x86LocalSystemHostID = new Guid("{C90BF0B5-56B2-49A6-9DAE-4973C01F891E}");
        public static readonly Guid LegacyCassiniClr2AnyCpuLocalSystemHostID = new Guid("{B95D4617-7056-4E3D-9706-9F7FEB4FBD59}");
        public static readonly Guid[] LegacyCassiniHostIDs = new Guid[] { LegacyCassiniClr1x86LocalSystemHostID, LegacyCassiniClr2AnyCpuLocalSystemHostID };

        public static bool IsDefaultHostID(Guid id)
        {
            foreach (Guid[] guidArray in new Guid[][] { DefaultNetworkServiceHostIDs, DefaultLocalSystemHostIDs })
            {
                foreach (Guid guid in guidArray)
                {
                    if (id == guid)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

