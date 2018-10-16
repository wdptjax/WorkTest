namespace UWS.Configuration
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class UniqueList<T> : List<T>
    {
        public void Add(T item)
        {
            if (!base.Contains(item))
            {
                base.Add(item);
            }
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (collection != null)
            {
                foreach (T local in collection)
                {
                    this.Add(local);
                }
            }
        }
    }
}

