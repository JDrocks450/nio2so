using nio2so.Data.Common.Serialization.Voltron;
using nio2so.Formats.Streams;

namespace nio2so.Voltron.Core.TSO.Serialization.Types
{
    /// <summary>
    /// A <see cref="TSOVoltronDBRequestWrapperPDU"/> that contains a <see cref="TSOSerializableStream"/>
    /// <para/>This implements helpful functions for managing the <see cref="TSOSerializableStream"/>
    /// </summary>
    public interface ITSOSerializableStreamPDU
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
}