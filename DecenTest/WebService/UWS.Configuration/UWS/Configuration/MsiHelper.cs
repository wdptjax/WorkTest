namespace UWS.Configuration
{
    using Microsoft.Win32;
    using System;
    using System.Diagnostics;
    using System.Text;
    using UWS.Framework;

    public static class MsiHelper
    {
        private const string msiUpgradeCode = "{F9943FF4-9257-4EAD-8B11-B92B2CD468D0}";

        public static string GetCachedProductMsiPath(string productCode)
        {
            using (RegistryKey key = GetMsiInstallPropsRegKey(productCode))
            {
                return ((key == null) ? null : ((string) key.GetValue("LocalPackage")));
            }
        }

        internal static RegistryKey GetMsiInstallPropsRegKey(string productCode)
        {
            string str = ProductCodeToRegistryProductID(new Guid(productCode));
            string subKeyName = string.Format(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\{0}\InstallProperties", str);
            return RegistryHelpers.Open64bitSubKey(Registry.LocalMachine, subKeyName, false);
        }

        public static string GetProductCode(string upgradeCode)
        {
            StringBuilder sbProductCode = new StringBuilder(0x400);
            if (MsiNativeMethods.MsiEnumRelatedProducts(upgradeCode, 0, 0, sbProductCode) != 0)
            {
                return null;
            }
            return sbProductCode.ToString();
        }

        public static string GetProductVersion(string productCode)
        {
            return GetProperty(productCode, "ProductVersion");
        }

        public static string GetProperty(string productCode, string propertyName)
        {
            if (string.IsNullOrEmpty(productCode) || string.IsNullOrEmpty(propertyName))
            {
                return null;
            }
            try
            {
                int num;
                StringBuilder valueBuf = new StringBuilder(0x400);
                if (propertyName.ToLowerInvariant() == "ProductVersion".ToLowerInvariant())
                {
                    num = 0x648;
                }
                else
                {
                    int capacity = valueBuf.Capacity;
                    valueBuf.Length = 0;
                    num = MsiNativeMethods.MsiGetProductInfo(productCode, propertyName, valueBuf, ref capacity);
                    if (num == 0xea)
                    {
                        capacity++;
                        valueBuf.EnsureCapacity(capacity);
                        num = MsiNativeMethods.MsiGetProductInfo(productCode, propertyName, valueBuf, ref capacity);
                    }
                }
                if (((num == 0x645) || (num == 0x648)) && ((string.Compare(propertyName, "ProductVersion", StringComparison.Ordinal) == 0) || (string.Compare(propertyName, "ProductName", StringComparison.Ordinal) == 0)))
                {
                    using (RegistryKey key = GetMsiInstallPropsRegKey(productCode))
                    {
                        if (key != null)
                        {
                            string name = "DisplayName";
                            if (string.Compare(propertyName, "ProductVersion", StringComparison.Ordinal) == 0)
                            {
                                name = "DisplayVersion";
                            }
                            string str2 = key.GetValue(name) as string;
                            if (!string.IsNullOrEmpty(str2))
                            {
                                valueBuf.Length = 0;
                                valueBuf.Append(str2);
                                num = 0;
                            }
                        }
                    }
                }
                return ((num == 0) ? valueBuf.ToString() : null);
            }
            catch (Exception exception)
            {
                Trace.TraceError(exception.ToString());
                return null;
            }
        }

        public static string GetUwsProductCode()
        {
            return GetProductCode("{F9943FF4-9257-4EAD-8B11-B92B2CD468D0}");
        }

        public static string ProductCodeToRegistryProductID(Guid productCode)
        {
            StringBuilder builder = new StringBuilder();
            foreach (byte num in productCode.ToByteArray())
            {
                int num2 = ((num & 15) << 4) + ((num & 240) >> 4);
                builder.AppendFormat("{0:X2}", num2);
            }
            return builder.ToString();
        }
    }
}

