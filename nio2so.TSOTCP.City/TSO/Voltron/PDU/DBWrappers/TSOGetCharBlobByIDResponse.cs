using nio2so.Formats.DB;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    internal class TSOGetCharBlobByIDResponse : TSODBRequestWrapper
    {                
        private static byte[] ReadDefaultSimData() => File.ReadAllBytes("/packets/const/TSOSimData.dat");

        [TSOVoltronDBWrapperField] public uint AvatarID { get; }
        [TSOVoltronDBWrapperField] public TSODBCharBlob CharBlob { get; }

        /// <summary>
        /// Makes a default response packet using the supplied parameters.
        /// </summary>
        /// <param name="AriesID"></param>
        /// <param name="MasterID"></param>
        public TSOGetCharBlobByIDResponse(string AriesID, string MasterID, uint avatarID, TSODBCharBlob AvatarBlob) :
            base(
                    AriesID,
                    MasterID,
                    0x00,
                    TSODBWrapperMessageSize.AutoSize,
                    TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                    0x21,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetCharBlobByID_Response,
                    CombineArrays(new byte[]
                    {
                        0x00,0x00,0x05,0x39, // <--- AVATARID
                        0x01,
                        0x00,0x00,0x03,0x20,
                        0x00,0x1B,0x03,0x00,0x00,
                    },
                    AvatarBlob.BlobData)
                )
        {
            AvatarID = avatarID;
            CharBlob = AvatarBlob;

            MoveBufferPositionToDBMessageBody();
            EmplaceBody(AvatarID); // <--- overwrite avatarid here for now
                                   
            ReadAdditionalMetadata();
        }
    }
}