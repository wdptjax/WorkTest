namespace UWS.Framework
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Management;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class SystemUtilites
    {
        private static string domain = null;
        public static readonly bool Is32BitProcess = (IntPtr.Size == 4);
        private static int is64bitOS = -1;
        public static readonly bool Is64BitProcess = (IntPtr.Size == 8);
        public static readonly string TcpMachineName = Dns.GetHostName();
        public static readonly string UwsRoot = Path.Combine(ProgramFilesPath, @"UltiDev\Web Server");
        public static readonly DirectoryInfo UwsRootDir = new DirectoryInfo(UwsRoot);

        public static void AclFolder(string folderPath, FileSystemRights accessRights, params IdentityReference[] usersToGrantAccessTo)
        {
            DirectoryInfo info = new DirectoryInfo(folderPath);
            if (!info.Exists)
            {
                throw new ApplicationException(string.Format("Can't grant access rights to folder \"{0}\" because it does not exist.", folderPath));
            }
            DirectorySecurity accessControl = info.GetAccessControl(AccessControlSections.Access);
            CanonicalizeDacl(accessControl);
            AuthorizationRuleCollection rules = accessControl.GetAccessRules(true, true, typeof(NTAccount));
            InheritanceFlags inheritanceFlags = InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit;
            bool flag = false;
            foreach (IdentityReference reference in usersToGrantAccessTo)
            {
                bool flag2 = false;
                foreach (FileSystemAccessRule rule in rules)
                {
                    if ((rule.IdentityReference.Equals(reference) && (rule.AccessControlType == AccessControlType.Allow)) && (((rule.FileSystemRights & accessRights) != 0) && ((rule.InheritanceFlags & inheritanceFlags) != InheritanceFlags.None)))
                    {
                        flag2 = true;
                        break;
                    }
                }
                if (!flag2)
                {
                    FileSystemAccessRule rule2 = new FileSystemAccessRule(reference, accessRights, inheritanceFlags, PropagationFlags.None, AccessControlType.Allow);
                    accessControl.AddAccessRule(rule2);
                    flag = true;
                }
            }
            if (flag)
            {
                CanonicalizeDacl(accessControl);
                info.SetAccessControl(accessControl);
            }
        }

        public static void AclFolder(string folderPath, FileSystemRights accessRights, params SecurityIdentifier[] sidsToGrantAccessTo)
        {
            List<IdentityReference> list = new List<IdentityReference>();
            foreach (SecurityIdentifier identifier in sidsToGrantAccessTo)
            {
                IdentityReference item = identifier.Translate(typeof(NTAccount));
                list.Add(item);
            }
            AclFolder(folderPath, accessRights, list.ToArray());
        }

        public static void AclFolder(string folderPath, FileSystemRights accessRights, params WellKnownSidType[] sidsToGrantAccessTo)
        {
            List<SecurityIdentifier> list = new List<SecurityIdentifier>();
            foreach (WellKnownSidType type in sidsToGrantAccessTo)
            {
                SecurityIdentifier item = new SecurityIdentifier(type, null);
                list.Add(item);
            }
            AclFolder(folderPath, accessRights, list.ToArray());
        }

        private static void CanonicalizeDacl(NativeObjectSecurity objectSecurity)
        {
            int aceIndex;
            RawAcl newDacl;
            if (objectSecurity == null)
            {
                throw new ArgumentNullException("objectSecurity");
            }
            if (!objectSecurity.AreAccessRulesCanonical)
            {
                RawSecurityDescriptor descriptor = new RawSecurityDescriptor(objectSecurity.GetSecurityDescriptorSddlForm(AccessControlSections.Access));
                List<CommonAce> list = new List<CommonAce>();
                List<CommonAce> list2 = new List<CommonAce>();
                List<CommonAce> list3 = new List<CommonAce>();
                List<CommonAce> list4 = new List<CommonAce>();
                List<CommonAce> list5 = new List<CommonAce>();
                AceEnumerator enumerator = descriptor.DiscretionaryAcl.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    CommonAce current = (CommonAce) enumerator.Current;
                    if (((byte) (current.AceFlags & AceFlags.Inherited)) == 0x10)
                    {
                        list3.Add(current);
                    }
                    else
                    {
                        switch (current.AceType)
                        {
                            case AceType.AccessAllowed:
                                list4.Add(current);
                                break;

                            case AceType.AccessDenied:
                                list.Add(current);
                                break;

                            case AceType.AccessAllowedObject:
                                list5.Add(current);
                                break;

                            case AceType.AccessDeniedObject:
                                list2.Add(current);
                                break;
                        }
                        continue;
                    }
                }
                aceIndex = 0;
                newDacl = new RawAcl(descriptor.DiscretionaryAcl.Revision, descriptor.DiscretionaryAcl.Count);
                list.ForEach(x => newDacl.InsertAce(aceIndex++, x));
                list2.ForEach(x => newDacl.InsertAce(aceIndex++, x));
                list4.ForEach(x => newDacl.InsertAce(aceIndex++, x));
                list5.ForEach(x => newDacl.InsertAce(aceIndex++, x));
                list3.ForEach(x => newDacl.InsertAce(aceIndex++, x));
                if (aceIndex == descriptor.DiscretionaryAcl.Count)
                {
                    descriptor.DiscretionaryAcl = newDacl;
                    objectSecurity.SetSecurityDescriptorSddlForm(descriptor.GetSddlForm(AccessControlSections.Access), AccessControlSections.Access);
                }
            }
        }

        public static void ClearReadOnly(DirectoryInfo di, bool recursive)
        {
            if (di.Exists)
            {
                foreach (FileInfo info in di.GetFiles())
                {
                    if (info.IsReadOnly)
                    {
                        info.IsReadOnly = false;
                    }
                }
                if (recursive)
                {
                    foreach (DirectoryInfo info2 in di.GetDirectories())
                    {
                        ClearReadOnly(info2, true);
                    }
                }
            }
        }

        public static void ClearReadOnly(string folderPath, bool recursive)
        {
            if (!string.IsNullOrEmpty(folderPath))
            {
                ClearReadOnly(new DirectoryInfo(folderPath), recursive);
            }
        }

        public static T CreateRemoteProxyForDotNetServicedComponent<T>(Guid comClsID)
        {
            Type typeFromCLSID = Type.GetTypeFromCLSID(comClsID, "localhost");
            if (typeFromCLSID == null)
            {
                throw new ApplicationException(string.Format("COM+/Serviced component with CLSID {0:B} is not found.", comClsID));
            }
            return (T) Activator.CreateInstance(typeFromCLSID);
        }

        public static ushort FindFreeTcpPort()
        {
            return FindFreeTcpPort(null);
        }

        public static ushort FindFreeTcpPort(IPAddress address)
        {
            if ((address == null) || (address == IPAddress.Any))
            {
                address = IPAddress.Loopback;
            }
            IPEndPoint localEP = new IPEndPoint(address, 0);
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP))
            {
                socket.Bind(localEP);
                localEP = (IPEndPoint) socket.LocalEndPoint;
                return (ushort) localEP.Port;
            }
        }

        public static string GetDefaultDomainName()
        {
            string str = Dns.GetHostName().ToLowerInvariant();
            string str2 = Environment.MachineName.ToLowerInvariant();
            if (str == str2)
            {
                return str;
            }
            if (IsXPorOlder)
            {
                return null;
            }
            return (str + ".local");
        }

        public static string GetFQDN()
        {
            string hostName = Dns.GetHostName();
            if (!IsMachineInDomain)
            {
                return hostName;
            }
            if (hostName.Contains(DomainName))
            {
                return hostName;
            }
            return (hostName + "." + DomainName);
        }

        public static Dictionary<NetworkInterface, List<IPAddressInformation>> GetMachineIpAddresses()
        {
            Dictionary<NetworkInterface, List<IPAddressInformation>> dictionary = new Dictionary<NetworkInterface, List<IPAddressInformation>>();
            foreach (NetworkInterface interface2 in NetworkInterface.GetAllNetworkInterfaces())
            {
                if ((interface2.NetworkInterfaceType != NetworkInterfaceType.Loopback) && (interface2.OperationalStatus == OperationalStatus.Up))
                {
                    List<IPAddressInformation> list = null;
                    int num = -1;
                    foreach (IPAddressInformation information in interface2.GetIPProperties().UnicastAddresses)
                    {
                        if ((information.Address.AddressFamily == AddressFamily.InterNetwork) || (information.Address.AddressFamily == AddressFamily.InterNetworkV6))
                        {
                            if (list == null)
                            {
                                list = new List<IPAddressInformation>();
                                dictionary[interface2] = list;
                            }
                            if (information.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                list.Insert(++num, information);
                            }
                            else
                            {
                                list.Add(information);
                            }
                        }
                    }
                }
            }
            return dictionary;
        }

        public static List<string> GetMachineIpList()
        {
            int num = -1;
            List<string> list = new List<string>();
            Dictionary<NetworkInterface, List<IPAddressInformation>> machineIpAddresses = GetMachineIpAddresses();
            foreach (NetworkInterface interface2 in machineIpAddresses.Keys)
            {
                foreach (IPAddressInformation information in machineIpAddresses[interface2])
                {
                    string item = IpAddressToString(information.Address);
                    if (information.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        list.Insert(++num, item);
                    }
                    else
                    {
                        list.Add(item);
                    }
                }
            }
            return list;
        }

        public static Dictionary<string, List<string>> GetMachineIPs()
        {
            Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
            Dictionary<NetworkInterface, List<IPAddressInformation>> machineIpAddresses = GetMachineIpAddresses();
            List<string> list = null;
            foreach (NetworkInterface interface2 in machineIpAddresses.Keys)
            {
                list = new List<string>();
                dictionary[interface2.Name] = list;
                foreach (IPAddressInformation information in machineIpAddresses[interface2])
                {
                    string item = IpAddressToString(information.Address);
                    list.Add(item);
                }
            }
            return dictionary;
        }

        private static int GetOSArchitectureLegacy()
        {
            string environmentVariable = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            if (!string.IsNullOrEmpty(environmentVariable) && (string.Compare(environmentVariable, 0, "x86", 0, 3, true) != 0))
            {
                return 0x40;
            }
            return 0x20;
        }

        public static string GetOSInfo()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM  Win32_OperatingSystem");
            string oSLegacy = "";
            int oSArchitectureLegacy = 0;
            try
            {
                foreach (ManagementObject obj2 in searcher.Get())
                {
                    object propertyValue = obj2.GetPropertyValue("Caption");
                    if (propertyValue != null)
                    {
                        oSLegacy = Regex.Replace(propertyValue.ToString(), "[^A-Za-z0-9 ]", "").Trim();
                        if (!string.IsNullOrEmpty(oSLegacy))
                        {
                            object obj4 = null;
                            try
                            {
                                obj4 = obj2.GetPropertyValue("ServicePackMajorVersion");
                                if ((obj4 != null) && (obj4.ToString() != "0"))
                                {
                                    oSLegacy = oSLegacy + " Service Pack " + obj4.ToString();
                                }
                                else
                                {
                                    oSLegacy = oSLegacy + GetOsServicePackLegacy();
                                }
                            }
                            catch (Exception)
                            {
                                oSLegacy = oSLegacy + GetOsServicePackLegacy();
                            }
                        }
                        object obj5 = null;
                        try
                        {
                            obj5 = obj2.GetPropertyValue("OSArchitecture");
                            if (obj5 != null)
                            {
                                oSArchitectureLegacy = obj5.ToString().Contains("64") ? 0x40 : 0x20;
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            if (oSLegacy == "")
            {
                oSLegacy = GetOSLegacy();
            }
            if (oSArchitectureLegacy == 0)
            {
                oSArchitectureLegacy = GetOSArchitectureLegacy();
            }
            return string.Format("{0} {1} bit ({2})", oSLegacy, oSArchitectureLegacy, Environment.OSVersion.Version);
        }

        private static string GetOSLegacy()
        {
            OperatingSystem oSVersion = Environment.OSVersion;
            Version version = oSVersion.Version;
            string str = "";
            if (oSVersion.Platform == PlatformID.Win32Windows)
            {
                switch (version.Minor)
                {
                    case 0:
                        str = "95";
                        break;

                    case 10:
                        if (version.Revision.ToString() == "2222A")
                        {
                            str = "98SE";
                        }
                        else
                        {
                            str = "98";
                        }
                        break;

                    case 90:
                        str = "Me";
                        break;
                }
            }
            else if (oSVersion.Platform == PlatformID.Win32NT)
            {
                switch (version.Major)
                {
                    case 3:
                        str = "NT 3.51";
                        break;

                    case 4:
                        str = "NT 4.0";
                        break;

                    case 5:
                        if (version.Minor != 0)
                        {
                            str = "XP/Server 2003";
                            break;
                        }
                        str = "2000";
                        break;

                    case 6:
                        if (version.Minor != 0)
                        {
                            str = "7/Server 2008 R2";
                            break;
                        }
                        str = "Vista/Server 2008";
                        break;
                }
            }
            if (str != "")
            {
                str = str + GetOsServicePackLegacy();
            }
            return str;
        }

        private static string GetOsServicePackLegacy()
        {
            string servicePack = Environment.OSVersion.ServicePack;
            if (((servicePack != null) && (servicePack.ToString() != "")) && (servicePack.ToString() != " "))
            {
                return (" " + servicePack.ToString());
            }
            return "";
        }

        public static string GetSpecialFolder(WindowsFolder folder, bool createIfNotExists = false)
        {
            StringBuilder pszPath = new StringBuilder(0x4ec);
            int num = (int) folder;
            if (createIfNotExists)
            {
                num |= 0x8000;
            }
            try
            {
                if (SHGetFolderPath(IntPtr.Zero, (int) folder, IntPtr.Zero, 0, pszPath) == 0)
                {
                    return pszPath.ToString();
                }
            }
            catch (Exception exception)
            {
                Trace.TraceError(string.Format("Failed to get path to {0} due to:\r\n{1}", folder, exception));
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool InternalCheckIsWow64()
        {
            if (is64bitOS == -1)
            {
                if (((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor >= 1)) || (Environment.OSVersion.Version.Major >= 6))
                {
                    using (Process process = Process.GetCurrentProcess())
                    {
                        bool flag;
                        if (IsWow64Process(process.Handle, out flag))
                        {
                            is64bitOS = flag ? 1 : 0;
                        }
                        else
                        {
                            is64bitOS = 0;
                        }
                        goto Label_0079;
                    }
                }
                is64bitOS = 0;
            }
        Label_0079:
            return (is64bitOS > 0);
        }

        public static string IpAddressToString(IPAddress ipAddress)
        {
            if (ipAddress.AddressFamily != AddressFamily.InterNetworkV6)
            {
                return ipAddress.ToString();
            }
            string str = ipAddress.ToString().Trim(new char[] { '[', ']' });
            string str2 = string.Format("%{0}", ipAddress.ScopeId);
            int index = str.IndexOf(str2);
            if (index > 0)
            {
                str = str.Substring(0, index);
            }
            if (str == "0000:0000:0000:0000:0000:0000:0.0.0.1")
            {
                str = "::1";
            }
            return string.Format("[{0}]", str);
        }

        public static bool IsClr4Installed(bool allowClientProfile)
        {
            List<string> list = new List<string> { @"Software\Microsoft\NET Framework Setup\NDP\v4\Full" };
            if (allowClientProfile)
            {
                list.Add(@"Software\Microsoft\NET Framework Setup\NDP\v4\Client");
            }
            foreach (string str in list)
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(str, false))
                {
                    if (key != null)
                    {
                        object obj2 = key.GetValue("Install");
                        if ((obj2 != null) && (obj2.ToString() == "1"))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool IsFullClr4Installed()
        {
            return IsClr4Installed(false);
        }

        public static bool IsPortFree(ushort port)
        {
            return IsPortFree(port, IPAddress.Loopback);
        }

        public static bool IsPortFree(ushort port, IPAddress address)
        {
            bool flag;
            if ((address == null) || (address == IPAddress.Any))
            {
                address = IPAddress.Loopback;
            }
            IPEndPoint localEP = new IPEndPoint(address, port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            try
            {
                socket.Bind(localEP);
                flag = true;
            }
            catch
            {
                flag = false;
            }
            finally
            {
                if (socket != null)
                {
                    socket.Close();
                }
            }
            return flag;
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true)]
        private static extern bool IsWow64Process([In] IntPtr hProcess, out bool wow64Process);
        public static string MassageHostName(string hostOrIP)
        {
            IPAddress address;
            if (!IPAddress.TryParse(hostOrIP, out address))
            {
                return hostOrIP;
            }
            return IpAddressToString(address);
        }

        public static RegistryKey OpenOrCreateRegistryKeyPath(RegistryKey start, string keyPath)
        {
            RegistryKey key = start.OpenSubKey(keyPath, true);
            if (key == null)
            {
                string[] strArray = keyPath.Split(new char[] { '\\', '/' });
                key = start;
                int index = 0;
                while (index < strArray.Length)
                {
                    RegistryKey key2 = key.OpenSubKey(strArray[index], true);
                    if (key2 == null)
                    {
                        key2 = key.CreateSubKey(strArray[index]);
                    }
                    if (key != start)
                    {
                        key.Close();
                    }
                    index++;
                    key = key2;
                }
            }
            return key;
        }

        [DllImport("shell32.dll")]
        private static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, uint dwFlags, [Out] StringBuilder pszPath);
        public static int StartNonInteractiveProcess(string exePath, string commandLine)
        {
            Process process = new Process {
                StartInfo = { 
                    FileName = exePath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Arguments = commandLine
                }
            };
            process.Start();
            return process.Id;
        }

        public static void TraceDirPermissions(string path, IdentityReference user)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            foreach (FileSystemAccessRule rule in info.GetAccessControl().GetAccessRules(true, true, typeof(NTAccount)))
            {
                if (rule.IdentityReference.Equals(user))
                {
                    Trace.TraceInformation("Permissions for user {0} to access folder \"{1}\":", new object[] { user, path });
                    Trace.TraceInformation("{0} {1} ({1:X})", new object[] { rule.AccessControlType, rule.FileSystemRights });
                    Trace.TraceInformation("Inherited: {0}, Inheritance flags: {1}", new object[] { rule.IsInherited, rule.InheritanceFlags });
                    Trace.TraceInformation("Propagation flags: {0}", new object[] { rule.PropagationFlags });
                }
            }
        }

        public static void TraceDirPermissions(string path, SecurityIdentifier sid)
        {
            IdentityReference user = sid.Translate(typeof(NTAccount));
            TraceDirPermissions(path, user);
        }

        public static void TraceDirPermissions(string path, WellKnownSidType userSidType)
        {
            SecurityIdentifier sid = new SecurityIdentifier(userSidType, null);
            TraceDirPermissions(path, sid);
        }

        public static string CommonProgramFilesPath
        {
            get
            {
                string name = Is64BitOperatingSystem ? "%CommonProgramFiles(x86)%" : "%CommonProgramFiles%";
                return Environment.ExpandEnvironmentVariables(name);
            }
        }

        public static string DomainName
        {
            get
            {
                if (domain == null)
                {
                    domain = IPGlobalProperties.GetIPGlobalProperties().DomainName;
                }
                return domain;
            }
        }

        public static bool Is64BitOperatingSystem
        {
            get
            {
                if (!Is64BitProcess)
                {
                    return InternalCheckIsWow64();
                }
                return true;
            }
        }

        public static bool IsMachineInDomain
        {
            get
            {
                return !string.IsNullOrEmpty(DomainName);
            }
        }

        public static bool IsXPorOlder
        {
            get
            {
                return (Environment.OSVersion.Version.Major <= 5);
            }
        }

        public static DirectoryInfo ProgramFilesDir
        {
            get
            {
                return new DirectoryInfo(ProgramFilesPath);
            }
        }

        public static string ProgramFilesPath
        {
            get
            {
                string name = Is64BitOperatingSystem ? "%ProgramFiles(x86)%" : "%ProgramFiles%";
                return Environment.ExpandEnvironmentVariables(name);
            }
        }

        public static DirectoryInfo WindowsDirectory
        {
            get
            {
                return new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), ".."));
            }
        }

        public static string WindowsDirectoryPath
        {
            get
            {
                return WindowsDirectory.FullName;
            }
        }

        public enum WindowsFolder
        {
            CommonAdministrativeToolsMenu = 0x2f,
            CommonApplicationData = 0x23,
            CommonDesktop = 0x19,
            CommonDocuments = 0x2e,
            CommonEnglishStartupMenu = 30,
            CommonFavorites = 0x1f,
            CommonProgramFiles = 0x2b,
            CommonProgramFilesX86 = 0x2c,
            CommonProgramsMenu = 0x17,
            CommonStartMenu = 0x16,
            CommonStartupMenu = 0x18,
            CommonTemplates = 0x2d,
            CSIDL_BITBUCKET = 10,
            CSIDL_CONNECTIONS = 0x31,
            CSIDL_CONTROLS = 3,
            CSIDL_DRIVES = 0x11,
            CSIDL_INTERNET = 1,
            CSIDL_NETWORK = 0x12,
            CSIDL_PRINTERS = 4,
            Fonts = 20,
            ProgramFiles = 0x26,
            ProgramFilesX86 = 0x2a,
            System32 = 0x25,
            SystemWow64 = 0x29,
            UserDesktop = 0,
            UserDesktopDirectory = 0x10,
            UserDocuments = 5,
            UserFavorites = 6,
            UserInternetHistory = 0x22,
            UserLocalAppData = 0x1c,
            UserPictures = 0x27,
            UserProfile = 40,
            UserRoamingAdministrativeToolsMenu = 0x30,
            UserRoamingApplicationData = 0x1a,
            UserRoamingEnglishStartupMenu = 0x1d,
            UserRoamingInternetCookies = 0x21,
            UserRoamingNetworkNeighborhood = 0x13,
            UserRoamingPrinthood = 0x1b,
            UserRoamingProgramsMenu = 2,
            UserRoamingRecent = 8,
            UserRoamingSendTo = 9,
            UserRoamingStartMenu = 11,
            UserRoamingStartupMenu = 7,
            UserRoamingTemplates = 0x15,
            UserTempInternetFiles = 0x20,
            WindowsFolder = 0x24
        }
    }
}

