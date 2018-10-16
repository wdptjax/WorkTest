namespace UWS.Configuration.COM
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [ComDefaultInterface(typeof(IComCompatibleCollection)), Guid("FEB0563D-B1DC-40B0-A62E-C3E89EDE83E0"), ComVisible(true), ClassInterface(ClassInterfaceType.None)]
    public class ComCompatibleCollection : IComCompatibleCollection
    {
        private IList collection;

        public ComCompatibleCollection() : this(null)
        {
        }

        public ComCompatibleCollection(IList collection)
        {
            if (collection == null)
            {
                this.collection = new ArrayList();
            }
            else
            {
                this.collection = collection;
            }
        }

        public virtual int Add(object value)
        {
            value = this.ComTypeToCollectionItem(value);
            int index = this.IndexOf(value);
            if (index < 0)
            {
                index = this.collection.Add(value);
            }
            return index;
        }

        public void Clear()
        {
            this.collection.Clear();
        }

        protected virtual object CollectionItemToComType(object obj)
        {
            return obj;
        }

        protected virtual object ComTypeToCollectionItem(object obj)
        {
            return obj;
        }

        public virtual bool Contains(object value)
        {
            value = this.ComTypeToCollectionItem(value);
            return this.collection.Contains(value);
        }

        public virtual int IndexOf(object value)
        {
            value = this.ComTypeToCollectionItem(value);
            return this.collection.IndexOf(value);
        }

        public virtual void Insert(int index, object value)
        {
            value = this.ComTypeToCollectionItem(value);
            this.collection.Insert(index, value);
        }

        public object Item(int index)
        {
            return this.CollectionItemToComType(this.collection[index]);
        }

        public virtual void Remove(object value)
        {
            value = this.ComTypeToCollectionItem(value);
            this.collection.Remove(value);
        }

        public virtual void RemoveAt(int index)
        {
            this.collection.RemoveAt(index);
        }

        [DispId(-4)]
        IEnumerator IComCompatibleCollection.GetEnumerator()
        {
            return this.collection.GetEnumerator();
        }

        //[DispId(0)]
        //public virtual object this[int index]
        //{
        //    get
        //    {
        //        return this.Item(index);
        //    }
        //    set
        //    {
        //        value = this.ComTypeToCollectionItem(value);
        //        if (index == this.collection.Count)
        //        {
        //            this.Add(value);
        //        }
        //        else
        //        {
        //            this.collection[index] = value;
        //        }
        //    }
        //}

        public int Count
        {
            get
            {
                return this.collection.Count;
            }
        }
    }
}

