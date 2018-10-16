namespace HttpConfig
{
    using System;
    using System.Runtime.InteropServices;

    internal class SecurityApi
    {
        private const int ERROR_INSUFFICIENT_BUFFER = 0x7a;
        private const int ERROR_NO_TOKEN = 0x3f0;
        private const int SecurityImpersonation = 2;
        private const int TOKEN_QUERY = 8;
        private const int WinBuiltinAdministratorsSid = 0x1a;

        [DllImport("Advapi32", SetLastError=true)]
        private static extern bool AddAccessAllowedAce(IntPtr pAcl, int dwAceRevision, int AccessMask, IntPtr pSid);
        [DllImport("Advapi32", CharSet=CharSet.Unicode)]
        private static extern void BuildExplicitAccessWithName(IntPtr pExplicitAccess, string pTrusteeName, int AccessPermissions, int AccessMode, int Inheritance);
        [DllImport("Advapi32", SetLastError=true)]
        private static extern bool CheckTokenMembership(IntPtr TokenHandle, IntPtr SidToCheck, out bool IsMember);
        [DllImport("Kernel32", SetLastError=true)]
        private static extern bool CloseHandle(IntPtr hObject);
        [DllImport("Advapi32.dll", EntryPoint="ConvertSidToStringSidW", SetLastError=true)]
        internal static extern bool ConvertSidToStringSid(IntPtr Sid, out IntPtr StringSid);
        [DllImport("Advapi32.dll", EntryPoint="ConvertStringSidToSidW", SetLastError=true)]
        internal static extern bool ConvertStringSidToSid([MarshalAs(UnmanagedType.LPWStr)] string StringSid, out IntPtr Sid);
        [DllImport("Advapi32", SetLastError=true)]
        private static extern bool CreateWellKnownSid(int WellKnownSidType, IntPtr DomainSid, IntPtr pSid, ref int cbSid);
        [DllImport("Kernel32", SetLastError=true)]
        private static extern IntPtr GetCurrentThread();
        [DllImport("Advapi32", SetLastError=true)]
        private static extern int GetLengthSid(IntPtr pSid);
        [DllImport("Advapi32", CharSet=CharSet.Unicode)]
        private static extern int GetNamedSecurityInfo(string pObjectName, int ObjectType, int SecurityInfo, int ppsidOwner, int ppsidGroup, out IntPtr ppDacl, int ppSacl, out IntPtr ppSecurityDescriptor);
        private static IntPtr GetSid(int sidType)
        {
            int cbSid = 0;
            CreateWellKnownSid(sidType, IntPtr.Zero, IntPtr.Zero, ref cbSid);
            int num2 = Marshal.GetLastWin32Error();
            if (num2 != 0x7a)
            {
                throw new Exception("CreateWellKnownSid failed (" + num2 + ").");
            }
            IntPtr pSid = Marshal.AllocHGlobal(cbSid);
            if (!CreateWellKnownSid(sidType, IntPtr.Zero, pSid, ref cbSid))
            {
                Marshal.FreeHGlobal(pSid);
                throw new Exception("CreateWellKnownSid failed (" + Marshal.GetLastWin32Error() + ").");
            }
            return pSid;
        }

        [DllImport("Advapi32", SetLastError=true)]
        private static extern bool ImpersonateSelf(int ImpersonationLevel);
        [DllImport("Advapi32", SetLastError=true)]
        private static extern bool InitializeAcl(IntPtr pAcl, int nAclLength, int dwAclRevision);
        [DllImport("Kernel32.dll", SetLastError=true)]
        internal static extern IntPtr LocalFree(IntPtr hMem);
        [DllImport("Advapi32.dll", EntryPoint="LookupAccountNameW", SetLastError=true)]
        internal static extern bool LookupAccountName([MarshalAs(UnmanagedType.LPWStr)] string lpSystemName, [MarshalAs(UnmanagedType.LPWStr)] string lpAccountName, IntPtr Sid, ref int cbSid, IntPtr ReferencedDomainName, ref int cchReferencedDomainName, out SidNameUse peUse);
        [DllImport("Advapi32.dll", EntryPoint="LookupAccountSidW", SetLastError=true)]
        internal static extern bool LookupAccountSid([MarshalAs(UnmanagedType.LPWStr)] string lpSystemName, IntPtr lpSid, IntPtr lpName, ref int cchName, IntPtr lpReferencedDomainName, ref int cchReferencedDomainName, out SidNameUse peUse);
        [DllImport("Advapi32", SetLastError=true)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, out IntPtr TokenHandle);
        [DllImport("Advapi32", SetLastError=true)]
        private static extern bool OpenThreadToken(IntPtr ThreadHandle, int DesiredAccess, bool OpenAsSelf, out IntPtr TokenHandle);
        [DllImport("Advapi32", SetLastError=true)]
        private static extern bool RevertToSelf();
        [DllImport("Advapi32", CharSet=CharSet.Unicode)]
        private static extern int SetEntriesInAcl(int cCountOfExplicitEntries, IntPtr pListOfExplicitEntries, IntPtr OldAcl, out IntPtr NewAcl);
        [DllImport("Advapi32", CharSet=CharSet.Unicode)]
        private static extern int SetNamedSecurityInfo(string pObjectName, int ObjectType, int SecurityInfo, IntPtr psidOwner, IntPtr psidGroup, IntPtr pDacl, IntPtr pSacl);

        internal static bool IsAdmin
        {
            get
            {
                bool flag2;
                IntPtr zero = IntPtr.Zero;
                IntPtr sidToCheck = IntPtr.Zero;
                try
                {
                    bool flag;
                    IntPtr currentThread = GetCurrentThread();
                    if (!OpenThreadToken(currentThread, 8, true, out zero))
                    {
                        if (Marshal.GetLastWin32Error() != 0x3f0)
                        {
                            throw new Exception("OpenThreadToken failed (" + Marshal.GetLastWin32Error() + ").");
                        }
                        if (!ImpersonateSelf(2))
                        {
                            throw new Exception("ImpersonateSelf failed (" + Marshal.GetLastWin32Error() + ").");
                        }
                        if (!OpenThreadToken(currentThread, 8, true, out zero))
                        {
                            throw new Exception("OpenThreadToken failed (" + Marshal.GetLastWin32Error() + ").");
                        }
                    }
                    sidToCheck = GetSid(0x1a);
                    if (!CheckTokenMembership(zero, sidToCheck, out flag))
                    {
                        throw new Exception("CheckTokenMembership failed (" + Marshal.GetLastWin32Error() + ").");
                    }
                    flag2 = flag;
                }
                finally
                {
                    if (zero != IntPtr.Zero)
                    {
                        CloseHandle(zero);
                    }
                    if (sidToCheck != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(sidToCheck);
                    }
                    if (!RevertToSelf())
                    {
                        throw new Exception("RevertToSelf failed (" + Marshal.GetLastWin32Error() + ").");
                    }
                }
                return flag2;
            }
        }

        internal enum Error
        {
            ERROR_INSUFFICIENT_BUFFER = 0x7a,
            ERROR_NONE_MAPPED = 0x534,
            ERROR_TRUSTED_RELATIONSHIP_FAILURE = 0x6fd,
            SUCCESS = 0
        }

        internal enum SidNameUse
        {
            SidTypeAlias = 4,
            SidTypeComputer = 9,
            SidTypeDeletedAccount = 6,
            SidTypeDomain = 3,
            SidTypeGroup = 2,
            SidTypeInvalid = 7,
            SidTypeUnknown = 8,
            SidTypeUser = 1,
            SidTypeWellKnownGroup = 5
        }
    }
}

