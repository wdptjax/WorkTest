namespace UltiDev.WebServer.Core
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Principal;

    internal class WindowsAuthHelper
    {
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern bool CloseHandle(IntPtr hHandle);
        internal static WindowsIdentity GetWindowsIdentity(string usernameWithDomain, string password)
        {
            string[] strArray = usernameWithDomain.Replace('/', '\\').Split(new char[] { '\\' });
            if (((strArray.Length != 2) || string.IsNullOrEmpty(strArray[0])) || string.IsNullOrEmpty(strArray[1]))
            {
                throw new ArgumentException(string.Format("User name \"{0}\" is invalid. It must be in the <domain>\\<username> format.", usernameWithDomain));
            }
            return GetWindowsIdentity(strArray[0], strArray[1], password);
        }

        internal static WindowsIdentity GetWindowsIdentity(string domain, string username, string password)
        {
            IntPtr zero = IntPtr.Zero;
            if (!LogonUser(username, domain, password, LogonType.LOGON32_LOGON_NETWORK, LogonProvider.LOGON32_PROVIDER_DEFAULT, out zero))
            {
                return null;
            }
            return new WindowsIdentity(zero);
        }

        [DllImport("advapi32.dll", SetLastError=true)]
        public static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, LogonType logonType, LogonProvider dwLogonProvider, out IntPtr phToken);

        public enum LogonProvider
        {
            LOGON32_PROVIDER_DEFAULT
        }

        public enum LogonType
        {
            LOGON32_LOGON_BATCH = 4,
            LOGON32_LOGON_INTERACTIVE = 2,
            LOGON32_LOGON_NETWORK = 3,
            LOGON32_LOGON_NETWORK_CLEARTEXT = 8,
            LOGON32_LOGON_NEW_CREDENTIALS = 9,
            LOGON32_LOGON_SERVICE = 5,
            LOGON32_LOGON_UNLOCK = 7
        }
    }
}

