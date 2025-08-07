using nio2so.Data.Common.Serialization.Voltron;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU
{
    /// <summary>
    /// The <see cref="TSOHostOnlinePDU"/> sends to the Client that this server is a Voltron server.
    /// <para>Encompasses a HostVersion and PacketSize</para>
    /// <code>Unverified!</code>
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.HOST_ONLINE_PDU)]
    public class TSOHostOnlinePDU : TSOVoltronPacket
    {
        // 07/13/2025
        // this packet has been wrong since the beginning
        // static analysis says it goes m_hostReservedWords and m_hostParams
        // today, it is now fixed.

        //CONST/TUNING
        public const ushort PACKET_SIZE_LIMIT = (ushort)TSOVoltronConst.SplitBufferPDU_DefaultChunkSize;

        public record TSOHostReservedWordsStruct
        {
            [TSOVoltronArrayLength(nameof(Words))] public ushort NumberOfWords { get; set; }
            public TSOPascalString[] Words { get; set; } = Array.Empty<TSOPascalString>();

            public TSOHostReservedWordsStruct(params string[] ReservedWords) => Words = ReservedWords.Select(x => new TSOPascalString(x)).ToArray();    
        }

        public record TSOHostParamsStruct
        {
            public TSOHostParamsStruct(ushort clientBufferSize = PACKET_SIZE_LIMIT, ushort hostVersion = 0x0)
            {
                HostVersion = hostVersion;
                ClientBufferSize = clientBufferSize;
            }

            public ushort HostVersion { get; set; }
            public ushort ClientBufferSize { get; set; }
        }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.HOST_ONLINE_PDU;        

        public TSOHostReservedWordsStruct HostReservedWords { get; set; }

        public TSOHostParamsStruct HostParams { get; set; }

        public TSOHostOnlinePDU() : base() {
            HostReservedWords = new TSOHostReservedWordsStruct();
            HostParams = new TSOHostParamsStruct();
            MakeBodyFromProperties();
        }

        public TSOHostOnlinePDU(ushort ClientBufferSize, params string[] ReservedWords) : this()
        {
            HostReservedWords = new(ReservedWords);
            HostParams = new TSOHostParamsStruct(ClientBufferSize);
            MakeBodyFromProperties();
        }
    }
}
