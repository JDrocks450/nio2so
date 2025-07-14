using System.Runtime.Serialization;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct
{
    /// <summary>
    /// A structure containing one string intended for a string containing numbers and a second string containing text
    /// <see cref="TSOAriesIDStruct"/>
    /// </summary>
    public interface ITSONumeralStringStruct
    {
        public const string DEFAULT_HEADER = "??";

        public string FormatSpecifier => DEFAULT_HEADER;
        public string IDString { get; set; }
        public string NameString { get; set; }

        /// <summary>
        /// <see langword="get"/>:
        /// <para/> <inheritdoc cref="FormatIDString(uint, string)"/>
        /// <para/><see langword="set"/>: <para/>
        /// <inheritdoc cref="TryParseAriesID(string, out uint)"/>
        /// </summary>
        [IgnoreDataMember] // DO NOT
        [TSOVoltronIgnorable] // SERIALIZE THIS
        public uint? NumericID
        {
            get
            {
                TryParseAriesID(IDString, out uint v);
                return v;
            }
            set
            {
                if (!value.HasValue) return;
                IDString = FormatIDString(value.Value, FormatSpecifier);
            }
        }

        /// <summary>
        /// Formats a string into the ID found in a <see cref="TSOAriesIDStruct"/>
        /// <para/>Example: <c>A 1338</c>        
        /// </summary>
        /// <param name="ID">Example: <c>A 1338</c>  </param>
        /// <param name="Header"></param>
        /// <returns></returns>
        public static string FormatIDString(uint ID, string Header = DEFAULT_HEADER) => Header + ID;
        /// <summary>
        /// Parses a <see cref="UInt32"/> matching this structure:
        /// <para/> <inheritdoc cref="FormatIDString(uint, string)"/>
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static uint ParseAriesID(string Input) => uint.Parse(getParseString(Input));
        private static string getParseString(string Input)
        {
            string parseString = "";
            for (int i = 0; i < Input.Length; i++)
            {
                char c = Input[i];
                if (!char.IsAsciiDigit(c)) continue;
                parseString += c;
            }
            return parseString;
        }
        /// <summary>
        /// Tries to: <inheritdoc cref="ParseAriesID(string)"/>
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static bool TryParseAriesID(string Input, out uint AriesID) => uint.TryParse(getParseString(Input), out AriesID);
    }
}
