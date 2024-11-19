using nio2so.Formats.DB;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetCharBlobByID_Response)]
    internal class TSOGetCharBlobByIDResponse : TSODBRequestWrapper
    {                
        private static byte[] ReadDefaultSimData() => File.ReadAllBytes("/packets/const/TSOSimData.dat");

        [TSOVoltronDBWrapperField]
        public uint AvatarID { get; set; }
        [TSOVoltronDBWrapperField]
        public uint BlobSize { get; set; } = 0x00;
        [TSOVoltronDBWrapperField]
        [TSOVoltronBodyArray]
        public byte[] BlobDataStream { get; set; } = new byte[0];

        public TSOGetCharBlobByIDResponse(uint AvatarID, TSODBCharBlob BlobData) :

            base(
                 TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                 TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                 TSO_PreAlpha_DBActionCLSIDs.SetCharBlobByID_Response
                )
        {
            this.AvatarID = AvatarID;
            BlobSize = BlobData.BlobSize;
            BlobDataStream = BlobData.BlobData;

            MakeBodyFromProperties();
        }
    }
}