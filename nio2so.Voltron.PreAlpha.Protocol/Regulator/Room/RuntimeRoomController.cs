using nio2so.Voltron.Core.Telemetry;
using nio2so.Voltron.Core.TSO.Struct;
using nio2so.Voltron.PreAlpha.Protocol.PDU;
using nio2so.Voltron.PreAlpha.Protocol.Struct;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.PreAlpha.Protocol.Regulator.Room
{
    /// <summary>
    /// Tracks and serves data on the active rooms in the <see cref="RoomProtocol"/>
    /// </summary>
    internal class RuntimeRoomController
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
        /// When a room is offline but a player is set to be joining it, there is a time period where the HSB isn't online yet and the player is waiting.
        /// <para>This list is those waiting players.</para>
        /// <para/> Maps: HouseID -> Queue of AvatarIDs
        /// </summary>
        private ConcurrentDictionary<uint, Queue<uint>> _joiningQueuesByHouseID { get; } = new();
        private ConcurrentDictionary<TSOAriesIDStruct, uint> _hsbAllocationList = new();

        private RoomProtocol Parent { get; }

        /// <summary>
        /// Creates a new <see cref="RuntimeRoomController"/> attached to the given <paramref name="Parent"/>
        /// </summary>
        /// <param name="Parent"></param>
        public RuntimeRoomController(RoomProtocol Parent)
        {
            this.Parent = Parent;
        }

        /// <summary>
        /// Sets the given <paramref name="AvatarID"/> to be tracked in the <paramref name="HouseID"/> room
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <param name="HouseID"></param>
        internal void SetPlayerInRoom(uint AvatarID, uint HouseID)
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
        internal bool TryGetRoomAvatarIsIn(uint AvatarID, out uint RoomID) => _playersInRooms.TryGetValue(AvatarID, out RoomID);
        /// <summary>
        /// Removes the <paramref name="AvatarID"/> from the room they're in, only in the <see cref="RuntimeRoomController"/>.
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        internal bool RemoveAvatarFromRoom(uint AvatarID, out uint HouseID) => _playersInRooms.Remove(AvatarID, out HouseID);

        /// <summary>
        /// Returns a list of all <see cref="TSORoomInfoStruct"/> for the rooms in this <see cref="RuntimeRoomController"/>
        /// </summary>
        /// <returns></returns>
        internal TSORoomInfoStruct[] GetAllRoomStructs(bool OnlineOnly = false) => [.. _roomsByHouseID.Where(y => OnlineOnly ? y.Value.IsOnline : true).Select(x => x.Value.RoomInfo)];

        /// <summary>
        /// <inheritdoc cref="TryGetByHouseID(uint, out RoomProtocolRoomInfo)"/>, if it exists.
        /// <para/>Will throw exceptions if the room is not found. Use: <see cref="TryGetByHouseID(uint, out RoomProtocolRoomInfo)"/> if this isn't desired.
        /// </summary>
        /// <param name="houseID"></param>
        /// <returns></returns>
        internal RoomProtocolRoomInfo GetByHouseID(uint houseID) => _roomsByHouseID[houseID];
        /// <summary>
        /// Returns whether a room exists or not. You can use <see cref="EnsureRoomExists(uint, TSOAriesIDStruct?, out bool)"/> and <see cref="CreateOrGetRoomWithLeader(uint, TSOAriesIDStruct, out bool)"/>
        /// to create rooms in the protocol.
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        internal bool RoomExists(uint HouseID) => _roomsByHouseID.ContainsKey(HouseID);

        /// <summary>
        /// Returns the <see cref="RoomProtocolRoomInfo"/> for the given <paramref name="houseID"/>
        /// </summary>
        /// <param name="houseID"></param>
        /// <param name="Room"></param>
        /// <returns></returns>
        internal bool TryGetByHouseID(uint houseID, out RoomProtocolRoomInfo Room) => _roomsByHouseID.TryGetValue(houseID, out Room);

        internal bool AddRoom(RoomProtocolRoomInfo Room)
        {
            bool result = false;
            if (Room.LeaderAvatarID == 0)
                throw new InvalidDataException($"LeaderID is invalid: {Room.LeaderAvatarID}");
            if (string.IsNullOrWhiteSpace(Room.LeaderID.MasterID)) // ensure this isn't blank for data integrity
                Room.LeaderID = Parent.GetVoltronIDStruct(Room.LeaderAvatarID);
            result = _roomsByHouseID.TryAdd(Room.LotID, Room);
            if (!result)
                result = MigrateHost(Room.LotID, Room.LeaderID);
            else
                Parent.LogConsole($"HouseID: {Room.LotID} is ONLINE.");
            return result;
        }

        /// <summary>
        /// Moves the <see cref="RoomProtocolRoomInfo.LeaderID"/> to be <paramref name="NewLeaderID"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <param name="NewLeaderID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private bool MigrateHost(uint HouseID, TSOAriesIDStruct NewLeaderID)
        {
            if (!_roomsByHouseID.ContainsKey(HouseID))
                throw new Exception("Cannot migrate host of a room that's closed.");
            var roomInfo = GetByHouseID(HouseID);
            if (roomInfo.LeaderID.AvatarID != NewLeaderID.AvatarID)
            {
                roomInfo.LeaderID = NewLeaderID;
                Parent.LogConsole($"HouseID: {HouseID} migrating host to: {NewLeaderID}.");
            }
            return true;
        }
        /// <summary>
        /// Adds this HSB Client as a pending host and will return which HouseID it will be hosting
        /// </summary>
        /// <param name="HSBID"></param>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        internal bool GetNextHouseIDForHSB(TSOAriesIDStruct HSBID, out uint HouseID)
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
        internal bool PopHouseHostingInfoForHSB(TSOAriesIDStruct HSBID, out (uint HouseID, Queue<uint> JoiningQueue)? Information, out string FailureReason)
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
            Information = (HouseID,joiners);
            return true;
        }

        internal bool AddJoinerToQueue(uint joiningAvatarID, uint houseID, out string FailureReason)
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
    }
}
