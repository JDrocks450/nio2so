using System.Text.Json;
using System.Text.Json.Serialization;

namespace nio2so.DataService.Common.Tokens
{
    public class UserTokenJsonConverter : JsonConverter<UserToken>
    {
        public override UserToken Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) => (UserToken)(reader.GetString() ?? UserToken.Error);

        public override void Write(
            Utf8JsonWriter writer,
            UserToken token,
            JsonSerializerOptions options) =>
                writer.WriteStringValue(token);
    }

    /// <summary>
    /// The <see cref="UserName"/> of the account and a unique <see cref="TokenValue"/> separated by a <see cref="SEPARATOR_CHAR"/>
    /// <code>bloaty#1234</code>
    /// </summary>
    /// <param name="UserName"></param>
    /// <param name="AvatarID"></param>
    [JsonConverter(typeof(UserTokenJsonConverter))]
    public record struct UserToken(string UserName, uint TokenValue)
    {
        public const char SEPARATOR_CHAR = '@';
        public static implicit operator string(UserToken ts) => ts.ToString();
        public static implicit operator UserToken(string val)
        {
            if (!val.Contains(SEPARATOR_CHAR))
                throw new InvalidDataException($"Format of UserToken {val} is incorrect. No {SEPARATOR_CHAR} found!");
            string[] components = val.Split(SEPARATOR_CHAR);
            if (components.Length > 2)
                throw new InvalidDataException($"Format of UserToken {val} is incorrect. Too many {SEPARATOR_CHAR} characters found!");
            return new UserToken(components[0], uint.Parse(components[1]));
        }

        public static UserToken Error => new UserToken();

        public bool Valid => !string.IsNullOrWhiteSpace(UserName) && TokenValue > 0;

        /// <summary>
        /// Creates a new <see cref="UserToken"/> instance
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public static UserToken Create(string UserName)
        {
            uint id = (uint)Random.Shared.Next(1, 10000); // 1-9999 inclusive
            return new(UserName, id);
        }

        public override string ToString()
        {
            return UserName + SEPARATOR_CHAR + TokenValue;
        }
    }
}
