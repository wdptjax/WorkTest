namespace UltiDev.Framework.Windows
{
    using System;
    using System.Runtime.InteropServices;

    internal class NativeMethods
    {
        private NativeMethods()
        {
        }

        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern bool ChangeServiceConfig(IntPtr hService, uint nServiceType, uint nStartType, uint nErrorControl, string lpBinaryPathName, string lpLoadOrderGroup, IntPtr lpdwTagId, [In] char[] lpDependencies, string lpServiceStartName, string lpPassword, string lpDisplayName);
        [DllImport("advapi32.dll")]
        public static extern int ChangeServiceConfig2(IntPtr hService, ServiceConfig2InfoLevel dwInfoLevel, IntPtr lpInfo);
        [DllImport("advapi32.dll")]
        public static extern int CloseServiceHandle(IntPtr hSCObject);
        [DllImport("advapi32.dll")]
        public static extern IntPtr OpenSCManager(string machineName, string databaseName, ServiceControlAccessRights desiredAccess);
        [DllImport("advapi32.dll")]
        public static extern IntPtr OpenService(IntPtr hSCManager, string serviceName, ServiceAccessRights desiredAccess);
        [DllImport("advapi32.dll")]
        public static extern int QueryServiceConfig2(IntPtr hService, ServiceConfig2InfoLevel dwInfoLevel, IntPtr lpBuffer, int cbBufSize, out int pcbBytesNeeded);
    }
}

