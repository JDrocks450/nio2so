using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace nio2so.DataService.Common.Tokens
{
    public class HouseIDTokenJsonConverter : JsonConverter<HouseIDToken>
    {
        public override HouseIDToken Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) => reader.GetUInt32();

        public override void Write(
            Utf8JsonWriter writer,
            HouseIDToken token,
            JsonSerializerOptions options) =>
                writer.WriteNumberValue(token.AvatarID);
    }

    /// <summary>
    /// Represents the ID of an Avatar (named <see cref="UInt32"/> wrapper)
    /// </summary>
    /// <param name="AvatarID"></param>
    [JsonConverter(typeof(HouseIDTokenJsonConverter))]
    public record struct HouseIDToken(uint AvatarID)
    {
        public static implicit operator uint(HouseIDToken ts)
        {
            return ts.AvatarID;
        }
        public static implicit operator HouseIDToken(uint val)
        {
            return new HouseIDToken(val);
        }
        public override string ToString()
        {
            return AvatarID.ToString();
        }
    }
}
