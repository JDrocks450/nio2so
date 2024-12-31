using nio2so.Data.Common.Serialization.Voltron;
using nio2so.TSOTCP.City.TSO.Voltron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    /// <summary>
    /// Sent to a remote party to update which room it is currently in
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.UPDATE_ROOM_PDU)]
    internal class TSOUpdateRoomPDU : TSOVoltronPacket
    {
        /// <summary>
        /// An object that has String1 and String2
        /// </summary>
        public interface IUpdateRoomStringPack
        {
            public string String1 { get; }
            public string String2 { get; }
        }

        /// <summary>
        /// Two strings one after another
        /// </summary>
        /// <param name="String1"></param>
        /// <param name="String2"></param>
        public record UpdateRoom_StringPack(
            [property: TSOVoltronString(TSOVoltronValueTypes.Pascal)] string String1,
            [property: TSOVoltronString(TSOVoltronValueTypes.Pascal)] string String2)
            : IUpdateRoomStringPack;

        /// <summary>
        /// A <see cref="IUpdateRoomStringPack"/> that contains the AvatarID and AvatarName
        /// </summary>
        public record UpdateRoom_AvatarInformationPack : IUpdateRoomStringPack
        {
            /// <summary>
            /// Follows format: "__[AvatarID]" where the first two character are seemingly ignored.
            /// <para/>Nio2so would follow this format: "A 1337" where 1337 is the avatar ID
            /// </summary>
            [TSOVoltronString(TSOVoltronValueTypes.Pascal)] public string AvatarIDString { get; set; }
            [TSOVoltronString(TSOVoltronValueTypes.Pascal)] public string AvatarName { get; set; }

            public UpdateRoom_AvatarInformationPack(uint AvatarID, string AvatarName)
            {
                AvatarIDString = "A " + AvatarID;
                this.AvatarName = AvatarName;
            }

            string IUpdateRoomStringPack.String1 => AvatarIDString;
            string IUpdateRoomStringPack.String2 => AvatarName;
        }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.UPDATE_ROOM_PDU;

        public TSOUpdateRoomPDU() : base() { }

        public TSOUpdateRoomPDU(uint roomID, string roomName, uint dbLotID,
            UpdateRoom_AvatarInformationPack lotOwnerInformation,
            UpdateRoom_AvatarInformationPack roomHostInformation)
        {
            RoomID = roomID;
            RoomNameLotIDPack = RoomNameLotIDPack2 = new(roomName, "A " + dbLotID);
            LotOwnerInformation = lotOwnerInformation;
            RoomHostInformation = roomHostInformation;

            MakeBodyFromProperties();
        }

        /// <summary>
        /// The RoomID of the Room
        /// </summary>
        public uint RoomID { get; set; }
        public byte DataStartByte { get; set; } = 0x01;

        /// <summary>
        /// A <see cref="UpdateRoom_StringPack"/> containing the RoomName and the LotIDString.
        /// <para/><i>LotIDString:</i><para/>
        /// Follows format: "__[LotID]" where the first two character are seemingly ignored.
        /// <para/>Nio2so would follow this format: "A 1337" where 1337 is the lot number
        /// </summary>
        public UpdateRoom_StringPack RoomNameLotIDPack { get; set; }
        public byte DataStartByte2 { get; set; } = 0x01;
        /// <summary>
        /// Avatar information for seemingly the owner of the lot for the room? Unsure of this.
        /// </summary>
        public UpdateRoom_AvatarInformationPack LotOwnerInformation { get; set; }
        /// <summary>
        /// Identical information to <see cref="RoomNameLotIDPack"/> .. subject to change!
        /// </summary>
        public UpdateRoom_StringPack RoomNameLotIDPack2 { get; set; }
        public uint Arg1 { get; set; } = 0x01;
        public uint Arg2 { get; set; } = 0x10;
        public ushort Arg3 { get; set; } = 0x0002; // could also be two consecutive bytes
        /// <summary>
        /// Unsure what this is currently used for.
        /// </summary>
        [TSOVoltronString(TSOVoltronValueTypes.Pascal)] public string String1 { get; set; } = "G";
        public ushort Arg4 { get; set; } = 0x0001;
        /// <summary>
        /// Avatar information for seemingly the host of the room? Unsure of this.
        /// </summary>
        public UpdateRoom_AvatarInformationPack RoomHostInformation { get; set; }
        public byte DataStartByte3 { get; set; } = 0x01;
        public ushort Arg5 { get; set; } = 0x01;
        /// <summary>
        /// Unsure what this is used for.
        /// </summary>
        public UpdateRoom_StringPack StringPack1 { get; set; } = new("J", "K");
        public byte DataStartByte4 { get; set; } = 0x00;
        public ushort Arg6 { get; set; } = 0x01;
        /// <summary>
        /// Unsure what this is used for.
        /// </summary>
        public UpdateRoom_StringPack StringPack2 { get; set; } = new("L", "M");
        public byte EndByte { get; set; } = 0x0;
    }
}
