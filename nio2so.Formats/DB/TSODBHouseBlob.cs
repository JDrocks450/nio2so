using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Formats.DB
{
    /// <summary>
    /// A wrapper for the House Blob stream that is sent to the client for HouseBlob PDUs
    /// </summary>
    public class TSODBHouseBlob
    {
        /// <summary>
        /// The house blob data stream
        /// </summary>
        [TSOVoltronBodyArray] public byte[] HouseBlobStream { get; set; }
        /// <summary>
        /// Default parameterless constructor for serialization
        /// </summary>
        public TSODBHouseBlob() { }
        /// <summary>
        /// Creates a new <see cref="TSODBHouseBlob"/> and sets the <see cref="HouseBlobStream"/> property
        /// </summary>
        /// <param name="houseBlobStream"></param>
        public TSODBHouseBlob(byte[] houseBlobStream) : this()
        {
            HouseBlobStream = houseBlobStream;
        }
    }
}
