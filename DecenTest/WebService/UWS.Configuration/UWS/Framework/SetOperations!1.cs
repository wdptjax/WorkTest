namespace UWS.Framework
{
    using System;
    using System.Collections.Generic;

    public static class SetOperations<T>
    {
        private static bool Contains(IEnumerable<T> set2, T elem)
        {
            foreach (T local in set2)
            {
                if (SetOperations<T>.Equal(local, elem))
                {
                    return true;
                }
            }
            return false;
        }

        public static List<T> Difference(ICollection<T> set1, ICollection<T> set2)
        {
            List<T> list = new List<T>();
            foreach (T local in set1)
            {
                if (!SetOperations<T>.Contains(set2, local))
                {
                    list.Add(local);
                }
            }
            return list;
        }

        private static bool Equal(T one, T two)
        {
            if (one == null)
            {
                return (two == null);
            }
            return one.Equals(two);
        }

        public static List<T> Intersection(ICollection<T> set1, ICollection<T> set2)
        {
            List<T> list = new List<T>();
            foreach (T local in set1)
            {
                if (SetOperations<T>.Contains(set2, local))
                {
                    list.Add(local);
                }
            }
            return list;
        }

        public static List<T> Union(ICollection<T> set1, ICollection<T> set2)
        {
            List<T> list = new List<T>();
            list.AddRange(set1);
            list.AddRange(SetOperations<T>.Difference(set2, set1));
            return list;
        }

        public static List<T> Xor(ICollection<T> set1, ICollection<T> set2)
        {
            List<T> list = new List<T>();
            list.AddRange(SetOperations<T>.Difference(set1, set2));
            list.AddRange(SetOperations<T>.Difference(set2, set1));
            return list;
        }
    }
}

