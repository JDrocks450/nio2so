using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace nio2so.DataService.API.Databases.Libraries
{
    internal sealed class JSONDictionaryLibrary<T1, T2> : JSONObjectLibrary<Dictionary<T1, T2>>, IDictionary<T1,T2> where T1 : notnull
    {
        public Dictionary<T1, T2> Dictionary => DataFile;

        public JSONDictionaryLibrary(string FilePath, Action EnsureDefaultValuesFunc, bool DelayedLoad = false) :
            base(FilePath, EnsureDefaultValuesFunc, DelayedLoad)
        {

        }

        #region DICTIONARY

        public ICollection<T1> Keys => ((IDictionary<T1, T2>)Dictionary).Keys;

        public ICollection<T2> Values => ((IDictionary<T1, T2>)Dictionary).Values;

        public int Count => ((ICollection<KeyValuePair<T1, T2>>)Dictionary).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<T1, T2>>)Dictionary).IsReadOnly;

        public T2 this[T1 key] { get => ((IDictionary<T1, T2>)Dictionary)[key]; set => ((IDictionary<T1, T2>)Dictionary)[key] = value; }        

        public void Add(T1 key, T2 value)
        {
            ((IDictionary<T1, T2>)Dictionary).Add(key, value);
        }

        public bool ContainsKey(T1 key)
        {
            return ((IDictionary<T1, T2>)Dictionary).ContainsKey(key);
        }

        public bool Remove(T1 key)
        {
            return ((IDictionary<T1, T2>)Dictionary).Remove(key);
        }

        public bool TryGetValue(T1 key, [MaybeNullWhen(false)] out T2 value)
        {
            return ((IDictionary<T1, T2>)Dictionary).TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<T1, T2> item)
        {
            ((ICollection<KeyValuePair<T1, T2>>)Dictionary).Add(item);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<T1, T2>>)Dictionary).Clear();
        }

        public bool Contains(KeyValuePair<T1, T2> item)
        {
            return ((ICollection<KeyValuePair<T1, T2>>)Dictionary).Contains(item);
        }

        public void CopyTo(KeyValuePair<T1, T2>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<T1, T2>>)Dictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<T1, T2> item)
        {
            return ((ICollection<KeyValuePair<T1, T2>>)Dictionary).Remove(item);
        }

        public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<T1, T2>>)Dictionary).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Dictionary).GetEnumerator();
        }
        #endregion
    }
}
