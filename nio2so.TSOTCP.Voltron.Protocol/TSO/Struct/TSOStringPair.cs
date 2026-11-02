using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.Core.TSO.Struct
{
    /// <summary>
    /// A handy type that is simply 2 <see cref="String"/>s in a pair, helps when building serializable types. 
    /// <para/>The format of each <see cref="String"/> is dictated by <see cref="TSOVoltronString"/> attribute which is:
    /// <para/> <inheritdoc cref="TSOVoltronString"/>
    /// </summary>
    /// <param name="String1"> The first string in this <see cref="TSOStringPair"/> </param>
    /// <param name="String2"> The second string in this <see cref="TSOStringPair"/> </param>
    public record TSOStringPair([property: TSOVoltronString] string String1, [property: TSOVoltronString] string String2)
    {
        public TSOStringPair() : this(default, default)
        {
        }
    }
}
