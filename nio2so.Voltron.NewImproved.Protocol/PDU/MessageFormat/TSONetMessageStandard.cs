using nio2so.Formats.Util.Endian;
using nio2so.Voltron.Core.TSO.Serialization;
using nio2so.Voltron.NewImproved.Protocol.PDU.DataService;
using QuazarAPI.Networking.Data;
using System.Runtime.Serialization;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.NewImproved.Protocol.PDU.MessageFormat
{    
    /// <summary>
    /// Represents a <see cref="TSO_NewImproved_TSOMessageType_CLSIDs.cTSONetMessageStandard"/> which is the most common type
    /// </summary>
    public class TSONetMessageStandard : TSOMessageFormatBase
    {
        const TSO_NewImproved_TSOMessageType_CLSIDs MYID = TSO_NewImproved_TSOMessageType_CLSIDs.cTSONetMessageStandard;

        private byte[] _bytes;

        /// <summary>
        /// <inheritdoc cref="TSODataServiceWrapperPDU.SendingAvatarID"/>
        /// </summary>
        public uint SendingAvatarID { get; set; }
        /// <summary>
        /// The flags dictating which Data properties are available in this <see cref="TSONetMessageStandard"/>
        /// </summary>
        public byte Flags { get; set; }
        /// <summary>
        /// The string entry in the TSODataDefinition corresponding with this query
        /// </summary>
        [TSOVoltronValue(Data.Common.Serialization.Voltron.TSOVoltronValueTypes.LittleEndian)] public TSO_NewImproved_TSOData_Constants DataDefinitionID { get; set; }
        /// <summary>
        /// The content of the message
        /// </summary>
        [TSOVoltronBodyArray]
        public byte[] MessageBytes
        {
            get => _bytes;
            set
            {
                _bytes = value;
                if (value != null && value.Length > 0)
                    ReadMessageData();
                else 
                    ClearPreviousMessage();
            }
        }

        //**autocalc**

        [TSOVoltronIgnorable]
        [IgnoreDataMember]
        public bool HasData1 => Data1.HasValue;
        [TSOVoltronIgnorable]
        [IgnoreDataMember]
        public uint? Data1 { get; set; }
        [TSOVoltronIgnorable]
        [IgnoreDataMember]
        public bool HasData2 => Data2.HasValue;
        [TSOVoltronIgnorable]
        [IgnoreDataMember]
        public uint? Data2 { get; set; }
        [TSOVoltronIgnorable]
        [IgnoreDataMember]
        public bool HasData3 => Data3.HasValue;
        [TSOVoltronIgnorable]
        [IgnoreDataMember]
        public uint? Data3 { get; set; }
        [TSOVoltronIgnorable]
        [IgnoreDataMember]
        public bool HasEmbeddedMessage => EmbeddedServiceMessage != null;
        [TSOVoltronIgnorable]
        [IgnoreDataMember]
        public TSOServiceMessage? EmbeddedServiceMessage { get; set; }
        [TSOVoltronIgnorable]
        [IgnoreDataMember]
        public bool HasStringContent => StringContent != null; // can be empty only check for null
        [TSOVoltronIgnorable]
        [IgnoreDataMember]
        public string? StringContent { get; set; }
        [TSOVoltronIgnorable]
        [IgnoreDataMember]
        public bool HasData4 => Data4.HasValue;
        [TSOVoltronIgnorable]
        [IgnoreDataMember]
        public uint? Data4 { get; set; }

        private void ReadMessageData()
        {
            /*
             * If (Flags & (1<<1)):
            Data1 - 4 bytes
            If (Flags & (1<<2)):
            Data2 - 4 bytes
            If (Flags & (1<<3)):
            Data3 - 4 bytes
            If (Flags & (1<<5)):
            Extra clsid - 4 bytes
            Extra body - variable bytes
            If (Flags & (1<<6)):
            String - A Pascal-like string using the variable-length coding scheme defined in the FC FF STR# format
            If (Flags & (1<<4)):
            Data4 - 4 bytes
             */
            using var stream = new MemoryStream(MessageBytes);
            uint readUInt(Endianness Endian = Endianness.BigEndian)
            {
                byte[] data = new byte[sizeof(uint)];
                int readData = stream.Read(data, 0, data.Length);
                if (readData != data.Length)
                    return default;
                return Endian == Endianness.BigEndian ? 
                    EndianBitConverter.Big.ToUInt32(data, 0) :
                    EndianBitConverter.Little.ToUInt32(data, 0);
            }

            ClearPreviousMessage();

            if ((Flags & (1 << 1)) != 0)
                Data1 = readUInt(Endianness.BigEndian);
            if ((Flags & (1 << 2)) != 0)
                Data2 = readUInt(Endianness.BigEndian);
            if ((Flags & (1 << 3)) != 0)
                Data3 = readUInt(Endianness.BigEndian);
            if ((Flags & (1 << 5)) != 0)
            {
                byte[] embeddedTSOMessage = stream.ReadToEnd();
                EmbeddedServiceMessage = TSOVoltronSerializer.Deserialize<TSOServiceMessage>(embeddedTSOMessage);
                return;
            }
            if ((Flags & (1 << 6)) != 0)
                StringContent = "STR# 23 FC FF NOT IMPLEMENTED.";
            if ((Flags & (1 << 4)) != 0)
                Data4 = readUInt(Endianness.BigEndian);
            if (stream.Length != stream.Position)
                ;
        }

        private void ClearPreviousMessage()
        {
            Data1 = Data2 = Data3 = Data4 = null;
            EmbeddedServiceMessage = null;
            StringContent = null;
        }

        public override TSO_NewImproved_TSOMessageType_CLSIDs GetMessageType() => MYID;

        internal uint[] GetDataValues()
        {
            List<uint> values = new List<uint>();
            if (Data1.HasValue)
                values.Add(Data1.Value);
            if (Data2.HasValue)
                values.Add(Data2.Value);
            if (Data3.HasValue)
                values.Add(Data3.Value);
            if (Data4.HasValue)
                values.Add(Data4.Value);
            return values.ToArray();
        }
    }
}
