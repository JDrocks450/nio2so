using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace nio2so.DataService.Common.Tokens
{
    public class AvatarIDTokenJsonConverter : JsonConverter<AvatarIDToken>
    {
        public override AvatarIDToken Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) => reader.GetUInt32();

        public override void Write(
            Utf8JsonWriter writer,
            AvatarIDToken token,
            JsonSerializerOptions options) =>
                writer.WriteNumberValue(token.AvatarID);
    }

    /// <summary>
    /// Represents the ID of an Avatar (named <see cref="UInt32"/> wrapper)
    /// </summary>
    /// <param name="AvatarID"></param>
    [JsonConverter(typeof(AvatarIDTokenJsonConverter))]
    public record struct AvatarIDToken(uint AvatarID)
    {
        public static implicit operator uint(AvatarIDToken ts)
        {
            return ts.AvatarID;
        }
        public static implicit operator AvatarIDToken(uint val)
        {
            return new AvatarIDToken(val);
        }
    }
}
