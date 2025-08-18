using nio2so.Voltron.Core.TSO;
using System.Runtime.Serialization;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PreAlpha.Protocol.Struct
{
    /// <summary>
    /// A structure for a <see cref="uint"/> <see cref="StatusCode"/> and pascal <see cref="string"/> <paramref name="ReasonText"/>
    /// </summary>
    [Serializable] public sealed record TSOStatusReasonStruct
    {
        public enum PlayerStatuses : uint
        {
            Online = 0x0,
            Offline = 0x01,
            LetterOnly = 0x21,
        }

        /// <summary>
        /// Creates a new <see cref="TSOStatusReasonStruct"/>
        /// </summary>
        public TSOStatusReasonStruct()
        {
        }
        /// <summary>
        /// <inheritdoc cref="TSOStatusReasonStruct()"/>
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="reasonText"></param>
        public TSOStatusReasonStruct(uint statusCode, string reasonText)
        {
            StatusCode = statusCode;
            ReasonText = reasonText;
        }
        /// <summary>
        /// The status code of the transaction 
        /// </summary>
        public uint StatusCode { get; set; } = 0x0;
        /// <summary>
        /// The reason text for the <see cref="StatusCode"/> provided 
        /// </summary>
        [TSOVoltronString]
        public string ReasonText { get; set; } = "";

        [IgnoreDataMember]
        public static TSOStatusReasonStruct Online => new((uint)PlayerStatuses.Online, "");
        [IgnoreDataMember]
        public static TSOStatusReasonStruct Offline => new((uint)PlayerStatuses.Offline,PlayerStatuses.Offline.ToString());
        [IgnoreDataMember]
        public static TSOStatusReasonStruct? Default => new(TSOVoltronConst.ResponsePDU_DefaultStatusCode,
            TSOVoltronConst.ResponsePDU_DefaultReasonText);
    }
}
