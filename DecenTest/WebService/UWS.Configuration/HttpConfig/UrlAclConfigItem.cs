namespace HttpConfig
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class UrlAclConfigItem
    {
        private bool needUpdate;
        private bool presentInHttpCfg;

        private UrlAclConfigItem()
        {
            this.needUpdate = true;
            this.Dacl = new Acl();
        }

        private UrlAclConfigItem(string url, string user) : this()
        {
            this.Url = url;
            this.Dacl.SetUser(user);
        }

        private void ApplyConfig(ConfigItemAction action)
        {
            IntPtr zero = IntPtr.Zero;
            HttpApi.HTTP_SERVICE_CONFIG_URLACL_SET structure = new HttpApi.HTTP_SERVICE_CONFIG_URLACL_SET {
                KeyDesc = { pUrlPrefix = this.Url },
                ParamDesc = { pStringSecurityDescriptor = this.Dacl.ToSddl() }
            };
            try
            {
                zero = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
                Marshal.StructureToPtr(structure, zero, false);
                if (this.presentInHttpCfg)
                {
                    ErrorCheck.VerifySuccess(HttpApi.HttpDeleteServiceConfiguration(IntPtr.Zero, HttpApi.HTTP_SERVICE_CONFIG_ID.HttpServiceConfigUrlAclInfo, zero, Marshal.SizeOf(structure), IntPtr.Zero), "HttpDeleteServiceConfiguration (URLACL) failed.", new object[0]);
                }
                if ((action == ConfigItemAction.Create) || (action == ConfigItemAction.Update))
                {
                    HttpApi.Error error = HttpApi.HttpSetServiceConfiguration(IntPtr.Zero, HttpApi.HTTP_SERVICE_CONFIG_ID.HttpServiceConfigUrlAclInfo, zero, Marshal.SizeOf(structure), IntPtr.Zero);
                    if (error != HttpApi.Error.ERROR_ALREADY_EXISTS)
                    {
                        ErrorCheck.VerifySuccess(error, "HttpSetServiceConfiguration (URLACL) failed.", new object[0]);
                    }
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.DestroyStructure(zero, typeof(HttpApi.HTTP_SERVICE_CONFIG_URLACL_SET));
                    Marshal.FreeHGlobal(zero);
                }
            }
        }

        private static UrlAclConfigItem Deserialize(IntPtr pUrlAclConfigSetStruct)
        {
            UrlAclConfigItem item = new UrlAclConfigItem();
            HttpApi.HTTP_SERVICE_CONFIG_URLACL_SET http_service_config_urlacl_set = (HttpApi.HTTP_SERVICE_CONFIG_URLACL_SET) Marshal.PtrToStructure(pUrlAclConfigSetStruct, typeof(HttpApi.HTTP_SERVICE_CONFIG_URLACL_SET));
            item.Url = http_service_config_urlacl_set.KeyDesc.pUrlPrefix;
            item.Dacl = Acl.FromSddl(http_service_config_urlacl_set.ParamDesc.pStringSecurityDescriptor);
            item.presentInHttpCfg = true;
            return item;
        }

        internal static UrlAclConfigItem LoadOrCreateConfigItem(string url, string user, ref object allUrlsRaw)
        {
            UrlAclConfigItem item;
            string key = url.ToLowerInvariant();
            Dictionary<string, UrlAclConfigItem> dictionary = null;
            if (allUrlsRaw != null)
            {
                dictionary = (Dictionary<string, UrlAclConfigItem>) allUrlsRaw;
            }
            else
            {
                dictionary = QueryConfig();
                allUrlsRaw = dictionary;
            }
            if (!dictionary.TryGetValue(key, out item))
            {
                item = new UrlAclConfigItem(url, user);
                dictionary[key] = item;
                return item;
            }
            if (!item.Dacl.MatchesUser(key))
            {
                item.Dacl.SetUser(user);
                return item;
            }
            item.needUpdate = false;
            return item;
        }

        internal static Dictionary<string, UrlAclConfigItem> QueryConfig()
        {
            Dictionary<string, UrlAclConfigItem> dictionary = new Dictionary<string, UrlAclConfigItem>();
            HttpApi.HTTP_SERVICE_CONFIG_URLACL_QUERY structure = new HttpApi.HTTP_SERVICE_CONFIG_URLACL_QUERY {
                QueryDesc = HttpApi.HTTP_SERVICE_CONFIG_QUERY_TYPE.HttpServiceConfigQueryNext
            };
            HttpApi.Error error = HttpApi.Error.NO_ERROR;
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(HttpApi.HTTP_SERVICE_CONFIG_URLACL_QUERY)));
            try
            {
                structure.dwToken = 0;
                while (true)
                {
                    Marshal.StructureToPtr(structure, ptr, false);
                    int requiredBufferLength = 0;
                    error = QueryServiceConfig(ptr, IntPtr.Zero, 0, out requiredBufferLength);
                    if (error == HttpApi.Error.ERROR_NO_MORE_ITEMS)
                    {
                        return dictionary;
                    }
                    if (error != HttpApi.Error.ERROR_INSUFFICIENT_BUFFER)
                    {
                        ErrorCheck.VerifySuccess(error, "HttpQueryServiceConfiguration (URLACL) failed.", new object[0]);
                    }
                    IntPtr dest = Marshal.AllocHGlobal(requiredBufferLength);
                    try
                    {
                        HttpApi.ZeroMemory(dest, requiredBufferLength);
                        ErrorCheck.VerifySuccess(QueryServiceConfig(ptr, dest, requiredBufferLength, out requiredBufferLength), "HttpQueryServiceConfiguration (URLACL) failed.", new object[0]);
                        UrlAclConfigItem item = Deserialize(dest);
                        dictionary.Add(item.Key, item);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(dest);
                    }
                    structure.dwToken++;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return dictionary;
        }

        private static HttpApi.Error QueryServiceConfig(IntPtr pInput, IntPtr pOutput, int outputLength, out int requiredBufferLength)
        {
            return HttpApi.HttpQueryServiceConfiguration(IntPtr.Zero, HttpApi.HTTP_SERVICE_CONFIG_ID.HttpServiceConfigUrlAclInfo, pInput, Marshal.SizeOf(typeof(HttpApi.HTTP_SERVICE_CONFIG_URLACL_QUERY)), pOutput, outputLength, out requiredBufferLength, IntPtr.Zero);
        }

        internal void ReigsterUrlItem()
        {
            this.ApplyConfig(ConfigItemAction.Create);
        }

        public override string ToString()
        {
            return this.Url;
        }

        internal void UnregisterUrlItem()
        {
            this.ApplyConfig(ConfigItemAction.Delete);
        }

        internal void UpdateUlrItem()
        {
            this.ApplyConfig(ConfigItemAction.Update);
        }

        internal Acl Dacl { get; set; }

        internal string Key
        {
            get
            {
                return this.Url.ToLowerInvariant();
            }
        }

        internal bool NeedUpdate
        {
            get
            {
                return this.needUpdate;
            }
        }

        internal string Url { get; set; }
    }
}

