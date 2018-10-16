namespace UWS.Configuration
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), Guid("E0486428-F1AA-4A04-84A9-A17F0BB966E6")]
    public enum RedirectCode
    {
        Found = 0x12e,
        Permanent = 0x12d,
        Temporary = 0x133
    }
}

