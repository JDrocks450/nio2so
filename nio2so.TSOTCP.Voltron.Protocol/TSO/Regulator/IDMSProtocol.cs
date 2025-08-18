namespace nio2so.Voltron.Core.TSO.Regulator
{
    public interface IDMSProtocol
    {
        void ON_DISCONNECT(uint QuazarID);
        TSOVoltronPacket GET_UPDATE_PLAYER(uint AvatarID, out string AvatarName);
        TSOVoltronPacket GET_HOST_ONLINE(ushort ClientBufferLength, params string[] Badwords);
    }
}
