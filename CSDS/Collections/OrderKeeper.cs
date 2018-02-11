using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CSDS.Collections
{
    public class Record<T>
    {
        public T Item;
        public Record<T> Previous;
        public Record<T> Next;
        internal OrderKeeper<T> Keeper;
        public ulong Label;
        public Record(OrderKeeper<T> keeper, T item, ulong label)
        {
            Keeper = keeper;
            Item = item;
            Label = label;
            Previous = this;
            Next = this;
        }
        public Record(OrderKeeper<T> keeper, T item, ulong label, Record<T> previous, Record<T> next)
        {
            Keeper = keeper;
            Item = item;
            Label = label;
            Previous = previous ?? keeper.First;
            Next = next ?? keeper.First;
        }
    }
    /// <summary>
    /// Implements an order-maintenance data structure as described in Dietz and Sleator, 88.
    /// http://www.cs.cmu.edu/~sleator/papers/maintaining-order.pdf
    /// </summary>
    /// <typeparam name="T">The type of record to maintain order for</typeparam>
    public class OrderKeeper<T> : KeyedCollection<T, Record<T>>, IEnumerable<T>
    {
#if EXTRA
        public int relabelings = 0;
#endif
        //public HashSet<ulong> LabelSet = new HashSet<ulong>();
        public Record<T> First { get; internal set; }
        /// <summary>
        /// Constructs an OrderKeeper with its necessary initial element (empty OrderKeepers aren't possible).
        /// </summary>
        /// <param name="initial">The first element to place in the ordering</param>
        public OrderKeeper(T initial) : base()
        {
            Add((First = new Record<T>(this, default, 0UL)));
            AddAfter(First, initial);
        }
        /// <summary>
        /// Constructs an OrderKeeper with its necessary initial element and optional extra elements (empty OrderKeepers aren't possible).
        /// </summary>
        /// <param name="initial">The first element to place in the ordering</param>
        public OrderKeeper(T initial, params T[] rest) : this(initial)
        {
            if(rest == null || rest.Length == 0)
                return;
            T current = rest[0];
            AddAfter(initial, current);
            for(int i = 1; i < rest.Length; i++)
            {
                AddAfter(current, current = rest[i]);
            }
        }

        protected override T GetKeyForItem(Record<T> record)
        {
            return record.Item;
        }

        /// <summary>
        /// Given two T items present in this OrderKeeper, compares their positions, returning true if x is earlier.
        /// </summary>
        /// <param name="x">A T item present in this OrderKeeper</param>
        /// <param name="y">A T item present in this OrderKeeper</param>
        /// <returns>true if x and y are both present and x is earlier in the ordering than y, otherwise false</returns>
        public bool OrderOf(T x, T y)
        {
            return Contains(x) && Contains(y) && this[x].Label - this[0].Label < this[y].Label - this[0].Label;
        }
        /// <summary>
        /// Given an existing Record of T to use as a starting point in the ordering and a T item to add, puts the
        /// item immediately after the Record in the ordering.
        /// </summary>
        /// <param name="existing">a Record of T that must exist in this OrderKeeper; commonly obtained via square-bracket access</param>
        /// <param name="adding">a T item to add to the OrderKeeper immediately after the Record</param>
        public void AddAfter(Record<T> existing, T adding)
        {
            AddAfter(existing.Item, adding);
        }
        /// <summary>
        /// Given an existing T item to use as a starting point in the ordering and another T item to add, puts the
        /// second item immediately after the first in the ordering.
        /// </summary>
        /// <param name="existing">a T item that must exist in this OrderKeeper</param>
        /// <param name="adding">a T item to add to the OrderKeeper immediately after the other item</param>
        public void AddAfter(T existing, T adding)
        {
            if((existing != null && !Contains(existing)) || Contains(adding))
                return;
            unchecked
            {
                Record<T> rec = existing == null ? First : this[existing], succ = rec.Next, put;
                ulong existingLabel = rec.Label, baseLabel = this[0].Label;
                if(succ.Equals(rec))
                {
                    put = new Record<T>(this, adding, (existingLabel - baseLabel >> 1) + 0x8000000000000000UL + baseLabel, rec, rec);
                    rec.Previous = put;
                    rec.Next = put;
                    Add(put);
                    return;
                }
                ulong j = 1UL, w;
                while((w = succ.Label - existingLabel) <= j * j)
                {
                    if(++j >= (ulong)Count)
                    {
                        break;
                    }
                    succ = succ.Next;
                }
                w = succ.Label - existingLabel;
                put = rec.Next;
                for(ulong k = 1UL; k < j; k++)
                {
                    if(j >= (ulong)Count)
                        put.Label = (0x8000000000000000UL / j * k << 1) + existingLabel;
                    else
                        put.Label = w / j * k + existingLabel;
                    put = put.Next;
#if EXTRA
                    ++relabelings;
#endif
                }
                baseLabel = this[0].Label;
                put = rec.Next.Equals(First)
                    ? new Record<T>(this, adding, (rec.Label - baseLabel >> 1) + 0x8000000000000000UL + baseLabel, rec, rec.Next)
                    : new Record<T>(this, adding, (rec.Label + rec.Next.Label >> 1), rec, rec.Next);
                rec.Next.Previous = put;
                rec.Next = put;
                Add(put);
            }
        }

        /// <summary>
        /// Given an existing Record of T to use as a starting point in the ordering and a T item to add, puts the
        /// item immediately before the Record in the ordering.
        /// </summary>
        /// <param name="existing">a Record of T that must exist in this OrderKeeper; commonly obtained via square-bracket access</param>
        /// <param name="adding">a T item to add to the OrderKeeper immediately before the Record</param>
        public void AddBefore(Record<T> existing, T adding)
        {
            AddBefore(existing.Item, adding);
        }

        /// <summary>
        /// Given an existing T item to use as a starting point in the ordering and another T item to add, puts the
        /// second item immediately before the existing item in the ordering.
        /// </summary>
        /// <param name="existing">a T item that must exist in this OrderKeeper</param>
        /// <param name="adding">a T item to add to the OrderKeeper immediately before the other item</param>
        public void AddBefore(T existing, T adding)
        {
            Record<T> ex = this[existing];
            if(ex.Equals(First))
            {
                AddAfter(First, adding);
                First = this[adding];
                ex.Previous.Next = First;
                ex.Next = First.Next;
                First.Previous = ex.Previous;
                ex.Previous = First;
                First.Next = ex;
            }
            else if(ex.Previous.Equals(First))
                AddAfter(First, adding);
            else
                AddAfter(ex.Previous.Item, adding);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            Record<T> rec = First.Next;
            yield return rec.Item;
            while(!(rec = rec.Next).Equals(First))
                yield return rec.Item;
        }

        public new System.Collections.IEnumerator GetEnumerator()
        {
            Record<T> rec = First.Next;
            yield return rec.Item;
            while(!(rec = rec.Next).Equals(First))
                yield return rec.Item;
        }


    }
}
