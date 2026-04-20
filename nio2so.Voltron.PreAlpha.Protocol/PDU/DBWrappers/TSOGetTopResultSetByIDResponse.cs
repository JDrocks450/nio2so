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
            public TSOTopListResultStruct(uint Rank, uint TargetID, string Name, string Caption, int Argument = 0, int Argument2 = 0, int Index = -1)
            {
                if (Index < 0)
                    Index = (int)Rank;
                this.Index = (uint)Index;
                this.Rank = Rank;
                this.TargetID = TargetID;
                this.Caption = Caption; 
                this.Title = Name;
                this.Argument2 = Argument2;
                this.Argument = Argument;
            }
            /// STRUCTURE IS WIP BUT WORKING
            public uint Index { get; set; } = 1; // 4
            public uint TargetID { get; set; } // 4
            public uint Rank { get; set; } = 1; // 4            
            public Int32 Argument { get; set; } = 0x01020304; // 4
            public Int32 Argument2 { get; set; } = 0x01; // 1

            [TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)]
            public string Caption { get; set; } = "1,100";

            [TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)]
            public string Title { get; set; } = "Bisquick";

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
