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
            base(TSO_PreAlpha_DBStructCLSIDs.cTSOSerializableStream,
                TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                TSO_PreAlpha_DBActionCLSIDs.GetCharBlobByID_Response,
                (uint)(0x21 + 8 + AvatarBlob.BlobData.Length)
            )
        {
            AvatarID = avatarID;
            CharBlob = AvatarBlob;
            MakeBodyFromProperties();

            MoveBufferPositionToDBMessageHeader();
            EmplaceBody(AvatarID); // <--- overwrite avatarid here for now
            EmplaceBody((uint)0x01); // <-- Parameter here, usually zero                     
            EmplaceBody(AvatarBlob.BlobData); // <-- write the blob data here
        }
    }
}