namespace nio2so.Formats.DB
{
    /// <summary>
    /// A data type not fully understood yet.
    /// </summary>
    public abstract class TSODBBlob
    {
        /// <summary>
        /// Creates a new <see cref="TSODBBlob"/> from the given data
        /// </summary>
        /// <param name="blobData"></param>
        protected TSODBBlob(byte[] blobData)
        {
            BlobData = blobData;
        }

        /// <summary>
        /// The raw data in this <see cref="TSODBBlob"/>
        /// </summary>
        public byte[] BlobData { get; }
    }
}
