using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Formats.DB
{
    /// <summary>
    /// Not known the exact format at this time for this data, using <see cref="TSODBBlob"/> for now.
    /// </summary>
    public class TSODBChar : TSODBBlob
    {
        public TSODBChar(byte[] blobData) : base(blobData)
        {

        }
    }
}
