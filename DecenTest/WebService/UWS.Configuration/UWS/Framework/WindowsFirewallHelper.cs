namespace UWS.Framework
{
    using NetFwTypeLib;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UWS.Configuration;

    public static class WindowsFirewallHelper
    {
        public static void AddApplicationException(string programPath, string appRegisteredName, NET_FW_SCOPE_ scope)
        {
            if ((programPath == null) || (programPath.Length == 0))
            {
                throw new Exception("Application path for creating firewall exception is not specified is not specified");
            }
            if ((appRegisteredName == null) || (appRegisteredName.Length == 0))
            {
                throw new Exception("Application name for creating firewall exception is not specified is not specified");
            }
            if (!IsAppEnabled(programPath))
            {
                INetFwAuthorizedApplications authorizedApplications = FirewallProfile.AuthorizedApplications;
                INetFwAuthorizedApplication app = (INetFwAuthorizedApplication) Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("{EC9846B3-2762-4A6B-A214-6ACB603462D2}")));
                app.ProcessImageFileName = programPath;
                app.Name = appRegisteredName;
                app.Scope = scope;
                app.IpVersion = NET_FW_IP_VERSION_.NET_FW_IP_VERSION_ANY;
                authorizedApplications.Add(app);
            }
        }

        public static void AddLanApplicationException(string programPath, string appRegisteredName)
        {
            AddApplicationException(programPath, appRegisteredName, NET_FW_SCOPE_.NET_FW_SCOPE_LOCAL_SUBNET);
        }

        public static string DumpCurrentWindowsFirewallSettings()
        {
            StringBuilder builder = new StringBuilder();
            foreach (INetFwAuthorizedApplication application in FirewallProfile.AuthorizedApplications)
            {
                builder.AppendFormat("{0} {1}\r\n", application.Name, application.Scope);
            }
            return builder.ToString();
        }

        public static string GetAppForOpenPort(ushort port)
        {
            foreach (INetFwOpenPort port2 in FirewallProfile.GloballyOpenPorts)
            {
                if ((port2.Port == port) && port2.Enabled)
                {
                    return port2.Name;
                }
            }
            return null;
        }

        public static List<ushort> GetApplicationPorts(string applicationName)
        {
            List<ushort> list = new List<ushort>();
            foreach (INetFwOpenPort port in FirewallProfile.GloballyOpenPorts)
            {
                if (port.Name.StartsWith(applicationName))
                {
                    list.Add((ushort) port.Port);
                }
            }
            return list;
        }

        public static string GetSystemFolderPath(bool is32bit)
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
            if (is32bit && SystemUtilites.Is64BitOperatingSystem)
            {
                return new DirectoryInfo(Path.Combine(folderPath, @"..\SysWOW64")).FullName;
            }
            return folderPath;
        }

        public static INetFwOpenPort InstantiateOpenPort()
        {
            return (INetFwOpenPort) Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwOpenPort"));
        }

        public static bool IsAppEnabled(string programPath)
        {
            INetFwAuthorizedApplications authorizedApplications = FirewallProfile.AuthorizedApplications;
            try
            {
                return authorizedApplications.Item(programPath).Enabled;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        public static void RemoveApplicationException(string programPath)
        {
            if ((programPath == null) || (programPath.Length == 0))
            {
                throw new Exception("Application path for creating firewall exception is not specified is not specified");
            }
            if (IsAppEnabled(programPath))
            {
                FirewallProfile.AuthorizedApplications.Remove(programPath);
            }
        }

        public static void RemoveApplicationExceptions(string applicationName)
        {
            List<ushort> list = new List<ushort>();
            foreach (INetFwOpenPort port in FirewallProfile.GloballyOpenPorts)
            {
                if (port.Name.StartsWith(applicationName))
                {
                    list.Add((ushort) port.Port);
                }
            }
            foreach (ushort num in list)
            {
                FirewallProfile.GloballyOpenPorts.Remove(num, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP);
            }
        }

        public static int RemovePortsFromException(string applicationName, params ushort[] ports)
        {
            if ((ports == null) || (ports.Length == 0))
            {
                return 0;
            }
            List<ushort> list = new List<ushort>();
            foreach (INetFwOpenPort port in FirewallProfile.GloballyOpenPorts)
            {
                if (port.Name.StartsWith(applicationName))
                {
                    foreach (ushort num in ports)
                    {
                        if (num == port.Port)
                        {
                            list.Add(num);
                            break;
                        }
                    }
                }
            }
            foreach (ushort num2 in list)
            {
                FirewallProfile.GloballyOpenPorts.Remove(num2, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP);
            }
            return list.Count;
        }

        public static string SetExceptionForPorts(string applicationName, params ushort[] ports)
        {
            UniqueList<ushort> list = new UniqueList<ushort>();
            List<ushort> applicationPorts = GetApplicationPorts(applicationName);
            if ((applicationPorts == null) || (applicationPorts.Count == 0))
            {
                list.AddRange(ports);
            }
            else
            {
                foreach (ushort num in ports)
                {
                    if (!applicationPorts.Contains(num))
                    {
                        list.Add(num);
                    }
                }
            }
            StringBuilder builder = new StringBuilder();
            foreach (ushort num2 in list)
            {
                string appForOpenPort = GetAppForOpenPort(num2);
                if ((appForOpenPort == null) || (applicationName == appForOpenPort))
                {
                    INetFwOpenPort port = InstantiateOpenPort();
                    port.Name = applicationName;
                    port.Port = num2;
                    port.Protocol = NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
                    port.Scope = NET_FW_SCOPE_.NET_FW_SCOPE_LOCAL_SUBNET;
                    port.Enabled = true;
                    FirewallProfile.GloballyOpenPorts.Add(port);
                    if (builder.Length > 0)
                    {
                        builder.Append(',');
                    }
                    builder.Append(num2);
                }
            }
            return builder.ToString();
        }

        public static INetFwProfile FirewallProfile
        {
            get
            {
                INetFwMgr mgr = null;
                mgr = (INetFwMgr) Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("{304CE942-6E39-40D8-943A-B913C40C9CD4}")));
                return mgr.LocalPolicy.GetProfileByType(mgr.CurrentProfileType);
            }
        }
    }
}

