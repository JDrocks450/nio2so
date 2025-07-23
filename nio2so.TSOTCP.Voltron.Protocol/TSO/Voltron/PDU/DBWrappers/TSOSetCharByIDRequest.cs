using nio2so.DataService.Common.Types.Avatar;
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
        /// The payload of the request, containing a <see cref="TSODBChar"/> serialized object
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
