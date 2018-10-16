namespace UWS.Framework
{
    using HttpConfig;
    using System;
    using System.Security.Principal;

    public static class SecurityHelper
    {
        private static readonly string[] standardUserNames = new string[] { GetWellKnownWindowsAccountNameLocalized(WellKnownSidType.NetworkServiceSid), GetWellKnownWindowsAccountNameLocalized(WellKnownSidType.LocalSystemSid) };

        public static void AclListenUrls(string userName, params string[] urlsToAcl)
        {
            using (new HttpApi())
            {
                ReAclListenUrlsInternal(userName, null, urlsToAcl);
            }
        }

        public static void AclListenUrls(ProcessIdentity userContext, params string[] urlsToAcl)
        {
            AclListenUrls(UserIdToUserName(userContext), urlsToAcl);
        }

        public static void DeAclListenUrls(string userName, params string[] urlsToDeAcl)
        {
            using (new HttpApi())
            {
                ReAclListenUrlsInternal(userName, urlsToDeAcl, null);
            }
        }

        public static void DeAclListenUrls(ProcessIdentity userContext, params string[] urlsToDeAcl)
        {
            DeAclListenUrls(UserIdToUserName(userContext), urlsToDeAcl);
        }

        public static string GetLocalizedWindowsAccountNameBySid(SecurityIdentifier sid)
        {
            return sid.Translate(typeof(NTAccount)).ToString();
        }

        public static string GetWellKnownWindowsAccountNameLocalized(WellKnownSidType accountSidType)
        {
            SecurityIdentifier sid = new SecurityIdentifier(accountSidType, null);
            return GetLocalizedWindowsAccountNameBySid(sid);
        }

        public static void ReAclListenUrls(string userName, string[] urlsToDeAcl, string[] urlsToAcl)
        {
            using (new HttpApi())
            {
                ReAclListenUrlsInternal(userName, urlsToDeAcl, urlsToAcl);
            }
        }

        public static void ReAclListenUrls(ProcessIdentity userContext, string[] urlsToDeAcl, string[] urlsToAcl)
        {
            ReAclListenUrls(UserIdToUserName(userContext), urlsToDeAcl, urlsToAcl);
        }

        private static void ReAclListenUrlsInternal(string userName, string[] urlsToDeAcl, string[] urlsToAcl)
        {
            object allUrlsRaw = null;
            if (urlsToDeAcl != null)
            {
                foreach (string str in urlsToDeAcl)
                {
                    UrlAclConfigItem.LoadOrCreateConfigItem(str, userName, ref allUrlsRaw).UnregisterUrlItem();
                }
            }
            if (urlsToAcl != null)
            {
                foreach (string str2 in urlsToAcl)
                {
                    UrlAclConfigItem item2 = UrlAclConfigItem.LoadOrCreateConfigItem(str2, userName, ref allUrlsRaw);
                    if (item2.NeedUpdate)
                    {
                        item2.ReigsterUrlItem();
                    }
                }
            }
        }

        public static string UserIdToUserName(ProcessIdentity userContext)
        {
            return standardUserNames[(int) userContext];
        }
    }
}

