using nio2so.Formats.DB;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.SetCharByID_Request)]
    public class TSOSetCharByIDRequest : TSODBRequestWrapper
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
        /// The char data payload
        /// </summary>
        [TSOVoltronDBWrapperField]
        public TSODBChar CharProfile { get; set; }

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
