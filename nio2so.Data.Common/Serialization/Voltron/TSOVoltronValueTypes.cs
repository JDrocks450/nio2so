namespace nio2so.Data.Common.Serialization.Voltron
{
    /// <summary>
    /// Value types that are commonly found in PDUs from TSO: Pre-Alpha
    /// </summary>
    public enum TSOVoltronValueTypes
    {
        /// <summary>
        /// Default value.
        /// </summary>
        Invalid,
        /// <summary>
        /// A string that ends with a null-terminator
        /// </summary>
        NullTerminated,
        /// <summary>
        /// A length-prefixed string plus a <see cref="UInt16"/> indicating data type (0x8000)
        /// <para>Looks like: <c>{ 0x8000 [WORD LENGTH] [UTF-8 *LENGTH* byte array] }</c></para>
        /// </summary>
        Pascal,
        /// <summary>
        /// One-Byte length followed by the string in UTF-8 format
        /// <code>[byte Length][byte[] UTF-8]</code>
        /// </summary>
        Length_Prefixed_Byte,
        /// <summary>
        /// A little-endian numeric type
        /// </summary>
        LittleEndian,
        /// <summary>
        /// A big-endian numeric type
        /// </summary>
        BigEndian
    }
}
