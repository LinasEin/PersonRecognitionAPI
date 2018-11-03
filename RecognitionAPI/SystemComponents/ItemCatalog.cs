using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WhosThat.Helper
{
    public class ItemCatalog<T> : IEnumerable<T>
    {
        private List<T> labels;

        public ItemCatalog()
        {
            labels= new List<T>();
        }

        public int Count()
        {
            return labels.Count;
        }
       
        public void Add(T value)
        {
            labels.Add(value);
        }
        public void Clear()
        {
            labels.Clear();
        }

        public bool Contains(T item)
        {
            return labels.Contains(item);
        }
        public IEnumerator<T> GetEnumerator()
        {
            return labels.GetEnumerator();
        }
        public T[] ToArray()
        {
            return labels.ToArray();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}