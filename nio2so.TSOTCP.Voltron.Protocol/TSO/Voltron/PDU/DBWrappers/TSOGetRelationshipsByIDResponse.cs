using nio2so.Formats.Util.Endian;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Needs revisiting, this format is not correct.
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetRelationshipsByID_Response)]
    public class TSOGetRelationshipsByIDResponse : TSODBRequestWrapper
    {
        /// <summary>
        /// The Avatar that is being asked about relationships for
        /// </summary>
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        [TSOVoltronDBWrapperField] public uint NumEntries { get; set; }
        [TSOVoltronDBWrapperField][TSOVoltronBodyArray] public byte[] RelationshipAvatarIDs { get; set; }

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
            NumEntries = (uint)FriendAvatarIDs.Length;
            RelationshipAvatarIDs = new byte[sizeof(uint) * NumEntries];

            int index = -1;
            foreach (uint aID in FriendAvatarIDs)
            {
                index++;
                byte[] aIdBytes = EndianBitConverter.Big.GetBytes(aID);
                RelationshipAvatarIDs[index * sizeof(uint)] = aIdBytes[0];
                RelationshipAvatarIDs[index * sizeof(uint) + 1] = aIdBytes[1];
                RelationshipAvatarIDs[index * sizeof(uint) + 2] = aIdBytes[2];
                RelationshipAvatarIDs[index * sizeof(uint) + 3] = aIdBytes[3];
            }
            MakeBodyFromProperties();
        }
    }
}
