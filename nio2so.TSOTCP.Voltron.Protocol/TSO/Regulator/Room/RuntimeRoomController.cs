using nio2so.DataService.Common.Tokens;
using nio2so.Voltron.Core.TSO.Struct;
using System.Collections.Concurrent;

namespace nio2so.Voltron.Core.TSO.Regulator.Room
{
    /// <summary>
    /// Stores a backlog of transactions that are waiting to be processed by the <see cref="RoomProtocol"/>. 
    /// This is used to ensure that transactions are processed in the correct order and that no transactions are lost.
    /// </summary>
    public class RuntimeRoomTransactionBacklog
    {
        public uint HouseID { get; }

        /// <summary>
        /// Stores a single transaction that is waiting to be processed by the <see cref="RoomProtocol"/>.
        /// </summary>
        public class RoomProtocolTransaction
        {
            public uint SendingAvatarID { get; set; }
            public uint DestinationAvatarID { get; set; }
            public bool IsBroadcast => DestinationAvatarID == 0;
            public TSOVoltronPacket PDU { get; set; }

            public RoomProtocolTransaction() { }
            /// <summary>
            /// Adds a transaction that is a Broadcast to the room.
            /// </summary>
            /// <param name="SendingAvatarID"></param>
            /// <param name="PDU"></param>
            public RoomProtocolTransaction(uint SendingAvatarID, TSOVoltronPacket PDU) : this(SendingAvatarID, 0, PDU)
            {
            }
            /// <summary>
            /// Adds a transmit PDU to the <paramref name="DestinationAvatarID"/>
            /// </summary>
            /// <param name="SendingAvatarID"></param>
            /// <param name="DestinationAvatarID"></param>
            /// <param name="PDU"></param>
            public RoomProtocolTransaction(uint SendingAvatarID, uint DestinationAvatarID, TSOVoltronPacket PDU)
            {
                this.SendingAvatarID = SendingAvatarID;
                this.DestinationAvatarID = DestinationAvatarID;
                this.PDU = PDU;
            }
        }

        ConcurrentDictionary<int, RoomProtocolTransaction> _backlog = new();
        public RuntimeRoomTransactionBacklog(uint HouseID)
        {
            this.HouseID = HouseID;
        }
        
