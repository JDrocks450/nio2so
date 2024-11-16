namespace nio2so.Formats.DB
{
    /// <summary>
    /// The body of a DBRequestWrapper PDU in Voltron sent in request/response to a (Get/Set)CharBlobByID command
    /// </summary>
    public class TSODBCharBlob : TSODBBlob
    {
        public TSODBCharBlob(byte[] blobData) : base(blobData)
        {

        }

    }
}
