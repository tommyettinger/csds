using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DSCCSDS
{ // Updated 2017-01-21

    public static class CircularLinkedListExtensions
    {
        public static LinkedListNode<T> GetNextCircular<T>(this LinkedListNode<T> node)
        {
            return node.Next ?? node.List.First;
        }
        public static LinkedListNode<T> GetPreviousCircular<T>(this LinkedListNode<T> node)
        {
            return node.Previous ?? node.List.Last;
        }
    }
    public class OrderingCollection<T>
    {

        public override string ToString()
        {
            if(list == null) return base.ToString();
            if(list.Count == 0) return "(empty)";
            return string.Join(", ", list.Select(x => x.Label));
        }

        ulong MOD_M(ulong label) => label;

        public int Insertions = 0;
        public int Relabelings = 0;

        private class OrderingNode
        {
            public T Element;
            public ulong Label;
            public OrderingNode(T element, ulong label)
            {
                Element = element;
                Label = label;
            }
        }

        public OrderingCollection()
        {
            list = new LinkedList<OrderingNode>();
            BaseRecord = new LinkedListNode<OrderingNode>(new OrderingNode(default(T), 0UL));
            list.AddFirst(BaseRecord);
            dict = new DefaultValueDictionary<T, LinkedListNode<OrderingNode>>();
        }

        private LinkedListNode<OrderingNode> BaseRecord; //todo rename?

        private LinkedList<OrderingNode> list;

        private DefaultValueDictionary<T, LinkedListNode<OrderingNode>> dict; // todo, names, maybe

        public bool Contains(T element) => dict.ContainsKey(element);

        //todo, this is wrong until these 2 values are relative to base, mod M.
        public int CompareTo(T first, T second) => dict[first].Value.Label.CompareTo(dict[second].Value.Label);

        public bool Remove(T element)
        {
            var node = dict[element];
            if(node == null) return false;
            list.Remove(node);
            dict.Remove(element);
            return true;
        }

        public void InsertAtStart(T newElement) => InsertAfter(newElement, BaseRecord);

        public void InsertAtEnd(T newElement) => InsertAfter(newElement, list.Last);

        public void InsertBefore(T newElement, T beforeElement)
        {
            if(beforeElement == null)
            {
                InsertAfter(newElement, list.Last);
            }
            else
            {
                var node = dict[beforeElement];
                if(node == null) throw new InvalidOperationException("Can't insert before an element that isn't in the collection.");
                InsertAfter(newElement, node.Previous);
            }
        }

        public void InsertAfter(T newElement, T afterElement)
        {
            if(afterElement == null)
            {
                InsertAfter(newElement, BaseRecord);
            }
            else
            {
                var node = dict[afterElement];
                if(node == null) throw new InvalidOperationException("Can't insert after an element that isn't in the collection.");
                InsertAfter(newElement, node);
            }
        }

        private ulong LabelRelativeToBase(LinkedListNode<OrderingNode> record)
        {
            unchecked { return MOD_M(record.Value.Label - BaseRecord.Value.Label); } // todo, base record what kind of node?
        }

        private ulong? NextLabelRelativeToBase(LinkedListNode<OrderingNode> record)
        {
            var next = record.GetNextCircular();
            if(next == BaseRecord) return null; // return M
            else return LabelRelativeToBase(next);
        }

        /// <summary>
        /// Returns w_i for values of i greater than zero. If start and finish are the same, returns null (for M) rather than zero.
        /// </summary>
        private ulong? LabelDistance(LinkedListNode<OrderingNode> start, LinkedListNode<OrderingNode> finish)
        {
            if(start == finish) return null; // return M

            unchecked
            {
                return MOD_M(finish.Value.Label - start.Value.Label);
            }
        }

        // Here we want to find the integer closest to x * (MAX_INT + 1).
        // So we'll do x*MAX_INT + x.
        ulong GetPortionOfM(double x) => (ulong)(x * int.MaxValue + x);

        ulong GetFractionOfM(ulong n, ulong d)
        {
            double x = (double)n / (double)d;
            return (ulong)(x * (double)ulong.MaxValue + x);
        }

        ulong GetFraction(ulong x, ulong n, ulong d)
        {
            decimal decX = x, decN = n, decD = d;
            return (ulong)(decX * decN / decD); // n is expected to be fairly small here.
        }

        private void InsertAfter(T newElement, LinkedListNode<OrderingNode> afterNode)
        {
            if(newElement == null) throw new ArgumentNullException("newElement", "Collection does not support null entries.");
            // Find the starting point for our possible relabeling.

            //afterNode can't currently be null - as long as BaseRecord exists.

            ++Insertions;

            ulong j = 1UL;
            LinkedListNode<OrderingNode> current = afterNode.GetNextCircular();
            ///string debugInfo1 = $"Comparing labels relative to {afterNode.Value.Label}";
            //string debugInfo2 = "";

            while(LabelDistance(afterNode, current) != null && LabelDistance(afterNode, current).Value <= j * j)
            {
                //debugInfo2 += $"Label diff {LabelDistance(afterNode, current).Value} <= {j}*{j}   ({j * j})\r\n";
                ++j;
                current = current.GetNextCircular();
            }


            // NEXT:

            //status:  just changed this finalLabel to handle the M case.
            //next is to correct the handling of the finalLabel.Value * k / j part.
            //this can be done easily,I think, because ulong_max fits into decimal.
            //

            ulong? finalLabel = LabelDistance(afterNode, current);

            Relabelings += (int)(j - 1);

            // Now, relabel (j-1) records:
            current = afterNode.GetNextCircular();
            for(ulong k = 1UL; k < j; ++k)
            {
                ulong relabel;
                if(finalLabel == null)
                {
                    //relabel = GetPortionOfM((double)k)
                    unchecked
                    {
                        relabel = MOD_M(GetFractionOfM(k, j) + afterNode.Value.Label);
                    }
                }
                else
                {
                    unchecked
                    {
                        //relabel = MOD_M(finalLabel.Value * k / j + afterNode.Value.Label);
                        relabel = MOD_M(GetFraction(finalLabel.Value, k, j) + afterNode.Value.Label);
                    }
                }
                //string debugInfo3 = $"Relabeling {current.Value.Label} to {relabel}";
                current.Value.Label = relabel;
                current = current.GetNextCircular();
            }

            // Insert the new element:
            const ulong halfM = 1UL << 63; // 2^63, half of 2^64
                                           //const ulong halfM = M / 2; //todo, replace after testing

            ulong newLabelRelativeToBase;
            if(NextLabelRelativeToBase(afterNode) == null)
            {
                // Averaging with M, so use halfM:
                newLabelRelativeToBase = (LabelRelativeToBase(afterNode) / 2UL) + halfM;
            }
            else
            {
                // Carefully avoid off-by-one errors here...
                newLabelRelativeToBase = (NextLabelRelativeToBase(afterNode).Value - LabelRelativeToBase(afterNode)) / 2UL
                    + LabelRelativeToBase(afterNode);
            }
            ulong newLabel;
            unchecked
            {
                newLabel = MOD_M(newLabelRelativeToBase + BaseRecord.Value.Label);
            }
            var newNode = new LinkedListNode<OrderingNode>(new OrderingNode(newElement, newLabel));
            list.AddAfter(afterNode, newNode);
            dict[newElement] = newNode;



            // If afterNode is null, we're inserting at the start of the list, and startLabel will be 0:
            //ulong startLabel = afterNode?.Value.Label ?? 0L;

        }

    }


    /// <summary>
    /// A hashset with an indexer for convenience.
    /// </summary>
    public class EasyHashSet<T> : HashSet<T>
    {
        /// <summary>
        /// GET: Returns true if the given element is present.
        /// SET: If 'true', add the given element. If 'false', remove the given element.
        /// </summary>
        public bool this[T t]
        {
            get { return Contains(t); }
            set
            {
                if(value) Add(t);
                else Remove(t);
            }
        }
        public EasyHashSet() { }
        public EasyHashSet(IEqualityComparer<T> comparer) : base(comparer) { }
        public EasyHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer = null) : base(collection, comparer) { }
    }
    /// <summary>
    /// A dictionary that returns a default value if the given key isn't present.
    /// </summary>
    public class DefaultValueDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        new public TValue this[TKey key]
        {
            get
            {
                TValue v;
                if(TryGetValue(key, out v) || getDefaultValue == null)
                { // TryGetValue sets 'v' to default(TValue) if not found.
                    return v;
                }
                else return getDefaultValue();
            }
            set
            {
                base[key] = value;
            }
        }
        private Func<TValue> getDefaultValue = null;
        /// <summary>
        /// If defined, the result of this method will be used instead of default(TValue).
        /// </summary>
        public Func<TValue> GetDefaultValue
        {
            get
            {
                if(getDefaultValue == null) return () => default(TValue);
                else return getDefaultValue;
            }
            set { getDefaultValue = value; }
        }
        public DefaultValueDictionary() { }
        public DefaultValueDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
        public DefaultValueDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer = null)
            : base(dictionary, comparer) { }
    }
    
    //todo, xml note about how this is a stable sort (right?)
    public class PriorityQueue<T, TSortKey> : IEnumerable<T>, IReadOnlyCollection<T>
    {
        public PriorityQueue(Func<T, TSortKey> keySelector, bool descending = false)
            : this(keySelector, Comparer<TSortKey>.Default.Compare, descending) { }
        public PriorityQueue(Func<T, TSortKey> keySelector, Func<TSortKey, TSortKey, int> compare, bool descending = false)
        {
            if(compare == null) compare = Comparer<TSortKey>.Default.Compare;
            set = new SortedSet<pqElement>(new PriorityQueueComparer(descending, keySelector, compare));
            DescendingOrder = descending;
        }
        public PriorityQueue(Func<T, TSortKey> keySelector, IEnumerable<T> collection, Func<TSortKey, TSortKey, int> compare = null, bool descending = false)
            : this(keySelector, compare, descending)
        {
            foreach(T item in collection) Enqueue(item);
        }
        public PriorityQueue(Func<T, TSortKey> keySelector, IComparer<TSortKey> comparer, bool descending = false)
            : this(keySelector, comparer.Compare, descending) { }
        public PriorityQueue(Func<T, TSortKey> keySelector, IEnumerable<T> collection, IComparer<TSortKey> comparer, bool descending = false)
            : this(keySelector, collection, comparer.Compare, descending) { }
        private SortedSet<pqElement> set;
        private struct pqElement
        {
            public readonly int idx;
            public readonly T item;
            public pqElement(int idx, T item)
            {
                this.idx = idx;
                this.item = item;
            }
        }
        private class PriorityQueueComparer : Comparer<pqElement>
        {
            private readonly bool descending;
            private readonly Func<T, TSortKey> getSortKey;
            private readonly Func<TSortKey, TSortKey, int> compare;
            public PriorityQueueComparer(bool descending, Func<T, TSortKey> keySelector, Func<TSortKey, TSortKey, int> compare)
            {
                this.descending = descending;
                getSortKey = keySelector;
                this.compare = compare;
            }
            public override int Compare(pqElement x, pqElement y)
            {
                int primarySort;
                if(descending) primarySort = compare(getSortKey(y.item), getSortKey(x.item)); // Flip x & y for descending order.
                else primarySort = compare(getSortKey(x.item), getSortKey(y.item));
                if(primarySort != 0) return primarySort;
                else return Comparer<int>.Default.Compare(x.idx, y.idx); // Use insertion order as the final tiebreaker.
            }
        }
        public readonly bool DescendingOrder;
        private static int nextIdx = 0;
        public void Enqueue(T item) => set.Add(new pqElement(nextIdx++, item));
        public T Dequeue()
        {
            if(set.Count == 0) throw new InvalidOperationException("The PriorityQueue is empty.");
            pqElement next = set.Min;
            set.Remove(next);
            return next.item;
        }
        public int Count => set.Count;
        public void Clear() => set.Clear();
        public bool Contains(T item) => set.Any(x => x.item.Equals(item));
        public T Peek()
        {
            if(set.Count == 0) throw new InvalidOperationException("The PriorityQueue is empty.");
            return set.Min.item;
        }
        public bool ChangePriority(T item, Action<T> change) => ChangePriority(item, () => change(item));
        //todo: xml, be sure to note that this preserves insertion order
        public bool ChangePriority(T item, Action change)
        {
            pqElement? found = null;
            foreach(var element in set)
            { // Linear search is the best we can do, given our constraints.
                if(element.item.Equals(item))
                {
                    found = element;
                    break;
                }
            }
            if(found == null) return false;
            set.Remove(found.Value); // Remove the element before changing - otherwise, the set can't find it.
            change();
            set.Add(found.Value); // Add it again with its new priority.
            return true;
        }
        public IEnumerator<T> GetEnumerator()
        {
            foreach(var x in set) yield return x.item;
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}