namespace UWS.Configuration.COM
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using UWS.Configuration;

    [ComVisible(true), ClassInterface(ClassInterfaceType.None), ComDefaultInterface(typeof(IComCompatibleCollection)), Guid("0AD29230-393D-48B3-B013-C31FBC49DE2A")]
    public class ListenAddressComCollection : ComCompatibleCollection
    {
        public ListenAddressComCollection(IList collection) : base(collection)
        {
        }

        private static string AddressToString(object address)
        {
            if (address is ListenAddress)
            {
                address = ((ListenAddress) address).RawAddress;
            }
            return (address as string);
        }

        protected override object CollectionItemToComType(object obj)
        {
            return AddressToString(obj);
        }

        protected override object ComTypeToCollectionItem(object obj)
        {
            return StringToAddress(obj);
        }

        private static ListenAddress StringToAddress(object address)
        {
            if (address is string)
            {
                string str = (string) address;
                address = new ListenAddress(str);
            }
            return (address as ListenAddress);
        }
    }
}

