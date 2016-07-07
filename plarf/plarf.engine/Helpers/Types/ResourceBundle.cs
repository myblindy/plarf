using Plarf.Engine.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Plarf.Engine.Helpers.Types
{
    public class ResourceBundle : IDictionary<ResourceClass, int>
    {
        private Dictionary<ResourceClass, int> InternalDict = new Dictionary<ResourceClass, int>();

        public void Add(ResourceClass rc, int amount)
        {
            int existing;
            if (InternalDict.TryGetValue(rc, out existing))
                InternalDict[rc] = existing + amount;
            else
                InternalDict.Add(rc, amount);
        }

        public void Add(ResourceBundle res)
        {
            foreach (var kvp in res)
                Add(kvp);
        }

        public void Remove(ResourceBundle res)
        {
            foreach (var kvp in res.Intersect(this))
                SafeSet(kvp.Key, kvp.Value);
        }

        public void RemoveAll(Func<KeyValuePair<ResourceClass, int>, bool> predicate)
        {
            foreach (var kvp in InternalDict.Where(w => predicate(w)).ToArray())
                InternalDict.Remove(kvp.Key);
        }

        public bool ContainsFully(ResourceBundle resources)
        {
            foreach (var kvp in resources)
                if (!ContainsKey(kvp.Key) || kvp.Value > this[kvp.Key])
                    return false;
            return true;
        }

        public bool ContainsAny(ResourceBundle resources)
        {
            foreach (var kvp in resources)
                if (ContainsKey(kvp.Key) && kvp.Value > 0 && this[kvp.Key] > 0)
                    return true;
            return false;
        }

        public static ResourceBundle operator +(ResourceBundle a, ResourceBundle b)
        {
            var res = new ResourceBundle();
            foreach (var kvp in a)
                res.Add(kvp);
            foreach (var kvp in b)
                if (res.ContainsKey(kvp.Key))
                    res[kvp.Key] += kvp.Value;
                else
                    res.Add(kvp);
            return res;
        }

        public ResourceBundle Intersect(ResourceBundle rb)
        {
            var res = new ResourceBundle();
            foreach (var key in Keys.Intersect(rb.Keys))
                res.Add(key, Math.Min(SafeGet(key) ?? 0, rb.SafeGet(key) ?? 0));

            return res;
        }

        public int? SafeGet(ResourceClass key)
        {
            int val;
            if (TryGetValue(key, out val))
                return val;
            return null;
        }

        public void SafeSet(ResourceClass key, int val)
        {
            if (ContainsKey(key))
                this[key] = val;
            else if (val != 0)
                Add(key, val);
        }

        public override string ToString() => this.Any() ? string.Join(", ", this.Select(kvp => kvp.Value + " " + kvp.Key.Name)) : "Nothing";

        #region IDictionary implementation
        public bool ContainsKey(ResourceClass key) => InternalDict.ContainsKey(key);

        public bool Remove(ResourceClass key) => InternalDict.Remove(key);

        public bool TryGetValue(ResourceClass key, out int value) => InternalDict.TryGetValue(key, out value);

        public void Add(KeyValuePair<ResourceClass, int> item) => Add(item.Key, item.Value);

        public void Clear() => InternalDict.Clear();

        public bool Contains(KeyValuePair<ResourceClass, int> item) => InternalDict.Contains(item);

        public void CopyTo(KeyValuePair<ResourceClass, int>[] array, int arrayIndex) { throw new NotImplementedException(); }

        public bool Remove(KeyValuePair<ResourceClass, int> item) => InternalDict.Remove(item.Key);

        public IEnumerator<KeyValuePair<ResourceClass, int>> GetEnumerator() => InternalDict.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => InternalDict.GetEnumerator();

        public double Weight => this.Sum(kvp => kvp.Key.UnitWeight * kvp.Value);

        public ICollection<ResourceClass> Keys => InternalDict.Keys;

        public ICollection<int> Values => InternalDict.Values;

        public int Count => InternalDict.Count;

        public bool IsReadOnly => false;

        public int this[ResourceClass key]
        {
            get { return InternalDict[key]; }
            set { InternalDict[key] = value; }
        }
        #endregion
    }
}
