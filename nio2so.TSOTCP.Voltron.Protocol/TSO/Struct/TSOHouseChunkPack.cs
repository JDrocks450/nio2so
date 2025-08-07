using nio2so.Data.Common.Serialization.Voltron;
using QuazarAPI.Networking.Data;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Struct
{
    public record TSOHouseChunk(TSO_PreAlpha_HouseStreamChunkHeaders Header, uint Param1, uint Size, byte[] Content)
    {
        public override string ToString()
        {
            return $"{Header.ToString().ToUpper()}({Param1})[{Size}]";
        }
    }

    /// <summary>
    /// A stream that has a 4-byte header, 4-bytes, then 4-byte size chunks
    /// </summary>
    public class TSOHouseChunkPack : ITSOCustomDeserialize
    {
        /// <summary>
        /// Maps read chunk to the index it is found at in the source data stream
        /// </summary>
        public Dictionary<TSOHouseChunk, long> Chunks { get; } = new();

        public TSOHouseChunkPack()
        {
        }

        public TSOHouseChunk? GetChunk(TSO_PreAlpha_HouseStreamChunkHeaders Type) => Chunks.Keys.LastOrDefault(x => x.Header == Type);

        private void PopulateChunks(Stream DataStream)
        {
            Chunks.Clear();

            TSO_PreAlpha_HouseStreamChunkHeaders MatchHeader()
            {
                //read next 4 bytes
                uint headerBytes = DataStream.ReadBodyDword(MiscUtil.Conversion.Endianness.BigEndian);
                //cast to enum
                TSO_PreAlpha_HouseStreamChunkHeaders header = (TSO_PreAlpha_HouseStreamChunkHeaders)headerBytes;
                //is this defined?
                if (!Enum.IsDefined(header))
                {
                    DataStream.Seek(-4, SeekOrigin.Current); // roll it back and read in little endian
                    headerBytes = DataStream.ReadBodyDword(MiscUtil.Conversion.Endianness.LittleEndian);
                    header = (TSO_PreAlpha_HouseStreamChunkHeaders)headerBytes;
                    if (!Enum.IsDefined(header))
                        throw new NotImplementedException($"{headerBytes} is not implemented in a HouseBlob response!");
                }
                //ensure this is a known-header
                return header;
            }

            long index = -1;
            //workaround to detect when the SECOND occurence of a chunk is
            HashSet<TSO_PreAlpha_HouseStreamChunkHeaders> processedList = new();

            while (DataStream.Position != DataStream.Length)
            {
                index = DataStream.Position;
                //read proceeding 4 bytes as a header and attempt to match it as a known-header type
                TSO_PreAlpha_HouseStreamChunkHeaders Header = MatchHeader();
                if (!processedList.Contains(Header))
                {
                    processedList.Add(Header);
                    DataStream.Seek(2 * sizeof(uint), SeekOrigin.Current); // advance because this may be a map of types to some kind of index
                    continue;
                }
                uint param1 = DataStream.ReadBodyDword(MiscUtil.Conversion.Endianness.LittleEndian);
                uint size = DataStream.ReadBodyDword(MiscUtil.Conversion.Endianness.LittleEndian);
                byte[] content = new byte[size];
                DataStream.ReadExactly(content, 0, (int)size);
                Chunks.Add(new TSOHouseChunk(Header, param1, size, content), index);
            }
        }

        public void OnDeserialize(Stream Stream) => PopulateChunks(Stream);

        public byte[] OnSerialize()
        {
            throw new NotImplementedException();
        }
    }
}
