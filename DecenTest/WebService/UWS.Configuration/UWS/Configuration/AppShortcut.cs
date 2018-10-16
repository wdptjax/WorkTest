namespace UWS.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using UWS.Configuration.COM;
    using UWS.Framework;

    public class AppShortcut : IAppShortcut1
    {
        private string fileContent;
        private string filePath;

        public AppShortcut(FileInfo filePath, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentException("content");
            }
            this.filePath = (filePath == null) ? null : filePath.FullName;
            this.fileContent = content;
        }

        public static FileInfo FormatShortcutFilePath(string appNameFileName, DirectoryInfo dirPath = null)
        {
            if (string.IsNullOrEmpty(appNameFileName))
            {
                throw new ArgumentNullException("appNameFileName cannot be blank");
            }
            if (dirPath == null)
            {
                dirPath = new DirectoryInfo(SystemUtilites.GetSpecialFolder(SystemUtilites.WindowsFolder.CommonDesktop, false));
            }
            if (!appNameFileName.ToLower().EndsWith(".url"))
            {
                appNameFileName = appNameFileName + ".url";
            }
            return new FileInfo(Path.Combine(dirPath.FullName, appNameFileName));
        }

        public bool Generate(string applicationName)
        {
            if (string.IsNullOrEmpty(this.fileContent))
            {
                throw new Exception("Application shortcut file content was not specified.");
            }
            try
            {
                FileInfo info;
                if (!string.IsNullOrEmpty(this.filePath))
                {
                    info = new FileInfo(this.filePath);
                }
                else
                {
                    if (string.IsNullOrEmpty(applicationName))
                    {
                        throw new Exception("Application shortcut file location must be specified");
                    }
                    info = FormatShortcutFilePath(applicationName, null);
                    this.filePath = info.FullName;
                }
                if (!info.Directory.Exists)
                {
                    Trace.TraceError(string.Format("Failed to generate application shortcut file at \"{0}\" because folder \"{1}\" does not exist.", this.filePath, info.Directory.FullName));
                    return false;
                }
                using (StreamWriter writer = new StreamWriter(this.filePath))
                {
                    writer.Write(this.fileContent);
                }
            }
            catch (Exception exception)
            {
                Trace.TraceError(string.Format("Failed to generate application shortcut file at \"{0}\" due to:\r\n{1}", this.filePath, exception));
                return false;
            }
            return true;
        }

        public static string GenerateUrlFileContent(string redirLink, string iconFilePath = null, uint iconIndex = 0)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("[InternetShortcut]\r\nURL={0}", redirLink);
            if (string.IsNullOrEmpty(iconFilePath) || !File.Exists(iconFilePath))
            {
                iconIndex = 0;
                iconFilePath = Path.Combine(AppPoolRegistrationHelper.ApplicationFolderPath, "UWS.Explorer.exe");
            }
            if (File.Exists(iconFilePath))
            {
                builder.AppendFormat("\r\nIconFile={0}\r\nIconIndex={1}", iconFilePath, iconIndex);
            }
            return builder.ToString();
        }

        public static AppShortcut Instantiate(Guid appID, FileInfo filePath, string host = null, string optionalPathAndQueryString = null, params ushort[] portOrder)
        {
            return new AppShortcut(filePath, GenerateUrlFileContent(Metabase.CreateAppRedirectLink(appID, host, optionalPathAndQueryString, portOrder), null, 0));
        }

        public static AppShortcut Instantiate(WebAppConfigEntry app, string location = null, string host = null, string optionalPathAndQueryString = null, params ushort[] portOrder)
        {
            FileInfo filePath = null;
            if (!string.IsNullOrEmpty(location))
            {
                filePath = LocationToFileInto(app.UIApplicationName, location);
            }
            if ((portOrder == null) || (portOrder.Length == 0))
            {
                portOrder = PortOrderFromAppEntry(app);
            }
            return Instantiate(app.ID, filePath, host, optionalPathAndQueryString, portOrder);
        }

        public static FileInfo LocationToFileInto(string applicationName, string location)
        {
            FileInfo info = null;
            if (string.IsNullOrEmpty(location))
            {
                return FormatShortcutFilePath(applicationName, null);
            }
            DirectoryInfo dirPath = new DirectoryInfo(location);
            if (dirPath.Exists)
            {
                return FormatShortcutFilePath(applicationName, dirPath);
            }
            info = new FileInfo(location);
            if (info.Extension.ToLower() != ".url")
            {
                throw new Exception(string.Format("Shortcut location \"{0}\" for application \"{1}\" is invalid because it's neither an existing folder, nor a .URL file to be created in an existing folder.", location, applicationName));
            }
            if (location.IndexOfAny(new char[] { ':', '\\', '/' }) < 0)
            {
                return FormatShortcutFilePath(location, null);
            }
            if (!info.Directory.Exists)
            {
                throw new Exception(string.Format("Shortcut location \"{0}\" for application \"{1}\" is invalid because it points to a directory that does not exist.", location, applicationName));
            }
            return info;
        }

        public static ushort[] PortOrderFromAppEntry(WebAppConfigEntry app)
        {
            List<ushort> allListenPorts = app.GetAllListenPorts();
            if ((allListenPorts.Count > 1) && (allListenPorts[0] != 80))
            {
                int num = 0;
                if (allListenPorts.Contains(80))
                {
                    allListenPorts.Remove(80);
                    allListenPorts.Insert(num++, 80);
                }
                if (((allListenPorts.Count > 2) && (allListenPorts[1] != 0x1bb)) && allListenPorts.Contains(0x1bb))
                {
                    allListenPorts.Remove(0x1bb);
                    allListenPorts.Insert(num++, 0x1bb);
                }
            }
            return allListenPorts.ToArray();
        }

        public string FileContent
        {
            get
            {
                return this.fileContent;
            }
        }

        public string FilePath
        {
            get
            {
                return this.filePath;
            }
        }
    }
}

