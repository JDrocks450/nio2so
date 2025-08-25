using nio2so.Formats.Util.Endian;
using nio2so.Voltron.PlayTest.Protocol.PDU.DataService;
using QuazarAPI.Networking.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PlayTest.Protocol.PDU.MessageFormat
{
    public interface ITSOMessage
    {

    }

    /// <summary>
    /// Represents a <see cref="TSO_PlayTest_MsgCLSIDs.cTSONetMessageStandard"/> which is the most common type
    /// </summary>
    public record TSONetMessageStandard : ITSOMessage
    {
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
        /// The ID attached to this transaction. Should match the response to this query
        /// </summary>
        public TSO_PlayTest_kMSGs kMSG { get; set; }                
        /// <summary>
        /// The string entry in the TSODataDefinition corresponding with this query
        /// </summary>
        public uint DataDefinitionID { get; set; }
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
        public bool HasEmbeddedMessage => EmbeddedMessageCLSID.HasValue && EmbeddedMessage != null;
        [TSOVoltronIgnorable]
        [IgnoreDataMember]
        public TSO_PlayTest_MsgCLSIDs? EmbeddedMessageCLSID { get; set; }
        [TSOVoltronIgnorable]
        [IgnoreDataMember]
        public byte[]? EmbeddedMessage { get; set; }
        [TSOVoltronIgnorable]
        [IgnoreDataMember]
        public bool HasStringContent => StringContent == null; // can be empty only check for null
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
                EmbeddedMessageCLSID = (TSO_PlayTest_MsgCLSIDs)readUInt(Endianness.BigEndian);
                EmbeddedMessage = stream.ReadToEnd();
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
            EmbeddedMessage = null;
            EmbeddedMessageCLSID = null;
            StringContent = null;
        }
    }
}
