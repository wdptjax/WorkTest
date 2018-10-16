namespace UWS.Configuration
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), Guid("3145F750-F730-47E0-9CFC-878F6955BE8F")]
    public enum ApplicationType
    {
        AspNetOrStaticHtml = 1,
        CGI = 3,
        FastCGI = 4,
        ISAPI = 2
    }
}

