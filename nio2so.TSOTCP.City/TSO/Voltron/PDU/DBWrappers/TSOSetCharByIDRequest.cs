using nio2so.Data.Common.Serialization.Voltron;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.SetCharByID_Request)]
    internal class TSOSetCharByIDRequest : TSODBRequestWrapper
    {        
        [TSOVoltronDBWrapperField]
        public uint AvatarID { get; set; }
        [TSOVoltronDBWrapperField]
        public uint StatusCode { get; set; } = 0x0;
        /// <summary>
        /// Currently unknown
        /// </summary>
        [TSOVoltronDBWrapperField]
        public uint Arg1 { get; set; } = 0x0;
        /// <summary>
        /// Currently unknown
        /// </summary>
        [TSOVoltronDBWrapperField]
        public uint Arg2 { get; set; } = 0x0;
        /// <summary>
        /// The name of the avatar
        /// </summary>
        [TSOVoltronDBWrapperField]
        [TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)]
        public string AvatarName { get; set; } = "";
        /// <summary>
        /// The avatar's description
        /// </summary>
        [TSOVoltronDBWrapperField]
        [TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)]
        public string AvatarDescription { get; set; } = "";
        /// <summary>
        /// The char data payload
        /// </summary>
        [TSOVoltronDBWrapperField]
        [TSOVoltronBodyArray]
        public byte[] CharData { get; set; }

        public TSOSetCharByIDRequest() : base(
                TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                TSO_PreAlpha_kMSGs.kDBServiceRequestMsg,
                TSO_PreAlpha_DBActionCLSIDs.SetCharByID_Request
            )
        {
            MakeBodyFromProperties();
        }
    }
}
