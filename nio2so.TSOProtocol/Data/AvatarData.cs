using nio2so.TSOProtocol.Packets.TSOXML.CitySelector;

namespace nio2so.TSOHTTPS.Protocol.Data
{
    /// <summary>
    /// Contains information on an avatar added to the TSOHTTPS API
    /// </summary>
    /// <param name="AvatarID"></param>
    /// <param name="AvatarName"></param>
    /// <param name="Simoleans"></param>
    /// <param name="Popularity"></param>
    /// <param name="ShardName"></param>
    internal record AvatarData(uint AvatarID, string AvatarName, uint Simoleans, byte Popularity, string ShardName)
    {
        public AvatarDataPacketStructure GetStructure() =>
            new AvatarDataPacketStructure(AvatarID, AvatarName, Simoleans, 0, Popularity, 0, ShardName);
    }
}
