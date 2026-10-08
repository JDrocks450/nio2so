using nio2so.Data.Common.Testing;
using nio2so.DataService.Common.Queries;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types.Avatar;
using nio2so.DataService.Common.Types.Lot;
using nio2so.Voltron.Core.Services;
using nio2so.Voltron.Core.Telemetry;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Aries;
using nio2so.Voltron.Core.TSO.Regulator;
using nio2so.Voltron.Core.TSO.Struct;
using nio2so.Voltron.PreAlpha.Protocol.PDU;
using nio2so.Voltron.PreAlpha.Protocol.PDU.Datablob;
using nio2so.Voltron.PreAlpha.Protocol.PDU.Datablob.Structures;
using nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers;
using nio2so.Voltron.PreAlpha.Protocol.Regulator.Room;
using nio2so.Voltron.PreAlpha.Protocol.Struct;
using System.Collections.Concurrent;

namespace nio2so.Voltron.PreAlpha.Protocol.Regulator
{
    /// <summary>
    /// Manages the current online simulation rooms in Voltron
    /// </summary>
    [TSORegulator]
    internal class RoomProtocol : TSOProtocol
    {
        public const uint MAX_OCCUPANTS = 24;
        public const double RESYNC_TIMEOUT = 0;
        private readonly int HSB_AWAKE_TIMEOUT = 10000;
        private DateTime _reSyncTime = DateTime.MinValue;
        private RuntimeRoomController _roomController;

        public RoomProtocol()
        {
            _roomController = new(this);
        }

        #region NON_PDU_HANDLER

        /// <summary>
        /// Clears this player from any rooms they may be in from their play session (used when disconnecting/reconnecting)
        /// </summary>
        /// <param name="voltronID"></param>
        /// <exception cref="NotImplementedException"></exception>
        public bool AvatarPurgePlaySession(TSOAriesIDStruct? voltronID, out string FailureReason) => 
            ClientUpdateRoom_LeaveRoom(voltronID, out FailureReason);

        /// <summary>
        /// <inheritdoc cref="TSOProtocolBase.LogConsole(string, string, TSOLoggerServiceBase.LogSeverity)"/>
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="Caption"></param>
        /// <param name="Severity"></param>
        internal new void LogConsole(string Message) => base.LogConsole(Message);

        /// <summary>
        /// Creates a new <see cref="RoomProtocolRoomInfo"/> that is empty, uses the info from the given <paramref name="HouseID"/> and is lead by <paramref name="Leader"/>
        /// <para/><paramref name="Created"/> True if the room as just now created, false if it already was open. <b>Will migrate host to this passed one if already existed.</b>
        /// <para/>Rooms are tracked and maintained in the protocol, you can use <see cref="EnsureRoomExists(uint, TSOAriesIDStruct?, out bool)"/> and <see cref="RoomExists(uint)"/>
        /// to poll whether or not a room exists or not, without risk of accidental host migrations.
        /// </summary>
        /// <param name="HouseID">The ID of the house in the Database</param>
        /// <param name="Leader">If null, will throw an exception -- This will be ensured to be the new HouseLeader whereby <see cref="MigrateHost(uint, TSOAriesIDStruct)"/> is used</param>
        /// <param name="Created">True if prior to calling this function, this runtime room was not yet created in the protocol.</param>
        /// <returns></returns>
        private RoomProtocolRoomInfo CreateOrGetRoomWithLeader(uint HouseID, TSOAriesIDStruct Leader, out bool Created)
        {
            Created = false;
            //download lot profile
            var lot = GetLotProfile(HouseID);
            if (Leader == null)
                throw new InvalidOperationException("Leader cannot be null when using " + nameof(CreateOrGetRoomWithLeader));
            //add the room
            Created = AddRoom(new RoomProtocolRoomInfo(new TSORoomIDStruct(lot.HouseID, lot.Name), Leader, HouseID));            
            return _roomController.GetByHouseID(HouseID);
        }

