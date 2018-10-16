namespace UWS.Configuration.COM
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("D7E61EBC-18D5-4A79-8585-C80C799B7755")]
    public interface IComCompatibleCollection
    {
        int Add(object value);
        void Clear();
        bool Contains(object value);
        int IndexOf(object value);
        void Insert(int index, object value);
        void Remove(object value);
        void RemoveAt(int index);
        int Count { get; }
        [DispId(-4)]
        IEnumerator GetEnumerator();
        //[DispId(0)]
        //object this[int index] { get; set; }
        object Item(int index);
    }
}

