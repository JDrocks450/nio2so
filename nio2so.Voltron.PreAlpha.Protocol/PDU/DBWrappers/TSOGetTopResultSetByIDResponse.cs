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

            /// <summary>
            /// The index of the item in the list
            /// </summary>
            public uint Index { get; set; } = 1; // 4
            /// <summary>
            /// The ID in the database of the item being displayed
            /// </summary>
            public uint TargetID { get; set; } // 4
            /// <summary>
            /// The number shown as the "Rank" of this item
            /// </summary>
            public uint Rank { get; set; } = 1; // 4
            /// <summary>
            /// Unknown
            /// </summary>
            public Int32 Argument { get; set; } = 0x01020304; // 4
            /// <summary>
            /// Unknown
            /// </summary>
            public Int32 Argument2 { get; set; } = 0x01; // 1

            /// <summary>
            /// The "score" shown, e.g. Simoleans, score, friends, etc.
            /// </summary>
            [TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)]
            public string Caption { get; set; } = "1,100";

            /// <summary>
            /// The name of the item being displayed
            /// </summary>
            [TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)]
            public string Title { get; set; } = "Bisquick";

        }

        /// <summary>
        /// Responds with a list of items contained in a Top 100 List
        /// </summary>
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
