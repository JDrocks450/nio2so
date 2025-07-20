using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace nio2so.DataService.Common.Tokens
{
    public class UserTokenJsonConverter : JsonConverter<UserToken>
    {
        public override UserToken Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) => reader.GetUInt32();

        public override void Write(
            Utf8JsonWriter writer,
            UserToken token,
            JsonSerializerOptions options) =>
                writer.WriteNumberValue(token.TokenValue);
    }

    [JsonConverter(typeof(UserTokenJsonConverter))]
    public record struct UserToken(uint TokenValue) : ITokenBase
    {
        public static implicit operator uint(UserToken ts)
        {
            return ts.TokenValue;
        }
        public static implicit operator UserToken(uint val)
        {
            return new UserToken(val);
        }
    }
}
