namespace UWS.Configuration
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public enum HostProcessClrAndBitness
    {
        Clr2AnyCPU,
        Clr1or2x86,
        Clr4AnyCPU,
        Clr4x86
    }
}

