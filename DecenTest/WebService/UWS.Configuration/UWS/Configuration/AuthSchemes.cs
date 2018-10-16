namespace UWS.Configuration
{
    using System;
    using System.Runtime.InteropServices;

    [Flags, ComVisible(true), Guid("61595C90-3F8F-4624-8AB1-EC8F19847909")]
    public enum AuthSchemes
    {
        Anonymous = 0x8000,
        Basic = 8,
        Digest = 1,
        IntegratedWindowsAuthentication = 6,
        Negotiate = 2,
        None = 0,
        Ntlm = 4
    }
}

