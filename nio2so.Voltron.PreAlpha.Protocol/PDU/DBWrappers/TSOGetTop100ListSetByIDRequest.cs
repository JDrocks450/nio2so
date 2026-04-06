using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU((uint)TSO_PreAlpha_DBActionCLSIDs.GetTopResultSetByID_Request)]
    internal class TSOGetTop100ListSetByIDRequest : TSODBRequestWrapper
    {
        /// <summary>
        /// The ID of the List Requested for its contents
        /// </summary>
        [TSOVoltronDBWrapperField]
        public uint ListID { get; set; }

        // ***
        // Unknown Values Below
        // ***

        [TSOVoltronDBWrapperField]
        public uint Param1 { get; set; }

        /// <summary>
        /// 00 00 00 FF
        /// </summary>
        [TSOVoltronDBWrapperField]
        public uint Param2 { get; set; }

        [TSOVoltronDBWrapperField]
        public uint Param3 { get; set; }

        [TSOVoltronDBWrapperField]
        public uint Param4 { get; set; }

        [TSOVoltronDBWrapperField]
        public uint Param5 { get; set; }

        [TSOVoltronDBWrapperField]
        public uint Param6 { get; set; }

        [TSOVoltronDBWrapperField]
        public uint Param7 { get; set; }

        [TSOVoltronDBWrapperField]
        public uint Param8 { get; set; }
        /// <summary>
        /// 0x00006DDC - unknown purpose, but this value is consistent across multiple captures of this request
        /// </summary>
        [TSOVoltronDBWrapperField]
        public uint Param9 { get; set; }
        /// <summary>
        /// 0x00006DEE in the captured request, but unknown what this does
        /// </summary>
        [TSOVoltronDBWrapperField]
        public uint Param10 { get; set; }
    }
}