        /// <summary>
        /// If the room exists, returns it without changing the host of the room. If it doesn't exist, it will create the room with the given <paramref name="Leader"/>        
        /// <para/>
        /// </summary>
        /// <param name="HouseID">The ID of the house in the database</param>
        /// <param name="Leader">If null, will use the Lot's Owner ID -- use with caution!</param>
        /// <param name="Created"></param>
        /// <returns></returns>
        private RoomProtocolRoomInfo EnsureRoomExists(uint HouseID, TSOAriesIDStruct? Leader, out bool Created)
        {
            Created = false;
            if (_roomController.TryGetByHouseID(HouseID, out RoomProtocolRoomInfo? Room))
                return Room;

            //download lot profile
            var lot = GetLotProfile(HouseID);
            if (Leader == null)
                Leader = GetVoltronIDStruct(lot.OwnerAvatar);
            return CreateOrGetRoomWithLeader(HouseID, Leader, out Created);
        }

        private bool AddRoom(RoomProtocolRoomInfo Room) => _roomController.AddRoom(Room);

        /// <summary>
        /// Tells all connected clients that they're disconnected and closes the room
        /// </summary>
        /// <param name="Room"></param>
        private void TakeRoomOffline(RoomProtocolRoomInfo Room)
        {
            LogConsole($"Room is going offline: RoomID: {Room.RoomID}\n {Room}");

            List<TSOAriesIDStruct> purgeList = new(Room.GetConnectedClients().ToArray());
            foreach (var client in purgeList)
            {
                if (client.AvatarID == Room.LeaderID.AvatarID) continue;
                ClientUpdateRoom_LeaveRoom(client, out string reason);
            }
            Room.ClientLeaveRoom(Room.LeaderID);
        }        

        /// <summary>
        /// <inheritdoc cref="RuntimeRoomController.RoomExists(uint)"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        private bool RoomExists(uint HouseID) => _roomController.RoomExists(HouseID);

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
            if (!TryDataServiceQuery(x => x.GetAllLotProfiles(), out N2GetLotListQueryResult? result, out string error))
                throw new InvalidDataException(error);          

            int i = 0;
            foreach (var houseID in result.Lots.Select(x => x.HouseID)) // UPDATE LATER TO BE ONLINE LOTS
            { // add the room if its not already in the list
                EnsureRoomExists(houseID, null, out _); // passing NULL here will set the LotLeader to the Owner for now, until a roommate or the Owner opens the lot.
                // in that case, the host is migrated
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
            RoomProtocolRoomInfo roomInfo = _roomController.GetByHouseID(HouseID);

            // send the occupant arrived pdu to the host
            bool result = true;
            JoinFailureReason = "Couldn't communicate with the host using Voltron. (Did they disconnect?)";
            if (!HSB)
            { // the host should not get this packet received for themself, causes issues.
                result = true; 
                BroadcastPDUToRoom(HouseID, new TSOOccupantArrivedPDU(playerInfo));
            }
            if (!result) return false;

            // ensure the client isn't already in the room
            JoinFailureReason = "Your account is already in this room.";
            result = roomInfo.ClientJoinRoom(JoinerVoltronID);
            if (!result) return false;

            // ensure we can set this avatar into the room in the protocol -- shouldn't fail
            JoinFailureReason = "Mapping which room your avatar is in failed.";
            _roomController.SetPlayerInRoom(avatarID, HouseID);
            JoinFailureReason = "success.";
            LogConsole($"VoltronID: {JoinerVoltronID} TSOClient has connected to HouseID: {HouseID} -- {(HSB ? "as a host" : "as a visitor")}.");
            return result;
        }

