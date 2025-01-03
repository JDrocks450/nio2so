using nio2so.Formats.DB;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Sends a confirmation to the Client that the <c>TSODBCharBlob</c> was received successfully
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.SetCharBlobByID_Response)]
    internal class TSOSetCharBlobByIDResponse : TSOGetCharBlobByIDResponse
    {
        /// <summary>
        /// Creates a new <see cref="TSOSetCharBlobByIDResponse"/> packet with the provided <see cref="TSODBCharBlob"/> payload
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <param name="BlobData"></param>
        public TSOSetCharBlobByIDResponse(uint AvatarID, TSODBCharBlob BlobData) : base(AvatarID, BlobData)
        {
            TSOSubMsgCLSID = (uint)TSO_PreAlpha_DBActionCLSIDs.SetCharBlobByID_Response;
            MakeBodyFromProperties();
        }
    }
}
