namespace UWS.Configuration
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using UWS.Configuration.COM;

    [Serializable, ComVisible(false)]
    public class RedirectionConfigEntry : ListenEndpoint
    {
        private Uri redirectToUri;

        public RedirectionConfigEntry()
        {
            base.ChangeID = Guid.NewGuid();
            this.RedirectAllToOne = false;
            this.RedirectionCode = RedirectCode.Permanent;
        }

        internal RedirectionConfigEntry(IRedirectionConfigEntry1 comRedir) : base(comRedir)
        {
            this.RedirectToUrl = comRedir.RedirectToUrl;
            this.RedirectionCode = comRedir.RedirectionCode;
            this.RedirectAllToOne = comRedir.RedirectAllToOne;
        }

        internal override void ApplyFinalDefaultsAndValidateBeforeSaving()
        {
            if (this.RedirectToUri == null)
            {
                throw new Exception("Target redirection URL must be specified.");
            }
            base.EnsureID();
            base.ApplyFinalDefaultsAndValidateBeforeSaving();
        }

        private static string CombineQueryStrings(string qs1, string qs2)
        {
            if (string.IsNullOrEmpty(qs2))
            {
                return MassageQsStart('?', qs1);
            }
            if (string.IsNullOrEmpty(qs1))
            {
                return MassageQsStart('?', qs2);
            }
            return (MassageQsStart('?', qs1) + MassageQsStart('&', qs2));
        }

        protected override ListenEndpoint FindExistingEndpoint(Guid id)
        {
            return Metabase.FindRedirection(id);
        }

        internal static RedirectionConfigEntry FromComConfigEntry(IRedirectionConfigEntry1 redir)
        {
            if (redir is RedirectionConfigEntryComServer)
            {
                return ((RedirectionConfigEntryComServer) redir).Entry;
            }
            return new RedirectionConfigEntry(redir);
        }

        private Uri GetBaseUrlWithBogusHost()
        {
            return new Uri(base.ListenAddresses[0].ToUIUrl("fake", this.VirtualDirectory));
        }

        public string GetRedirectionUrlFor(Uri sourceUrl)
        {
            if (this.RedirectAllToOne)
            {
                return this.RedirectToUrl.ToString();
            }
            Uri baseUrlWithBogusHost = this.GetBaseUrlWithBogusHost();
            StringBuilder builder = new StringBuilder();
            builder.Append(TrimQueryString(this.RedirectToUri));
            Math.Min(sourceUrl.Segments.Length, baseUrlWithBogusHost.Segments.Length);
            for (int i = 0; i < sourceUrl.Segments.Length; i++)
            {
                if ((i >= baseUrlWithBogusHost.Segments.Length) || (baseUrlWithBogusHost.Segments[i].ToLowerInvariant() != sourceUrl.Segments[i].ToLowerInvariant()))
                {
                    builder.Append(sourceUrl.Segments[i]);
                }
            }
            string str = CombineQueryStrings(sourceUrl.Query, this.RedirectToUri.Query);
            if (!string.IsNullOrEmpty(str))
            {
                builder.Append(str);
            }
            return builder.ToString();
        }

        private static string MassageQsStart(char mustChar, string qs)
        {
            if (string.IsNullOrEmpty(qs) || (qs[0] == mustChar))
            {
                return qs;
            }
            return (mustChar + qs.TrimStart(new char[] { '?', '&' }));
        }

        private static string TrimQueryString(Uri uri)
        {
            string str = uri.ToString();
            if (!string.IsNullOrEmpty(uri.Query))
            {
                int length = str.Length - uri.Query.Length;
                str = str.Substring(0, length);
            }
            return str;
        }

        public bool RedirectAllToOne { get; set; }

        public RedirectCode RedirectionCode { get; set; }

        public Uri RedirectToUri
        {
            get
            {
                return this.redirectToUri;
            }
        }

        public string RedirectToUrl
        {
            get
            {
                return this.redirectToUri.ToString();
            }
            set
            {
                if (value == null)
                {
                    throw new Exception("Recirection URL cannot be blank.");
                }
                this.redirectToUri = new Uri(value);
            }
        }

        public override string UIApplicationName
        {
            get
            {
                return this.RedirectToUrl;
            }
        }
    }
}

