using nio2so.Voltron.Core.Services;
using nio2so.Voltron.Core.TSO.Aries;

namespace nio2so.Voltron.Core.TSO.Regulator
{
    public interface IDMSProtocol
    {
        void ON_DISCONNECT(uint QuazarID);
        TSOVoltronPacket GET_UPDATE_PLAYER(uint AvatarID, out string AvatarName);
        TSOVoltronPacket GET_HOST_ONLINE(ushort ClientBufferLength, params string[] Badwords);
        /// <summary>
        /// This function is run when the <see cref="TSOVoltronBasicServer"/> receives the <see cref="TSOAriesClientSessionInfo"/> packet from the TSOClient
        /// <para/>This function is used to handle version specific logic for the TSOClient joining the Voltron server, AFTER the HostOnlinePDU has been sent to the client.
        /// <para/>For example, in the PreAlpha protocol, UpdatePlayerPDU is sent here, but later versions this is not the intended behavior.
        /// <para/>Note: This function is not responsible for adding the Client to the <see cref="nio2soClientSessionService"/> - the server already handles that before this function is called.
        /// </summary>
        bool ON_CLIENT_JOIN_VOLTRON(uint AvatarID, out IEnumerable<TSOVoltronPacket> SendPackets);
    }
}
