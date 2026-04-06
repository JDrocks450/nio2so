using nio2so.Data.Common.Serialization.Voltron;
using nio2so.Voltron.PreAlpha.Protocol.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{

    [TSOVoltronDBRequestWrapperPDU((uint)TSO_PreAlpha_DBActionCLSIDs.GetTopResultSetByID_Response)]
    internal class TSOGetTopResultSetByIDResponse : TSODBRequestWrapper
    {
        /// <summary>
        /// Represents a search result in the <see cref="TSOSearchResponseStruct"/>
        /// </summary>
        /// <param name="ItemID"></param>
        /// <param name="ItemName"></param>
        public class TSOTopListResultStruct
        {
            public TSOTopListResultStruct(uint itemID, string itemName)
            {
                ItemName = itemName;
                ItemID = itemID;
            }

            [TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)]
            public string ItemName { get; set; } = "";

            public uint ItemID { get; set; }
            

            [TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)]
            public string ItemSubtitle { get; set; } = "1000";
        }

        public TSOGetTopResultSetByIDResponse() : base(
            TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
            TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
            TSO_PreAlpha_DBActionCLSIDs.GetTopResultSetByID_Response)
        {
            MakeBodyFromProperties();
        }
        public TSOGetTopResultSetByIDResponse(uint ListID, uint ListType, params TSOTopListResultStruct[] Results) : this()
        {
            this.ListID = ListID;
            //this.ListType = ListType;
            this.Results = Results;
            MakeBodyFromProperties();
        }

        /// <summary>
        /// The category to search for a resource in
        /// </summary>
        [TSOVoltronDBWrapperField]
        public uint ListID { get; set; } = 0x0001;

        /// <summary>
        /// <inheritdoc cref="TSOVoltronArrayLength"/>
        /// </summary>
        [TSOVoltronDBWrapperField] [TSOVoltronArrayLength(nameof(Results))] public uint ResultsCount { get; set; }
        
        /// <summary>
        /// List of returned <see cref="TSOSearchResultStruct"/> items
        /// </summary>
        [TSOVoltronDBWrapperField] public TSOTopListResultStruct[] Results { get; set; } = new TSOTopListResultStruct[0];
    }
}