        /// <summary>
        /// Adds a transaction to the end of the backlog
        /// </summary>
        /// <param name="transaction"></param>
        public void AddTransaction(RoomProtocolTransaction transaction)
        {
            int index = _backlog.Count;
            while (!_backlog.TryAdd(index++, transaction))
                ;
        }
        public bool TryGetNextTransaction(out RoomProtocolTransaction? transaction, uint DestinationAvatarID = 0)
        {
            bool isBroadcast = DestinationAvatarID == 0;

            transaction = null;
            int[] keys = _backlog.Keys.OrderBy(x => x).ToArray();
            if (_backlog.Count == 0 || keys.Length == 0) return false;
            for (int i = 0; i < keys.Length; i++)
            {
                var peek = _backlog[keys[i]];
                if (!isBroadcast) // looking specifically for one avatar or the next broadcast
                {
                    //if the next one has 0 as the destination, then it's a broadcast and we can return it to the caller
                    if (!peek.IsBroadcast && peek.DestinationAvatarID != DestinationAvatarID)
                        continue;
                }
                if (_backlog.TryRemove(keys[i], out RoomProtocolTransaction? nextTransaction))
                {
                    transaction = nextTransaction;
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Tracks and serves data on the active rooms in the <see cref="RoomProtocol"/>
    /// </summary>
    public class RuntimeRoomController
    {
        /// <summary>
        /// List of rooms mapped to their <see cref="RoomProtocolRoomInfo.RoomID"/>
        /// </summary>
        private ConcurrentDictionary<uint, RoomProtocolRoomInfo> _roomsByHouseID { get; } = new();

        /// <summary>
        /// List of clients added to rooms using <see cref="ClientEnterRoom(uint, TSOAriesIDStruct, bool)"/>
        /// </summary>
        private ConcurrentDictionary<uint, uint> _playersInRooms { get; } = new();

        /// <summary>
        /// List of all transaction backlog instances for each room, mapped to their <see cref="RoomProtocolRoomInfo.RoomID"/>
        /// </summary>
        private ConcurrentDictionary<uint, RuntimeRoomTransactionBacklog> _transactionBacklogsByHouseID { get; } = new();

        /// <summary>
        /// When a room is offline but a player is set to be joining it, there is a time period where the HSB isn't online yet and the player is waiting.
        /// <para>This list is those waiting players.</para>
        /// <para/> Maps: HouseID -> Queue of AvatarIDs
        /// </summary>
        private ConcurrentDictionary<uint, Queue<uint>> _joiningQueuesByHouseID { get; } = new();
        private ConcurrentDictionary<TSOAriesIDStruct, uint> _hsbAllocationList = new();

        private Func<AvatarIDToken, TSOAriesIDStruct> GetVoltronIDStruct => Parent.GetVoltronIDStruct;
        public const uint MAX_OCCUPANTS = 24;

        private TSOProtocolBase Parent { get; }

        /// <summary>
        /// Creates a new <see cref="RuntimeRoomController"/> attached to the given <paramref name="Parent"/>
        /// </summary>
        /// <param name="Parent"></param>
        public RuntimeRoomController(TSOProtocolBase Parent)
        {
            this.Parent = Parent;
        }

        /// <summary>
        /// Sets the given <paramref name="AvatarID"/> to be tracked in the <paramref name="HouseID"/> room
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <param name="HouseID"></param>
        public void SetPlayerInRoom(uint AvatarID, uint HouseID)
        {
            if (!RoomExists(HouseID))
                throw new InvalidOperationException($"Player: {AvatarID} was attempted to put into HouseID: {HouseID}, but that room doesn't exist.");
            if (!_playersInRooms.TryAdd(AvatarID, HouseID))
                _playersInRooms[AvatarID] = HouseID;
        }
        /// <summary>
        /// Returns the Room the <paramref name="AvatarID"/> is supposed to be in. See: <see cref="SetPlayerInRoom(uint, uint)"/> and <see cref="RemoveAvatarFromRoom(uint, out uint)"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <param name="RoomID"></param>
        /// <returns></returns>
        public bool TryGetRoomAvatarIsIn(uint AvatarID, out uint RoomID) => _playersInRooms.TryGetValue(AvatarID, out RoomID);
        /// <summary>
        /// Removes the <paramref name="AvatarID"/> from the room they're in, only in the <see cref="RuntimeRoomController"/>.
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        public bool RemoveAvatarFromRoom(uint AvatarID, out uint HouseID) => _playersInRooms.Remove(AvatarID, out HouseID);

        /// <summary>
        /// Returns a list of all <see cref="TSORoomInfoStruct"/> for the rooms in this <see cref="RuntimeRoomController"/>
        /// </summary>
        /// <returns></returns>
        public TSORoomInfoStruct[] GetAllRoomStructs(bool OnlineOnly = false) => [.. _roomsByHouseID.Where(y => OnlineOnly ? y.Value.IsOnline : true).Select(x => x.Value.RoomInfo)];

        /// <summary>
        /// <inheritdoc cref="TryGetByHouseID(uint, out RoomProtocolRoomInfo)"/>, if it exists.
        /// <para/>Will throw exceptions if the room is not found. Use: <see cref="TryGetByHouseID(uint, out RoomProtocolRoomInfo)"/> if this isn't desired.
        /// </summary>
        /// <param name="houseID"></param>
        /// <returns></returns>
        public RoomProtocolRoomInfo GetByHouseID(uint houseID) => _roomsByHouseID[houseID];
        /// <summary>
        /// Returns whether a room exists or not. You can use <see cref="EnsureRoomExists(uint, TSOAriesIDStruct?, out bool)"/> and <see cref="CreateOrGetRoomWithLeader(uint, TSOAriesIDStruct, out bool)"/>
        /// to create rooms in the protocol.
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        public bool RoomExists(uint HouseID) => _roomsByHouseID.ContainsKey(HouseID);

        /// <summary>
        /// Returns the <see cref="RoomProtocolRoomInfo"/> for the given <paramref name="houseID"/>
        /// </summary>
        /// <param name="houseID"></param>
        /// <param name="Room"></param>
        /// <returns></returns>
        public bool TryGetByHouseID(uint houseID, out RoomProtocolRoomInfo Room) => _roomsByHouseID.TryGetValue(houseID, out Room);

        /// <summary>
        /// Adds the room to the <see cref="RuntimeRoomController"/> and returns the <see cref="RoomProtocolRoomInfo"/> for it.
        /// </summary>
        /// <param name="RoomName"></param>
        /// <param name="HouseID"></param>
        /// <param name="HostID"></param>
        /// <param name="LotLeader"></param>
        /// <param name="Room"></param>
        /// <returns></returns>
        public bool AddRoom(string RoomName, uint HouseID, TSOAriesIDStruct HostID, TSOAriesIDStruct LotLeader, out RoomProtocolRoomInfo Room)
        {
            Room = new RoomProtocolRoomInfo(new(HouseID, RoomName), HostID, LotLeader, HouseID);
            return AddRoom(Room);
        }
        /// <summary>
        /// <inheritdoc cref="AddRoom(string, uint, TSOAriesIDStruct, TSOAriesIDStruct, out RoomProtocolRoomInfo)"/>
        /// </summary>
        /// <param name="Room"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public bool AddRoom(RoomProtocolRoomInfo Room)
        {
            bool result = false;
            if (Room.HostHSBAvatarID == 0)
                throw new InvalidDataException($"LeaderID is invalid: {Room.HostHSBAvatarID}");
            if (Room.HostID == null) // ensure this isn't blank for data integrity
                Room.HostID = GetVoltronIDStruct(Room.HostHSBAvatarID);
            result = _roomsByHouseID.TryAdd(Room.LotID, Room);
            if (!result)
                result = MigrateHost(Room.LotID, Room.HostID);
            else
                Parent.LogConsole($"HouseID: {Room.LotID} is ONLINE.");
            return result;
        }

        /// <summary>
        /// Moves the <see cref="RoomProtocolRoomInfo.HostID"/> to be <paramref name="NewHostID"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <param name="NewHostID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private bool MigrateHost(uint HouseID, TSOAriesIDStruct NewHostID)
        {
            if (!_roomsByHouseID.ContainsKey(HouseID))
                throw new Exception("Cannot migrate host of a room that's closed.");
            var roomInfo = GetByHouseID(HouseID);
            if (roomInfo.HostID.AvatarID != NewHostID.AvatarID)
            {
                roomInfo.HostID = NewHostID;
                Parent.LogConsole($"HouseID: {HouseID} migrating host to: {NewHostID}.");
            }
            return true;
        }

        /// <summary>
        /// Adds this HSB Client as a pending host and will return which HouseID it will be hosting
        /// </summary>
        /// <param name="HSBID"></param>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        public bool GetNextHouseIDForHSB(TSOAriesIDStruct HSBID, out uint HouseID)
        {
            IEnumerable<uint> awaitingJoinerHouseIDs = _joiningQueuesByHouseID.Keys;
            foreach (uint houseID in awaitingJoinerHouseIDs)
            {
                if (!_hsbAllocationList.Values.Contains(houseID))
                {
                    _hsbAllocationList.TryAdd(HSBID, houseID);
                    HouseID = houseID;
                    return true;
                }
            }
            HouseID = 0;
            return false;
        }
        /// <summary>
        /// Removes all joiner and HSB hosting information from the pending lists, this is the final stage of HSB hosting procedures.
        /// <para/>This will return which lot this HSB is hosting and the people waiting to join it.
        /// </summary>
        /// <param name="HSBID"></param>
        /// <param name="Information"></param>
        /// <param name="FailureReason"></param>
        /// <returns></returns>
        public bool PopHouseHostingInfoForHSB(TSOAriesIDStruct HSBID, out (uint HouseID, Queue<uint> JoiningQueue)? Information, out string FailureReason)
        {
            FailureReason = $"This VoltronID {HSBID} doesn't match any pending HSB host clients.";
            Information = default;

            //this is a serious failure
            if (!_hsbAllocationList.Remove(HSBID, out uint HouseID))
                return false;
            //this is not, though extremely unlikely
            _joiningQueuesByHouseID.Remove(HouseID, out Queue<uint>? joiners);

            if (joiners == null) joiners = new();

            FailureReason = "success.";
            Information = (HouseID, joiners);
            return true;
        }

        public bool AddJoinerToQueue(uint joiningAvatarID, uint houseID, out string FailureReason)
        {
            FailureReason = "success.";
            if (_joiningQueuesByHouseID.TryGetValue(houseID, out var queue))
            {
                if (!queue.Any(x => x == joiningAvatarID))
                    queue.Append(joiningAvatarID);
            }
            else
                _joiningQueuesByHouseID.TryAdd(houseID, new([joiningAvatarID]));

            return true;
        }

        public bool AddTransactionToBacklog(uint houseID, RuntimeRoomTransactionBacklog.RoomProtocolTransaction transaction)
        {
            if (!_transactionBacklogsByHouseID.TryGetValue(houseID, out RuntimeRoomTransactionBacklog? backlog))
            {
                backlog = new(houseID);
                _transactionBacklogsByHouseID.TryAdd(houseID, backlog);
            }
            backlog.AddTransaction(transaction);
            return true;
        }

        public bool TryGetNextTransactionFromBacklog(uint houseID, out RuntimeRoomTransactionBacklog.RoomProtocolTransaction? transaction, uint DestinationAvatarID = 0)
        {
            transaction = null;
            if (!_transactionBacklogsByHouseID.TryGetValue(houseID, out RuntimeRoomTransactionBacklog? backlog))
                return false;
            return backlog.TryGetNextTransaction(out transaction, DestinationAvatarID);
        }
    }
}
