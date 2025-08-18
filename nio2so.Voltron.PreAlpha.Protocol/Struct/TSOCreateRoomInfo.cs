using nio2so.Voltron.PreAlpha.Protocol.PDU;
using nio2so.Voltron.PreAlpha.Protocol.Regulator;

namespace nio2so.Voltron.PreAlpha.Protocol.Struct
{
    /// <summary>
    /// Used in the <see cref="TSOCreateAndJoinRoomPDU"/> to qualify the room that has been created
    /// </summary>
    [Serializable]
    public class TSOCreateRoomInfo
    {
        public TSOCreateRoomInfo()
        {
            
        }

        public TSOCreateRoomInfo(TSORoomIDStruct stageID, bool unknown, string password, uint maxOccupancy, byte roomType, string group) : this()
        {
            RoomName = stageID.RoomName;
            Unknown = unknown;
            Password = password;
            MaxOccupancy = maxOccupancy;
            StageID = stageID;
            RoomType = roomType;
            Group = group;
        }
        /// <summary>
        /// The name of the created room
        /// </summary>
        public string RoomName { get; set; } = "";
        public bool Unknown { get; set; } = false;
        /// <summary>
        /// The password (if applicable) attached to the room (room passwords don't seem possible in any version of TSO)
        /// </summary>
        public string Password { get; set; } = "";
        /// <summary>
        /// The maximum amount of Occupants allowed in this room at any given time
        /// </summary>
        public uint MaxOccupancy { get; set; } = RoomProtocol.MAX_OCCUPANTS;
        /// <summary>
        /// The lot that this room is taking place on
        /// </summary>
        public TSORoomIDStruct StageID { get; set; } = new();
        public byte RoomType { get; set; } = 0x0;
        public string Group { get; set; } = "";
    }
}
