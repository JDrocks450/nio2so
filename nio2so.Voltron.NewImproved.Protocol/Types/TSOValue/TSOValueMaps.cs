using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.NewImproved.Protocol.Types.TSOValue
{
    internal abstract class TSOValue_MapBase<T> : TSOValueBase<IDictionary<uint, T>>, ITSOValueMap
    {
        protected TSOValue_MapBase() : base() { }
        protected TSOValue_MapBase(params KeyValuePair<uint, T>[] Values) : base(new Dictionary<uint,T>(Values)) { }
    }

    internal class TSOValue_Uint32Map : TSOValue_MapBase<uint>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueMap_Uint32;

        public TSOValue_Uint32Map()
        {
        }

        public TSOValue_Uint32Map(params KeyValuePair<uint, uint>[] Values) : base(Values)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }
    internal class TSOValue_BooleanMap : TSOValue_MapBase<bool>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValueMap_Bool;

        public TSOValue_BooleanMap()
        {
        }

        public TSOValue_BooleanMap(params KeyValuePair<uint, bool>[] Values) : base(Values)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }
}
