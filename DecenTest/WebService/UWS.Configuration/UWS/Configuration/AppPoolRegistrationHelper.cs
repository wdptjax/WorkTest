namespace UWS.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using UltiDev.Framework;
    using UWS.Framework;

    [ComVisible(false)]
    public static class AppPoolRegistrationHelper
    {
        public static readonly string[] AppHostFileNames = new string[] { "UWS.AppHost.Clr2.AnyCpu", "UWS.AppHost.Clr2.x86", "UWS.AppHost.Clr4.AnyCPU", "UWS.AppHost.Clr4.x86" };
        private static readonly string[] clrFolderNames = new string[] { "v2.0.50727", "v4.0.30319" };
        internal const string EventSource = "UWS.Configuration";
        public static readonly IniFileHelper globalSettings = new IniFileHelper(GetGlobalSettingsFilePath(), "UltiDev Web Server Settings");
        public static readonly Guid LegacyCassiniExplorerAppID = new Guid("{4fd8b3f7-bc73-4583-95fe-e7b69b10a3ae}");
        private const string registrySettingsPath = @"Software\UltiDev\Web Server Pro\AppPoolRegistrarFolder";

        public static bool AclFolderForNetworkServiceSafely(string path, bool createIfDoesntExist, FileSystemRights accessFlags = FileSystemRights.Modify)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            try
            {
                if (!di.Exists)
                {
                    if (!createIfDoesntExist)
                    {
                        return true;
                    }
                    Directory.CreateDirectory(path);
                }
                SystemUtilites.AclFolder(di.FullName, accessFlags, new WellKnownSidType[] { WellKnownSidType.NetworkServiceSid, WellKnownSidType.LocalSystemSid });
                SystemUtilites.ClearReadOnly(di, true);
                return true;
            }
            catch (Exception exception)
            {
                Trace.TraceWarning(string.Format("Failed to grant \"Network Service\" user account access rights to \"{0}\" folder due to:\r\n{1}", di.FullName, exception));
                return false;
            }
        }

        public static bool EnsureAppHostEventSource(HostProcessClrAndBitness template)
        {
            string appHostEventSourceName = GetAppHostEventSourceName(template);
            if (EventLog.SourceExists(appHostEventSourceName))
            {
                return false;
            }
            EventLog.CreateEventSource(appHostEventSourceName, "Application");
            return true;
        }

        public static bool EnsureAspNetFoldersAccessForNetworkServiceUser(HostProcessClrAndBitness hostProcessClrAndBitness)
        {
            string frameworkPath = GetFrameworkPath(hostProcessClrAndBitness);
            if (!Directory.Exists(frameworkPath))
            {
                Trace.TraceWarning(".NET Framework folder \"{0}\" does not exist.", new object[] { frameworkPath });
                return false;
            }
            string[] strArray = new string[] { "Temporary ASP.NET Files", "asp.netclientfiles" };
            bool flag = true;
            foreach (string str2 in strArray)
            {
                flag = AclFolderForNetworkServiceSafely(Path.Combine(frameworkPath, str2), true, FileSystemRights.Modify) && flag;
            }
            return flag;
        }

        public static Dictionary<HostProcessClrAndBitness, bool> EnsureAspNetFoldersAccessForNetworkServiceUserToAllClrs()
        {
            Dictionary<HostProcessClrAndBitness, bool> dictionary = new Dictionary<HostProcessClrAndBitness, bool>();
            foreach (HostProcessClrAndBitness bitness in Enum.GetValues(typeof(HostProcessClrAndBitness)))
            {
                bool flag = EnsureAspNetFoldersAccessForNetworkServiceUser(bitness);
                dictionary[bitness] = flag;
            }
            return dictionary;
        }

        internal static HostProcessClrAndBitness GetActualHostType(HostProcessClrAndBitness hostType)
        {
            if (((hostType == HostProcessClrAndBitness.Clr2AnyCPU) || (hostType == HostProcessClrAndBitness.Clr4AnyCPU)) || SystemUtilites.Is64BitOperatingSystem)
            {
                return hostType;
            }
            if (hostType == HostProcessClrAndBitness.Clr1or2x86)
            {
                return HostProcessClrAndBitness.Clr2AnyCPU;
            }
            return HostProcessClrAndBitness.Clr4AnyCPU;
        }

        internal static HostProcessClrAndBitness GetActualHostType(RuntimeVersion aspNetVersion, bool run32bitOnx64)
        {
            if (!run32bitOnx64 || !SystemUtilites.Is64BitOperatingSystem)
            {
                if (aspNetVersion != RuntimeVersion.AspNet_1_2or3x)
                {
                    return HostProcessClrAndBitness.Clr4AnyCPU;
                }
                return HostProcessClrAndBitness.Clr2AnyCPU;
            }
            if (aspNetVersion != RuntimeVersion.AspNet_1_2or3x)
            {
                return HostProcessClrAndBitness.Clr4x86;
            }
            return HostProcessClrAndBitness.Clr1or2x86;
        }

        public static string GetAppHostEventSourceName(HostProcessClrAndBitness template)
        {
            return ("UWS Host " + GetAppHostFileName(template));
        }

        public static string GetAppHostExeName(HostProcessClrAndBitness template)
        {
            return (GetAppHostFileName(template) + ".exe");
        }

        public static ICollection<string> GetAppHostExeNames()
        {
            List<string> list = new List<string>();
            foreach (HostProcessClrAndBitness bitness in Enum.GetValues(typeof(HostProcessClrAndBitness)))
            {
                list.Add(GetAppHostExeName(bitness));
            }
            return list;
        }

        private static string GetAppHostFileName(HostProcessClrAndBitness template)
        {
            return AppHostFileNames[(int)template];
        }

        public static Guid GetDefaultHostID(HostProcessClrAndBitness template, ProcessIdentity processIdentity)
        {
            int index = (int)template;
            if (processIdentity == ProcessIdentity.LocalSystem)
            {
                return AppPoolIDs.DefaultLocalSystemHostIDs[index];
            }
            return AppPoolIDs.DefaultNetworkServiceHostIDs[index];
        }

        private static string GetDefaultRegistrarPath(HostProcessClrAndBitness template)
        {
            string path = Path.Combine(ApplicationFolderPath, GetAppHostExeName(template));
            if (!File.Exists(path))
            {
                path = GetRegistrarPathFromRegistry(template);
                if (string.IsNullOrEmpty(path))
                {
                    throw new ApplicationException("Application pool registrar paths are not found in the registry.");
                }
                if (!File.Exists(path))
                {
                    throw new ApplicationException(string.Format("Application pool registrar is not found at \"{0}\"", path));
                }
            }
            return path;
        }

        public static string GetDefaultRegistrarPathSafely(HostProcessClrAndBitness template)
        {
            try
            {
                return GetDefaultRegistrarPath(template);
            }
            catch
            {
                return null;
            }
        }

        public static string GetFrameworkPath(HostProcessClrAndBitness hostType)
        {
            bool flag;
            return GetFrameworkPath(ToClrAndBitness(hostType, out flag), !flag);
        }

        public static string GetFrameworkPath(RuntimeVersion clrVer, bool is64Bit)
        {
            string str = Path.Combine(SystemUtilites.WindowsDirectoryPath, @"Microsoft.NET\Framework");
            if (is64Bit && SystemUtilites.Is64BitOperatingSystem)
            {
                str = str + "64";
            }
            string str2 = clrFolderNames[(clrVer == RuntimeVersion.AspNet_1_2or3x) ? 0 : 1];
            return Path.Combine(str, str2);
        }

        private static string GetGlobalSettingsFilePath()
        {
            return Path.Combine(Metabase.ConfigFileFolder, "UWS.GlobalSettings.ini");
        }

        public static string GetGlobalSettingValue(string settingName, string defaultValue)
        {
            return globalSettings.GetSetting(settingName, defaultValue);
        }

        public static HostProcessClrAndBitness GetHostType(RuntimeVersion aspNetVersion, bool run32bitOnx64)
        {
            if (run32bitOnx64)
            {
                if (aspNetVersion != RuntimeVersion.AspNet_1_2or3x)
                {
                    return HostProcessClrAndBitness.Clr4x86;
                }
                return HostProcessClrAndBitness.Clr1or2x86;
            }
            if (aspNetVersion != RuntimeVersion.AspNet_1_2or3x)
            {
                return HostProcessClrAndBitness.Clr4AnyCPU;
            }
            return HostProcessClrAndBitness.Clr2AnyCPU;
        }

        public static string GetInteractiveHostExeFileName(HostProcessClrAndBitness hostType)
        {
            bool flag;
            return GetInteractiveHostExeFileName(ToClrAndBitness(hostType, out flag), flag);
        }

        public static string GetInteractiveHostExeFileName(RuntimeVersion clrVer, bool for32Bit)
        {
            return string.Format("UWS.InteractiveServer.Clr{0}{1}.exe", (int)clrVer, (for32Bit && SystemUtilites.Is64BitOperatingSystem) ? "x86" : "AnyCPU");
        }

        public static string GetInteractiveHostExePath(HostProcessClrAndBitness hostType)
        {
            return Path.Combine(SystemUtilites.UwsRoot, GetInteractiveHostExeFileName(hostType));
        }

        public static string GetInteractiveHostExePath(RuntimeVersion clrVer, bool for32Bit)
        {
            return Path.Combine(SystemUtilites.UwsRoot, GetInteractiveHostExeFileName(clrVer, for32Bit));
        }

        private static string GetRegistrarPathFromRegistry(HostProcessClrAndBitness hostType)
        {
            return Path.Combine(GetGlobalSettingValue("ExeFolderPath", @"c:\Program Files\UltiDev\Web Server"), GetAppHostExeName(hostType));
        }

        public static void SaveExeFolderPathInRegistry(string path)
        {
            SetGlobalSettingValue("ExeFolderPath", path);
        }

        public static void SaveRegistrarDirInRegistry(HostProcessClrAndBitness template, string path)
        {
            FileInfo info = new FileInfo(path);
            if (!info.Exists)
            {
                throw new ArgumentException("Application pool registrar not found at \"{0}\".", info.FullName);
            }
            if (!path.ToLowerInvariant().EndsWith(GetAppHostExeName(template).ToLowerInvariant()))
            {
                throw new ApplicationException(string.Format("Mismatch between template \"{0}\" and registrar file name \"{1}\".", template, info.Name));
            }
            SaveExeFolderPathInRegistry(info.Directory.FullName);
        }

        public static bool SetGlobalSettingValue(string name, string value)
        {
            return globalSettings.SetSetting(name, value);
        }

        public static RuntimeVersion ToClrAndBitness(HostProcessClrAndBitness template, out bool run32on64)
        {
            switch (template)
            {
                case HostProcessClrAndBitness.Clr2AnyCPU:
                    run32on64 = false;
                    return RuntimeVersion.AspNet_1_2or3x;

                case HostProcessClrAndBitness.Clr1or2x86:
                    run32on64 = true;
                    return RuntimeVersion.AspNet_1_2or3x;

                case HostProcessClrAndBitness.Clr4AnyCPU:
                    run32on64 = false;
                    return RuntimeVersion.AspNet_4;

                case HostProcessClrAndBitness.Clr4x86:
                    run32on64 = true;
                    return RuntimeVersion.AspNet_4;
            }
            throw new Exception(string.Format("{0} clr & bitness need to be handled.", template));
        }

        internal static string ApplicationFolderPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"UltiDev\Web Server");
            }
        }

        public static HostProcessClrAndBitness CurrentHostProcessClrAndBitness
        {
            get
            {
                return GetActualHostType(CurrentRuntimeVersion, SystemUtilites.Is32BitProcess);
            }
        }

        public static RuntimeVersion CurrentRuntimeVersion
        {
            get
            {
                if (Environment.Version.Major < 4)
                {
                    return RuntimeVersion.AspNet_1_2or3x;
                }
                return RuntimeVersion.AspNet_4;
            }
        }
    }
}

