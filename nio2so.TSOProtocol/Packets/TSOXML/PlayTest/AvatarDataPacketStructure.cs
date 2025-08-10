using nio2so.Data.Common.Testing;
using System.Text.Json.Serialization;

namespace nio2so.TSOHTTPS.Protocol.Packets.TSOXML.PlayTest
{
    /// <summary>
    /// <inheritdoc cref="AvatarDataPacket{object}.IVersionedPacketStructure"/>
    /// </summary>
    /// <param name="AvatarID">The ID of the Avatar in Voltron</param>
    /// <param name="Name">The Avatar's name</param>
    /// <param name="ShardName">Which server shard they live on</param>
    public record AvatarDataPacketStructure(uint AvatarID, string Name, [property: JsonPropertyName("Shard-Name")] string ShardName) : IVersionedPacketStructure
    {
        public IVersionedPacketStructure GetDefault() => new AvatarDataPacketStructure(TestingConstraints.MyAvatarID,TestingConstraints.MyAvatarName,TestingConstraints.MyShardName);
    }
}
