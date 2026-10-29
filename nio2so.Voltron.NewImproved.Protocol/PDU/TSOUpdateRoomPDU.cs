using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Struct;
using nio2so.Voltron.NewImproved.Protocol;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU
{
    /// <summary>
    /// Sent to a remote party to update which room it is currently in
    /// </summary>
    [TSOVoltronPDU((uint)TSO_NewImproved_VoltronPacketTypes.UpdateRoomPDU)]
    [TSOVoltronDuplicate("PreAlpha")]
    public class TSOUpdateRoomPDU : TSOVoltronPacket
    {
        /// <summary>
        /// <inheritdoc cref="ReasonCode"/>
        /// <para/>This is from the niotso wiki and transcribed into an enum.
        /// <para/>http://wiki.niotso.org/Maxis_Protocol#UpdateRoomPDU
        /// </summary>
        public enum ReasonCodes : uint
        {
            Success = 1,

            /// <summary>
            /// Meaning: Disconnected From Property. You have been disconnected from the property. Click OK to return to Map View.
            /// </summary>
            Disconnected_2 = 2,

            /// <summary>
            /// <inheritdoc cref="ReasonCodes.Disconnected_2"/>
            /// </summary>
            Disconnected_3 = 3,

            /// <summary>
            /// Meaning: Disconnected From Property. You have been disconnected from the property for an unknown reason. Click OK to return to Map View.
            /// Notes: The only way to reach the default case is with m_reasonCode=7.
            /// </summary>
            DefaultUnknown = 7,

            /// <summary>
            /// Meaning: Disconnected From Property. You have been disconnected from the property by a sysop. Click OK to return to Map View.
            /// </summary>
            DisconnectedBySysop = 5,

            /// <summary>
            /// Meaning: Disconnected From Property. You have been ejected from the property by a sysop. Click OK to return to Map View.
            /// </summary>
            EjectedBySysop = 8,

            /// <summary>
            /// Meaning: Disconnected From Property. You have been disconnected from the property. Click OK to return to Map View.
            /// </summary>
            Disconnected_128 = 128,

            /// <summary>
            /// Meaning: Disconnected From Property. You have been kicked out of the property. Click OK to return to Map View.
            /// </summary>
            KickedOut = 129,

            /// <summary>
            /// Meaning: ??? <para/>
            /// <i>Notes: Interesting logic at TSCSimulatorClientD_base+0x5248. No dialog seems to be shown. The player is still kicked off the lot.</i>
            /// </summary>
            Unknown_130 = 130,

            /// <summary>
            /// Meaning: Rejoin Attempt Too Soon. The propertySim believes your Sim is still present in this property. Click OK to return to map view.
            /// </summary>
            RejoinAttemptTooSoon = 131,

            /// <summary>
            /// Meaning: Room Full. The PropertySim contains the maximum number of occupants. Click OK to return to Map View.
            /// </summary>
            RoomFull = 132,

            /// <summary>
            /// Meaning: No Roommates Home. None of the roommates are available to host this property. Please try again later. Thank you!
            /// </summary>
            NoRoommatesHome = 133,

            /// <summary>
            /// Meaning: Property Closed. The Property is being disassembled. Please, give us a second, OK? Maybe we can find you a spot in the back, by the kitchen..
            /// </summary>
            PropertyClosed = 134,

            /// <summary>
            /// Meaning: Property Closing. Whoops! All the roommates have left the property. Please try again later. Thank you!
            /// </summary>
            PropertyClosing = 135,

            /// <summary>
            /// Meaning: ???<para/>
            /// <i>Notes: Interesting logic at TSCSimulatorClientD_base+0x5841. No dialog seems to be shown. The player is still kicked off the lot.</i>             
            /// </summary>
            Unknown_136 = 136,

            /// <summary>
            /// Meaning: No dialog.<para/>
            /// <i>Notes: References id 0x1b (27) which doesn't exist in _223_hcstvisitorprotocol.cst, hence, the ShowDialog function call at TSCSimulatorClientD_base+0x580b fails and no dialog appears.</i>
            /// </summary>
            NoDialog_140 = 140,

            /// <summary>
            /// Meaning: Disconnected From Property. You tried to join a lot while you were in the middle of an action. Please wait a moment and then try to join the lot again. (Avatar Leasing Error)
            /// </summary>
            AvatarLeasingError = 141
        }
        public override ushort VoltronPacketType => (ushort)TSO_NewImproved_VoltronPacketTypes.UpdateRoomPDU;

        public TSOUpdateRoomPDU() : base() { }

        /// <summary>
        /// Creates a new <see cref="TSOUpdateRoomPDU"/> with the given information                   
        /// </summary>
        /// <param name="ReasonCode"></param>
        /// <param name="RoomInfo"></param>
        /// <param name="DataStartByte"></param>
        public TSOUpdateRoomPDU(TSORoomInfoStruct RoomInfo, bool OccupantListFollows = true, ReasonCodes ReasonCode = ReasonCodes.Success)
        {
            this.ReasonCode = ReasonCode;
            NewRoomInfo = RoomInfo;
            this.OccupantListFollows = OccupantListFollows;

            MakeBodyFromProperties();
        }

        /// <summary>
        /// Reason codes that dictate the functionality of this <see cref="TSOUpdateRoomPDU"/>
        /// </summary>
        public ReasonCodes ReasonCode { get; set; }
        /// <summary>
        /// Byte either 0 or 1.
        /// <para/>m_occupantListFollows
        /// </summary>
        public bool OccupantListFollows { get; set; } = false;
        /// <summary>
        /// The new <see cref="TSORoomInfoStruct"/> the client should update itself to a be a member of
        /// </summary>
        public TSORoomInfoStruct NewRoomInfo { get; set; }
#if true
        public uint Param1 { get; set; }
        public uint Param2 { get; set; }
        public uint Param3 { get; set; }
#endif
    }

    /// <summary>
    /// Sent to a remote party an empty <see cref="TSOUpdateRoomPDU"/> with just the reason code.
    /// </summary>
    [TSOVoltronPDU((uint)TSO_NewImproved_VoltronPacketTypes.UpdateRoomPDU)]
    public class TSOUpdateRoomFailedPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_NewImproved_VoltronPacketTypes.UpdateRoomPDU;

        public TSOUpdateRoomFailedPDU() : base() { }

        /// <summary>
        /// Creates a new <see cref="TSOUpdateRoomFailedPDU"/>                   
        /// </summary>
        /// <param name="ReasonCode"></param>
        /// <param name="RoomInfo"></param>
        /// <param name="DataStartByte"></param>
        public TSOUpdateRoomFailedPDU(TSOUpdateRoomPDU.ReasonCodes ReasonCode)
        {
            this.ReasonCode = ReasonCode;

            MakeBodyFromProperties();
        }

        /// <summary>
        /// Reason codes that dictate the functionality of this <see cref="TSOUpdateRoomPDU"/>
        /// </summary>
        public TSOUpdateRoomPDU.ReasonCodes ReasonCode { get; set; }
    }
}
