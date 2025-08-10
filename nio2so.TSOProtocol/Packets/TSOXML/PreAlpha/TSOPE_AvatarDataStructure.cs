using nio2so.Data.Common.Testing;
using System.Text.Json.Serialization;

namespace nio2so.TSOHTTPS.Protocol.Packets.TSOXML.PreAlpha
{
    /// <summary>
    /// <para/>For use with The Sims Online: Pre-Alpha only
    /// <inheritdoc cref="AvatarDataPacket{object}.IVersionedPacketStructure"/>
    /// </summary>
    /// <param name="AvatarID">The ID of the Avatar in Voltron</param>
    /// <param name="Name">The Avatar's name</param>
    /// <param name="Simoleans">How many simoleans they have</param>
    /// <param name="SimoleanDelta">How many simoleans they've made today(?)</param>
    /// <param name="Popularity">How many friends they have</param>
    /// <param name="PopularityDelta">How many friends they've made today(?)</param>
    /// <param name="ShardName">Which server shard they live on</param>
    public record TSOPE_AvatarDataStructure(uint AvatarID,
        string Name,
        long Simoleans,
        [property: JsonPropertyName("Simolean-Delta")] long SimoleanDelta,
        byte Popularity,
        [property: JsonPropertyName("Popularity-Delta")] byte PopularityDelta,
        [property: JsonPropertyName("Shard-Name")] string ShardName) : IVersionedPacketStructure
    {
        public static TSOPE_AvatarDataStructure Empty => new(0, "", 0, 0, 0, 0, "");
        public static TSOPE_AvatarDataStructure Default => new(TestingConstraints.MyAvatarID, TestingConstraints.MyAvatarName, TestingConstraints.StartingFunds, 5000, 5, 1, TestingConstraints.MyShardName);

        public IVersionedPacketStructure GetDefault() => Default;
    }
}
