using nio2so.Formats.Util.Endian;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Structure works
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetRelationshipsByID_Response)]
    public class TSOGetRelationshipsByIDResponse : TSODBRequestWrapper
    {
        public record TSORelationshipStructure
        {            
            public uint Param1 { get; set; } = 1; // unknown -- seems to indicate friendship or enemy?
            public uint FirstLevel { get; set; } = 1337; // avatarID
            public uint SecondLevel { get; set; } = 161; // avatarID
            public uint ThirdLevel { get; set; } = 0; // avatarID
            public uint Param2 { get; set; } = 1; // unknown
        }

        /// <summary>
        /// The Avatar that is being asked about relationships for
        /// </summary>
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        [TSOVoltronDBWrapperField] public uint StatusCode { get; set; }
        [TSOVoltronDBWrapperField] public TSORelationshipStructure Test { get; set; } = new();

        /// <summary>
        /// Default parameterless constructor. Please use overload for programmatically creating PDUs.
        /// </summary>
        public TSOGetRelationshipsByIDResponse() : base() { }

        /// <summary>
        /// Creates a new <see cref="TSOGetRelationshipsByIDResponse"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <param name="FriendAvatarIDs"></param>
        public TSOGetRelationshipsByIDResponse(uint AvatarID, params uint[] FriendAvatarIDs) : base(
                TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                TSO_PreAlpha_DBActionCLSIDs.GetRelationshipsByID_Response
            )
        {
            this.AvatarID = AvatarID;
            MakeBodyFromProperties();
        }
    }
}
