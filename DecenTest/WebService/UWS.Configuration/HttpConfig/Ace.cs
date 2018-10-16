namespace HttpConfig
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    internal class Ace
    {
        private bool _accountNameMapped;
        private string _otherPerm;
        private UrlPermission _permission;
        private string _user;

        private Ace()
        {
        }

        internal Ace(string user) : this(user, true, UrlPermission.Registration, null)
        {
        }

        internal Ace(string user, bool accountNameMapped, UrlPermission permission, string otherPerm)
        {
            this._user = user;
            this._accountNameMapped = accountNameMapped;
            this._permission = permission;
            this._otherPerm = otherPerm;
        }

        internal void AddSddl(StringBuilder sddl)
        {
            sddl.Append("(A;;");
            switch (this._permission)
            {
                case UrlPermission.All:
                    sddl.Append("GA");
                    break;

                case UrlPermission.Registration:
                    sddl.Append("GX");
                    break;

                case UrlPermission.Delegation:
                    sddl.Append("GW");
                    break;

                case UrlPermission.Other:
                    sddl.Append(this._otherPerm);
                    break;
            }
            sddl.Append(";;;");
            sddl.Append(this._accountNameMapped ? this.EncodeSid() : this._user);
            sddl.Append(")");
        }

        private static bool DecodeSid(string stringSid, out string accountName)
        {
            bool flag;
            IntPtr pSid = IntPtr.Zero;
            IntPtr pAccount = IntPtr.Zero;
            IntPtr pDomain = IntPtr.Zero;
            try
            {
                SecurityApi.SidNameUse use;
                accountName = stringSid;
                if (!SecurityApi.ConvertStringSidToSid(stringSid, out pSid))
                {
                    throw new Exception("ConvertStringSidToSid failed.  Error = " + Marshal.GetLastWin32Error().ToString());
                }
                int accountLength = 0;
                int domainLength = 0;
                int error = 0;
                Thread thread = new Thread(() =>
                {
                    if (!SecurityApi.LookupAccountSid(null, pSid, pAccount, ref accountLength, pDomain, ref domainLength, out use))
                    {
                        error = Marshal.GetLastWin32Error();
                    }
                });

                thread.Start();
                if (!thread.Join(0x2710))
                {
                    thread.Abort();
                    thread = null;
                    Trace.TraceWarning(string.Format("Timed out while trying to lookup SID of \"{0}\" account. There might be a problem with the DNS.", accountName));
                    return false;
                }
                if ((error != 0) && (error != 0x7a))
                {
                    if ((error != 0x534) && (error != 0x6fd))
                    {
                        throw new Exception("LookupAccountSid failed.  Error = " + Marshal.GetLastWin32Error().ToString());
                    }
                    return false;
                }
                error = 0;
                pAccount = Marshal.AllocHGlobal((int)(accountLength * 2));
                pDomain = Marshal.AllocHGlobal((int)(domainLength * 2));
                thread = new Thread(() =>
                {
                    if (!SecurityApi.LookupAccountSid(null, pSid, pAccount, ref accountLength, pDomain, ref domainLength, out use))
                    {
                        error = Marshal.GetLastWin32Error();
                    }
                });
                thread.Start();
                if (!thread.Join(0x2710))
                {
                    thread.Abort();
                    thread = null;
                    Trace.TraceWarning(string.Format("Timed out while trying to lookup SID of \"{0}\" account. There might be a problem with the DNS.", accountName));
                    return false;
                }
                if (error != 0)
                {
                    if ((error != 0x534) && (error != 0x6fd))
                    {
                        throw new Exception("LookupAccountSid failed.  Error = " + error.ToString());
                    }
                    return false;
                }
                accountName = Marshal.PtrToStringUni(pDomain) + @"\" + Marshal.PtrToStringUni(pAccount);
                flag = true;
            }
            finally
            {
                if (pSid != IntPtr.Zero)
                {
                    SecurityApi.LocalFree(pSid);
                }
                if (pAccount != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pAccount);
                }
                if (pDomain != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pDomain);
                }
            }
            return flag;
        }

        private string EncodeSid()
        {
            string str2;
            IntPtr zero = IntPtr.Zero;
            IntPtr stringSid = IntPtr.Zero;
            IntPtr referencedDomainName = IntPtr.Zero;
            try
            {
                SecurityApi.SidNameUse use;
                int cbSid = 0;
                int cchReferencedDomainName = 0;
                if (!SecurityApi.LookupAccountName(null, this._user, zero, ref cbSid, referencedDomainName, ref cchReferencedDomainName, out use) && (Marshal.GetLastWin32Error() != 0x7a))
                {
                    throw new Exception(string.Format("LookupAccountName buffer length detection failed for user \"{0}\". Win32 Error: {1}.", this._user, Marshal.GetLastWin32Error()));
                }
                zero = Marshal.AllocHGlobal(cbSid);
                referencedDomainName = Marshal.AllocHGlobal((int)(cchReferencedDomainName * 2));
                if (!SecurityApi.LookupAccountName(null, this._user, zero, ref cbSid, referencedDomainName, ref cchReferencedDomainName, out use))
                {
                    throw new Exception(string.Format("LookupAccountName failed for user \"{0}\". Win32 Error: {1}.", this._user, Marshal.GetLastWin32Error()));
                }
                if (!SecurityApi.ConvertSidToStringSid(zero, out stringSid))
                {
                    throw new Exception(string.Format("ConvertSidToStringSid failed for user \"{0}\". Win32 Error: {1}.", this._user, Marshal.GetLastWin32Error()));
                }
                str2 = Marshal.PtrToStringUni(stringSid);
            }
            catch (Exception exception)
            {
                throw new Exception(string.Format("Failed to encode SID for account \"{0}\". See inner exception for more details.", this._user), exception);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    SecurityApi.LocalFree(zero);
                }
                if (stringSid != IntPtr.Zero)
                {
                    SecurityApi.LocalFree(stringSid);
                }
                if (referencedDomainName != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(referencedDomainName);
                }
            }
            return str2;
        }

        internal static Ace FromSddl(string sddl)
        {
            string[] strArray = sddl.Split(new char[] { ';' });
            if (strArray.Length != 6)
            {
                throw new ArgumentException("Invalid SDDL string.  Too many or too few tokens.", "sddl");
            }
            string str = strArray[2];
            string stringSid = strArray[5];
            Ace ace = new Ace();
            switch (str)
            {
                case "GA":
                    ace._permission = UrlPermission.All;
                    break;

                case "GX":
                    ace._permission = UrlPermission.Registration;
                    break;

                case "GW":
                    ace._permission = UrlPermission.Delegation;
                    break;

                default:
                    ace._permission = UrlPermission.Other;
                    ace._otherPerm = str;
                    break;
            }
            ace._accountNameMapped = DecodeSid(stringSid, out ace._user);
            return ace;
        }

        internal bool AccountNameMapped
        {
            get
            {
                return this._accountNameMapped;
            }
            set
            {
                this._accountNameMapped = value;
            }
        }

        internal string OtherPerm
        {
            get
            {
                return this._otherPerm;
            }
            set
            {
                this._otherPerm = value;
            }
        }

        internal UrlPermission Permission
        {
            get
            {
                return this._permission;
            }
            set
            {
                this._permission = value;
            }
        }

        internal string User
        {
            get
            {
                return this._user;
            }
            set
            {
                this._user = value;
            }
        }
    }
}

