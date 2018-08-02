using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSDS.Collections
{
    /// <summary>
    /// A set that can also be used as a list of unique elements, and that keeps elements in insertion order.
    /// You can use an indexer with an int index to get or set the key in the IndexedSet at that position in the order.
    /// You can also use an indexer with a K index to get the presence of that item in the IndexedSet as a bool, or if
    /// setting with a K index, true to add that item or false to remove it.
    /// </summary>
    /// <typeparam name="K">Any reference type</typeparam>
    public class IndexedSet<K> : ISet<K>, IList<K>, IEnumerable<K>, ICollection<K>, IEnumerable where K : class
    {
        private HashSet<K> Items;
        private List<K> Order;
        public IndexedSet()
        {
            Items = new HashSet<K>();
            Order = new List<K>();
        }
        public IndexedSet(int capacity)
        {
            Items = new HashSet<K>();
            Order = new List<K>(capacity);
        }
        public IndexedSet(IEqualityComparer<K> comparer)
        {
            Items = new HashSet<K>(comparer);
            Order = new List<K>();
        }
        public IndexedSet(IEnumerable<K> collection)
        {
            Order = new List<K>(collection.Distinct());
            Items = new HashSet<K>(Order);
        }

        public IndexedSet(IEnumerable<K> collection, IEqualityComparer<K> comparer)
        {
            Order = new List<K>(collection.Distinct(comparer));
            Items = new HashSet<K>(Order, comparer);
        }

        public K this[int index] { get => Order[index]; set { Items.Remove(Order[index]); if(Items.Add(value)) Order[index] = value; } }
        public bool this[K item] { get => Contains(item); set { if (value) Add(item); else Remove(item); } }
        public int Count => Order.Count;

        public bool IsReadOnly => false;

        public bool Add(K item)
        {
            if (Items.Add(item))
            {
                Order.Add(item);
                return true;
            }
            else return false;
        }

        public void Clear()
        {
            Items.Clear();
            Order.Clear();
        }

        public bool Contains(K item)
        {
            return Items.Contains(item);
        }

        public void CopyTo(K[] array, int arrayIndex)
        {
            Order.CopyTo(array, arrayIndex);
        }

        public void ExceptWith(IEnumerable<K> other)
        {
            Items.ExceptWith(other);
            Order.RemoveAll(item => !Items.Contains(item));
        }

        public void ExceptWith(IndexedSet<K> other)
        {
            Items.ExceptWith(other.Items);
            Order.RemoveAll(item => !Items.Contains(item));
        }

        public IEnumerator<K> GetEnumerator()
        {
            return Order.GetEnumerator();
        }

        public int IndexOf(K item)
        {
            return Order.IndexOf(item);
        }

        public void Insert(int index, K item)
        {
            if (Items.Add(item))
            {
                Order.Insert(index, item);
            }
            else
            {
                Order.Remove(item);
                Order.Insert(index, item);
            }
        }

        public void IntersectWith(IEnumerable<K> other)
        {
            Items.IntersectWith(other);
            Order.RemoveAll(item => !Items.Contains(item));
        }

        public void IntersectWith(IndexedSet<K> other)
        {
            Items.IntersectWith(other.Items);
            Order.RemoveAll(item => !Items.Contains(item));
        }

        public bool IsProperSubsetOf(IndexedSet<K> other)
        {
            return Items.IsProperSubsetOf(other.Items);
        }

        public bool IsProperSubsetOf(IEnumerable<K> other)
        {
            return Items.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IndexedSet<K> other)
        {
            return Items.IsProperSupersetOf(other.Items);
        }

        public bool IsProperSupersetOf(IEnumerable<K> other)
        {
            return Items.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IndexedSet<K> other)
        {
            return Items.IsSubsetOf(other.Items);
        }

        public bool IsSubsetOf(IEnumerable<K> other)
        {
            return Items.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IndexedSet<K> other)
        {
            return Items.IsSupersetOf(other.Items);
        }

        public bool IsSupersetOf(IEnumerable<K> other)
        {
            return Items.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<K> other)
        {
            return Items.Overlaps(other);
        }

        public bool Remove(K item)
        {
            if (Items.Remove(item))
            {
                Order.Remove(item);
                return true;
            }
            else return false;
        }

        public void RemoveAt(int index)
        {
            Items.Remove(Order[index]);
            Order.RemoveAt(index);
        }

        public bool SetEquals(IEnumerable<K> other)
        {
            return Items.SetEquals(other);
        }

        public void SymmetricExceptWith(IEnumerable<K> other)
        {
            foreach(K key in other.Distinct())
            {
                if (Items.Contains(key))
                    Remove(key);
                else
                    Add(key);
            }
        }

        public void UnionWith(IEnumerable<K> other)
        {
            foreach (K key in other)
                Add(key);
        }

        void ICollection<K>.Add(K item)
        {
            Add(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Order.GetEnumerator();
        }
    }
}
