using nio2so.Formats.Util.Endian;
using nio2so.Voltron.Core.TSO.Serialization;
using nio2so.Voltron.NewImproved.Protocol.Constants;
using nio2so.Voltron.NewImproved.Protocol.PDU.DataService;
using QuazarAPI.Networking.Data;
using System.Runtime.Serialization;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.NewImproved.Protocol.PDU.MessageFormat
{    
    public interface ITSONetMessageStandard : ITSOMessage
    {
        /// <summary>
        /// The number assigned to this Request/Response
        /// <para/>See: <see cref="ITSOMessage.GetResponseID"/>
        /// </summary>
        public uint MessageID { get; set; }
        /// <summary>
        /// <inheritdoc cref="TSODataServiceWrapperPDU.SendingAvatarID"/>
        /// </summary>
        public uint SendingAvatarID { get; set; }
        /// <summary>
        /// The flags dictating which Data properties are available in this <see cref="TSONetMessageStandard"/>
        /// </summary>
        public byte Flags { get; set; }

        [TSOVoltronValue(Data.Common.Serialization.Voltron.TSOVoltronValueTypes.LittleEndian)] public uint FormatID { get; set; }

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
    }
    public interface ITSODataServiceNetMessageStandard : ITSONetMessageStandard
    {
        /// <summary>
        /// The string entry in the TSODataDefinition corresponding with this query
        /// </summary>
        [TSOVoltronIgnorable]
        public TSO_NewImproved_TSOData_Constants FormatID { get; set; }
    }
    public interface ITSODatabaseNetMessageStandard : ITSONetMessageStandard
    {        
        /// <summary>
        /// The string entry in the TSODataDefinition corresponding with this query
        /// </summary>
        [TSOVoltronIgnorable]
        public TSO_NewImproved_DBServiceConstants FormatID { get; set; }
    }

    /// <summary>
    /// Represents a <see cref="TSO_NewImproved_TSOMessageType_CLSIDs.cTSONetMessageStandard"/> which is the most common type
    /// </summary>
    public class TSONetMessageStandard : TSOMessageFormatBase, ITSODataServiceNetMessageStandard, ITSODatabaseNetMessageStandard
    {
        const TSO_NewImproved_TSOMessageType_CLSIDs MYID = TSO_NewImproved_TSOMessageType_CLSIDs.cTSONetMessageStandard;

        private byte[] _bytes;

        /// <summary>
        /// The number assigned to this Request/Response
        /// <para/>See: <see cref="ITSOMessage.GetResponseID"/>
        /// </summary>
        public uint MessageID { get; set; } = 0x2EEC9157;
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
        [TSOVoltronValue(Data.Common.Serialization.Voltron.TSOVoltronValueTypes.LittleEndian)] public uint FormatID { get; set; }
        [TSOVoltronIgnorable]
        TSO_NewImproved_TSOData_Constants ITSODataServiceNetMessageStandard.FormatID { 
            get => (TSO_NewImproved_TSOData_Constants)FormatID;
            set => FormatID = (uint)value;
        }
        [TSOVoltronIgnorable]
        TSO_NewImproved_DBServiceConstants ITSODatabaseNetMessageStandard.FormatID
        {
            get => (TSO_NewImproved_DBServiceConstants)FormatID;
            set => FormatID = (uint)value;
        }

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

        public TSONetMessageStandard() : base()
        {

        }
        public TSONetMessageStandard(byte[] MessageData) : this()
        {
            MessageBytes = MessageData;
        }

        public TSONetMessageStandard(uint messageID,
                                     uint sendingAvatarID,
                                     uint formatID,
                                     uint? data1 = null,
                                     uint? data2 = null,
                                     uint? data3 = null,
                                     uint? data4 = null) : 
            this(messageID, sendingAvatarID, formatID, null, data1,data2,data3,data4)
        {
            
        }
        public TSONetMessageStandard(uint messageID,
                                     uint sendingAvatarID,
                                     uint formatID,
                                     TSOServiceMessage Embedded,
                                     uint? data1 = null,
                                     uint? data2 = null,
                                     uint? data3 = null,
                                     uint? data4 = null) : this()
        {
            MessageID = messageID;
            SendingAvatarID = sendingAvatarID;
            FormatID = formatID;
            Data1 = data1;
            Data2 = data2;
            Data3 = data3;
            Data4 = data4;
            EmbeddedServiceMessage = Embedded;

            _bytes = WriteMessageData();
        }
        public TSONetMessageStandard(uint messageID, uint sendingAvatarID, uint formatID, TSOServiceMessage Embedded) : 
            this(messageID,sendingAvatarID, formatID, Embedded, null,null,null,null)
        {

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

        byte[] WriteMessageData()
        {
            using var stream = new MemoryStream();

            void writeUInt(uint value, Endianness Endian = Endianness.BigEndian)
            {
                byte[] data = Endian == Endianness.BigEndian ?
                    EndianBitConverter.Big.GetBytes(value) :
                    EndianBitConverter.Little.GetBytes(value);
                stream.Write(data, 0, data.Length);
            }

            // reset Flags before evaluation
            Flags = 0;

            // Data1 (Flag 1 << 1)
            if (Data1.HasValue)
            {
                Flags |= (1 << 1);
                writeUInt(Data1.Value, Endianness.BigEndian);
            }

            // Data2 (Flag 1 << 2)
            if (Data2.HasValue)
            {
                Flags |= (1 << 2);
                writeUInt(Data2.Value, Endianness.BigEndian);
            }

            // Data3 (Flag 1 << 3)
            if (Data3.HasValue)
            {
                Flags |= (1 << 3);
                writeUInt(Data3.Value, Endianness.BigEndian);
            }

            // EmbeddedServiceMessage (Flag 1 << 5)
            if (EmbeddedServiceMessage != null)
            {
                Flags |= (1 << 5);
                byte[] embeddedTSOMessage = TSOVoltronSerializer.Serialize(EmbeddedServiceMessage);
                stream.Write(embeddedTSOMessage, 0, embeddedTSOMessage.Length);
                return stream.ToArray();                
            }

            // StringContent (Flag 1 << 6)
            if (!string.IsNullOrEmpty(StringContent))
            {
                Flags |= (1 << 6);
                // TODO: Implement FC FF STR# serialization scheme here
            }

            // Data4 (Flag 1 << 4) written after StringContent to match Read function order
            if (Data4.HasValue)
            {
                Flags |= (1 << 4);
                writeUInt(Data4.Value, Endianness.BigEndian);
            }

            return stream.ToArray();
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

        public override uint GetResponseID() => MessageID;
        public override void SetResponseID(uint NewResponseID) => MessageID = NewResponseID;
    }
}
