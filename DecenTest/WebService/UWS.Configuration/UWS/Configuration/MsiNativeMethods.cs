namespace UWS.Configuration
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class MsiNativeMethods
    {
        internal const int ErrorMoreData = 0xea;
        internal const int ErrorNoMoreItems = 0x103;
        internal const int ErrorUnknownProduct = 0x645;
        internal const int ErrorUnknownProperty = 0x648;
        internal const int MaxGuidChars = 0x26;
        internal const int NoError = 0;

        [DllImport("msi.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern uint MsiEnumRelatedProducts(string strUpgradeCode, int reserved, int iIndex, StringBuilder sbProductCode);
        [DllImport("msi.dll", CharSet=CharSet.Unicode)]
        internal static extern int MsiGetProductInfo(string product, string property, [Out] StringBuilder valueBuf, ref int len);
    }
}

