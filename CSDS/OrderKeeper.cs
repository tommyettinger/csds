using System.Collections.ObjectModel;

namespace CSDS
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
            Previous = previous ?? keeper.BaseRecord;
            Next = next ?? keeper.BaseRecord;
        }
    }
    /// <summary>
    /// Implements an order-maintenance data structure as described in Dietz and Sleator, 88.
    /// http://www.cs.cmu.edu/~sleator/papers/maintaining-order.pdf
    /// </summary>
    /// <typeparam name="T">The type of record to maintain order for</typeparam>
    public class OrderKeeper<T> : KeyedCollection<T, Record<T>>
    {
        /// <summary>
        /// The first element placed in the ordering.
        /// </summary>
        public Record<T> BaseRecord;
        
        /// <summary>
        /// Constructs an OrderKeeper with its necessary initial element (empty OrderKeepers aren't possible).
        /// </summary>
        /// <param name="initial">The first element to place in the ordering</param>
        public OrderKeeper(T initial) : base()
        {
            BaseRecord = new Record<T>(this, initial, 0UL);
            Add(BaseRecord);
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
            return Contains(x) && Contains(y) && this[x].Label < this[y].Label;
        }
        /// <summary>
        /// Given an existing T item to use as a starting point in the ordering and another T item to add, puts the
        /// second item immediately after the first in the ordering.
        /// </summary>
        /// <param name="existing">a T item that must exist in this OrderKeeper</param>
        /// <param name="adding">a T item to add to the OrderKeeper immediately after the other item</param>
        public void AddAfter(T existing, T adding)
        {
            if(!Contains(existing) || Contains(adding))
                return;
            Record<T> rec = this[existing], succ = rec.Next, put;
            ulong existingLabel = rec.Label, baseLabel = BaseRecord.Label;
            if(succ.Equals(rec))
            {
                put = new Record<T>(this, adding, (existingLabel - baseLabel) / 2 + 0x8000000000000000UL + baseLabel, rec, rec);
                rec.Previous = put;
                rec.Next = put;
                Add(put);
                return;
            }
            ulong j = 1UL, w;
            while((w = succ.Label - existingLabel) <= j * j)
            {
                if(++j >= (ulong)Count)
                    break;
                succ = succ.Next;
            }
            succ = rec.Next;
            for(uint k = 1U; k < j; k++)
            {
                succ.Label = w * k / j + existingLabel;
                succ = succ.Next;
            }
            w = existingLabel - baseLabel;
            put = new Record<T>(this, adding, (rec.Next.Label - baseLabel - w) / 2 + w + baseLabel, rec, rec.Next);
            rec.Next.Previous = put;
            rec.Next = put;
            Add(put);
        }
    }
}
