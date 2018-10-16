namespace UWS.Configuration
{
    using CassiniConfiguration;
    using HttpConfig;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.ServiceProcess;
    using System.Text;
    using System.Threading;
    using UltiDev.Framework;
    using UWS.Framework;

    public static class Metabase
    {
        public const string cassiniServiceNamePrefix = "UltiDev Cassini Web Server for ASP.NET ";
        public static readonly Assembly ConfigApiAssembly = typeof(UWS.Configuration.Metabase).Assembly;
        private const string configFileName = "UWS.Configuration.xml";
        private const string dataFolder = @"\UltiDev\WebServer";
        private const string firewallApplicationName = "UltiDev Web Server Pro";
        private const string legacyMetabaseFile = @"\UltiDev\Cassini\CassiniMetabase.xml";
        private const HostProcessClrAndBitness redirectionHostType = HostProcessClrAndBitness.Clr2AnyCPU;
        private const ProcessIdentity redirectorIdentity = ProcessIdentity.NetworkService;

        static Metabase()
        {
            if (!Directory.Exists(ConfigFileFolder))
            {
                Directory.CreateDirectory(ConfigFileFolder);
            }
        }

        private static void AddRedirectionsToHost(List<HostProcessConfigEntry> appPools, List<RedirectionConfigEntry> redirections)
        {
            HostProcessConfigEntry item = null;
            Guid defaultHostID = AppPoolRegistrationHelper.GetDefaultHostID(HostProcessClrAndBitness.Clr2AnyCPU, ProcessIdentity.NetworkService);
            foreach (HostProcessConfigEntry entry2 in appPools)
            {
                if (entry2.HostID == defaultHostID)
                {
                    item = entry2;
                    break;
                }
            }
            if (item == null)
            {
                item = HostProcessConfigEntry.CreateDefaultHostConfigObject(HostProcessClrAndBitness.Clr2AnyCPU, ProcessIdentity.NetworkService);
                appPools.Add(item);
            }
            foreach (RedirectionConfigEntry entry3 in redirections)
            {
                item.Applications.Add(entry3.ID, entry3);
            }
        }

        private static void ApplyHttpApiSslConfig(ListenEndpoint site)
        {
            List<SslConfigItem> list2;
            List<SslConfigItem> applicationSslItems = SslConfigItem.GetApplicationSslItems(site.ID, null);
            List<SslConfigItem> sslConfigItemsToUpdate = site.GetSslConfigItemsToUpdate(ref applicationSslItems, out list2);
            if ((applicationSslItems.Count > 0) || (sslConfigItemsToUpdate.Count > 0))
            {
                using (new HttpConfig.HttpApi())
                {
                    foreach (SslConfigItem item in list2)
                    {
                        item.ApplyConfig(ConfigItemAction.Delete);
                    }
                    foreach (SslConfigItem item2 in sslConfigItemsToUpdate)
                    {
                        if (item2.AppId != site.ID)
                        {
                            throw new Exception(string.Format("Application ID ({0}) does not match SSL configuration item ID ({1}).", site.ID, item2.AppId));
                        }
                        if (!item2.Persisted && !string.IsNullOrEmpty(item2.HashString))
                        {
                            item2.ApplyConfig(ConfigItemAction.Create);
                        }
                    }
                }
            }
        }

        private static void AttemptToAddPortsToFirewallException(ListenEndpoint application)
        {
            try
            {
                ushort[] ports = application.GetAllListenPorts().ToArray();
                WindowsFirewallHelper.SetExceptionForPorts("UltiDev Web Server Pro", ports);
            }
            catch (Exception exception)
            {
                Trace.TraceWarning("Failed to add ports of \"{0}\" to the Windows firewall exception due to \"{1}\".", new object[] { application.UIApplicationName, exception });
            }
        }

        private static void AttemptToRemovePortsFromFirewallException(ListenEndpoint application)
        {
            try
            {
                ushort[] ports = application.GetAllListenPorts().ToArray();
                WindowsFirewallHelper.RemovePortsFromException("UltiDev Web Server Pro", ports);
            }
            catch (Exception exception)
            {
                Trace.TraceWarning("Failed to remove ports of \"{0}\" from the Windows firewall exception due to \"{1}\".", new object[] { application.UIApplicationName, exception });
            }
        }

        public static string CreateAppRedirectLink(Guid appID, string host = null, string optionalPathAndQueryString = null, params ushort[] portOrder)
        {
            if (string.IsNullOrEmpty(host) || (host == "*IOS*"))
            {
                host = GetDefaultHost(host == "*IOS*");
            }
            string[] browseUrls = FindApplication(AppPoolRegistrationHelper.LegacyCassiniExplorerAppID).GetBrowseUrls(new string[] { host });
            StringBuilder builder = new StringBuilder();
            builder.Append(browseUrls[0]);
            builder.AppendFormat("GoToApplication.aspx/{0}", appID.ToString().ToUpper());
            if (portOrder != null)
            {
                foreach (ushort num in portOrder)
                {
                    builder.AppendFormat(",{0}", num);
                }
            }
            builder.Append("/");
            if (!string.IsNullOrEmpty(optionalPathAndQueryString))
            {
                builder.Append(optionalPathAndQueryString.TrimStart(new char[] { '/' }));
            }
            return builder.ToString();
        }

        public static string CreateWindowsFirewallException()
        {
            List<ushort> list = new List<ushort>();
            foreach (HostProcessConfigEntry entry in LoadHostAndAppConfigFromMetabase())
            {
                foreach (ListenEndpoint endpoint in entry.Applications.Values)
                {
                    list.AddRange(endpoint.GetAllListenPorts());
                }
            }
            return WindowsFirewallHelper.SetExceptionForPorts("UltiDev Web Server Pro", list.ToArray());
        }

        public static WebAppConfigEntry FindApplication(Guid appID)
        {
            HostProcessConfigEntry entry;
            return FindApplicationAndAppHost(appID, out entry);
        }

        private static WebAppConfigEntry FindApplication(MetabaseSerializationHelper metabase, Guid appID)
        {
            HostProcessConfigEntry entry;
            return FindApplicationAndAppHost(metabase, appID, out entry);
        }

        private static WebAppConfigEntry FindApplicationAndAppHost(Guid appID, out HostProcessConfigEntry hostProcess)
        {
            return FindApplicationAndAppHost(LoadMetabase(), appID, out hostProcess);
        }

        private static WebAppConfigEntry FindApplicationAndAppHost(MetabaseSerializationHelper metabase, Guid appID, out HostProcessConfigEntry hostProcess)
        {
            List<HostProcessConfigEntry> allAppPools = LoadHostAndAppConfigFromMetabase(metabase);
            return FindApplicationAndHost(appID, allAppPools, out hostProcess);
        }

        private static WebAppConfigEntry FindApplicationAndHost(Guid appID, List<HostProcessConfigEntry> allAppPools, out HostProcessConfigEntry hostProcess)
        {
            WebAppConfigEntry entry = FindEndpointAndHost(appID, allAppPools, out hostProcess) as WebAppConfigEntry;
            if (entry == null)
            {
                hostProcess = null;
            }
            return entry;
        }

        public static ListenEndpoint FindEndpoint(Guid appID, List<HostProcessConfigEntry> allAppPools)
        {
            HostProcessConfigEntry entry;
            return FindEndpointAndHost(appID, allAppPools, out entry);
        }

        public static ListenEndpoint FindEndpointAndHost(Guid appID, List<HostProcessConfigEntry> allAppPools, out HostProcessConfigEntry hostProcess)
        {
            foreach (HostProcessConfigEntry entry in allAppPools)
            {
                ListenEndpoint endpoint = entry.GetEndpoint(appID);
                if (endpoint != null)
                {
                    hostProcess = entry;
                    return endpoint;
                }
            }
            hostProcess = null;
            return null;
        }

        public static HostProcessConfigEntry FindHost(Guid hostID)
        {
            MetabaseSerializationHelper helper;
            return FindHost(hostID, out helper);
        }

        private static HostProcessConfigEntry FindHost(Guid hostID, out MetabaseSerializationHelper metabase)
        {
            metabase = LoadMetabase();
            foreach (HostProcessConfigEntry entry in LoadHostAndAppConfigFromMetabase(metabase))
            {
                if (entry.HostID == hostID)
                {
                    return entry;
                }
            }
            if (AppPoolIDs.IsDefaultHostID(hostID))
            {
                return HostProcessConfigEntry.CreateDefaultHostConfigObject(hostID);
            }
            return null;
        }

        public static RedirectionConfigEntry FindRedirection(Guid redirectionID)
        {
            return LoadMetabase().GetRedirection(redirectionID);
        }

        private static bool FrameworkHasDedicatedVersionOfCassini(string appFramework)
        {
            Version version = new Version(appFramework);
            foreach (ServiceController controller in ServiceController.GetServices())
            {
                if (IsCassiniService(controller.ServiceName))
                {
                    Version frameworkVersionFromCassiniSvcName = GetFrameworkVersionFromCassiniSvcName(controller.ServiceName);
                    if (version == frameworkVersionFromCassiniSvcName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static List<ListenEndpoint> GetAllApplications(bool excludeUwsRedirector)
        {
            List<ListenEndpoint> list = new List<ListenEndpoint>();
            foreach (HostProcessConfigEntry entry in LoadHostAndAppConfigFromMetabase())
            {
                foreach (ListenEndpoint endpoint in entry.Applications.Values)
                {
                    if (!excludeUwsRedirector || (endpoint.ID != AppPoolRegistrationHelper.LegacyCassiniExplorerAppID))
                    {
                        list.Add(endpoint);
                    }
                }
            }
            return list;
        }

        public static string GetDefaultHost(bool forIOS)
        {
            if (!forIOS)
            {
                return SystemUtilites.GetFQDN();
            }
            string tcpMachineName = SystemUtilites.TcpMachineName;
            if (!SystemUtilites.IsMachineInDomain)
            {
                tcpMachineName = tcpMachineName + ".local";
            }
            return tcpMachineName;
        }

        private static Version GetFrameworkVersionFromCassiniSvcName(string cassiniSvcName)
        {
            string str = cassiniSvcName.Substring("UltiDev Cassini Web Server for ASP.NET ".Length).Split(new char[] { ' ' })[0];
            return new Version(string.Format("{0}.{1}.{1}", str, 0x7fffffff));
        }

        private static Guid GetLegacyMetabaseVersion()
        {
            FileInfo info = new FileInfo(CassiniConfiguration.Metabase.ConfigFileLocation);
            if (!info.Exists || (info.Length == 0L))
            {
                return Guid.Empty;
            }
            long ticks = info.LastWriteTimeUtc.Ticks;
            byte[] b = Guid.Empty.ToByteArray();
            int num2 = 8;
            for (int i = 0; i < num2; i++)
            {
                b[i] = (byte)(ticks >> (i * 8));
            }
            return new Guid(b);
        }

        public static RedirectionConfigEntry GetRedirectionEntry(Guid redirectionID)
        {
            if (redirectionID == Guid.Empty)
            {
                return new RedirectionConfigEntry();
            }
            RedirectionConfigEntry entry = FindRedirection(redirectionID);
            if ((entry == null) && (entry == null))
            {
                entry = new RedirectionConfigEntry
                {
                    ID = redirectionID
                };
            }
            return entry;
        }

        public static int GetRegisteredApplicationCount()
        {
            int num = 0;
            foreach (HostProcessConfigEntry entry in LoadHostAndAppConfigFromMetabase())
            {
                if (entry.Applications.Count > 0)
                {
                    num += entry.Applications.Count;
                    foreach (ListenEndpoint endpoint in entry.Applications.Values)
                    {
                        if (endpoint.ID == AppPoolRegistrationHelper.LegacyCassiniExplorerAppID)
                        {
                            num--;
                            break;
                        }
                    }
                }
            }
            return num;
        }

        public static WebAppConfigEntry GetUninstalledAppEntry(Guid applicationID)
        {
            WebAppConfigEntry entry;
            if (!LoadMetabase().UninstalledApps.TryGetValue(applicationID, out entry))
            {
                return null;
            }
            WebAppConfigEntry entry2 = new WebAppConfigEntry
            {
                ID = applicationID
            };
            entry2.InitDefaultsFromUninstalledApp(entry);
            return entry2;
        }

        public static WebAppConfigEntry GetWebAppEntry(Guid applicationID)
        {
            return GetWebAppEntry(applicationID, true);
        }

        public static WebAppConfigEntry GetWebAppEntry(Guid applicationID, bool includeUninstalledInSearch)
        {
            if (applicationID == Guid.Empty)
            {
                return new WebAppConfigEntry();
            }
            WebAppConfigEntry uninstalledAppEntry = FindApplication(applicationID);
            if (uninstalledAppEntry == null)
            {
                if (includeUninstalledInSearch)
                {
                    uninstalledAppEntry = GetUninstalledAppEntry(applicationID);
                }
                if (uninstalledAppEntry == null)
                {
                    uninstalledAppEntry = new WebAppConfigEntry
                    {
                        ID = applicationID
                    };
                }
            }
            return uninstalledAppEntry;
        }

        public static bool HasRegisteredApplications()
        {
            foreach (HostProcessConfigEntry entry in LoadHostAndAppConfigFromMetabase())
            {
                if (entry.Applications.Count > 1)
                {
                    return true;
                }
                foreach (ListenEndpoint endpoint in entry.Applications.Values)
                {
                    if (endpoint.ID != AppPoolRegistrationHelper.LegacyCassiniExplorerAppID)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool HostCollectionHasApp(Guid appID, List<HostProcessConfigEntry> hosts)
        {
            HostProcessConfigEntry entry;
            return (null != FindApplicationAndHost(appID, hosts, out entry));
        }

        private static bool IsCassini11ServiceInstalled()
        {
            return FrameworkHasDedicatedVersionOfCassini("1.1");
        }

        private static bool IsCassiniService(string serviceName)
        {
            return serviceName.StartsWith("UltiDev Cassini Web Server for ASP.NET ");
        }

        public static bool LaunchTheAppInTheBrowser(Guid appID, string optionalPathAndQueryString = null, int timeoutMilliseconds = -1)
        {
            if (!WaitForAppToStart(appID, timeoutMilliseconds))
            {
                return false;
            }
            string fileName = CreateAppRedirectLink(appID, null, optionalPathAndQueryString, new ushort[0]);
            try
            {
                Trace.TraceInformation("Launching application at \"{0}\".", new object[] { fileName });
                Process.Start(fileName);
                Trace.TraceInformation("Successfully started application at \"{0}\".", new object[] { fileName });
                return true;
            }
            catch (Exception exception)
            {
                Trace.TraceWarning("Unable to launch application at \"{0}\" due to {1}.", new object[] { fileName, exception });
                return false;
            }
        }

        public static List<HostProcessConfigEntry> LoadHostAndAppConfigFromMetabase()
        {
            return LoadHostAndAppConfigFromMetabase(LoadMetabase());
        }

        private static List<HostProcessConfigEntry> LoadHostAndAppConfigFromMetabase(MetabaseSerializationHelper metabase)
        {
            List<HostProcessConfigEntry> appPools = new List<HostProcessConfigEntry>(metabase.Hosts.Values);
            List<RedirectionConfigEntry> redirections = new List<RedirectionConfigEntry>(metabase.Redirections.Values);
            if ((redirections != null) && (redirections.Count > 0))
            {
                AddRedirectionsToHost(appPools, redirections);
            }
            LoadLegacyCassiniAppData(appPools);
            return appPools;
        }

        internal static List<HostProcessConfigEntry> LoadLegacyCassiniAppData()
        {
            List<HostProcessConfigEntry> appPools = new List<HostProcessConfigEntry>();
            LoadLegacyCassiniAppData(appPools);
            return appPools;
        }

        private static void LoadLegacyCassiniAppData(List<HostProcessConfigEntry> appPools)
        {
            Guid legacyMetabaseVersion = GetLegacyMetabaseVersion();
            HostProcessConfigEntry item = new HostProcessConfigEntry(AppPoolIDs.legacyCassini2AppPoolName, new Guid?(AppPoolIDs.LegacyCassiniClr2AnyCpuLocalSystemHostID), HostProcessClrAndBitness.Clr2AnyCPU, ProcessIdentity.LocalSystem)
            {
                VersionID = legacyMetabaseVersion
            };
            HostProcessConfigEntry entry2 = null;
            if (SystemUtilites.Is64BitOperatingSystem && !IsCassini11ServiceInstalled())
            {
                entry2 = new HostProcessConfigEntry(AppPoolIDs.legacyCassini1AppPoolName, new Guid?(AppPoolIDs.LegacyCassiniClr1x86LocalSystemHostID), HostProcessClrAndBitness.Clr1or2x86, ProcessIdentity.LocalSystem)
                {
                    VersionID = legacyMetabaseVersion
                };
            }
            CassiniConfiguration.Metabase metabase = null;
            Exception innerException = null;
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    metabase = CassiniConfiguration.Metabase.Load();
                    innerException = null;
                    break;
                }
                catch (Exception exception2)
                {
                    innerException = exception2;
                    Thread.Sleep(500);
                }
            }
            if (innerException != null)
            {
                throw new ApplicationException("Failed to load legacy Cassini metabase.", innerException);
            }
            foreach (ApplicationEntry entry4 in metabase.Applications)
            {
                if (!HostCollectionHasApp(entry4.ApplicationID, appPools))
                {
                    WebAppConfigEntry entry5 = new WebAppConfigEntry(entry4, legacyMetabaseVersion);
                    if ((entry2 != null) && (entry4.FrameworkVersion == "1.1"))
                    {
                        entry2.Applications[entry5.ID] = entry5;
                    }
                    else
                    {
                        item.Applications[entry5.ID] = entry5;
                    }
                }
            }
            if ((entry2 != null) && (entry2.Applications.Count > 0))
            {
                appPools.Add(entry2);
            }
            if (item.Applications.Count > 0)
            {
                appPools.Add(item);
            }
        }

        private static MetabaseSerializationHelper LoadMetabase()
        {
            MetabaseSerializationHelper helper = XmlSerializationHelper.XmlDesrializeFromFile<MetabaseSerializationHelper>(ConfigFileLocation);
            if (helper == null)
            {
                helper = new MetabaseSerializationHelper();
            }
            return helper;
        }

        internal static void RegisterApplication(HostProcessClrAndBitness hostType, WebAppConfigEntry application, ICollection<AppShortcut> shortcuts)
        {
            RegisterApplicationWith(hostType, true, ProcessIdentity.NetworkService, application, shortcuts);
        }

        public static void RegisterApplication(RuntimeVersion aspNetVersion, bool run32bitOnx64, WebAppConfigEntry application)
        {
            RegisterApplication(aspNetVersion, run32bitOnx64, true, ProcessIdentity.NetworkService, application);
        }

        public static void RegisterApplication(RuntimeVersion aspNetVersion, bool run32bitOnx64, WebAppConfigEntry application, ICollection<AppShortcut> shortcuts)
        {
            RegisterApplication(aspNetVersion, run32bitOnx64, true, ProcessIdentity.NetworkService, application, shortcuts);
        }

        public static void RegisterApplication(RuntimeVersion aspNetVersion, bool run32bitOnx64, ProcessIdentity hostUserIdentity, WebAppConfigEntry application)
        {
            RegisterApplication(aspNetVersion, run32bitOnx64, true, hostUserIdentity, application);
        }

        public static void RegisterApplication(RuntimeVersion aspNetVersion, bool run32bitOnx64, bool useDefaultSharedHostProcess, ProcessIdentity processIdentity, WebAppConfigEntry application)
        {
            RegisterApplication(aspNetVersion, run32bitOnx64, useDefaultSharedHostProcess, processIdentity, application, null);
        }

        public static void RegisterApplication(RuntimeVersion aspNetVersion, bool run32bitOnx64, ProcessIdentity hostUserIdentity, WebAppConfigEntry application, ICollection<AppShortcut> shortcuts)
        {
            RegisterApplication(aspNetVersion, run32bitOnx64, true, hostUserIdentity, application, shortcuts);
        }

        public static void RegisterApplication(RuntimeVersion aspNetVersion, bool run32bitOnx64, bool useDefaultSharedHostProcess, ProcessIdentity processIdentity, WebAppConfigEntry application, ICollection<AppShortcut> shortcuts)
        {
            RegisterApplicationWith(AppPoolRegistrationHelper.GetActualHostType(aspNetVersion, run32bitOnx64), useDefaultSharedHostProcess, processIdentity, application, shortcuts);
        }

        public static void RegisterApplicationInternal(HostProcessConfigEntry hostProcess, WebAppConfigEntry application)
        {
            RegisterApplicationInternal(hostProcess, application, null);
        }

        private static void RegisterApplicationInternal(HostProcessConfigEntry hostProcess, WebAppConfigEntry application, ICollection<AppShortcut> shortcuts)
        {
            if (hostProcess == null)
            {
                throw new ArgumentNullException("HostProcessConfigEntry hostProcess");
            }
            if (application == null)
            {
                throw new ArgumentNullException("WebAppConfigEntry application");
            }
            application.ApplyFinalDefaultsAndValidateBeforeSaving();
            string[] httpListenerUrls = application.GetHttpListenerUrls();
            string[] strArray2 = new string[0];
            Guid hostID = hostProcess.HostID;
            bool flag = false;
            try
            {
                HostProcessConfigEntry entry;
                MetabaseSerializationHelper cfg = LoadMetabase();
                WebAppConfigEntry appEntry = cfg.FindAppEntry(application.ID, out entry);
                if (appEntry != null)
                {
                    if (entry.HostID != hostProcess.HostID)
                    {
                        UnregisterApplication(cfg, entry, appEntry, application.ID, false);
                    }
                    else
                    {
                        strArray2 = appEntry.GetHttpListenerUrls();
                        List<string> list = SetOperations<string>.Difference(strArray2, httpListenerUrls);
                        List<string> list2 = SetOperations<string>.Difference(httpListenerUrls, strArray2);
                        strArray2 = list.ToArray();
                        httpListenerUrls = list2.ToArray();
                    }
                }
                if (((hostProcess.Identity != ProcessIdentity.LocalSystem) || (strArray2.Length > 0)) || (httpListenerUrls.Length > 0))
                {
                    SecurityHelper.ReAclListenUrls(hostProcess.Identity, strArray2, httpListenerUrls);
                    flag = true;
                }
                ApplyHttpApiSslConfig(application);
                HostProcessConfigEntry hostConfig = cfg.GetHostConfig(hostID);
                if (!hostProcess.Equals(hostConfig))
                {
                    cfg.Hosts[hostProcess.HostID] = hostProcess;
                    if (hostConfig != null)
                    {
                        foreach (ListenEndpoint endpoint in hostConfig.Applications.Values)
                        {
                            WebAppConfigEntry entry4 = endpoint as WebAppConfigEntry;
                            if (entry4 != null)
                            {
                                hostProcess.Applications.Add(entry4.ID, entry4);
                            }
                        }
                        hostConfig.Applications.Clear();
                    }
                }
                else
                {
                    hostProcess = hostConfig;
                    if (hostProcess.HostID == application.ID)
                    {
                        hostProcess.Name = application.ApplicationName;
                    }
                }
                hostProcess.Applications[application.ID] = application;
                if (hostProcess.Identity == ProcessIdentity.NetworkService)
                {
                    AppPoolRegistrationHelper.AclFolderForNetworkServiceSafely(application.PhysicalDirectory, false, FileSystemRights.Read);
                    AppPoolRegistrationHelper.AclFolderForNetworkServiceSafely(application.PhysicalDirectory, false, FileSystemRights.ExecuteFile);
                }
                AppPoolRegistrationHelper.AclFolderForNetworkServiceSafely(Path.Combine(application.PhysicalDirectory, "App_Data"), false, FileSystemRights.Modify);
                AppPoolRegistrationHelper.EnsureAspNetFoldersAccessForNetworkServiceUser(hostProcess.HostType);
                if ((shortcuts != null) && (application.ID != AppPoolRegistrationHelper.LegacyCassiniExplorerAppID))
                {
                    foreach (AppShortcut shortcut in shortcuts)
                    {
                        if ((shortcut != null) && shortcut.Generate(application.UIApplicationName))
                        {
                            application.RegisteredShortcuts.Add(shortcut.FilePath);
                        }
                    }
                }
                SaveMetabase(cfg);
            }
            catch (Exception exception)
            {
                Trace.TraceError(exception.ToString());
                try
                {
                    if (flag)
                    {
                        SecurityHelper.ReAclListenUrls(hostProcess.Identity, httpListenerUrls, strArray2);
                    }
                }
                catch
                {
                }
                throw;
            }
            AttemptToAddPortsToFirewallException(application);
        }

        internal static void RegisterApplicationWith(HostProcessClrAndBitness hostType, bool useDefaultSharedHostProcess, ProcessIdentity processIdentity, WebAppConfigEntry application, ICollection<AppShortcut> shortcuts)
        {
            HostProcessConfigEntry entry;
            hostType = AppPoolRegistrationHelper.GetActualHostType(hostType);
            if (useDefaultSharedHostProcess)
            {
                entry = HostProcessConfigEntry.CreateDefaultHostConfigObject(hostType, processIdentity);
            }
            else
            {
                entry = new HostProcessConfigEntry(application.ApplicationName, new Guid?(application.ID), hostType, processIdentity);
            }
            RegisterApplicationInternal(entry, application, shortcuts);
        }

        public static void RegisterLegacyCassiniApp(Guid applicationID, bool isAspNet1, string name, int port, string description, string physicalPath, string defaultDocument, bool keepRunning)
        {
            CassiniConfiguration.Metabase metabase = CassiniConfiguration.Metabase.Load();
            ApplicationEntry appEntry = new ApplicationEntry(applicationID, name, description, port, physicalPath, defaultDocument, keepRunning)
            {
                FrameworkVersion = isAspNet1 ? "1.1" : "2.0"
            };
            metabase.RegisterApplication(appEntry, true);
        }

        public static void RegisterRedirector(RedirectionConfigEntry redirectionConfig)
        {
            if (redirectionConfig == null)
            {
                throw new ArgumentNullException("RedirectionConfigEntry redirectionConfig");
            }
            redirectionConfig.ApplyFinalDefaultsAndValidateBeforeSaving();
            SecurityHelper.AclListenUrls(ProcessIdentity.NetworkService, redirectionConfig.GetHttpListenerUrls());
            try
            {
                MetabaseSerializationHelper mb = LoadMetabase();
                mb.Redirections[redirectionConfig.ID] = redirectionConfig;
                ApplyHttpApiSslConfig(redirectionConfig);
                SaveMetabase(mb);
            }
            catch
            {
                try
                {
                    SecurityHelper.DeAclListenUrls(ProcessIdentity.NetworkService, redirectionConfig.GetHttpListenerUrls());
                }
                catch
                {
                }
                throw;
            }
            AttemptToAddPortsToFirewallException(redirectionConfig);
        }

        public static bool RemoveHostAndAllItsApps(Guid hostID)
        {
            bool flag2;
            try
            {
                HostProcessConfigEntry entry;
                MetabaseSerializationHelper mb = LoadMetabase();
                if (!mb.Hosts.TryGetValue(hostID, out entry))
                {
                    return false;
                }
                bool flag = entry.CanRemoveFromMetabaseExplicitly();
                mb.Hosts.Remove(hostID);
                SaveMetabase(mb);
                flag2 = flag;
            }
            catch (Exception exception)
            {
                Trace.TraceError(exception.ToString());
                throw;
            }
            return flag2;
        }

        private static void RemoveHttpApiSslConfig(Guid appID)
        {
            List<SslConfigItem> applicationSslItems = SslConfigItem.GetApplicationSslItems(appID, null);
            if (applicationSslItems.Count > 0)
            {
                using (new HttpConfig.HttpApi())
                {
                    foreach (SslConfigItem item in applicationSslItems)
                    {
                        item.ApplyConfig(ConfigItemAction.Delete);
                    }
                }
            }
        }

        private static void RemoveRedirectorsFromApplicationsCollection(MetabaseSerializationHelper mb)
        {
            foreach (HostProcessConfigEntry entry in mb.Hosts.Values)
            {
                List<Guid> list = new List<Guid>();
                foreach (ListenEndpoint endpoint in entry.Applications.Values)
                {
                    if (endpoint is RedirectionConfigEntry)
                    {
                        list.Add(endpoint.ID);
                    }
                }
                foreach (Guid guid in list)
                {
                    entry.Applications.Remove(guid);
                }
            }
        }

        private static void RemoveShortcuts(UniqueList<string> shortcutFilePaths)
        {
            if (shortcutFilePaths != null)
            {
                foreach (string str in shortcutFilePaths)
                {
                    try
                    {
                        if (System.IO.File.Exists(str))
                        {
                            System.IO.File.Delete(str);
                        }
                    }
                    catch (Exception exception)
                    {
                        Trace.TraceWarning("Failed to remove shortcut file \"{0}\" due to:\r\n{1}", new object[] { str, exception });
                    }
                }
                shortcutFilePaths.Clear();
            }
        }

        public static string RemoveWindowsFirewallException()
        {
            WindowsFirewallHelper.RemoveApplicationExceptions("UltiDev Web Server Pro");
            return "UltiDev Web Server Pro";
        }

        private static void SaveMetabase(MetabaseSerializationHelper mb)
        {
            RemoveRedirectorsFromApplicationsCollection(mb);
            try
            {
                XmlSerializationHelper.XmlSerializeToFile(mb, ConfigFileLocation);
            }
            catch (Exception exception)
            {
                throw new ApplicationException(string.Format("Failed to save application configuration from \"{0}\". Error: {1}", ConfigFileLocation, exception.GetBaseException().Message), exception);
            }
        }

        private static bool SendPingRequest(string url, int timeoutMillisec)
        {
            HttpWebResponse response = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = false;
                request.CookieContainer = new CookieContainer();
                request.UserAgent = "UltiDev Web Server application wake-up caller 2.1";
                if (timeoutMillisec > 0)
                {
                    request.Timeout = timeoutMillisec;
                }
                else
                {
                    request.Timeout = 0x1388;
                }
                response = request.GetResponse() as HttpWebResponse;
            }
            catch (WebException exception)
            {
                response = exception.Response as HttpWebResponse;
            }
            catch
            {
            }
            return ((response != null) && response.StatusCode != HttpStatusCode.InternalServerError);
        }

        public static void UnregisterApplication(Guid appID)
        {
            HostProcessConfigEntry entry;
            MetabaseSerializationHelper metabase = LoadMetabase();
            WebAppConfigEntry appEntry = FindApplicationAndAppHost(metabase, appID, out entry);
            UnregisterApplication(metabase, entry, appEntry, appID, true);
        }

        private static void UnregisterApplication(MetabaseSerializationHelper cfg, HostProcessConfigEntry hostConfig, WebAppConfigEntry appEntry, Guid appID, bool saveMetabase)
        {
            if (appEntry != null)
            {
                if (cfg == null)
                {
                    throw new ArgumentNullException("MetabaseSerializationHelper cfg");
                }
                if (appEntry.AllowSavingToUninstalledAppSection)
                {
                    cfg.UninstalledApps[appID] = appEntry;
                }
                if (appEntry.ID != appID)
                {
                    throw new ArgumentException(string.Format("appID {0:B} does not match appEntry.ApplicationID {1:B}", appID, appEntry.ID));
                }
                if (hostConfig != null)
                {
                    Guid? privateAppID = null;
                    if (hostConfig.IsPrivateHost)
                    {
                        privateAppID = new Guid?(appEntry.ID);
                    }
                    hostConfig.Applications.Remove(appEntry.ID);
                    if (hostConfig.CanRemoveHostFromMetabaseDuringAppUnreg(privateAppID))
                    {
                        cfg.Hosts.Remove(hostConfig.HostID);
                    }
                    else
                    {
                        hostConfig.VersionID = Guid.NewGuid();
                    }
                }
                RemoveShortcuts(appEntry.RegisteredShortcuts);
                if (saveMetabase)
                {
                    SaveMetabase(cfg);
                }
                AttemptToRemovePortsFromFirewallException(appEntry);
                SecurityHelper.DeAclListenUrls(hostConfig.Identity, appEntry.GetHttpListenerUrls());
                RemoveHttpApiSslConfig(appEntry.ID);
            }
        }

        public static void UnregisterCassiniApplication(Guid applicationID)
        {
            CassiniConfiguration.Metabase.UnregisterApplication(applicationID);
        }

        public static void UnregisterRedirector(Guid redirectiorID)
        {
            MetabaseSerializationHelper mb = LoadMetabase();
            RedirectionConfigEntry redirection = mb.GetRedirection(redirectiorID);
            if (redirection != null)
            {
                mb.Redirections.Remove(redirectiorID);
                SaveMetabase(mb);
                AttemptToRemovePortsFromFirewallException(redirection);
                SecurityHelper.DeAclListenUrls(ProcessIdentity.NetworkService, redirection.GetHttpListenerUrls());
                RemoveHttpApiSslConfig(redirectiorID);
            }
        }

        public static bool UpdateHostSettings(Guid hostID, string hostName)
        {
            MetabaseSerializationHelper helper;
            HostProcessConfigEntry entry = FindHost(hostID, out helper);
            if (entry == null)
            {
                return false;
            }
            bool flag = false;
            if ((!string.IsNullOrEmpty(hostName) && !entry.IsDefaultSharedHost) && (!entry.IsLegacyCassiniHost && !entry.IsPrivateHost))
            {
                entry.Name = hostName;
                flag = true;
            }
            if (flag)
            {
                entry.VersionID = Guid.NewGuid();
                SaveMetabase(helper);
            }
            return flag;
        }

        public static bool WaitForAppToStart(Guid appID, int timeoutMilliseconds = -1)
        {
            WebAppConfigEntry entry = FindApplication(appID);
            if (entry == null)
            {
                return false;
            }
            return WaitForAppToStart(entry.GetPingUrl(true), timeoutMilliseconds);
        }

        public static bool WaitForAppToStart(string pingUrl, int timeoutMilliseconds)
        {
            TimeSpan span;
            DateTime utcNow = DateTime.UtcNow;
            Label_0006:
            span = (TimeSpan)(DateTime.UtcNow - utcNow);
            int timeoutMillisec = timeoutMilliseconds - ((int)span.TotalMilliseconds);
            if (SendPingRequest(pingUrl, timeoutMillisec))
            {
                return true;
            }
            if (timeoutMilliseconds < 0)
            {
                goto Label_0006;
            }
            TimeSpan span2 = (TimeSpan)(DateTime.UtcNow - utcNow);
            if (span2.TotalMilliseconds < timeoutMilliseconds)
            {
                goto Label_0006;
            }
            return false;
        }

        public static string ConfigFileFolder
        {
            get
            {
                return (Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\UltiDev\WebServer");
            }
        }

        public static string ConfigFileLocation
        {
            get
            {
                return Path.Combine(ConfigFileFolder, "UWS.Configuration.xml");
            }
        }

        public static string LegacyFileLocation
        {
            get
            {
                return CassiniConfiguration.Metabase.ConfigFileLocation;
            }
        }
    }
}

