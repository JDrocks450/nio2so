using nio2so.DataService.Common.Types.Avatar;
using nio2so.Formats.DB;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// The response PDU to a <see cref="TSOGetCharByIDRequest"/> PDU
    /// <para/>It contains extremely surface-level info about an Avatar, like their Name and Description
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetCharByID_Response)]
    public class TSOGetCharByIDResponse : TSODBRequestWrapper
    {
        /// <summary>
        /// The Avatar this packet is in reference to
        /// </summary>
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        /// <summary>
        /// Not sure what this is used for yet, probably 0x00000000 most of the time?
        /// </summary>
        [TSOVoltronDBWrapperField] public uint Filler { get; set; } = 0xBAADF00D;
        /// <summary>
        /// The <see cref="TSODBChar"/> data to send, in bytes
        /// </summary>
        [TSOVoltronDBWrapperField] public TSODBChar CharProfile { get; set; }

        /// <summary>
        /// Default parameterless constructor. Please use overload for programmatically creating PDUs.
        /// </summary>
        public TSOGetCharByIDResponse() : base() { }

        /// <summary>
        /// Creates a new <see cref="TSOGetCharByIDResponse"/> packet with the provided <see cref="TSODBChar"/>
        /// </summary>
        /// <param name="AriesID"></param>
        /// <param name="MasterID"></param>
        public TSOGetCharByIDResponse(uint AvatarID, TSODBChar CharData) :
            base(
                    TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetCharByID_Response
                )
        {
            this.AvatarID = AvatarID;
            CharProfile = CharData;

            MakeBodyFromProperties();
        }
    }
}
