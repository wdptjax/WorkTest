namespace UltiDev.Framework.Windows
{
    using System;

    [Flags]
    internal enum ServiceControlAccessRights
    {
        SC_MANAGER_ALL_ACCESS = 0xf003f,
        SC_MANAGER_CONNECT = 1,
        SC_MANAGER_CREATE_SERVICE = 2,
        SC_MANAGER_ENUMERATE_SERVICE = 4,
        SC_MANAGER_LOCK = 8,
        SC_MANAGER_MODIFY_BOOT_CONFIG = 0x20,
        SC_MANAGER_QUERY_LOCK_STATUS = 0x10
    }
}

