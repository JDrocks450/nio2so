using nio2so.Formats.Util.Endian;
using nio2so.Voltron.Core.TSO.Serialization;
using nio2so.Voltron.NewImproved.Protocol.Constants;
using nio2so.Voltron.NewImproved.Protocol.PDU.DataService;
using nio2so.Voltron.NewImproved.Protocol.PDU.MessageFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.NewImproved.Protocol.PDU.Database
{
    /// <summary>
    /// Temporary name
    /// </summary>
    internal class TSOLoadAvatarByIDRequestMessage
    {
        const TSO_NewImproved_TSOMessageType_CLSIDs MYID = TSO_NewImproved_TSOMessageType_CLSIDs.LoadAvatarByIDFormatMessage;
        private byte[] _bytes;

        public TSO_NewImproved_TSOMessageType_CLSIDs GetMessageType() => MYID;
       
        /// <summary>
        /// Length is 0x18 (24) -- This is found in the LoadAvatarByID MessageFormat packet, one PreferredLanguage packet, one LoadAvatarByID packet.
        /// </summary>
        /// <param name="Request"></param>
        /// <param name=""></param>
        public class TSOLoadAvatarByIDChunk
        {
            const int Width = 0x18; // 24

            public TSOLoadAvatarByIDChunk(TSO_NewImproved_DBServiceConstants Request, uint Value1, uint Value2, uint Value3, uint Value4, uint Value5, uint Value6)
            {
                this.Request = Request;
                this.Value1 = Value1;
                this.Value2 = Value2;
                this.Value3 = Value3;
                this.Value4 = Value4;
                this.Value5 = Value5;
                this.Value6 = Value6;
            }
            public TSOLoadAvatarByIDChunk()
            {

            }

            public TSO_NewImproved_DBServiceConstants Request { get; set; }
            public uint Value1 { get; set; }
            public uint Value2 { get; set; }
            public uint Value3 { get; set; }
            public uint Value4 { get; set; }
            public uint Value5 { get; set; }
            public uint Value6 { get; set; }

            public override string ToString() => $"{Request} " +
                                               $"[{(uint)Request:X8}] " +
                                               $"{Value1:X8} {Value2:X8} {Value3:X8} {Value4:X8} {Value5:X8}";
        }

        [TSOVoltronBodyArray]
        public byte[] MessageBytes
        {
            get => _bytes;
            set
            {
                _bytes = value;
                Parse();
            }
        }

        public readonly IList<TSOLoadAvatarByIDChunk> Chunks = new List<TSOLoadAvatarByIDChunk>();

        void Parse()
        {
            using Stream ms = new MemoryStream(_bytes);
            while (ms.Position < ms.Length)
            {
                uint reqID = EndianBitConverter.Big.ToUInt32(_bytes, (int)ms.Position);
                Chunks.Add(TSOVoltronSerializer.Deserialize<TSOLoadAvatarByIDChunk>(ms));
            }
        }
    }
}
