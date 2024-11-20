using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace nio2so.TSOHTTPS.Protocol.Data
{
    internal abstract class TSOHTTPRegulator<Key,Value> : Dictionary<Key, Value> where Key : IEquatable<Key>
    {
        protected abstract string FileName { get; }

        public static T? GetRegulator<T>(string FileName) where T : TSOHTTPRegulator<Key,Value> => JsonSerializer.Deserialize<T>(File.ReadAllText(FileName));
        protected void SaveToDisk()
        {
            string text = JsonSerializer.Serialize(this);
            File.WriteAllText(FileName, text);  
        }
    }
}
