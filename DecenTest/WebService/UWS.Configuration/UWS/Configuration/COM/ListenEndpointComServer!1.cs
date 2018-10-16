namespace UWS.Configuration.COM
{
    using System;

    [Serializable]
    public abstract class ListenEndpointComServer<T> : IListenEndpoint1 where T: ListenEndpoint
    {
        protected internal T Entry;

        internal ListenEndpointComServer(T entry)
        {
            this.Entry = default(T);
            this.Entry = entry;
        }

        public string ID
        {
            get
            {
                return this.Entry.ID.ToString("B");
            }
            set
            {
                this.Entry.ID = new Guid(value);
            }
        }

        public IComCompatibleCollection ListenAddresses
        {
            get
            {
                return new ListenAddressComCollection(this.Entry.ListenAddresses);
            }
        }

        public string UIApplicationName
        {
            get
            {
                return this.Entry.UIApplicationName;
            }
        }

        public string VirtualDirectory
        {
            get
            {
                return this.Entry.VirtualDirectory;
            }
            set
            {
                this.Entry.VirtualDirectory = value;
            }
        }
    }
}

