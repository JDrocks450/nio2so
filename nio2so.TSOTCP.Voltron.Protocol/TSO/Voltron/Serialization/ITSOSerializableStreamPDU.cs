using nio2so.Formats.Streams;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Serialization
{
    /// <summary>
    /// A <see cref="TSOVoltronDBRequestWrapperPDU"/> that contains a <see cref="TSOSerializableStream"/>
    /// <para/>This implements helpful functions for managing the <see cref="TSOSerializableStream"/>
    /// </summary>
    interface ITSOSerializableStreamPDU
    {
        public TSOSerializableStream GetStream();

        public bool TryUnpackStream<T>(out T? Structure) where T : new()
        {
            Structure = default;
            if (GetStream == null) return false;

            byte[] streamBytes = GetStream().DecompressRefPack();
            Structure = TSOVoltronSerializer.Deserialize<T>(streamBytes);
            return true;
        }
    }

    //nio2so legacy code
#if false
    interface ITSOSerializableStreamPDU
    {
        /// <summary>
        /// 0x00 - None? 0x01 - little endian? 0x03 - Big endian?
        /// <para/>For context, only 0x01 has ever been seen from/to the client in this version
        /// <see cref="TSOSerializableStream"/>
        /// </summary>
        [TSOVoltronDBWrapperField]
        byte CompressionMode { get; set; }
        /// <summary>
        /// The decompressed size of this TSOSerializableStream object
        /// </summary>
        [TSOVoltronDBWrapperField]
        [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)]
        uint DecompressedSize { get; set; }
        /// <summary>
        /// The compressed size of the TSOSerializableStream -- basically just the distance from the end of this DWORD to 
        /// the end of the payload
        /// </summary>
        [TSOVoltronDBWrapperField]
        [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)]
        uint CompressedSize { get; set; }
        /// <summary>
        /// The size of the proceeding stream including these 4 bytes.
        /// </summary>
        [TSOVoltronDBWrapperField]
        [TSOVoltronValue(TSOVoltronValueTypes.LittleEndian)]
        uint StreamBytesSize { get; set; }
        /// <summary>
        /// The payload of this <see cref="ITSOSerializableStreamPDU"/>
        /// </summary>
        [TSOVoltronDBWrapperField]
        [TSOVoltronBodyArray]
        byte[] StreamBytes { get; set; }
    }
#endif
}