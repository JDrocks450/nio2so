using System.Text.Json.Serialization;

namespace nio2so.DataService.Common.Types.Avatar
{
    /// <summary>
    /// Structure for TSOHTTP(S) AvatarDataServlet. Contains basic profile information about an Avatar
    /// </summary>
    /// <param name="AvatarID">The ID of the Avatar in Voltron</param>
    /// <param name="Name">The Avatar's name</param>
    /// <param name="Simoleans">How many simoleans they have</param>
    /// <param name="SimoleanDelta">How many simoleans they've made today(?)</param>
    /// <param name="Popularity">How many friends they have</param>
    /// <param name="PopularityDelta">How many friends they've made today(?)</param>
    /// <param name="ShardName">Which server shard they live on</param>
    public record AvatarProfile(uint AvatarID, 
        string Name,
        long Simoleans,
        [property: JsonPropertyName("Simolean-Delta")] long SimoleanDelta, 
        byte Popularity, 
        [property: JsonPropertyName("Popularity-Delta")] byte PopularityDelta,         
        [property: JsonPropertyName("Shard-Name")] string ShardName)
    {
        public static AvatarProfile Empty => new(0, "", 0, 0, 0, 0, "");
    }
}
