using System.Text;

namespace nio2so.Formats.DB
{
    /// <summary>
    /// The body of a DBRequestWrapper PDU in Voltron sent in request/response to a (Get/Set)CharBlobByID command
    /// </summary>
    public class TSODBCharBlob : TSODBBlob
    {
        public string AvatarName
        {
            get
            {
                try
                {
                    byte length = BlobData[0];
                    return Encoding.UTF8.GetString(BlobData,1,length);
                }
                catch(Exception e)
                {

                }
                return "";
            }
        }

        public TSODBCharBlob(byte[] blobData) : base(blobData)
        {

        }

    }
}
