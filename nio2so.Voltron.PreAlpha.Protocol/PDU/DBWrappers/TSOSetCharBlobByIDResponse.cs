using nio2so.DataService.Common.Types.Avatar;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{
    /// <summary>
    /// Sends a confirmation to the Client that the <see cref="TSODBCharBlob"/> was received successfully.
    /// <para/>This format needs revisiting, it is not correct.
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU((uint)TSO_PreAlpha_DBActionCLSIDs.SetCharBlobByID_Response)]
    public class TSOSetCharBlobByIDResponse : TSOGetCharBlobByIDResponse
    {
        public TSOSetCharBlobByIDResponse() : base()
        {
            TSOSubMsgCLSID = TSO_PreAlpha_DBActionCLSIDs.SetCharBlobByID_Response;
            MakeBodyFromProperties();
        }

        /// <summary>
        /// Creates a new <see cref="TSOSetCharBlobByIDResponse"/> packet with the provided <see cref="TSODBCharBlob"/> payload
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <param name="BlobData"></param>
        public TSOSetCharBlobByIDResponse(uint AvatarID, TSODBCharBlob BlobData) : base(AvatarID, BlobData)
        {
            TSOSubMsgCLSID = TSO_PreAlpha_DBActionCLSIDs.SetCharBlobByID_Response;
            MakeBodyFromProperties();
        }
    }
}
