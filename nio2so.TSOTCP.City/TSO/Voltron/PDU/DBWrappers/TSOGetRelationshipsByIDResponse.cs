using nio2so.Formats.Util.Endian;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetRelationshipsByID_Response)]
    internal class TSOGetRelationshipsByIDResponse : TSODBRequestWrapper
    {
        /// <summary>
        /// The Avatar that is being asked about relationships for
        /// </summary>
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        [TSOVoltronDBWrapperField] public uint NumEntries { get; set; }
        [TSOVoltronDBWrapperField] [TSOVoltronBodyArray] public byte[] RelationshipAvatarIDs { get; set; }

        public TSOGetRelationshipsByIDResponse(uint AvatarID, params uint[] FriendAvatarIDs) : base(
                TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                TSO_PreAlpha_DBActionCLSIDs.GetRelationshipsByID_Response
            )
        {
            this.AvatarID = AvatarID;
            this.NumEntries = (uint)FriendAvatarIDs.Length;
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
