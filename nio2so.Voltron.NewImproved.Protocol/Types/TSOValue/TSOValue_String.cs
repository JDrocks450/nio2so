using nio2so.Data.Common.Serialization.Voltron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.NewImproved.Protocol.Types.TSOValue
{
    internal class TSOValue_String : TSOValueBase
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_String;

        public TSOValue_String()
        {
            TypeClassID = MYID;
        }

        public TSOValue_String(string value) : this()
        {            
            Value = value;
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
        [TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)]
        public string Value { get; set; } = "";
    }
}
