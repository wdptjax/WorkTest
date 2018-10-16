namespace UltiDev.Framework.Windows
{
    using System;

    [Flags]
    internal enum ServiceAccessRights : uint
    {
        SERVICE_ALL_ACCESS = 0xf01ff,
        SERVICE_CHANGE_CONFIG = 2,
        SERVICE_ENUMERATE_DEPENDENTS = 8,
        SERVICE_INTERROGATE = 0x80,
        SERVICE_NO_CHANGE = 0xffffffff,
        SERVICE_PAUSE_CONTINUE = 0x40,
        SERVICE_QUERY_CONFIG = 1,
        SERVICE_QUERY_STATUS = 4,
        SERVICE_START = 0x10,
        SERVICE_STOP = 0x20,
        SERVICE_USER_DEFINED_CONTROL = 0x100
    }
}

