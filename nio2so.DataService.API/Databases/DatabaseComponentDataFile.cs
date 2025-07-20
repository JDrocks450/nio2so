using Microsoft.OpenApi.Any;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace nio2so.DataService.API.Databases
{
    internal interface IDatabaseComponentDataFile
    {
        bool HasDefaultValues();
    }

    internal class DataComponentDictionaryDataFile<T1, T2> : Dictionary<T1, T2>, IDatabaseComponentDataFile
    {
        /// <summary>
        /// Checks if this <see cref="IDictionary{T1, T2}"/> has any content
        /// </summary>
        /// <returns></returns>
        public bool HasDefaultValues() => this.Any();
    }
}
