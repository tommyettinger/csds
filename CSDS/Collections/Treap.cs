/*
The MIT License(MIT)

Copyright(c) 2013 Clayton Stangeland

Modifications have been made to the original work and use the same license.
Modifications Copyright(c) 2018 Tommy Ettinger

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE. 
*/

using CSDS.Utilities;
using System.Collections.Generic;

namespace CSDS.Collections
{
    /// <summary>
    /// A node in a Treap, holding a random ulong priority and a T item, as well as references to the left and right children, if any.
    /// Most of the code here is from https://github.com/stangelandcl/Cls.Treap , with small changes to organization and the random aspect.
    /// </summary>
    /// <typeparam name="T">The type of the item this node holds</typeparam>
    public class TreapNode<T>
    {

        public TreapNode(T v, ulong p)
        {
            Value = v;
            Priority = p;
        }

        public T Value { get; set; }
        public ulong Priority { get; set; }
        public TreapNode<T> Left;
        public TreapNode<T> Right;
    }
    /// <summary>
    /// A probabilistic tree-heap data structure noted for its simplicity.
    /// It must hold items that can be compared, either using a provided IComparer or Comparer.Default if unspecified.
    /// Most of the code here is from https://github.com/stangelandcl/Cls.Treap , with small changes to organization and the random aspect.
    /// </summary>
    /// <typeparam name="T">A comparable type, which doesn't need to implement IComparable if an IComparer is provided in the constructor</typeparam>
    public class Treap<T> : ICollection<T>
    {
        public Treap() : this(Comparer<T>.Default, 0L) { }
        public Treap(IComparer<T> comparer) : this(comparer, 0L) { }
        public Treap(IComparer<T> comparer, ulong state)
        {
            this.comparer = comparer;
            random = new PRNG5(state);
        }

        public IComparer<T> comparer;
        TreapNode<T> root;
        PRNG5 random;

        #region ICollection implementation	

        public void Add(T item)
        {
            Add(ref root, item);
        }

        void Add(ref TreapNode<T> node, T item)
        {
            if (node == null)
            {
                node = new TreapNode<T>(item, random.NextULong());
                Count++;
                return;
            }

            var c = comparer.Compare(item, node.Value);
            if (c < 0)
            {
                Add(ref node.Left, item);
                if (node.Left.Priority > node.Priority)
                {
                    var x = node.Left;
                    node.Left = x.Right;
                    x.Right = node;
                    node = x;
                }
            }
            else if (c > 0)
            {
                Add(ref node.Right, item);
                if (node.Priority < node.Right.Priority)
                {
                    var x = node.Right;
                    node.Right = x.Left;
                    x.Left = node;
                    node = x;
                }
            }
            else node.Value = item;
        }

        public void Clear()
        {
            root = null;
            Count = 0;
        }

        public bool TryGetValue(T key, out T item)
        {
            return TryGetValue(root, key, out item);
        }

        bool TryGetValue(TreapNode<T> node, T key, out T item)
        {
            if (node == null)
            {
                item = default;
                return false;
            }

            int c = comparer.Compare(key, node.Value);
            if (c < 0) return TryGetValue(node.Left, key, out item);
            if (c > 0) return TryGetValue(node.Right, key, out item);
            item = node.Value;
            return true;
        }

        public bool Contains(T item)
        {
            return Contains(root, item);
        }

        bool Contains(TreapNode<T> node, T item)
        {
            if (node == null)
                return false;
            var c = comparer.Compare(item, node.Value);
            if (c < 0) return Contains(node.Left, item);
            if (c > 0) return Contains(node.Right, item);
            return true;
        }

        public long Path(T item)
        {
            return Path(root, item, 0L);
        }

        long Path(TreapNode<T> node, T item, long found)
        {
            if (node == null)
                return -1L;
            var c = comparer.Compare(item, node.Value);
            if (c < 0) return Path(node.Left, item, found * 3);
            if (c > 0) return Path(node.Right, item, found * 3 + 2);
            return found * 3 + 1;
        }

        /// <summary>
        /// There is no reason to use this currently; use <code>treap.Contains(item1) && (!treap.Contains(item2) || treap.comparator.Compare(item1, item2) &lt; 0)</code> instead.
        /// </summary>
        /// <param name="item1">A T item to compare</param>
        /// <param name="item2">Another T item to compare</param>
        /// <returns>true if item1 is present in this Treap and item2 is either after item1 or not present in this Treap; false if item1 is equal to item2 or item1 is after item2</returns>
        public bool Before(T item1, T item2)
        {
            long p1 = Path(item1), p2 = Path(item2);
            if (p1 == p2) return false;
            return (p1 < p2 || p2 == -1L);
        }
        /// <summary>
        /// There is no reason to use this currently; use <code>treap.Contains(item1) && (!treap.Contains(item2) || treap.comparator.Compare(item1, item2) &gt; 0)</code> instead.
        /// </summary>
        /// <param name="item1">A T item to compare</param>
        /// <param name="item2">Another T item to compare</param>
        /// <returns>true if item1 is present in this Treap and item2 is either before item1 or not present in this Treap; false if item1 is equal to item2 or item1 is before item2</returns>
        public bool After(T item1, T item2)
        {
            long p1 = Path(item1), p2 = Path(item2);
            if (p1 == p2) return false;
            return (p1 > p2);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var item in this)
                array[arrayIndex++] = item;
        }

        static void Reorder(ref TreapNode<T> node, TreapNode<T> left, TreapNode<T> right)
        {
            if (left == null)
            {
                node = right;
                return;
            }
            if (right == null)
            {
                node = left;
                return;
            }
            if (left.Priority > right.Priority)
            {
                node = left;
                Reorder(ref node.Right, node.Right, right);
            }
            else
            {
                node = right;
                Reorder(ref node.Left, left, node.Left);
            }
        }

        public bool Remove(T item)
        {
            return Remove(ref root, item);
        }

        bool Remove(ref TreapNode<T> node, T item)
        {
            if (node == null)
                return false;
            var c = comparer.Compare(item, node.Value);
            if (c < 0) return Remove(ref node.Left, item);
            else if (c > 0) return Remove(ref node.Right, item);

            if (node.Left != null)
            {
                if (node.Right != null) Reorder(ref node, node.Left, node.Right);
                else node = node.Left;
            }
            else node = node.Right;

            Count--;
            return true;
        }

        public int Count { get; private set; }
        public bool IsReadOnly { get { return false; } }

        #endregion

        #region IEnumerable implementation
        public IEnumerator<T> GetEnumerator()
        {
            return Next(root).GetEnumerator();
        }

        IEnumerable<T> Next(TreapNode<T> node)
        {
            if (node == null) yield break;
            foreach (var t in Next(node.Left))
                yield return t;
            yield return node.Value;
            foreach (var t in Next(node.Right))
                yield return t;
        }

        #endregion

        #region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
