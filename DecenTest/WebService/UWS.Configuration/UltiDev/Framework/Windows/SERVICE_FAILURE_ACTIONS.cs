namespace UltiDev.Framework.Windows
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct SERVICE_FAILURE_ACTIONS
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint dwResetPeriod;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpRebootMsg;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpCommand;
        [MarshalAs(UnmanagedType.U4)]
        public uint cActions;
        public IntPtr lpsaActions;
    }
}

