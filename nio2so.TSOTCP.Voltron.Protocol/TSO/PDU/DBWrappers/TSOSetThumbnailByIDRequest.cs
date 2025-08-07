using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU.DBWrappers
{
    /// <summary>
    /// Sent from a Client by the VisitorProtocol when <see cref="TSO_PreAlpha_MasterConstantsTable.kMSGID_SaveGame"/> is fired to update the thumbnail of the HouseID
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.SetHouseThumbByID_Request)]
    public class TSOSetThumbnailByIDRequest : TSODBRequestWrapper
    {
        /// <summary>
        /// The House to update the thumbnail
        /// </summary>
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }
        [TSOVoltronDBWrapperField] public uint Arg1 { get; set; }
        [TSOVoltronDBWrapperField] [TSOVoltronArrayLength(nameof(PNGBytes))] public uint PNGLength { get; set; }
        /// <summary>
        /// Byte array containing the PNG File
        /// </summary>
        [TSOVoltronDBWrapperField] [TSOVoltronBodyArray] public byte[] PNGBytes { get; set; } = new byte[0];
        public TSOSetThumbnailByIDRequest() : base(
            TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
            TSO_PreAlpha_kMSGs.kDBServiceRequestMsg,
            TSO_PreAlpha_DBActionCLSIDs.SetHouseThumbByID_Request
        )
        { }
        public TSOSetThumbnailByIDRequest(uint HouseID, byte[] PNGFile) : base(
            TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
            TSO_PreAlpha_kMSGs.kDBServiceRequestMsg,
            TSO_PreAlpha_DBActionCLSIDs.SetHouseThumbByID_Request
        )
        {
            this.HouseID = HouseID;
            PNGBytes = PNGFile;
            MakeBodyFromProperties();
        }
    }
}
