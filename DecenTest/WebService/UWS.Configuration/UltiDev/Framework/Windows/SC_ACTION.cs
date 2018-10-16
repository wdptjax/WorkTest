namespace UltiDev.Framework.Windows
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct SC_ACTION
    {
        [MarshalAs(UnmanagedType.U4)]
        public SC_ACTION_TYPE Type;
        [MarshalAs(UnmanagedType.U4)]
        public uint Delay;
        public SC_ACTION(SC_ACTION_TYPE action, int delayMillisec)
        {
            this.Type = action;
            this.Delay = (uint) delayMillisec;
        }
    }
}

