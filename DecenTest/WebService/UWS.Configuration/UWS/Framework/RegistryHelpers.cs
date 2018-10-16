namespace UWS.Framework
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal static class RegistryHelpers
    {
        private static IntPtr _getRegistryKeyHandle(RegistryKey registryKey)
        {
            Type type = typeof(RegistryKey);
            SafeHandle handle = (SafeHandle) type.GetField("hkey", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(registryKey);
            return handle.DangerousGetHandle();
        }

        private static RegistryKey _pointerToRegistryKey(IntPtr hKey, bool writable, bool ownsHandle)
        {
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
            Type type = typeof(SafeHandleZeroOrMinusOneIsInvalid).Assembly.GetType("Microsoft.Win32.SafeHandles.SafeRegistryHandle");
            Type[] types = new Type[] { typeof(IntPtr), typeof(bool) };
            object obj2 = type.GetConstructor(bindingAttr, null, types, null).Invoke(new object[] { hKey, ownsHandle });
            Type type2 = typeof(RegistryKey);
            Type[] typeArray2 = new Type[] { type, typeof(bool) };
            return (RegistryKey) type2.GetConstructor(bindingAttr, null, typeArray2, null).Invoke(new object[] { obj2, writable });
        }

        public static RegistryKey Open64bitSubKey(RegistryKey parentKey, string subKeyName, bool writable = false)
        {
            if (SystemUtilites.Is32BitProcess && SystemUtilites.Is64BitOperatingSystem)
            {
                return OpenSubKeyEx(parentKey, subKeyName, writable, RegWow64Options.KEY_WOW64_64KEY);
            }
            return parentKey.OpenSubKey(subKeyName, writable);
        }

        public static RegistryKey OpenSubKeyEx(RegistryKey parentKey, string subKeyName, bool writable, RegWow64Options options)
        {
            int num2;
            if ((parentKey == null) || (_getRegistryKeyHandle(parentKey) == IntPtr.Zero))
            {
                return null;
            }
            int num = 0x20019;
            if (writable)
            {
                num = 0x20006;
            }
            int error = RegOpenKeyEx(_getRegistryKeyHandle(parentKey), subKeyName, 0, num | (int)options, out num2);
            if (error != 0)
            {
                throw new Exception("Exception encountered opening registry key.", new Win32Exception(error));
            }
            return _pointerToRegistryKey((IntPtr) num2, writable, false);
        }

        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        public static extern int RegOpenKeyEx(IntPtr hKey, string subKey, int ulOptions, int samDesired, out int phkResult);
    }
}

