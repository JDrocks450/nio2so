using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Sent when the Client requests the thumbnail after a successful <see cref="TSOGetLotByID_Response"/>
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetHouseThumbByID_Response)]
    public class TSOGetHouseThumbByIDResponse : TSODBRequestWrapper
    {
        /// <summary>
        /// The database ID of the requested lot
        /// </summary>
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }
        /// <summary>
        /// The size of the <see cref="PNGByteStream"/> payload
        /// </summary>
        [TSOVoltronDBWrapperField] public uint ArraySize { get; set; }
        /// <summary>
        /// The size of the <see cref="PNGByteStream"/> payload
        /// </summary>
        [TSOVoltronDBWrapperField] public uint ArraySize2 { get; set; }
        /// <summary>
        /// A PNG Image stream containing the thumbnail to display
        /// </summary>
        [TSOVoltronDBWrapperField] [TSOVoltronBodyArray] public byte[] PNGByteStream { get; set; }
        /// <summary>
        /// Creates a new <see cref="TSOGetHouseThumbByIDResponse"/> for the given <paramref name="HouseID"/> containing a PNG file <paramref name="PNGBytes"/>
        /// </summary>
        /// <param name="HouseID">The HouseID this is corresponding with</param>
        /// <param name="PNGBytes">A byte stream containing a PNG image</param>
        public TSOGetHouseThumbByIDResponse(uint HouseID, byte[] PNGBytes) : base(
                    TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetHouseThumbByID_Response
                )
        {
            this.HouseID = HouseID;
            PNGByteStream = PNGBytes;
            ArraySize = ArraySize2 = (uint)PNGByteStream.Length;
            MakeBodyFromProperties();
        }
    }
}
