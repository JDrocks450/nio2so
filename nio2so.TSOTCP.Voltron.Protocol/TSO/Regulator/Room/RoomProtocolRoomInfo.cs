using nio2so.Voltron.Core.TSO.Struct;

namespace nio2so.Voltron.Core.TSO.Regulator.Room
{
    public class RoomProtocolRoomInfo
    {
        private Dictionary<uint, TSOPlayerInfoStruct> occupants = new();
        private Dictionary<uint, TSOAriesIDStruct> clients = new();

        public RoomProtocolRoomInfo(TSORoomIDStruct roomID, TSOAriesIDStruct hsbID, TSOAriesIDStruct lotLeader, uint lotID)
        {
            RoomID = roomID;
            HostID = hsbID;
            LotLeader = lotLeader;
            LotID = lotID;
        }

        public TSORoomIDStruct RoomID { get; set; }
        /// <summary>
        /// The AvatarID of the Avatar who owns the lot.
        /// </summary>
        public TSOAriesIDStruct LotLeader { get; set; }
        public TSOAriesIDStruct? HostID { get; set; }

        public uint HostHSBAvatarID => (HostID as ITSONumeralStringStruct)?.NumericID ?? 0;
        public uint LotID { get; set; }
        public IEnumerable<TSOPlayerInfoStruct> Occupants => occupants.Values;
        public uint OccupantsCount => (uint)occupants.Count;
        public uint MaxOccupants { get; set; } = RuntimeRoomController.MAX_OCCUPANTS;
        public bool IsLocked { get; set; } = false;
        public IEnumerable<TSOAriesIDStruct> Admins => [new("??1337", "bisquick")];
        /// <summary>
        /// Gets if this room is online, meaning that the <see cref="HostID"/> is still connected to this room, hosting it.
        /// </summary>
        public bool IsOnline => HostID != null && clients.ContainsKey(HostHSBAvatarID);
        /// <summary>
        /// Creates a new <see cref="TSORoomInfoStruct"/> matching the configuration for this <see cref="RoomProtocolRoomInfo"/>
        /// </summary>
        public TSORoomInfoStruct RoomInfo => new TSORoomInfoStruct(RoomID, HostID, OccupantsCount, MaxOccupants, IsLocked, Admins.ToArray());

        public bool ClientInRoom(TSOAriesIDStruct VoltronID) => clients.ContainsKey(VoltronID.AvatarID);
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
        public TSOAriesIDStruct[] GetConnectedClients() => clients.Values.ToArray();
    }
}