        /// <summary>
        /// Cleanly removes a <paramref name="VoltronID"/> from the room they're in
        /// </summary>
        /// <param name="VoltronID"></param>
        /// <param name="FailureReason"></param>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        bool ClientUpdateRoom_LeaveRoom(TSOAriesIDStruct VoltronID, out string FailureReason, uint HouseID = 0)
        {
            uint avatarID = VoltronID.AvatarID;
            FailureReason = $"{VoltronID} is not valid.";
            if (avatarID == 0)
                return false;
            FailureReason = $"AvatarID: {avatarID} isn't in a room and one wasn't provided to remove them from.";
            _roomController.TryGetRoomAvatarIsIn(avatarID, out uint currentRoom);
            if (HouseID == 0)
                HouseID = currentRoom;
            else if (HouseID != currentRoom)
            {
                FailureReason = $"AvatarID: {avatarID} is actually in room: {currentRoom} but is supposed to be leaving: {HouseID}. Not matching! No action taken.";
                return false;
            }
            if (HouseID == 0)
                return false;

            _roomController.RemoveAvatarFromRoom(avatarID, out HouseID);
            var roomInfo = _roomController.GetByHouseID(HouseID);
            bool result = roomInfo.ClientLeaveRoom(VoltronID);

            //**update client to not be in a room
            TrySendTo(VoltronID, new TSOUpdateRoomPDU(134, TSORoomInfoStruct.NoRoom));

            //Check if this client is the host of the lot
            if (roomInfo.LeaderAvatarID == VoltronID.AvatarID) // Leader of the lot (host)
                TakeRoomOffline(roomInfo);
            else
            {
                TrySendTo(VoltronID, new TSOOccupantDepartedPDU(GetPlayerInfoStruct(roomInfo.LeaderAvatarID))); // testing
                TrySendTo(roomInfo.LeaderID, new TSOOccupantDepartedPDU(GetPlayerInfoStruct(VoltronID)));
            }

            //**notify all connected clients the updated avatar list
            UpdateLotOccupants(roomInfo);
            //**notify mapview that the playercount has changed
            NotifyLotsOnline();

            LogConsole($"AvatarID: {avatarID} was removed from room HouseID: {HouseID}.");
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
            try
            {
                //refer to the map of clients to rooms to find which room I'm supposed to be in
                if (!_roomController.TryGetRoomAvatarIsIn(AvatarID, out uint HouseID))
                    throw new InvalidOperationException($"AvatarID {AvatarID} is not in a room, yet is sending a broadcast PDU.");
                var roomInfo = _roomController.GetByHouseID(HouseID);                            

                //add the avatar to the occupants list
                roomInfo.AdmitOccupant(GetPlayerInfoStruct(AvatarID));

                //**notify all connected clients the updated avatar list
                UpdateLotOccupants(roomInfo);

                LogConsole($"AvatarID: {AvatarID} is now an occupant of: {HouseID}");
                return true;
            }
            catch (Exception e)
            {
                LogError(e);
            }
            return false;
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
            RoomProtocolRoomInfo roomInfo = _roomController.GetByHouseID(HouseID);
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
            RoomProtocolRoomInfo roomInfo = _roomController.GetByHouseID(HouseID);
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
            _roomController.TryGetRoomAvatarIsIn(AvatarID, out uint HouseID);
            _roomController.TryGetByHouseID(HouseID, out RoomProtocolRoomInfo? roomInfo);
            return roomInfo?.RoomInfo ?? TSORoomInfoStruct.NoRoom;
        }
        /// <summary>
        /// <inheritdoc cref="RoomProtocolRoomInfo.IsOnline"/>
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        public bool RoomIsOnline(uint HouseID) => _roomController.TryGetByHouseID(HouseID, out RoomProtocolRoomInfo? RoomInfo) && RoomInfo.IsOnline;

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
            if (_roomController.TryGetByHouseID(HouseID, out RoomProtocolRoomInfo? roomInstance))
            { // online room
                IsOnline =  true;
                return new TSOListOccupantsResponsePDU(roomInstance.RoomID, [..roomInstance.Occupants]); // respond with occupants
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
            if (_roomController.TryGetByHouseID(HouseID, out RoomProtocolRoomInfo? roomInstance))
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
        private void NotifyLotsOnline() => 
            BroadcastToServer(new TSOListRoomsResponsePDU(_roomController.GetAllRoomStructs(true)));

        protected override bool OnUnknownDataBlobPDU(ITSODataBlobPDU PDU)
        {
            OnStandardMessage(PDU);
            return true;
        }        

        private void AvatarEnteringLot(TSOAriesIDStruct JoiningClient, uint HouseID)
        {
            var joiningClient = JoiningClient;

            //is the lot online?
            bool isOnline = false;
            if (_roomController.TryGetByHouseID(HouseID, out RoomProtocolRoomInfo? roomInfo) && roomInfo != null)
                isOnline = roomInfo.IsOnline; // is only online if there is an active room entry

            //get the joining player's ID
            uint joiningAvatarID = (joiningClient as ITSONumeralStringStruct)?.NumericID ?? 0;
            if (joiningAvatarID == 0)
                throw new Exception("Joining client is not identified. AvatarID: " + joiningAvatarID);

            /// <summary>
            /// Set when a HSB is opening the room and hosting it.
            /// </summary>
            bool IsHSB = joiningAvatarID >= Server.VoltronSettings.PreAlpha_HSBAvatarID;           

            //get the roomname only if the lot is currently ONLINE
            string? RoomName = roomInfo?.RoomID?.RoomName;
            TSORoomIDStruct? roomIDStruct = roomInfo?.RoomID;

            bool successfulJoin = false;
            bool hosting = false;
            string failureReason = "none set.";

            LogConsole($"AvatarID: {joiningAvatarID} (HSB?:{IsHSB}) is joining house: {HouseID} ({(isOnline ? "Online" : "Offline")})");

            //is this lot online?
            if (!isOnline)
            {   // room is OFFLINE ... try to host a new lobby using an available HSB or set me as a delayed joiner
                // remember that when a Client selects an offline lot as an owner or roommate they need to join the HSB that comes online to host their lot

                if (IsHSB)
                { // ah, our host has arrived. Do the host join process.
                    successfulJoin = HSB_BeginHosting(HouseID, joiningClient, out failureReason, ref RoomName, ref roomIDStruct);
                    hosting = true;
                }
                else
                { // the lot owner is here, but their room isn't ready yet! set them into the joiner queue.
                    if (!_roomController.AddJoinerToQueue(joiningAvatarID, HouseID, out failureReason))
                        throw new InvalidOperationException("A lot owner waiting for an HSB to host their lot failed: " + failureReason);
                    return;
                }
            }
            else if (isOnline) // redundant -- clarity only
            {   // room is ONLINE ... I am a visitor
                // ask host if I can join and ensure the lot is ONLINE

                if (IsHSB)
                    throw new InvalidOperationException($"An HSB Client (ID: {joiningAvatarID}) is trying to join an already online lot! Ignoring...");
                
                //idk if this is valid here
                TrySendTo(JoiningClient, new TSOJoinRoomPDU(roomInfo.RoomID, "bloatytime3!"));
                //add this player to the room
                successfulJoin = ClientUpdateRoom_EnterRoom(HouseID, joiningClient, false, out failureReason);
            }

            //**join failed            
            if (!successfulJoin)
            {
                ClientJoinHouseFailed(10,failureReason,joiningClient,roomIDStruct);
                return;
            }

            //**join succeeded
            //tell the client to join this new room
            //**set the room the client is in to be this new one
            ClientJoinHouse(HouseID, joiningClient, hosting);
        }

        private void ClientJoinHouseFailed(uint errorCode, string FailureReason, TSOAriesIDStruct JoiningClient, TSORoomIDStruct RoomIDStruct)
        {
            TrySendTo(JoiningClient, new TSOJoinRoomFailedPDU(errorCode, FailureReason, RoomIDStruct));
            LogConsole($"VoltronID: {JoiningClient} FAILED to join HouseID: {RoomIDStruct?.HouseID}. Reason: {FailureReason}");
        }
        
        private void ClientJoinHouse(uint HouseID, TSOAriesIDStruct joiningClient, bool hosting = false)
        {
            uint joiningAvatarID = joiningClient.AvatarID;

            // refresh the room value after the dust settles
            if (!_roomController.TryGetByHouseID(HouseID, out RoomProtocolRoomInfo? roomInfo) && roomInfo == null)
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
            TrySendTo(joiningClient, new TSOUpdateRoomPDU(0xFFFFFFFF, roomInfo.RoomInfo, true));
            LogConsole($"Updated VoltronID: {joiningClient} to be in room: {roomInfo.RoomID}!");

            UpdateLotOccupants(roomInfo);
        }

        /// <summary>
        /// Procedure for setting up an HSB TSOClient (-hsb_mode) to host the lot
        /// </summary>
        /// <param name="HouseID"></param>
        /// <param name="HSBID"></param>
        /// <param name="failureReason"></param>
        /// <param name="RoomName"></param>
        /// <param name="roomIDStruct"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        private bool HSB_BeginHosting(uint HouseID, TSOAriesIDStruct HSBID, out string failureReason, ref string RoomName, ref TSORoomIDStruct roomIDStruct)
        {
            // get the lot profile first  
            LotProfile thisLot = GetLotProfile(HouseID);
            if (thisLot == null)
                throw new NullReferenceException($"LotID: {HouseID} was not found in the data service!");

            //update these with the actual lot info
            RoomName = thisLot.Name ?? "BloatyWorld";
            roomIDStruct = new(thisLot.HouseID, RoomName);

            //download the roommates of this house
            if (!TryDataServiceQuery(x => x.GetRoommatesByHouseID(HouseID), out IEnumerable<AvatarIDToken>? lotRoommates, out string error))
                throw new InvalidDataException(error);

            uint joiningAvatarID = HSBID.AvatarID;
            bool IsHSB = joiningAvatarID >= Server.VoltronSettings.PreAlpha_HSBAvatarID; // double verification
            failureReason = "success.";
            bool successful = false;

            // check if this is an HSB client
            if (IsHSB)
            {   // Initiate the host protocol
                // ADD ROOM TO PROTOCOL (CreateRoom Now)

                //get create pdu with room details
                //add this room to the RoomProtocol Voltron Protocol
                var createRoomInfo = CreateOrGetRoomWithLeader(HouseID, HSBID, out bool Created);

                successful = true;
                if (successful) // TELL THE CLIENT TO START THE HOST PROTOCOL   
                {
                    TrySendTo(HSBID, new TSOCreateRoomResponsePDU(createRoomInfo.RoomID));
                    TrySendTo(HSBID, new TSOHouseSimConstraintsResponsePDU(HouseID)); // transition to lot view as Host
                    successful = ClientUpdateRoom_EnterRoom(thisLot.HouseID, HSBID, true, out failureReason); // join this avatar into the new room as an HSB
                }
                //**list of online rooms (or data about the room) has changed ... tell everyone.
                NotifyLotsOnline();
            }
            else
            {
                failureReason = $"Room is OFFLINE and this client ({HSBID}) is NOT an HSB client (Server Setting: AvatarID: {Server.VoltronSettings.PreAlpha_HSBAvatarID}).";
                successful = false;
            }
            return successful;
        }              

        /// <summary>
        /// Gets the HouseID that this newly allocated HSB Client should be hosting
        /// </summary>
        /// <param name="PDU">A PDU to use to reverse-lookup this client for its AvatarID</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        uint HSB_GetNextHouseIDToHost(TSOVoltronPacket PDU)
        {
            // HSB clients will ask for a house they should be hosting, now we will provide it.
            uint houseID = 0;

            //identify which client is sending this by AvatarID
            if (GetService<nio2soClientSessionService>().GetVoltronClientByPDU(PDU, out TSOAriesIDStruct? VoltronID))
            { // identified -- are they an HSB avatar?
                if (VoltronID.AvatarID != Server.VoltronSettings.PreAlpha_HSBAvatarID) return 0; // they are not an HSB avatar, they cannot host

                if (!_roomController.GetNextHouseIDForHSB(VoltronID, out houseID))
                    return 0;

                if (RoomIsOnline(houseID))
                    ;// throw new InvalidDataException(failure);                    
            }
            else throw new Exception("Could not identify what Client sent this PDU."); // not good if this happens, indicates a Login to Voltron issue. Client needs to be initialized using Aries ClientSessionInfo
            return houseID;
        }        

        /// <summary>
        /// Procedure for when the HSB comes online and the lot is ready to accept visitors
        /// </summary>
        /// <param name="HouseID"></param>
        /// <param name="JoiningAvatarIDs"></param>
        void HSB_LotIsOnline(uint HouseID, Queue<uint> JoiningAvatarIDs)
        {         
            //**hsb is online, let all the joiners into the room
            // i dont think that the game can support multiple joiners at once... HOWEVER, the game has the necessary infrastructure to handle this failure on its own.

            var roomInfo = _roomController.GetByHouseID(HouseID);
            var roomIDStruct = roomInfo.RoomID;

            foreach (var AvatarID in JoiningAvatarIDs)
            { // let each joiner in, one by one
                var joiningClient = GetVoltronIDStruct(AvatarID);                
                uint joiningAvatarID = joiningClient.AvatarID;

                string failureReason = "";

                TrySendTo(joiningAvatarID, new TSOJoinRoomPDU(roomInfo.RoomID, "bloatytime3!"));
                //add this player to the room
                bool successfulJoin = ClientUpdateRoom_EnterRoom(HouseID, joiningClient, false, out failureReason);

                //**join failed            
                if (!successfulJoin)
                {
                    ClientJoinHouseFailed(10, failureReason, joiningClient, roomIDStruct);
                    return;
                }

                ClientJoinHouse(HouseID, joiningClient, false);
            }
        }
        #endregion

        #region PDU_HANDLER
        [TSOProtocolDatablobHandler((uint)TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage)]
        public void OnStandardMessage(ITSODataBlobPDU PDU)
        {
            TSOBroadcastDatablobPacket? broadcastPDU = PDU as TSOBroadcastDatablobPacket;
            TSOAriesIDStruct sender = PDU.SenderSessionID.PlayerID;
            uint avatarID = (sender as ITSONumeralStringStruct)?.NumericID ?? 0;
            if (avatarID == 0)
                throw new InvalidDataException($"AvatarID sending a broadcast PDU is {avatarID}, ARIESID: {sender}");
            if (!_roomController.TryGetRoomAvatarIsIn(avatarID, out uint HouseID))
                throw new InvalidOperationException($"AvatarID {avatarID} is not in a room, yet is sending a broadcast PDU.");
            var roomInfo = _roomController.GetByHouseID(HouseID);

            if (PDU is TSOTransmitDataBlobPacket transmitPDU)
            {
                broadcastPDU = new TSOBroadcastDatablobPacket(transmitPDU);
                TrySendTo(transmitPDU.DestinationSessionID, broadcastPDU);
            }
            if (PDU is TSOBroadcastDatablobPacket && broadcastPDU != null)
            {
                if (avatarID != roomInfo.LeaderAvatarID)
                {
                    broadcastPDU.SenderSessionID = GetPlayerInfoStruct(roomInfo.LeaderID);
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
        /// <para/>This seems to disable the 30 second house timeout when it matches the joining avatar ID
        /// </summary>
        /// <param name="PDU"></param>
        /// <exception cref="NullReferenceException"></exception>
        [TSOProtocolDatabaseHandler((uint)TSO_PreAlpha_DBActionCLSIDs.GetHouseLeaderByLotID_Request)]
        public void GetHouseLeaderByLotID_Request(TSODBRequestWrapper PDU)
        {
            uint HouseID = ((TSOGetHouseLeaderByIDRequest)PDU).HouseID;
            if (_roomController.TryGetByHouseID(HouseID, out RoomProtocolRoomInfo? room))
                RespondTo(PDU, new TSOGetHouseLeaderByIDResponse(HouseID, room.LeaderAvatarID));
        }

        [TSOProtocolHandler((uint)TSO_PreAlpha_VoltronPacketTypes.LIST_ROOMS_PDU)]
        public void LIST_ROOMS_PDU(TSOVoltronPacket PDU)
        {
            SyncRoomsWithLotDataService();
            NotifyLotsOnline();
            return;

            //test message pdu ... doesn't work
            RespondWith(new TSOAnnouncementMsgPDU(new TSOPlayerInfoStruct(new TSOAriesIDStruct(161, "FriendlyBuddy")), "Testing"));
        }

        [TSOProtocolHandler((uint)TSO_PreAlpha_VoltronPacketTypes.LOT_ENTRY_REQUEST_PDU)]
        public void LOT_ENTRY_REQUEST_PDU(TSOVoltronPacket PDU)
        {
            dynamic roomPDU = PDU;

            void OnError(string ErrorMessage)
            {
                RespondWith(new TSOJoinRoomFailedPDU(10, ErrorMessage, TSORoomIDStruct.Error));
                throw new InvalidOperationException(ErrorMessage);
            }

            //identify client
            nio2soClientSessionService clientSessionService = GetService<nio2soClientSessionService>();
            if (!clientSessionService.GetVoltronClientByPDU(PDU, out TSOAriesIDStruct? joiningClient) || joiningClient == null)
            {
                OnError("Cannot identify who sent this packet to Voltron.");
                return;
            }

            if (Server.VoltronSettings.PreAlpha_HSBEnabled) // Attempt to wake up the HSB
            {
                if (joiningClient.AvatarID < Server.VoltronSettings.PreAlpha_HSBAvatarID)
                    BroadcastToServer(new TSORoomServerInitializedPDU());
            }

            AvatarEnteringLot(joiningClient, roomPDU.HouseID);
        }
        [TSOProtocolHandler((uint)TSO_PreAlpha_VoltronPacketTypes.DESTROY_ROOM_PDU)]
        public void DESTROY_ROOM_PDU(TSOVoltronPacket PDU)
        {
            TSODestroyRoomPDU destroyRoomPDU = (TSODestroyRoomPDU)PDU;
            TSORoomIDStruct destroyingRoom = destroyRoomPDU.RoomID;

            //*disconnect from room first
            DETACH_FROM_ROOM_PDU(new TSODetachFromRoomPDU(destroyingRoom));

            RespondWith(new TSODestroyRoomResponsePDU(TSOStatusReasonStruct.Online, destroyingRoom));

            //get room info
            var roomInfo = _roomController.GetByHouseID(((ITSONumeralStringStruct)destroyingRoom).NumericID.Value);
            TakeRoomOffline(roomInfo);
        }

        [TSOProtocolHandler((uint)TSO_PreAlpha_VoltronPacketTypes.DETACH_FROM_ROOM_PDU)]
        public void DETACH_FROM_ROOM_PDU(TSOVoltronPacket PDU)
        {
            TSODetachFromRoomPDU detachPDU = (TSODetachFromRoomPDU)PDU;
            TSORoomIDStruct leavingRoom = detachPDU.RoomID;

            //Get the avatar who is trying to leave and destroy the room behind them
            uint HouseID = (leavingRoom as ITSONumeralStringStruct)?.NumericID ?? 0;
            if (HouseID == 0)
            {   //TODO send detach from room failed
                //RespondWith();
                throw new InvalidDataException($"{nameof(DETACH_FROM_ROOM_PDU)}(): {nameof(HouseID)} is {HouseID}! Ignoring...");
            }

            //get room info
            var roomInfo = _roomController.GetByHouseID(HouseID);

            //invoke the session service
            if (!GetService<nio2soClientSessionService>().GetVoltronClientByPDU(PDU, out TSOAriesIDStruct? VoltronID))
                throw new InvalidOperationException("Could not identify the client who sent this PDU.");

            ClientUpdateRoom_LeaveRoom(VoltronID, out string failReason, HouseID);            

            RespondWith(detachPDU);
        }
        [TSOProtocolHandler((uint)TSO_PreAlpha_VoltronPacketTypes.CHAT_MSG_PDU)]
        public void CHAT_MSG_PDU(TSOVoltronPacket PDU)
        {
            var msg = (TSOChatMessagePDU)PDU;
            BroadcastToServer(msg);
            //RespondWith(new TSOChatMessageFailedPDU(msg.Message));
        }
        /// <summary>
        /// INVOKES THE HSB MODE
        /// </summary>
        /// <param name="PDU"></param>
        [TSOProtocolHandler((uint)TSO_PreAlpha_VoltronPacketTypes.LOAD_HOUSE_RESPONSE_PDU)]
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

            // FILE OFFSET 0x1856 is the value in the .data section of the application (TSOClient.exe) storing the client type:
            // 0x01 for HouseSimServer and 0x02 for TSOClient
            // 00469FE8 is the bit that controls hosting or not

            //read the houseID to make sure its 0 -- if not, how did that happen?
            var houseID = ((TSOLoadHouseResponsePDU)PDU).HouseID;

            if (houseID == 0 && Server.VoltronSettings.PreAlpha_HSBEnabled)
            { // HSB hosting mode, redirect to hosting a lot ... host the lot that we need hosted by an HSB
                houseID = HSB_GetNextHouseIDToHost(PDU);
                //**if 0, there are no more houses that need hosted at this time. this HSB will safely close after a timeout on its own.
            }

            if (houseID == 0) return;
            ((TSOLoadHouseResponsePDU)PDU).HouseID = houseID;

            // fib a LotEntry Request PDU to Voltron for this client
            LOT_ENTRY_REQUEST_PDU(PDU);
        }

        /// <summary>
        /// Sent by the HSB Mode TSOClient when it has completed loading the house.
        /// </summary>
        /// <param name="PDU"></param>
        [TSOProtocolHandler((uint)TSO_PreAlpha_VoltronPacketTypes.LOAD_HOUSE_PDU)]
        public void LOAD_HOUSE_PDU(TSOVoltronPacket PDU)
        {
            var loadHousePDU = (TSOLoadHousePDU)PDU;

            //identify which client is sending this by AvatarID
            if (!GetService<nio2soClientSessionService>().GetVoltronClientByPDU(PDU, out TSOAriesIDStruct? VoltronID))
                throw new InvalidOperationException("Client could not be identified when the HSB is alerting us it is ready to accept visitors.");

            // identified -- wait some time then let the pending visitors join in.

            //get host info
            bool success = _roomController.PopHouseHostingInfoForHSB(VoltronID, out (uint HouseID, Queue<uint> JoiningQueue)? info, out string failureReason);
            if (!success)
                throw new InvalidOperationException("When HSB was alerting us the lot is ready, a failure happened: " + failureReason);

            //wait a bit
            Thread.Sleep(HSB_AWAKE_TIMEOUT);
            //run hsb joiner process 
            HSB_LotIsOnline(info.Value.HouseID, info.Value.JoiningQueue);
        }
        #endregion
    }
}
