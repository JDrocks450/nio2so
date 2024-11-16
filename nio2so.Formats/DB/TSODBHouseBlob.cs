using nio2so.Formats.Util.Endian;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Formats.DB
{
    /// <summary>
    /// The body of a DBRequestWrapper PDU in Voltron sent in request/response to a (Get/Set)HouseBlobByID command
    /// </summary>
    public class TSODBHouseBlob : TSODBBlob
    {
        public uint RASSize => EndianBitConverter.Big.ToUInt32(BlobData,0);
        public TSODBHouseBlob(byte[] blobData) : base(blobData)
        {

        }
    }
}
