using nio2so.Formats.DB;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetCharBlobByID_Response)]
    internal class TSOGetCharBlobByIDResponse : TSODBRequestWrapper
    {                
        /// <summary>
        /// The AvatarID in the database that is being received
        /// </summary>
        [TSOVoltronDBWrapperField]
        public uint AvatarID { get; set; }

        /// <summary>
        /// Unknown
        /// </summary>
        [TSOVoltronDBWrapperField]
        public byte Filler { get; set; } = 0x01;
        /// <summary>
        /// The size of the Blob data being received
        /// </summary>
        [TSOVoltronDBWrapperField]        
        public uint BlobSize { get; set; } = 0x00;
        [TSOVoltronDBWrapperField]
        public uint Arg2 { get; set; } = 0x001B0300; // 1770240
        [TSOVoltronDBWrapperField]
        public byte Filler2 { get; set; } = 0x00;

        [TSOVoltronDBWrapperField]
        [TSOVoltronBodyArray]
        public byte[] BlobDataStream { get; set; } = new byte[0];

        public TSOGetCharBlobByIDResponse(uint AvatarID, TSODBCharBlob BlobData) :

            base(
                 TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                 TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                 TSO_PreAlpha_DBActionCLSIDs.GetCharBlobByID_Response
                )
        {
            this.AvatarID = AvatarID;
            BlobSize = (uint)BlobData.BlobData.Length;
            BlobDataStream = BlobData.BlobData;

            MakeBodyFromProperties();
        }
    }
}