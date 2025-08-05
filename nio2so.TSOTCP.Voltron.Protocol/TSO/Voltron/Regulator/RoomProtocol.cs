using nio2so.Data.Common.Testing;
using nio2so.DataService.Common.Queries;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types;
using nio2so.DataService.Common.Types.Lot;
using nio2so.TSOTCP.Voltron.Protocol.Services;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.Datablob;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.Datablob.Structures;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Regulator
{
    /// <summary>
    /// Manages the current online simulation rooms in Voltron
    /// </summary>
    [TSORegulator]
    public class RoomProtocol : TSOProtocol
    {
        public const uint MAX_OCCUPANTS = 10;
        public const double RESYNC_TIMEOUT = 0;

        private class RoomProtocolRoomInfo
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
            public uint MaxOccupants { get; set; } = MAX_OCCUPANTS;
            public bool IsLocked { get; set; } = false;
            public IEnumerable<TSOAriesIDStruct> Admins => [new("??1337","bisquick")];
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
            /// </summary>
            /// <param name="VoltronID"></param>
            /// <returns></returns>
            public bool ClientJoinRoom(TSOAriesIDStruct VoltronID) => clients.TryAdd(VoltronID.AvatarID, VoltronID);
            public bool ClientLeaveRoom(TSOAriesIDStruct VoltronID)
            {
                bool result = clients.Remove(VoltronID.AvatarID, out _);
                occupants.Remove(VoltronID.AvatarID);
                return result;
            }
            /// <summary>
            /// Attempts to admit the <paramref name="Player"/> to this room. They must be a Client of this room, first. See: <see cref="ClientJoinRoom(TSOAriesIDStruct)"/>
            /// </summary>
            /// <param name="Player"></param>
            /// <returns></returns>
            /// <exception cref="InvalidOperationException"></exception>
            public bool AdmitOccupant(TSOPlayerInfoStruct Player)
            {
                if (clients.ContainsKey(Player.PlayerID.AvatarID))
                    return occupants.TryAdd(Player.PlayerID.AvatarID,Player);
                throw new InvalidOperationException($"This player, {Player.PlayerID} is not a client of this room. They cannot be admitted this way.");
            }
            public IEnumerable<TSOAriesIDStruct> GetConnectedClients() => clients.Values.ToArray();            
        }

        private DateTime _reSyncTime = DateTime.MinValue;
        private ConcurrentDictionary<uint, RoomProtocolRoomInfo> _roomsByHouseID { get; } = new();
        /// <summary>
        /// List of clients added to rooms using <see cref="ClientEnterRoom(uint, TSOAriesIDStruct, bool)"/>
        /// </summary>
        private ConcurrentDictionary<uint, uint> _playersInRooms { get; } = new();

        /// <summary>
        /// Clears this player from any rooms they may be in from their play session (used when disconnecting/reconnecting)
        /// </summary>
        /// <param name="voltronID"></param>
        /// <exception cref="NotImplementedException"></exception>
        public bool AvatarPurgePlaySession(TSOAriesIDStruct? voltronID, out string FailureReason) => 
            ClientUpdateRoom_LeaveRoom(voltronID, out FailureReason);
        /// <summary>
        /// Creates a new <see cref="RoomProtocolRoomInfo"/> that is empty, uses the info from the given <paramref name="HouseID"/> and is lead by <paramref name="Leader"/>
        /// <para/><paramref name="Created"/> True if the room as just now created, false if it already was open. Will migrate host to this passed one if already existed.
        /// </summary>
        /// <param name="HouseID"></param>
        /// <param name="Leader">If null, will use the Lot's Owner ID ... CHANGE LATER.</param>
        /// <param name="Created"></param>
        /// <returns></returns>
        private RoomProtocolRoomInfo CreateOrGetRoom(uint HouseID, TSOAriesIDStruct? Leader, out bool Created)
        {
            Created = false;
            //download lot profile
            var lot = GetLotProfile(HouseID);
            if (Leader == null) 
                Leader = GetVoltronIDStruct(lot.OwnerAvatar);
            //add the room
            Created = AddRoom(new RoomProtocolRoomInfo(new TSORoomIDStruct(lot.HouseID, lot.Name), Leader, HouseID));            
            return _roomsByHouseID[HouseID];
        }

        private bool AddRoom(RoomProtocolRoomInfo Room)
        {
            bool result = false;
            if (Room.LeaderAvatarID == 0)
                throw new InvalidDataException($"LeaderID is invalid: {Room.LeaderAvatarID}");
            if (string.IsNullOrWhiteSpace(Room.LeaderID.MasterID)) // ensure this isn't blank for data integrity
                Room.LeaderID = GetVoltronIDStruct(Room.LeaderAvatarID);
            result = _roomsByHouseID.TryAdd(Room.LotID, Room);
            if (!result) 
                result = MigrateHost(Room.LotID, Room.LeaderID);
            else
                LogConsole($"HouseID: {Room.LotID} is ONLINE.");
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
            var roomInfo = _roomsByHouseID[HouseID];
            if (roomInfo.LeaderID.AvatarID != NewLeaderID.AvatarID)
            {
                roomInfo.LeaderID = NewLeaderID;
                LogConsole($"HouseID: {HouseID} migrating host to: {NewLeaderID}.");
            }
            return true;
        }

        /// <summary>
        /// Ensures every lot in the data service has a room entry <para/>
        /// Every lot needs a Room entry in the list -- having a player count of ZERO/NONZERO will take it offline/online.
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        private void SyncRoomsWithLotDataService()
        {
            if ((DateTime.Now - _reSyncTime).TotalSeconds < RESYNC_TIMEOUT)
                return; // too quick since last sync!

            //download all lots from data service ... change later to be rooms
            if (!TryDataServiceQuery(() => GetDataService().GetAllLotProfiles(), out N2GetLotListQueryResult? result, out string error))
                throw new InvalidDataException(error);          

            int i = 0;
            foreach (var houseID in result.Lots.Select(x => x.HouseID)) // UPDATE LATER TO BE ONLINE LOTS
            { // add the room if its not already in the list
                CreateOrGetRoom(houseID,null,out _);
            }
        }

        /// <summary>
        /// Designates a TSOClient as being in this room. See: <see cref="RoomProtocolRoomInfo.ClientJoinRoom(TSOAriesIDStruct)"/>
        /// <para/><inheritdoc cref="RoomProtocolRoomInfo.ClientJoinRoom(TSOAriesIDStruct)"/>
        /// <para/>Sends the Leader a <see cref="TSOOccupantArrivedPDU"/>
        /// <para/>Maps this Avatar to this room.
        /// <para/>Does NOT send <see cref="TSOUpdateRoomPDU"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <param name="JoinerVoltronID"></param>
        /// <param name="HSB">If true, this Avatar is not a physical avatar is will not be treated as an occupant</param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        bool ClientUpdateRoom_EnterRoom(uint HouseID, TSOAriesIDStruct JoinerVoltronID, bool HSB, out string JoinFailureReason)
        {
            TSOPlayerInfoStruct playerInfo = GetPlayerInfoStruct(JoinerVoltronID);
            uint avatarID = (JoinerVoltronID as ITSONumeralStringStruct)?.NumericID ?? 0;
            if (avatarID == 0)
                throw new InvalidDataException($"AvatarID passed was {avatarID} (invalid)");
            RoomProtocolRoomInfo roomInfo = _roomsByHouseID[HouseID];
            bool result = true;
            JoinFailureReason = "Couldn't communicate with the host using Voltron. (Did they disconnect?)";
            if (!HSB) // the host should not get this packet received for themself, causes issues.
                result = TrySendTo(roomInfo.LeaderID, new TSOOccupantArrivedPDU(playerInfo));
            JoinFailureReason = "Your account is already in this room.";
            if (result)
                result = roomInfo.ClientJoinRoom(JoinerVoltronID);
            JoinFailureReason = "Mapping which room your avatar is in failed.";
            if (result)
            {
                if (!_playersInRooms.TryAdd(avatarID, HouseID))
                    _playersInRooms[avatarID] = HouseID;
            }
            JoinFailureReason = "success.";
            LogConsole($"VoltronID: {JoinerVoltronID} TSOClient has connected to HouseID: {HouseID} -- {(HSB ? "as a host" : "as a visitor")}.");
            return result;
        }      
        bool ClientUpdateRoom_LeaveRoom(TSOAriesIDStruct VoltronID, out string FailureReason, uint HouseID = 0)
        {
            uint avatarID = VoltronID.AvatarID;
            FailureReason = $"{VoltronID} is not valid.";
            if (avatarID == 0)
                return false;
            FailureReason = $"AvatarID: {avatarID} isn't in a room and you didn't provide one to remove them from.";
            _playersInRooms.TryGetValue(avatarID, out uint supposedRoom);
            if (HouseID == 0)
                HouseID = supposedRoom;
            else if (HouseID != supposedRoom)
            {
                FailureReason = $"AvatarID: {avatarID} is in room: {supposedRoom} but is leaving: {HouseID}. Not matching!";
                return false;
            }
            if (HouseID == 0)
                return false;

            _playersInRooms.Remove(avatarID, out HouseID);
            var roomInfo = _roomsByHouseID[HouseID];
            bool result = roomInfo.ClientLeaveRoom(VoltronID);            

            //**update client to not be in a room
            TrySendTo(VoltronID, new TSOUpdateRoomPDU(134, TSORoomInfoStruct.NoRoom));
            //**notify all connected clients the updated avatar list
            UpdateLotOccupants(roomInfo);
            //**notify mapview that the playercount has changed
            NotifyLotsOnline();

            LogConsole($"AvatarID: {avatarID} is not in a room.");
            return true;
        }
        /// <summary>
        /// Adds the <paramref name="AvatarID"/> to this Occupants list of the room they're joining and broadcasts <see cref="TSOUpdateOccupantsPDU"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        bool AdmitAvatarToRoom(uint AvatarID)
        {
            //refer to the map of clients to rooms to find which room I'm supposed to be in
            if (!_playersInRooms.TryGetValue(AvatarID, out uint HouseID))
                throw new InvalidOperationException($"AvatarID {AvatarID} is not in a room, yet is sending a broadcast PDU.");
            var roomInfo = _roomsByHouseID[HouseID];

            //add the avatar to the occupants list
            bool success = roomInfo.AdmitOccupant(GetPlayerInfoStruct(AvatarID));
            if (!success) return false;

            //**notify all connected clients the updated avatar list
            UpdateLotOccupants(roomInfo);

            LogConsole($"AvatarID: {AvatarID} is now an occupant of: {HouseID}");
            return true;
        }
        void UpdateLotOccupants(RoomProtocolRoomInfo roomInfo)
        {
            //**tell the clients (NOT the host) the new list of players currently in this room            
            if (GetRoomUpdateOccupantsPDUByRoomID(roomInfo.LotID, roomInfo.LeaderAvatarID, out TSOUpdateOccupantsPDU? HostOccupantsPDU))
                BroadcastPDUToRoom(roomInfo.LotID, HostOccupantsPDU, true);

            TSOListOccupantsResponsePDU occupantsPDU = GetOccupantsPDUByRoomID(roomInfo.LotID, out _);
            BroadcastPDUToRoom(roomInfo.LotID, occupantsPDU);
        }

        /// <summary>
        /// Sends this <paramref name="PDU"/> to every client connected to this <paramref name="HouseID"/> <see cref="RoomProtocolRoomInfo"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <param name="PDU"></param>
        /// <param name="ExcludeHost">Do not send this PDU to the host if true</param>
        void BroadcastPDUToRoom(uint HouseID, TSOVoltronPacket PDU, bool ExcludeHost = false)
        {
            RoomProtocolRoomInfo roomInfo = _roomsByHouseID[HouseID];
            BroadcastPDUToRoom(HouseID, PDU, ExcludeHost ? [roomInfo.LeaderAvatarID] : Array.Empty<uint>());
        }
        /// <summary>
        /// <inheritdoc cref="BroadcastPDUToRoom(uint, TSOVoltronPacket, bool)"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <param name="PDU"></param>
        /// <param name="ExcludeAvatarIDs">Do not send this PDU to any avatar in this list</param>
        void BroadcastPDUToRoom(uint HouseID, TSOVoltronPacket PDU, params uint[] ExcludeAvatarIDs)
        {
            RoomProtocolRoomInfo roomInfo = _roomsByHouseID[HouseID];
            IEnumerable<TSOAriesIDStruct> clients = roomInfo.GetConnectedClients();
            foreach (var client in clients)
            {
                if (ExcludeAvatarIDs.Contains((client as ITSONumeralStringStruct)?.NumericID ?? 0)) 
                    continue;
                TrySendTo(client, PDU);
            }
        }

        /// <summary>
        /// <inheritdoc cref="LotProtocol.GetLotProfile(HouseIDToken)"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        public LotProfile GetLotProfile(HouseIDToken HouseID) => GetRegulator<LotProtocol>().GetLotProfile(HouseID);
        /// <summary>
        /// <inheritdoc cref="AvatarProtocol.GetAvatarIDStruct(AvatarIDToken)"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        public TSOAriesIDStruct GetVoltronIDStruct(AvatarIDToken AvatarID) => GetRegulator<AvatarProtocol>().GetVoltronIDStruct(AvatarID);
        public TSOPlayerInfoStruct GetPlayerInfoStruct(TSOAriesIDStruct VoltronID) => GetRegulator<AvatarProtocol>().GetPlayerInfoStruct(VoltronID);
        public TSOPlayerInfoStruct GetPlayerInfoStruct(AvatarIDToken AvatarID) => GetRegulator<AvatarProtocol>().GetPlayerInfoStruct(AvatarID);
        public TSORoomInfoStruct GetRoomByPlayerID(uint AvatarID)
        {
            _playersInRooms.TryGetValue(AvatarID, out uint HouseID);
            _roomsByHouseID.TryGetValue(HouseID, out RoomProtocolRoomInfo? roomInfo);
            return roomInfo?.RoomInfo ?? TSORoomInfoStruct.NoRoom;
        }
        /// <summary>
        /// <inheritdoc cref="RoomProtocolRoomInfo.IsOnline"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        public bool RoomIsOnline(uint HouseID) => _roomsByHouseID.TryGetValue(HouseID, out RoomProtocolRoomInfo? RoomInfo) && RoomInfo.IsOnline;

        /// <summary>
        /// Returns a <see cref="TSOListOccupantsResponsePDU"/> with the occupants of a given room.
        /// <para/>If the room given by <paramref name="HouseID"/> is offline, an empty response is created.
        /// </summary>
        /// <param name="HouseID"></param>
        /// <param name="OccupantsPDU"></param>
        /// <returns></returns>
        public TSOListOccupantsResponsePDU GetOccupantsPDUByRoomID(uint HouseID, out bool IsOnline)
        {
            //check if room is online
            if (_roomsByHouseID.TryGetValue(HouseID, out RoomProtocolRoomInfo? roomInstance))
            { // online room
                IsOnline =  true;
                return new TSOListOccupantsResponsePDU(roomInstance.RoomID, roomInstance.Occupants.ToArray()); // respond with occupants
            }
            //**basic response
            var profile = GetLotProfile(HouseID); // download profile
            //no occupants           
            IsOnline = false;
            return new TSOListOccupantsResponsePDU(new(profile.HouseID, profile.Name));
        }
        /// <summary>
        /// Returns a <see cref="TSOUpdateOccupantsPDU"/> with the occupants of a given room.
        /// <para/>If the room given by <paramref name="HouseID"/> is offline, return false.
        /// </summary>
        /// <param name="HouseID"></param>
        /// <param name="OccupantsPDU"></param>
        /// <returns></returns>
        public bool GetRoomUpdateOccupantsPDUByRoomID(uint HouseID, uint OmitAvatarID, out TSOUpdateOccupantsPDU? OccupantsPDU)
        {
            OccupantsPDU = default;
            //check if room is online
            if (_roomsByHouseID.TryGetValue(HouseID, out RoomProtocolRoomInfo? roomInstance))
            { // online room
                if (roomInstance.Occupants?.Any() ?? false)
                {
                    OccupantsPDU = new TSOUpdateOccupantsPDU(roomInstance.RoomInfo, roomInstance.Occupants.Where(x => (x.PlayerID as ITSONumeralStringStruct).NumericID != OmitAvatarID).ToArray()); // respond with occupants                
                    return true;
                }
            }
#if false
            //**basic response
            var profile = GetLotProfile(HouseID); // download profile

            OccupantsPDU = new TSOUpdateOccupantsPDU(new TSORoomInfoStruct(
                new(profile.PhoneNumber, profile.Name),new(profile.OwnerAvatar,""), 2),
                [GetPlayerInfoStruct(1338),GetPlayerInfoStruct(161)]); // respond with occupants
            return true;
#endif
            return false;
        }

        /// <summary>
        /// Helper function to send to all connected clients an updated list of online rooms
        /// <para/>If all lots are offline when a client connects to the MapView, this NEEDS to be called to get them to refresh their room list, as they
        /// will never automatically ask again if this is true.
        /// </summary>
        private void NotifyLotsOnline() => BroadcastToServer(new TSOListRoomsResponsePDU([.. _roomsByHouseID.Select(x => x.Value.RoomInfo)]));

        protected override bool OnUnknownDataBlobPDU(ITSODataBlobPDU PDU)
        {
            OnStandardMessage(PDU);
            return true;
        }

        [TSOProtocolDatablobHandler(TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage)]
        public void OnStandardMessage(ITSODataBlobPDU PDU)
        {
            TSOBroadcastDatablobPacket? broadcastPDU = (PDU as TSOBroadcastDatablobPacket);
            TSOAriesIDStruct sender = PDU.SenderInfo.PlayerID;
            uint avatarID = (sender as ITSONumeralStringStruct)?.NumericID ?? 0;
            if (avatarID == 0)
                throw new InvalidDataException($"AvatarID sending a broadcast PDU is {avatarID}, ARIESID: {sender}");
            if (!_playersInRooms.TryGetValue(avatarID, out uint HouseID))
                throw new InvalidOperationException($"AvatarID {avatarID} is not in a room, yet is sending a broadcast PDU.");
            var roomInfo = _roomsByHouseID[HouseID];

            if (PDU is TSOTransmitDataBlobPacket transmitPDU)
            {
                broadcastPDU = new TSOBroadcastDatablobPacket(transmitPDU);
                TrySendTo(transmitPDU.DestinationSessionID, broadcastPDU);
            }
            if (PDU is TSOBroadcastDatablobPacket && broadcastPDU != null)
            {                                
                if (avatarID != roomInfo.LeaderAvatarID)
                {
                    broadcastPDU.SenderInfo = GetPlayerInfoStruct(roomInfo.LeaderID);
                }
                BroadcastPDUToRoom(HouseID, broadcastPDU); // host to room
            }

            //check if this is confirming entry to a room, in which case transition this avatar into the room they're joining
            if (PDU.DataBlobContentObject.TryGetByCLSID(TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage, out ITSODataBlobContentObject? obj))
                if ((obj as TSOStandardMessageContent).kMSG == TSO_PreAlpha_MasterConstantsTable.kMSGID_HouseReceived)
                    if (!AdmitAvatarToRoom(avatarID))
                        throw new InvalidOperationException($"Could not admit: {avatarID} into room {HouseID} after {TSO_PreAlpha_MasterConstantsTable.kMSGID_HouseReceived}!");
        }


        /// <summary>
        /// This function is invoked when the <see cref="RoomProtocol"/> receives an incoming <see cref="TSOGetHouseLeaderByIDRequest"/>
        /// </summary>
        /// <param name="PDU"></param>
        /// <exception cref="NullReferenceException"></exception>
        [TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs.GetHouseLeaderByLotID_Request)]
        public void GetHouseLeaderByLotID_Request(TSODBRequestWrapper PDU)
        {
            uint HouseID = ((TSOGetHouseLeaderByIDRequest)PDU).HouseID;
            if (_roomsByHouseID.TryGetValue(HouseID, out RoomProtocolRoomInfo? room))
                RespondTo(PDU, new TSOGetHouseLeaderByIDResponse(HouseID, room.LeaderAvatarID));
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.LIST_ROOMS_PDU)]
        public void LIST_ROOMS_PDU(TSOVoltronPacket PDU)
        {
            SyncRoomsWithLotDataService();
            NotifyLotsOnline();
            //NotifyLotOccupants();
            return;

            //test message pdu ... doesn't work
            RespondWith(new TSOAnnouncementMsgPDU(new TSOPlayerInfoStruct(new(161, "FriendlyBuddy")), "Testing"));
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.LOT_ENTRY_REQUEST_PDU)]
        public void LOT_ENTRY_REQUEST_PDU(TSOVoltronPacket PDU)
        {
            dynamic roomPDU = PDU;
            //identify client
            nio2soClientSessionService clientSessionService = GetService<nio2soClientSessionService>();
            if (!clientSessionService.GetVoltronClientByPDU(PDU, out TSOAriesIDStruct? joiningClient) || joiningClient == null)
                throw new InvalidOperationException("Cannot identify who sent this packet to Voltron.");
            ENTER_LOT(joiningClient,roomPDU.HouseID);
        }
        private void ENTER_LOT(TSOAriesIDStruct JoiningClient, uint HouseID)
        {
            var joiningClient = JoiningClient;

            //is the lot online?
            bool isOnline = false;
            if (_roomsByHouseID.TryGetValue(HouseID, out RoomProtocolRoomInfo? roomInfo) && roomInfo != null)
                isOnline = roomInfo.IsOnline; // is only online if there is 

            //get the joining player's ID
            uint joiningAvatarID = (joiningClient as ITSONumeralStringStruct)?.NumericID ?? 0;
            if (joiningAvatarID == 0)
                throw new Exception("Joining client is not identified. AvatarID: " + joiningAvatarID);

            //get the roomname only if the lot is currently ONLINE
            string? RoomName = roomInfo?.RoomID?.RoomName;
            TSORoomIDStruct? roomIDStruct = roomInfo?.RoomID;

            bool successfulJoin = false;
            bool hosting = false;
            string failureReason = "none set.";

            LogConsole($"AvatarID: {joiningAvatarID} is joining house: {HouseID} ({(isOnline ? "Online" : "Offline")})");

            //is this lot online?
            if (!isOnline)
            {   // room is OFFLINE ... try to host a new lobby
                // get the lot profile first
                LotProfile thisLot = GetLotProfile(HouseID);
                if (thisLot == null)
                    throw new NullReferenceException($"LotID: {HouseID} was not found in the data service!");

                //update these with the lot info
                RoomName = thisLot.Name ?? "BloatyWorld";
                roomIDStruct = new(thisLot.HouseID, RoomName);

                //download the roommates of this house
                if (!TryDataServiceQuery(() => GetDataService().GetRoommatesByHouseID(HouseID), out IEnumerable<AvatarIDToken>? lotRoommates, out string error))
                    throw new InvalidDataException(error);

                // check if i am one of those roommates (or the owner)
                if ((lotRoommates != null && lotRoommates.Contains(joiningAvatarID)) || FORCE_ENTRY)
                {   // Initiate the host protocol
                    // ADD ROOM TO PROTOCOL (CreateRoom Now)
                    hosting = true;

                    //get create pdu with room details
                    //add this room to the RoomProtocol Voltron Protocol
                    var createRoomInfo = CreateOrGetRoom(HouseID, joiningClient, out bool Created);

                    successfulJoin = true;
                    if (successfulJoin) // TELL THE CLIENT TO START THE HOST PROTOCOL   
                    {
                        RespondWith(new TSOCreateRoomResponsePDU(createRoomInfo.RoomID));
                        RespondWith(new TSOHouseSimConstraintsResponsePDU(HouseID)); // transition to lot view as Host
                        successfulJoin = ClientUpdateRoom_EnterRoom(thisLot.HouseID, joiningClient, true, out failureReason); // join this avatar into the new room as an HSB
                    }
                    //**list of online rooms (or data about the room) has changed ... tell everyone.
                    NotifyLotsOnline();
                }
                else
                {
                    failureReason = "Room is OFFLINE and you're NOT a roommate.";
                    successfulJoin = false;
                }
            }
            else if (isOnline) // redundant -- clarity only
            {   // room is ONLINE ... I am a visitor
                // ask host if I can join and ensure the lot is ONLINE

                //idk if this is valid here
                RespondWith(new TSOJoinRoomPDU(roomInfo.RoomID, "bloatytime3!"));
                //add this player to the room
                successfulJoin = ClientUpdateRoom_EnterRoom(HouseID, joiningClient, false, out failureReason);
            }

            //**join failed            
            if (!successfulJoin)
            {
                uint errorCode = 0;
                RespondWith(new TSOJoinRoomFailedPDU(10, "", roomIDStruct));
                LogConsole($"VoltronID: {joiningClient} FAILED to join HouseID: {HouseID}. Reason: {failureReason}");
                return;
            }

            // refresh the room value after the dust settles
            if (!_roomsByHouseID.TryGetValue(HouseID, out roomInfo) && roomInfo == null)
                throw new InvalidOperationException("Joining is not possible due to an unknown error"); // cannot continue if this is null            

            if (hosting)
            { // add the host to the lot
                if (!roomInfo.AdmitOccupant(GetPlayerInfoStruct(joiningAvatarID)))
                    throw new InvalidOperationException($"Could not add avatar {joiningAvatarID} to the new room.");
                return;
            }

            //**join succeeded
            //tell the client to join this new room
            //**set the room the client is in to be this new one
            RespondWith(new TSOUpdateRoomPDU(0xFFFFFFFF, roomInfo.RoomInfo, true));
            LogConsole($"Updated VoltronID: {joiningClient} to be in room: {roomInfo.RoomID}!");

            UpdateLotOccupants(roomInfo);
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.DESTROY_ROOM_PDU)]
        public void DESTROY_ROOM_PDU(TSOVoltronPacket PDU)
        {
            TSODestroyRoomPDU destroyRoomPDU = (TSODestroyRoomPDU)PDU;
            TSORoomIDStruct destroyingRoom = destroyRoomPDU.RoomID;

            //*disconnect from room first
            DETACH_FROM_ROOM_PDU(new TSODetachFromRoomPDU(destroyingRoom));
            
            RespondWith(new TSODestroyRoomResponsePDU(TSOStatusReasonStruct.Online, destroyingRoom));
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.DETACH_FROM_ROOM_PDU)]
        public void DETACH_FROM_ROOM_PDU(TSOVoltronPacket PDU)
        {
            TSODetachFromRoomPDU detachPDU = (TSODetachFromRoomPDU)PDU;
            TSORoomIDStruct leavingRoom = detachPDU.RoomID;

            //Get the avatar who is trying to leave and destroy the room behind them
            uint RoomID = (leavingRoom as ITSONumeralStringStruct)?.NumericID ?? 0;
            if (RoomID == 0)
            {   //TODO send detach from room failed
                //RespondWith();
                throw new InvalidDataException($"{nameof(DETACH_FROM_ROOM_PDU)}(): {nameof(RoomID)} is {RoomID}! Ignoring...");
            }
            //get room info
            var roomInfo = _roomsByHouseID[RoomID];
            //invoke the session service
            if (!GetService<nio2soClientSessionService>().GetVoltronClientByPDU(PDU, out TSOAriesIDStruct? VoltronID))
                throw new InvalidOperationException("Could not identify the client who sent this PDU.");

            ClientUpdateRoom_LeaveRoom(VoltronID, out string failReason, RoomID);  
            if (VoltronID.AvatarID != roomInfo.LeaderAvatarID)
                TrySendTo(roomInfo.LeaderID, new TSOOccupantDepartedPDU(GetPlayerInfoStruct(VoltronID)));
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.CHAT_MSG_PDU)]
        public void CHAT_MSG_PDU(TSOVoltronPacket PDU)
        {
            var msg = (TSOChatMessagePDU)PDU;            
            BroadcastToServer(msg);
            //RespondWith(new TSOChatMessageFailedPDU(msg.Message));
        }

        bool FORCE_ENTRY = false;
        /// <summary>
        /// INVOKES THE HSB MODE
        /// </summary>
        /// <param name="PDU"></param>
        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.LOAD_HOUSE_RESPONSE_PDU)]
        public void LOAD_HOUSE_RESPONSE_PDU(TSOVoltronPacket PDU)
        {
            /* From niotso:                  hello, fatbag - bisquick :]
             * TODO: It is known that sending HouseSimConstraintsResponsePDU (before the
            ** GetCharBlobByID response and other packets) is necessary for the game to post
            ** kMSGID_LoadHouse and progress HouseLoadRegulator from kStartedState to kLoadingState.
            ** However, the game appears to never send HouseSimConstraintsPDU to the server at
            ** any point.
            **
            ** The question is:
            ** Is there a packet that we can send to the game to have it send us
            ** HouseSimConstraintsPDU?
            ** Actually, (in New & Improved at least), manually sending a kMSGID_LoadHouse packet
            ** to the client (in an RSGZWrapperPDU) will cause the client to send
            ** HouseSimConstraintsPDU to the server.
            ** It is not known at this point if that is the "correct" thing to do.
            **
            ** So, for now, we will send the response packet to the game, without it having explicitly
            ** sent us a request packet--just like (in response to HostOnlinePDU) the game sends us a
            ** LoadHouseResponsePDU without us ever having sent it a LoadHousePDU. ;)
            */

            var houseID = ((TSOLoadHouseResponsePDU)PDU).HouseID;

            //The client will always send a LoadHouseResponsePDU with a blank houseID, so this can be ignored
            //when that happens
            if (houseID == 0)
            {
                uint hsbLot = TestingConstraints.HSBAutoJoinHouseID;
                if (hsbLot != 0 && !RoomIsOnline(hsbLot))
                    houseID = hsbLot;
            }
            if (houseID == 0) return;
            ((TSOLoadHouseResponsePDU)PDU).HouseID = houseID;
            
            FORCE_ENTRY = true;
            LOT_ENTRY_REQUEST_PDU(PDU);
            FORCE_ENTRY = false;
            return; 
            
            //**BASIC REWRITE OF ENTER_LOT TO REMOVE CERTAIN PDUS
            
            //FILE OFFSET 0x1856 is the value in the .data section of the application storing the client type:
            // 0x01 for HouseSimServer and 0x02 for TSOClient
            // 00469FE8 is the bit that controls hosting or not

            //im in a lot -- send it the lot im supposed to be in
            RespondWith(new TSOHouseSimConstraintsResponsePDU(houseID)); // dictate what lot to load here.
            LotProfile thisLot = GetLotProfile(houseID);
            if (thisLot == null)
                throw new NullReferenceException($"LotID: {houseID} was not found in the data service!");
            nio2soClientSessionService clientSessionService = GetService<nio2soClientSessionService>();
            if (!clientSessionService.GetVoltronClientByPDU(PDU, out TSOAriesIDStruct? joiningClient) || joiningClient == null)
                throw new InvalidOperationException("Cannot identify who sent this packet to Voltron.");
            //update these with the lot info
            var RoomName = thisLot.Name ?? "BloatyWorld";
            TSORoomIDStruct roomIDStruct = new(thisLot.HouseID, RoomName);
            //get create pdu with room details
            var createRoomInfo = new RoomProtocolRoomInfo(roomIDStruct, joiningClient, thisLot.HouseID);
            //add this room to the RoomProtocol Voltron Protocol
            AddRoom(createRoomInfo);
            ClientUpdateRoom_EnterRoom(houseID, joiningClient, true, out _);
            return;      
        }        
    }
}
