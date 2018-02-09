using System;
using System.Collections.Generic;

namespace CSDS.Collections
{
    public class OrderedSetNode<K>
    {
        public K Key;
        public OrderedSetNode<K> Next, Previous;
        public OrderedSetNode(K key)
        {
            Key = key;
        }
        public OrderedSetNode(K key, OrderedSetNode<K> previous, OrderedSetNode<K> next)
        {
            Key = key;
            Next = next;
            Previous = previous;
        }
    }

    public class OrderedSet<T> : ICollection<T>, IEnumerable<T>, ISet<T>
    {
        public OrderedSetNode<T> First, Last;
        Dictionary<T, OrderedSetNode<T>> Mapping;

        public int Count
        {
            get
            {
                return Mapping.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public OrderedSet() : base()
        {
            Mapping = new Dictionary<T, OrderedSetNode<T>>();
        }
        public OrderedSet(int capacity) : base()
        {
            Mapping = new Dictionary<T, OrderedSetNode<T>>(capacity);
        }

        public bool AddAfter(OrderedSetNode<T> existing, T adding)
        {
            if(Mapping.ContainsKey(adding))
               return false;
            if(First == null)
            {
                OrderedSetNode<T> nd = new OrderedSetNode<T>(adding);
                Mapping[adding] = nd;
                First = nd;
                Last = nd;
                return true;
            }
            OrderedSetNode<T> n = new OrderedSetNode<T>(adding, existing, existing.Next);
            Mapping[adding] = n;
            if(n.Next == null)
            {
                Last = n;
                existing.Next = n;
                return true;
            }
            existing.Next.Previous = n;
            existing.Next = n;
            return true;
        }

        public bool Add(T adding)
        {
            if(Mapping.ContainsKey(adding))
               return false;
            if(First == null)
            {
                OrderedSetNode<T> nd = new OrderedSetNode<T>(adding);
                Mapping[adding] = nd;
                First = nd;
                Last = nd;
                return true;
            }
            OrderedSetNode<T> n = new OrderedSetNode<T>(adding, Last, null);
            Mapping[adding] = n;
            Last.Next = n;
            Last = n;
            return true;       
        }

        public bool AddAfter(T existing, T adding)
        {
            if(First == null)
            {
                OrderedSetNode<T> n = new OrderedSetNode<T>(adding);
                Mapping[adding] = n;
                First = n;
                Last = n;
                return true;
            }
            else if(existing == null)
            {
                return AddBefore(First, adding);
            }
            else
            {
                return AddAfter(Mapping[existing], adding);
            }
        }
        
        public bool AddBefore(OrderedSetNode<T> existing, T adding)
        {
            if(Mapping.ContainsKey(adding))
               return false;
            if(Last == null)
            {
                OrderedSetNode<T> nd = new OrderedSetNode<T>(adding);
                Mapping[adding] = nd;
                First = nd;
                Last = nd;
                return true;
            }
            OrderedSetNode<T> n = new OrderedSetNode<T>(adding, existing.Previous, existing);
            Mapping[adding] = n;
            if(n.Previous == null)
            {
                First = n;
                existing.Previous = n;
                return true;
            }
            existing.Previous.Next = n;
            existing.Previous = n;
            return true;
        }

        public bool AddBefore(T existing, T adding)
        {
            if(Last == null)
            {
                OrderedSetNode<T> n = new OrderedSetNode<T>(adding);
                Mapping[adding] = n;
                First = n;
                Last = n;
                return true;
            }
            else if(existing == null)
            {
                return AddAfter(Last, adding);
            }
            else
            {
                return AddAfter(Mapping[existing], adding);
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            if(First == null)
                yield break;
            OrderedSetNode<T> rec = First;
            yield return rec.Key;
            while((rec = rec.Next) != null)
                yield return rec.Key;
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            if(First == null)
                yield break;
            OrderedSetNode<T> rec = First;
            yield return rec.Key;
            while((rec = rec.Next) != null)
                yield return rec.Key;
        }

        public void Clear()
        {
            Mapping.Clear();
            First = null;
            Last = null;
        }

        public bool Contains(T item)
        {
            return Mapping.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if(First == null)
                return;
            int len = array.Length;
            OrderedSetNode<T> rec = First;
            array[arrayIndex++] = rec.Key;
            while((rec = rec.Next) != null)
                array[arrayIndex++] = rec.Key;

        }

        public bool Remove(T item)
        {
            OrderedSetNode<T> n = Mapping[item];
            if(n != null)
            {
                if(n.Next == null && n.Previous == null)
                {
                    First = null;
                    Last = null;
                }
                else if(n.Next == null)
                {
                    Last = n.Previous;
                    n.Previous.Next = null;
                }
                else if(n.Previous == null)
                {
                    First = n.Next;
                    n.Next.Previous = null;
                }
                else
                {
                    n.Next.Previous = n.Previous;
                    n.Previous.Next = n.Next;
                }
                Mapping.Remove(item);
                return true;
            }
            return false;
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public void UnionWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }
    }
    
}
