namespace UltiDev.Framework.Interop
{
    using Microsoft.Win32;
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security.Principal;

    public static class UacHelper
    {
        private static uint STANDARD_RIGHTS_READ = 0x20000;
        private static uint TOKEN_QUERY = 8;
        private static uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);
        private const string uacRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Policies\System";
        private const string uacRegistryValue = "EnableLUA";

        [DllImport("advapi32.dll", SetLastError=true)]
        public static extern bool GetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, uint TokenInformationLength, out uint ReturnLength);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("advapi32.dll", SetLastError=true)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

        public static bool IsProcessElevated
        {
            get
            {
                IntPtr ptr;
                if (!IsUacEnabled)
                {
                    WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
                if (!OpenProcessToken(Process.GetCurrentProcess().Handle, TOKEN_READ, out ptr))
                {
                    throw new ApplicationException("Could not get process token.  Win32 Error Code: " + Marshal.GetLastWin32Error());
                }
                int structure = 1;
                int cb = Marshal.SizeOf(structure);
                uint returnLength = 0;
                IntPtr tokenInformation = Marshal.AllocHGlobal(cb);
                if (!GetTokenInformation(ptr, TOKEN_INFORMATION_CLASS.TokenElevationType, tokenInformation, (uint) cb, out returnLength))
                {
                    throw new ApplicationException("Unable to determine the current elevation.");
                }
                structure = Marshal.ReadInt32(tokenInformation);
                Marshal.FreeHGlobal(tokenInformation);
                if (structure == 2)
                {
                    return true;
                }
                structure = 0;
                cb = Marshal.SizeOf(structure);
                tokenInformation = Marshal.AllocHGlobal(cb);
                if (!GetTokenInformation(ptr, TOKEN_INFORMATION_CLASS.TokenElevation, tokenInformation, (uint) cb, out returnLength))
                {
                    throw new ApplicationException("Unable to get process elevation info.");
                }
                bool flag3 = Marshal.ReadInt32(tokenInformation) != 0;
                Marshal.FreeHGlobal(tokenInformation);
                return flag3;
            }
        }

        public static bool IsUacEnabled
        {
            get
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", false))
                {
                    if (key == null)
                    {
                        return false;
                    }
                    object obj2 = key.GetValue("EnableLUA");
                    if (obj2 == null)
                    {
                        return false;
                    }
                    return obj2.Equals(1);
                }
            }
        }

        public enum TOKEN_ELEVATION_TYPE
        {
            TokenElevationTypeDefault = 1,
            TokenElevationTypeFull = 2,
            TokenElevationTypeLimited = 3
        }

        public enum TOKEN_INFORMATION_CLASS
        {
            MaxTokenInfoClass = 0x1d,
            TokenAccessInformation = 0x16,
            TokenAuditPolicy = 0x10,
            TokenDefaultDacl = 6,
            TokenElevation = 20,
            TokenElevationType = 0x12,
            TokenGroups = 2,
            TokenGroupsAndPrivileges = 13,
            TokenHasRestrictions = 0x15,
            TokenImpersonationLevel = 9,
            TokenIntegrityLevel = 0x19,
            TokenLinkedToken = 0x13,
            TokenLogonSid = 0x1c,
            TokenMandatoryPolicy = 0x1b,
            TokenOrigin = 0x11,
            TokenOwner = 4,
            TokenPrimaryGroup = 5,
            TokenPrivileges = 3,
            TokenRestrictedSids = 11,
            TokenSandBoxInert = 15,
            TokenSessionId = 12,
            TokenSessionReference = 14,
            TokenSource = 7,
            TokenStatistics = 10,
            TokenType = 8,
            TokenUIAccess = 0x1a,
            TokenUser = 1,
            TokenVirtualizationAllowed = 0x17,
            TokenVirtualizationEnabled = 0x18
        }
    }
}

