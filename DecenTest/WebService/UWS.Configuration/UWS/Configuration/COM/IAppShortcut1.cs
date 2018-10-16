namespace UWS.Configuration.COM
{
    using System;
    using System.Runtime.InteropServices;

    [Guid("CA1A7648-1462-4CA3-B40B-B379B8FE3FAD"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
    public interface IAppShortcut1
    {
        string FilePath { get; }
        string FileContent { get; }
    }
}

