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

        public double Weight => this.Sum(kvp => kvp.Key.Weight * kvp.Value);

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
