using nio2so.DataService.API.Controllers;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types.Search;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace nio2so.DataService.API.Databases.Libraries
{
    internal sealed class JSONDictionaryLibrary<T1, T2> : JSONObjectLibrary<Dictionary<T1, T2>>, IDictionary<T1,T2>, ISearchable<T1> where T1 : notnull
    {
        public Dictionary<T1, T2> Dictionary => DataFile;

        public JSONDictionaryLibrary(string FilePath, Action EnsureDefaultValuesFunc, bool DelayedLoad = false) :
            base(FilePath, EnsureDefaultValuesFunc, DelayedLoad)
        {

        }

        public IDictionary<T1, string> SearchExact(string QueryString) => 
            searchBase(QueryString, 10, (string keyword) => keyword.Trim().Equals(QueryString.Trim(), StringComparison.OrdinalIgnoreCase));
        public IDictionary<T1, string> SearchBroad(string QueryString, int MaxResults) => 
            searchBase(QueryString, MaxResults, (string keyword) => keyword.Trim().Contains(QueryString.Trim(), StringComparison.OrdinalIgnoreCase));

        IDictionary<T1, string> searchBase(string QueryString, int MaxResults, Func<string, bool> MatchingFunction)
        {
            Dictionary<T1, string> results = new();
            foreach (var item in this)
            {
                if (results.Count >= MaxResults) break;
                if (item.Value is not ISearchableItem searchItem) continue;
                foreach (var keyword in searchItem.SearchableKeywords)
                {
                    if (MatchingFunction(keyword))
                    {
                        results.Add(item.Key, searchItem.SearchableKeywords.ElementAt(0));
                        break;
                    }
                }
            }
            return results;
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
