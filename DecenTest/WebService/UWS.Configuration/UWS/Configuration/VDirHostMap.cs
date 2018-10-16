namespace UWS.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    [ComVisible(false)]
    public class VDirHostMap : Dictionary<string, List<HostInfo>>
    {
        internal static readonly string wildcardHost;

        static VDirHostMap()
        {
            Guid guid2 = new Guid();
            wildcardHost = guid2.ToString("D").ToLowerInvariant();
        }

        public VDirHostMap(HttpListenerPrefixCollection prefixes)
        {
            foreach (string str in prefixes)
            {
                Regex.Replace(str, @"[\*\+]", wildcardHost);
                Uri uri = new Uri(str.ToLowerInvariant());
                string uriVroot = GetUriVroot(uri);
                List<HostInfo> list = null;
                if (!base.TryGetValue(uriVroot, out list))
                {
                    list = new List<HostInfo>();
                    base.Add(uriVroot, list);
                }
                list.Add(new HostInfo(uri));
            }
        }

        private static string GetUriVroot(Uri uri)
        {
            string absolutePath = uri.AbsolutePath;
            if ((absolutePath == "/") || string.IsNullOrEmpty(absolutePath))
            {
                return "/";
            }
            return absolutePath.TrimEnd(new char[] { '/' }).ToLowerInvariant();
        }

        public string GetVDir(Uri uri)
        {
            int length = 0;
            for (string str = GetUriVroot(uri); (str.Length > 0) && (length >= 0); str = str.Substring(0, length))
            {
                List<HostInfo> list = null;
                if (base.TryGetValue(str, out list))
                {
                    foreach (HostInfo info in list)
                    {
                        if (info.Matches(uri))
                        {
                            return str;
                        }
                    }
                }
            }
            throw new ApplicationException(string.Format("Unable to match \"{0}\" to any registered HttpListener prefixes.", uri));
        }
    }
}

