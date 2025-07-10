using nio2so.Data.Common.Serialization.Voltron;
using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    /// <summary>
    /// Sent to a remote party to update which room it is currently in
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.UPDATE_ROOM_PDU)]
    public class TSOUpdateRoomPDU : TSOVoltronPacket
    {               
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.UPDATE_ROOM_PDU;

        public TSOUpdateRoomPDU() : base() { }

        /// <summary>
        /// Creates a new <see cref="TSOUpdatePlayerPDU"/> with the given information
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="lotPhoneNumber"></param>
        /// <param name="roomName"></param>
        /// <param name="lotOwnerInformation"></param>
        /// <param name="roomHostInformation"></param>
        public TSOUpdateRoomPDU(uint roomID, string lotPhoneNumber, string roomName, 
            TSORoomAvatarInformationStringPackStruct lotOwnerInformation,
            TSORoomAvatarInformationStringPackStruct roomHostInformation)
        {
            RoomID = roomID;
            NewRoomInfo = new(new TSORoomLotInformationStringPackStruct(lotPhoneNumber, roomName),
                lotOwnerInformation, 1, roomHostInformation);

            MakeBodyFromProperties();
        }

        /// <summary>
        /// The RoomID of the Room
        /// </summary>
        public uint RoomID { get; set; }
        public byte DataStartByte { get; set; } = 0x01;
        /// <summary>
        /// The new <see cref="TSORoomInfo"/> the client should update itself to a be a member of
        /// </summary>
        public TSORoomInfo NewRoomInfo { get; set; }
    }
}
