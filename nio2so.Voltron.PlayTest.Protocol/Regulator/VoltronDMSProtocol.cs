using nio2so.Voltron.Core.Services;
using nio2so.Voltron.Core.Telemetry;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Regulator;
using nio2so.Voltron.Core.TSO.Struct;
using nio2so.Voltron.PlayTest.Protocol.PDU;

namespace nio2so.Voltron.PlayTest.Protocol.Regulator
{
    [TSORegulator]
    /// <summary>
    /// This protocol handles lower-level functions of the Voltron Data Service such as 
    /// <see cref="TSOHostOnlinePDU"/>, <see cref="TSOClientBye"/>
    /// </summary>
    internal class VoltronDMSProtocol : TSOProtocol, IDMSProtocol
    {
        [TSOProtocolHandler((uint)TSO_PlayTest_VoltronPacketTypes.ClientByePDU)]
        public void BYE_PDU(TSOVoltronPacket PDU)
        {
            var bye_pdu = (TSOClientBye)PDU;
            LogConsole($"TSOClient is disconnecting. ReasonCode: {bye_pdu.ReasonCode} Text: {bye_pdu.ReasonText}." +
                $" Safely disposing of the connection after this frame...");
            RespondWith(new TSOClientBye(bye_pdu.ReasonCode));
            SafeDisconnect(bye_pdu);
        }
        /// <summary>
        /// A TSOClient is logging into Voltron
        /// </summary>
        /// <param name="PDU"></param>
        [TSOProtocolHandler((uint)TSO_PlayTest_VoltronPacketTypes.ClientOnlinePDU)]
        public void CLIENT_ONLINE_PDU(TSOVoltronPacket PDU)
        {
            if (GetService<nio2soClientSessionService>().GetVoltronClientByPDU(PDU, out TSOAriesIDStruct? VoltronID))
            {
                //PURGE PREVIOUS SESSION IF NOT CAUGHT BY ON_DISCONNECT
                string error = "not implemented";
                //GetRegulator<RoomProtocol>().AvatarPurgePlaySession(VoltronID, out string error);
                LogConsole($"AvatarPurgePreviousSession(): AvatarID: {VoltronID.AvatarID}:" + error);
                //SET ONLINE STATUS TO TRUE
                if(GetDataService().SetOnlineStatusByAvatarID(VoltronID.AvatarID, true).Result.IsSuccessStatusCode) 
                    LogConsole($"SetAvatarOnlineStatus(): AvatarID: {VoltronID.AvatarID} Value: {true}");
            }
            return;
            /*
            uint avatarID = TSOVoltronConst.MyAvatarID;
            string avatarName = TSOVoltronConst.MyAvatarName;
            RespondWith(new TSOUpdatePlayerPDU(new Struct.TSOAriesIDStruct(avatarID, avatarName)));*/
        }
        /// <summary>
        /// Logic to run when a TSOClient disconnects from Voltron
        /// </summary>
        public void ON_DISCONNECT(uint QuazarID)
        {
            nio2soClientSessionService clientSession = GetService<nio2soClientSessionService>();
            if(clientSession.RemoveClient(QuazarID, out TSOAriesIDStruct? VoltronID))
            {
                //SET ONLINE STATUS TO FALSE
                if (GetDataService().SetOnlineStatusByAvatarID(VoltronID.AvatarID, true).Result.IsSuccessStatusCode)
                    LogConsole($"SetAvatarOnlineStatus(): AvatarID: {VoltronID.AvatarID} Value: {false}");
                LogConsole($"{nameof(ON_DISCONNECT)}(): AvatarID: {VoltronID.AvatarID} is leaving Voltron... bye-bye!", nameof(ON_DISCONNECT), TSOLoggerServiceBase.LogSeverity.Warnings);
            }
            else LogConsole($"{nameof(ON_DISCONNECT)}(): QuaZarID: {QuazarID}(NO VOLTRON_ID!!) is leaving Voltron... cya!", nameof(ON_DISCONNECT), TSOLoggerServiceBase.LogSeverity.Errors);
        }

        public TSOVoltronPacket GET_UPDATE_PLAYER(uint AvatarID, out string AvatarName)
        {
            AvatarName = "default";
            return new TSOUpdatePlayerPDU(new TSOPlayerInfoStruct(new TSOAriesIDStruct(AvatarID,AvatarName)));
        }

        public TSOVoltronPacket GET_HOST_ONLINE(ushort ClientBufferLength, params string[] Badwords) => new TSOHostOnlinePDU(ClientBufferLength, Badwords);
    }
}
