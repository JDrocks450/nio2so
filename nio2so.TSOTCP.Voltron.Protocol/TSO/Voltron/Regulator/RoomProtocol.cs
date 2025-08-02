using nio2so.Data.Common.Testing;
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
            private HashSet<TSOPlayerInfoStruct> occupants = new();

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
            public IEnumerable<TSOPlayerInfoStruct> Occupants => occupants;
            public uint OccupantsCount => (uint)occupants.Count;
            public uint MaxOccupants { get; set; } = MAX_OCCUPANTS;
            public bool IsLocked { get; set; } = false;
            public IEnumerable<TSOAriesIDStruct> Admins => [new("??1337","bisquick")];

            public TSORoomInfoStruct RoomInfo => new TSORoomInfoStruct(RoomID, LeaderID, OccupantsCount, MaxOccupants, IsLocked, Admins.ToArray());

            public bool AvatarJoin(TSOPlayerInfoStruct Player) => occupants.Add(Player);
            public IEnumerable<TSOAriesIDStruct> GetConnectedClients() => [.. occupants.Select(x => x.PlayerID)];
            public void SetOccupants(params TSOPlayerInfoStruct[] Players) => occupants = [..Players];
        }

        private DateTime _reSyncTime = DateTime.MinValue;
        private ConcurrentDictionary<uint, RoomProtocolRoomInfo> _roomsByHouseID { get; } = new();
        private ConcurrentDictionary<uint, uint> _playersInRooms { get; } = new();

        private bool AddRoom(RoomProtocolRoomInfo Room)
        {
            bool result = false;
            if (Room.LeaderAvatarID == 0)
                throw new InvalidDataException($"LeaderID is invalid: {Room.LeaderAvatarID}");
            if (string.IsNullOrWhiteSpace(Room.LeaderID.MasterID)) // ensure this isn't blank for data integrity
                Room.LeaderID = GetVoltronIDStruct(Room.LeaderAvatarID);
            result = _roomsByHouseID.TryAdd(Room.LotID, Room);
            if (!result) result = MigrateHost(Room.LotID, Room.LeaderID);              
            return result;
        }

        private bool MigrateHost(uint HouseID, TSOAriesIDStruct NewLeaderID)
        {
            if (!_roomsByHouseID.ContainsKey(HouseID))
                throw new Exception("Cannot migrate host of a room that's closed.");
            _roomsByHouseID[HouseID].LeaderID = NewLeaderID;
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
            if (!TryGetService<nio2soVoltronDataServiceClient>(out var client))
                throw new NullReferenceException("nio2so data service client could not be found.");
            var lots = client.GetAllLotProfiles().Result.Lots;

            int i = 0;
            foreach (var houseID in lots.Select(x => x.HouseID)) // UPDATE LATER TO BE ONLINE LOTS
            { // add the room if its not already in the list
                if (_roomsByHouseID.ContainsKey(houseID)) continue;

                //download lot profile
                var lot = GetLotProfile(houseID);
                //add the room
                AddRoom(new RoomProtocolRoomInfo(
                    new TSORoomIDStruct(lot.PhoneNumber, lot.Name),
                    GetVoltronIDStruct(lot.OwnerAvatar), // change this later
                    houseID)
                );
                if (false && houseID == 1338)
                {
                    _roomsByHouseID[houseID].SetOccupants([GetPlayerInfoStruct(1337), GetPlayerInfoStruct(161)]);
                    _playersInRooms[1337] = 1338;
                    _playersInRooms[1337] = 161;
                }
            }
        }

        bool AvatarJoinRoom(uint HouseID, TSOAriesIDStruct JoinerVoltronID, bool Host)
        {
            TSOPlayerInfoStruct playerInfo = GetPlayerInfoStruct(JoinerVoltronID);
            uint avatarID = (playerInfo.PlayerID as ITSONumeralStringStruct)?.NumericID ?? 0;
            if (avatarID == 0)
                throw new InvalidDataException($"AvatarID passed was {avatarID} (invalid)");
            RoomProtocolRoomInfo roomInfo = _roomsByHouseID[HouseID];
            bool result = true;
            if (!Host) // the host should not get this packet received for themself, causes issues.
                TrySendTo(roomInfo.LeaderID, new TSOOccupantArrivedPDU(playerInfo));
            if (result)
                result = roomInfo.AvatarJoin(playerInfo);
            if (result)
            {
                if (!_playersInRooms.TryAdd(avatarID, HouseID))
                    _playersInRooms[avatarID] = HouseID;
            }
            return result;
        }        

        void BroadcastPDUToRoom(uint HouseID, TSOVoltronPacket PDU, bool ExcludeHost = false)
        {
            RoomProtocolRoomInfo roomInfo = _roomsByHouseID[HouseID];
            BroadcastPDUToRoom(HouseID, PDU, ExcludeHost ? [roomInfo.LeaderAvatarID] : Array.Empty<uint>());
        }
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
            return new TSOListOccupantsResponsePDU(new(profile.PhoneNumber, profile.Name));
        }
        /// <summary>
        /// Returns a <see cref="TSOUpdateOccupantsPDU"/> with the occupants of a given room.
        /// <para/>If the room given by <paramref name="HouseID"/> is offline, return false.
        /// </summary>
        /// <param name="HouseID"></param>
        /// <param name="OccupantsPDU"></param>
        /// <returns></returns>
        public bool UpdateOccupantsPDUByRoomID(uint HouseID, uint OmitAvatarID, out TSOUpdateOccupantsPDU? OccupantsPDU)
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
        private void NotifyLotOccupants()
        {
            foreach(var onlineLot in _roomsByHouseID)
                if (UpdateOccupantsPDUByRoomID(onlineLot.Key,0, out TSOUpdateOccupantsPDU? OccupantsPDU))
                    BroadcastToServer(OccupantsPDU);
        }

        protected override bool OnUnknownDataBlobPDU(ITSODataBlobPDU PDU)
        {
            OnStandardMessage(PDU);
            return true;
        }

        [TSOProtocolDatablobHandler(TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage)]
        public void OnStandardMessage(ITSODataBlobPDU PDU)
        {
            void Activate(uint avatarID) {
                if (!_playersInRooms.TryGetValue(avatarID, out uint HouseID))
                    throw new InvalidOperationException($"AvatarID {avatarID} is not in a room, yet is sending a broadcast PDU.");
                var roomInfo = _roomsByHouseID[HouseID];

                TSOListOccupantsResponsePDU occupantsPDU = GetOccupantsPDUByRoomID(roomInfo.LotID, out _);
                BroadcastPDUToRoom(roomInfo.LotID, occupantsPDU);

                //**tell the joiner AND host the new list of players currently in this room            
                if (UpdateOccupantsPDUByRoomID(roomInfo.LotID,0, out TSOUpdateOccupantsPDU? HostOccupantsPDU))                
                    BroadcastPDUToRoom(roomInfo.LotID, HostOccupantsPDU);
            }
            
            if (PDU is TSOTransmitDataBlobPacket transmitPDU)
            {
                TrySendTo(transmitPDU.DestinationSessionID, new TSOBroadcastDatablobPacket(transmitPDU));
                return;
            }
            if (PDU is TSOBroadcastDatablobPacket broadcastPDU)
            {                
                TSOAriesIDStruct sender = broadcastPDU.SenderInfo.PlayerID;
                uint avatarID = (sender as ITSONumeralStringStruct)?.NumericID ?? 0;
                if (avatarID == 0)
                    throw new InvalidDataException($"AvatarID sending a broadcast PDU is {avatarID}, ARIESID: {sender}");
                if (!_playersInRooms.TryGetValue(avatarID, out uint HouseID))
                    throw new InvalidOperationException($"AvatarID {avatarID} is not in a room, yet is sending a broadcast PDU.");
                var roomInfo = _roomsByHouseID[HouseID];
                if (avatarID != roomInfo.LeaderAvatarID)
                    BroadcastPDUToRoom(HouseID, broadcastPDU, avatarID);
                else 
                    BroadcastPDUToRoom(HouseID, broadcastPDU);

                if (broadcastPDU.DataBlobContentObject.TryGetByCLSID(TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage, out ITSODataBlobContentObject? obj))
                    if ((obj as TSOStandardMessageContent).kMSG == TSO_PreAlpha_MasterConstantsTable.kMSGID_HouseReceived)
                        Activate(avatarID);
            }
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
            var roomPDU = (TSOLotEntryRequestPDU)PDU;

            //identify client
            nio2soClientSessionService clientSessionService = GetService<nio2soClientSessionService>();
            if (!clientSessionService.GetVoltronClientByPDU(PDU, out TSOAriesIDStruct? joiningClient) || joiningClient == null)
                throw new InvalidOperationException("Cannot identify who sent this packet to Voltron.");

            //is the lot online?
            bool isOnline = _roomsByHouseID.TryGetValue(roomPDU.HouseID, out RoomProtocolRoomInfo? roomInfo) && roomInfo != null;

            //get the lot
            LotProfile thisLot = GetLotProfile(roomPDU.HouseID);
            if (thisLot == null)
                throw new NullReferenceException($"LotID: {roomPDU.HouseID} was not found in the data service!");

            //get the joining player's ID
            uint joiningAvatarID = (joiningClient as ITSONumeralStringStruct)?.NumericID ?? 0;
            if (joiningAvatarID == 0)
                throw new Exception("Joining client is not identified. AvatarID: " + joiningAvatarID);

            //get the phone number of the lot
            string phoneNumber = thisLot?.PhoneNumber ?? TestingConstraints.MyHousePhoneNumber;

            //Update which room they're in currently
            string LotName = thisLot?.Name ?? "BloatyLand";
            string RoomName = roomInfo?.RoomID.RoomName ?? LotName;
            TSORoomIDStruct roomIDStruct = roomInfo?.RoomID ?? new(phoneNumber, RoomName);

            bool successfulJoin = false;
            bool hosting = false;

            // check if i am a visitor
            if (thisLot.OwnerAvatar != joiningAvatarID)
            {   // ask host if I can join and ensure the lot is ONLINE
                if (isOnline)
                {
                    //idk if this is valid here
                    RespondWith(new TSOJoinRoomPDU(roomInfo.RoomID, "bloatytime3!"));
                    //get the host of the lot voltron ID
                    TSOAriesIDStruct hostID = roomInfo.LeaderID;
                    successfulJoin = AvatarJoinRoom(thisLot.HouseID, joiningClient, false);
                }
            }
            // check if i am the host
            else if (thisLot.OwnerAvatar == joiningAvatarID)
            { // Initiate the host protocol
                //ADD ROOM TO PROTOCOL
                var createRoomInfo = new RoomProtocolRoomInfo(roomIDStruct, joiningClient, thisLot.HouseID);
                AddRoom(createRoomInfo);
                successfulJoin = true;
                if (successfulJoin) // TELL THE CLIENT TO START THE HOST PROTOCOL   
                {
                    //RespondWith(new TSOCreateRoomResponsePDU(createRoomInfo.RoomID));
                    RespondWith(new TSOHouseSimConstraintsResponsePDU(roomPDU.HouseID)); // transition to lot view as Host
                    successfulJoin = AvatarJoinRoom(thisLot.HouseID, joiningClient, true); // join this avatar into the new room
                }
                //**list of online rooms (or data about the room) has changed ... tell everyone.
                NotifyLotsOnline();
                hosting = true;               
            }

            //**join failed            
            if (!successfulJoin)
            {
                uint errorCode = 0;
                RespondWith(new TSOJoinRoomFailedPDU(10, "", roomIDStruct));
                return;
            }

            // refresh the room value after the dust settles
            if (!_roomsByHouseID.TryGetValue(roomPDU.HouseID, out roomInfo) && roomInfo == null)
                throw new InvalidOperationException("Joining is not possible due to an unknown error"); // cannot continue if this is null
            //**tell the joiner AND host the new list of players currently in this room            
            if (UpdateOccupantsPDUByRoomID(roomInfo.LotID, joiningAvatarID, out TSOUpdateOccupantsPDU? HostOccupantsPDU))
            {
                RespondWith(HostOccupantsPDU);
                BroadcastPDUToRoom(roomInfo.LotID, HostOccupantsPDU, joiningAvatarID);
            }    

            //**join succeeded
            //tell the client to join this new room
            //**set the room the client is in to be this new one
            TSOUpdateRoomPDU updateRoomPDU = new TSOUpdateRoomPDU(0xFFFFFFFF, roomInfo.RoomInfo, true);
            RespondWith(updateRoomPDU);            

            TSOListOccupantsResponsePDU occupantsPDU = GetOccupantsPDUByRoomID(roomPDU.HouseID, out _);
            RespondWith(occupantsPDU);           
        }
        
        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.DESTROY_ROOM_PDU)]
        public void DESTROY_ROOM_PDU(TSOVoltronPacket PDU)
        {
            TSODestroyRoomPDU destroyRoomPDU = (TSODestroyRoomPDU)PDU;
            TSORoomIDStruct destroyingRoom = destroyRoomPDU.RoomInfo;

            //Get the avatar who is trying to leave and destroy the room behind them
            uint RoomID = ((ITSONumeralStringStruct)destroyRoomPDU.RoomInfo)?.NumericID ?? 0;
            if (RoomID == 0)
                throw new InvalidDataException($"{nameof(DESTROY_ROOM_PDU)}(): {nameof(RoomID)} is {RoomID}! Ignoring...");

            RespondWith(new TSOUpdateRoomPDU(134, TSORoomInfoStruct.NoRoom));
            RespondWith(new TSODestroyRoomResponsePDU(TSOStatusReasonStruct.Online, destroyingRoom));
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.CHAT_MSG_PDU)]
        public void CHAT_MSG_PDU(TSOVoltronPacket PDU)
        {
            var msg = (TSOChatMessagePDU)PDU;            
            BroadcastToServer(msg);
            //RespondWith(new TSOChatMessageFailedPDU(msg.Message));
        }        
    }
}
