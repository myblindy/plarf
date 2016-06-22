using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.Helpers.Types
{
    class PriorityList<P, V> : IList<V>
    {
        private SortedDictionary<P, IList<V>> List = new SortedDictionary<P, IList<V>>();

        private void DecomposeIndex(int idx, out P priority, out int pidx)
        {
            pidx = 0;

            foreach (var kvp in List)
                if (idx < kvp.Value.Count)
                {
                    priority = kvp.Key;
                    pidx += idx;
                    return;
                }
                else
                    pidx += kvp.Value.Count;

            throw new InvalidOperationException();
        }

        public V this[int index]
        {
            get
            {
                P priority;
                int pidx;
                DecomposeIndex(index, out priority, out pidx);
                return List[priority][pidx];
            }
            set
            {
                P priority;
                int pidx;
                DecomposeIndex(index, out priority, out pidx);
                List[priority][pidx] = value;
            }
        }

        public int Count => List.Sum(kvp => kvp.Value.Count);

        public bool IsReadOnly => false;

        public void Add(V item) => Add(default(P), item);

        public void Add(P p, V item)
        {
            IList<V> lst;
            if (!List.TryGetValue(p, out lst))
                List.Add(p, lst = new List<V>());

            lst.Add(item);
        }

        public void Clear() => List.Clear();

        public bool Contains(V item) => List.Any(kvp => kvp.Value.Contains(item));

        public void CopyTo(V[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<V> GetEnumerator()
        {
            foreach (var lst in List.Values)
                foreach (var item in lst)
                    yield return item;
        }

        public int IndexOf(V item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, V item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(V item)
        {
            bool removed = false;
            foreach (var lst in List.Values)
                removed |= lst.Remove(item);
            return removed;
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void RemoveAll(Func<V, bool> predicate)
        {
            foreach (var lst in List.Values)
                foreach (var item in lst.Where(predicate).ToArray())
                    lst.Remove(item);
        }

        public void RemoveAll(Func<P, V, bool> predicate)
        {
            foreach (var kvp in List)
                foreach (var item in kvp.Value.Where(i => predicate(kvp.Key, i)).ToArray())
                    kvp.Value.Remove(item);
        }

        public IEnumerable<V> OrderBy<TKey>(Func<V, TKey> selector) =>
            List.SelectMany(kvp => kvp.Value.OrderBy(elem => selector(elem)));
    }
}
