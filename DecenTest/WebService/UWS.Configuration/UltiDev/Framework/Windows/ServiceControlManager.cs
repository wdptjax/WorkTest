namespace UltiDev.Framework.Windows
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.ServiceProcess;

    public class ServiceControlManager : IDisposable
    {
        private bool disposed;
        private IntPtr SCManager = UltiDev.Framework.Windows.NativeMethods.OpenSCManager(null, null, ServiceControlAccessRights.SC_MANAGER_CONNECT);

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        public ServiceControlManager()
        {
            if (this.SCManager == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to open Service Control Manager.");
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        public static void ChangeStartMode(string serviceName, ServiceStartMode mode)
        {
            IntPtr hSCManager = UltiDev.Framework.Windows.NativeMethods.OpenSCManager(null, null, ServiceControlAccessRights.SC_MANAGER_ALL_ACCESS);
            if (hSCManager == IntPtr.Zero)
            {
                throw new ExternalException("Open Service Manager Error");
            }
            IntPtr hService = UltiDev.Framework.Windows.NativeMethods.OpenService(hSCManager, serviceName, ServiceAccessRights.SERVICE_CHANGE_CONFIG | ServiceAccessRights.SERVICE_QUERY_CONFIG);
            if (hService == IntPtr.Zero)
            {
                throw new ExternalException("Open Service Error");
            }
            if (!UltiDev.Framework.Windows.NativeMethods.ChangeServiceConfig(hService, uint.MaxValue, (uint) mode, uint.MaxValue, null, null, IntPtr.Zero, null, null, null, null))
            {
                Win32Exception exception = new Win32Exception(Marshal.GetLastWin32Error());
                throw new ExternalException("Could not change service start type: " + exception.Message);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed && (this.SCManager != IntPtr.Zero))
            {
                UltiDev.Framework.Windows.NativeMethods.CloseServiceHandle(this.SCManager);
                this.SCManager = IntPtr.Zero;
            }
            this.disposed = true;
        }

        ~ServiceControlManager()
        {
            this.Dispose(false);
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        public bool HasRestartOnFailure(string serviceName)
        {
            bool flag2;
            IntPtr zero = IntPtr.Zero;
            IntPtr lpBuffer = IntPtr.Zero;
            bool flag = false;
            try
            {
                zero = this.OpenService(serviceName, ServiceAccessRights.SERVICE_QUERY_CONFIG);
                int pcbBytesNeeded = 0;
                lpBuffer = Marshal.AllocHGlobal(0x2000);
                if (UltiDev.Framework.Windows.NativeMethods.QueryServiceConfig2(zero, ServiceConfig2InfoLevel.SERVICE_CONFIG_FAILURE_ACTIONS, lpBuffer, 0x2000, out pcbBytesNeeded) == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to query the Service configuration.");
                }
                UltiDev.Framework.Windows.SERVICE_FAILURE_ACTIONS service_failure_actions = (UltiDev.Framework.Windows.SERVICE_FAILURE_ACTIONS) Marshal.PtrToStructure(lpBuffer, typeof(UltiDev.Framework.Windows.SERVICE_FAILURE_ACTIONS));
                if (service_failure_actions.cActions != 0)
                {
                    UltiDev.Framework.Windows.SC_ACTION sc_action = (UltiDev.Framework.Windows.SC_ACTION) Marshal.PtrToStructure(service_failure_actions.lpsaActions, typeof(UltiDev.Framework.Windows.SC_ACTION));
                    flag = sc_action.Type == SC_ACTION_TYPE.SC_ACTION_RESTART;
                }
                flag2 = flag;
            }
            finally
            {
                if (lpBuffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(lpBuffer);
                }
                if (zero != IntPtr.Zero)
                {
                    UltiDev.Framework.Windows.NativeMethods.CloseServiceHandle(zero);
                }
            }
            return flag2;
        }

        private IntPtr OpenService(string serviceName, ServiceAccessRights desiredAccess)
        {
            IntPtr ptr = UltiDev.Framework.Windows.NativeMethods.OpenService(this.SCManager, serviceName, desiredAccess);
            if (ptr == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to open the requested Service.");
            }
            return ptr;
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode=true)]
        public void SetRestartOnFailure(string serviceName, params UltiDev.Framework.Windows.SC_ACTION[] actions)
        {
            if (((actions == null) || (actions.Length < 1)) || (actions.Length > 3))
            {
                throw new ArgumentException("actions", "Service recovery actions argument must have from 1 to 3 action items.");
            }
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            IntPtr hglobal = IntPtr.Zero;
            int num = Marshal.SizeOf(typeof(UltiDev.Framework.Windows.SC_ACTION));
            try
            {
                zero = this.OpenService(serviceName, ServiceAccessRights.SERVICE_CHANGE_CONFIG | ServiceAccessRights.SERVICE_START);
                hglobal = Marshal.AllocHGlobal((int) (num * actions.Length));
                for (int i = 0; i < actions.Length; i++)
                {
                    Marshal.StructureToPtr(actions[i], (IntPtr) (((long) hglobal) + (num * i)), false);
                }
                UltiDev.Framework.Windows.SERVICE_FAILURE_ACTIONS structure = new UltiDev.Framework.Windows.SERVICE_FAILURE_ACTIONS {
                    dwResetPeriod = 0,
                    cActions = (uint) actions.Length,
                    lpsaActions = hglobal
                };
                ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(UltiDev.Framework.Windows.SERVICE_FAILURE_ACTIONS)));
                Marshal.StructureToPtr(structure, ptr, false);
                if (UltiDev.Framework.Windows.NativeMethods.ChangeServiceConfig2(zero, ServiceConfig2InfoLevel.SERVICE_CONFIG_FAILURE_ACTIONS, ptr) == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to change the Service configuration.");
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptr);
                }
                if (hglobal != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(hglobal);
                }
                if (zero != IntPtr.Zero)
                {
                    UltiDev.Framework.Windows.NativeMethods.CloseServiceHandle(zero);
                }
            }
        }

        public static void SetServiceImmediateAutorestartRecovery(string serviceName)
        {
            UltiDev.Framework.Windows.SC_ACTION sc_action = new UltiDev.Framework.Windows.SC_ACTION(SC_ACTION_TYPE.SC_ACTION_RESTART, 0);
            using (ServiceControlManager manager = new ServiceControlManager())
            {
                UltiDev.Framework.Windows.SC_ACTION[] actions = new UltiDev.Framework.Windows.SC_ACTION[] { sc_action, sc_action, sc_action };
                manager.SetRestartOnFailure(serviceName, actions);
            }
        }
    }
}

