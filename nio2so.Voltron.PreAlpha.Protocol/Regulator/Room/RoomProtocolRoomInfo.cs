using nio2so.Voltron.Core.TSO.Struct;
using nio2so.Voltron.PreAlpha.Protocol.Struct;

namespace nio2so.Voltron.PreAlpha.Protocol.Regulator
{
    internal class RoomProtocolRoomInfo
    {
        private Dictionary<uint, TSOPlayerInfoStruct> occupants = new();
        private Dictionary<uint, TSOAriesIDStruct> clients = new();

        public RoomProtocolRoomInfo(TSORoomIDStruct roomID, TSOAriesIDStruct leaderID, uint lotID)
        {
            RoomID = roomID;
            LeaderID = leaderID;
            LotID = lotID;
        }

        public TSORoomIDStruct RoomID { get; set; }
        public TSOAriesIDStruct LeaderID { get; set; }
        public uint LeaderAvatarID => (LeaderID as ITSONumeralStringStruct).NumericID.Value;
        public uint LotID { get; set; }
        public IEnumerable<TSOPlayerInfoStruct> Occupants => occupants.Values;
        public uint OccupantsCount => (uint)occupants.Count;
        public uint MaxOccupants { get; set; } = RoomProtocol.MAX_OCCUPANTS;
        public bool IsLocked { get; set; } = false;
        public IEnumerable<TSOAriesIDStruct> Admins => [new("??1337", "bisquick")];
        /// <summary>
        /// Gets if this room is online, meaning that the <see cref="LeaderID"/> is still connected to this room, hosting it.
        /// </summary>
        public bool IsOnline => clients.ContainsKey(LeaderAvatarID);
        /// <summary>
        /// Creates a new <see cref="TSORoomInfoStruct"/> matching the configuration for this <see cref="RoomProtocolRoomInfo"/>
        /// </summary>
        public TSORoomInfoStruct RoomInfo => new TSORoomInfoStruct(RoomID, LeaderID, OccupantsCount, MaxOccupants, IsLocked, Admins.ToArray());

        /// <summary>
        /// Adds this client to the list of clients in this room. This VoltronID will now receive network transmissions related to this room.
        /// <para/>To be a 'Client', means that this TSOClient instance is in the process of joining the lot, just after the user selects to join the lot.
        /// </summary>
        /// <param name="VoltronID"></param>
        /// <returns></returns>
        public bool ClientJoinRoom(TSOAriesIDStruct VoltronID) => clients.TryAdd(VoltronID.AvatarID, VoltronID);
        /// <summary>
        /// Once the Client leaves this room, they are also removed as an occupant, if they were admitted prior.
        /// </summary>
        /// <param name="VoltronID"></param>
        /// <returns></returns>
        public bool ClientLeaveRoom(TSOAriesIDStruct VoltronID)
        {
            bool result = clients.Remove(VoltronID.AvatarID, out _);
            occupants.Remove(VoltronID.AvatarID);
            return result;
        }
        /// <summary>
        /// Attempts to admit the <paramref name="Player"/> to this room. They must be a Client of this room, first. See: <see cref="ClientJoinRoom(TSOAriesIDStruct)"/>
        /// <para/> To 'Admit' means that the joining process has completed, where the client is now showing the lot on their screen. Otherwise, they're just a Client of the room, not yet admitted.
        /// </summary>
        /// <param name="Player"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool AdmitOccupant(TSOPlayerInfoStruct Player)
        {
            if (clients.ContainsKey(Player.PlayerID.AvatarID))
            {
                if (occupants.ContainsKey(Player.PlayerID.AvatarID)) return true; // already admitted into this room, rejoining?
                return occupants.TryAdd(Player.PlayerID.AvatarID, Player);
            }
            throw new InvalidOperationException($"This player, {Player.PlayerID} is not a client of this room. They cannot be admitted this way.");
        }
        public IEnumerable<TSOAriesIDStruct> GetConnectedClients() => clients.Values.ToArray();
    }
}
