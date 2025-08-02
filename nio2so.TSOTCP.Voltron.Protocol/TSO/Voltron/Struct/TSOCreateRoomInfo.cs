using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Regulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct
{
    public class TSOCreateRoomInfo
    {
        public TSOCreateRoomInfo(TSORoomIDStruct stageID, bool unknown, string password, uint maxOccupancy, byte roomType, string group)
        {
            Name = stageID.RoomName;
            Unknown = unknown;
            Password = password;
            MaxOccupancy = maxOccupancy;
            StageID = stageID;
            RoomType = roomType;
            Group = group;
        }

        public string Name { get; set; } = "";
        public bool Unknown { get; set; } = false;
        public string Password { get; set; } = "";
        public uint MaxOccupancy { get; set; } = RoomProtocol.MAX_OCCUPANTS;
        public TSORoomIDStruct StageID { get; set; } = new();
        public byte RoomType { get; set; } = 0x0;
        public string Group { get; set; } = "";
    }
}
