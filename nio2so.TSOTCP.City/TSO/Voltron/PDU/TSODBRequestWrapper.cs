using MiscUtil.Conversion;
using nio2so.TSOTCP.City.TSO.Aries;
using nio2so.TSOTCP.City.TSO.Voltron.PDU;
using nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers;
using nio2so.TSOTCP.City.TSO.Voltron.Util;
using QuazarAPI.Networking.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace nio2so.TSOTCP.City.TSO.Voltron
{
    /// <summary>
    /// A class for cTSONetMessageStandard structs wrapped inside a <see cref="TSODBRequestWrapper"/> PDU
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.DB_REQUEST_WRAPPER_PDU)]
    internal class TSODBRequestWrapper : TSOVoltronPacket
    {
        /// <summary>
        /// The distance from <see cref="MessageSize"/> -> the start of <see cref="DBMessageBody"/>. Used here: <see cref="TSODBRequestWrapper()"/>
        /// </summary>
        const uint DBWRAPPER_MESSAGESIZE_TO_BODY_DISTANCE = (sizeof(uint) * 3) + 1;

        public TSODBRequestWrapper() : base() 
        {
            MakeBodyFromProperties();
        }
        public TSODBRequestWrapper(string ariesID,
                                   string masterID,
                                   ushort bitfield_Arg1,
                                   uint messageSize,
                                   TSO_PreAlpha_DBStructCLSIDs tSOPacketFormatCLSID,
                                   byte flags,
                                   uint transactionID,
                                   TSO_PreAlpha_DBActionCLSIDs tSOSubMsgCLSID,
                                   byte[] body)
            : this(
                    ariesID,
                    masterID, 
                    bitfield_Arg1, 
                    messageSize, 
                    (uint)tSOPacketFormatCLSID, 
                    flags, 
                    transactionID, 
                    (uint)tSOSubMsgCLSID, 
                    body
            )
        {
        
        }              

        public TSODBRequestWrapper(string ariesID,
                                   string masterID,
                                   ushort bitfield_Arg1,
                                   uint messageSize,
                                   uint tSOPacketFormatCLSID,
                                   byte flags,
                                   uint transactionID,                                   
                                   uint tSOSubMsgCLSID,
                                   byte[] body) : base()
        {
            AriesID = ariesID;
            MasterID = masterID;
            Bitfield_Arg1 = bitfield_Arg1;
            if (((TSODBWrapperMessageSize)messageSize).IsAutoSize)
                messageSize = (uint)(DBWRAPPER_MESSAGESIZE_TO_BODY_DISTANCE + body.Length);
            if (messageSize > short.MaxValue)
                throw new OverflowException($"DBRequestWrapperPDU::MessageSize ({messageSize}) is way too large. (max: {short.MaxValue})");
            MessageSize = messageSize;
            TSOPacketFormatCLSID = tSOPacketFormatCLSID;
            TransactionID = transactionID;
            Flags = flags;
            TSOSubMsgCLSID = tSOSubMsgCLSID;
            DBMessageBody = body;
            FillPacketToAvailableSpace();
            MakeBodyFromProperties();
            ReadAdditionalMetadata();
        }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.DB_REQUEST_WRAPPER_PDU;

        [TSOVoltronString]
        public string AriesID { get; set; }
        [TSOVoltronString]
        public string MasterID { get; set; }
        public ushort Bitfield_Arg1 { get; set; }
        /// <summary>
        /// The distance (in bytes) from the end of the DWORD to the end of the packet. 
        /// <para>Basically, all other fields after this one are included in the "Body" of the packet.</para>
        /// <para>For clarity and usability, values stored in the Body have been pulled-up to the class level,
        /// such as <see cref="TSOPacketFormatCLSID"/></para>
        /// </summary>
        public uint MessageSize { get; set; }
        /// <summary>
        /// TSO has different classes in the library that correspond with the structure of these requests.
        /// <para>This is the identifier for which class should be created to house the data.</para>
        /// </summary>
        public uint TSOPacketFormatCLSID { get; set; }        
        public uint TransactionID { get; set; }   
        public byte Flags { get; set; }     
        /// <summary>
        /// Beneath the overall packet type there is a CLSID for the individual request being made.
        /// </summary>
        public uint TSOSubMsgCLSID { get; set; }
        /// <summary>
        /// All remaining bytes after the <see cref="TSOSubMsgCLSID"/>
        /// <para>This property is also used to get the values for Data1, 2, 3, ExtraCLSID, etc.</para>
        /// </summary>
        [TSOVoltronBodyArray]
        public byte[] DBMessageBody { get; set; }

        [TSOVoltronIgnorable] public bool HasData1 => (Flags & (1 << 1)) != 0;        
        [TSOVoltronIgnorable] public bool HasData2 => (Flags & (1 << 2)) != 0;        
        [TSOVoltronIgnorable] public bool HasData3 => (Flags & (1 << 3)) != 0;        
        [TSOVoltronIgnorable] public bool HasExtraCLSID => (Flags & (1 << 5)) != 0;        
        [TSOVoltronIgnorable] public bool HasString => (Flags & (1 << 6)) != 0;        
        [TSOVoltronIgnorable] public bool HasData4 => (Flags & (1 << 4)) != 0;        

        [TSOVoltronIgnorable] public uint Data1 { get; protected set; }        
        [TSOVoltronIgnorable] public uint Data2 { get; protected set; }        
        [TSOVoltronIgnorable] public uint Data3 { get; protected set; }
        [TSOVoltronIgnorable] public uint ExtraCLSID { get; protected set; }
        [TSOVoltronIgnorable] public string MessageString { get; protected set; }
        [TSOVoltronIgnorable] public uint Data4 { get; protected set; }

#if NewAndImprovedImplementation
        /// <summary>
        /// Uses the <see cref="DBMessageBody"/> combined with <see cref="Flags"/> to get remaining fields in this message.
        /// </summary>
        internal void ReadAdditionalMetadata()
        {
            SetPosition((int)(BodyLength - DBMessageBody.Length));
            if (HasData1)
                Data1 = ReadBodyDword();
            if (HasData2)
                Data2 = ReadBodyDword();
            if (HasData3)
                Data3 = ReadBodyDword();
            if (HasExtraCLSID)            
                ExtraCLSID = ReadBodyDword();
            if (HasString)
                MessageString = "FC FF strings not implemented yet!";
            if (HasData4)
                Data4 = ReadBodyDword();
        }
#else
        /// <summary>
        /// Uses the <see cref="DBMessageBody"/> combined with <see cref="Flags"/> to get remaining fields in this message.
        /// </summary>
        internal void ReadAdditionalMetadata()
        {
            IEnumerable<string> ReadStrings()
            {
                List<string> strings = new List<string>();
                while (true)
                {
                    ushort length = ReadBodyUshort();
                    if (length <= 0) break;
                    byte[] buffer = new byte[length];
                    ReadBodyByteArray(length);
                    strings.Add(Encoding.UTF8.GetString(buffer,0,length));
                }
                return strings;
            }
            SetPosition((int)(BodyLength - DBMessageBody.Length));
            uint value = ReadBodyDword();
            if (HasData1)
                Data1 = value;
            value = ReadBodyDword();
            if (HasData2)
                Data2 = value;
            value = ReadBodyDword();
            if (HasData3)
                Data3 = value;
            value = ReadBodyDword();
            if (HasExtraCLSID)
                ExtraCLSID = value;            
            value = ReadBodyDword();
            if (HasData4)
                Data4 = value;
            _bodyBuffer.Position += 3;
            MessageString = HasString ? string.Join(", ", ReadStrings()) : "None.";
        }
#endif
        /// <summary>
        /// Use this function to extend the Body property to the <see cref="MessageSize"/> parameter.
        /// <para>See: <see cref="MessageSize"/> property for more info.</para>
        /// </summary>
        protected void FillPacketToAvailableSpace()
        {
            //TSOPacketFormat + Flags + TransID + SubMSgCLSID + CurrentBodySize
            int currentSize = (int)DBWRAPPER_MESSAGESIZE_TO_BODY_DISTANCE + DBMessageBody.Length;
            int delta = (int)(MessageSize - currentSize); // size diff
            if (delta <= 0) return; // yikes, not necessary to even do this.
            int newSize = delta + DBMessageBody.Length;            
            DBMessageBody = DBMessageBody.TSOFillArray((uint)newSize);
            currentSize = (int)DBWRAPPER_MESSAGESIZE_TO_BODY_DISTANCE + DBMessageBody.Length;
            if (currentSize != MessageSize)
                throw new Exception("This should never happen.");
        }

        protected override IEnumerable<PropertyInfo> GetPropertiesToCopy() =>         
            GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(
                //Hard-Coded absolutely avoided names
                x => x.Name != "VoltronPacketType" && x.Name != "FriendlyPDUName" &&
                //Filter out all low-level properties that should not be encoded
                x.DeclaringType != typeof(TSOVoltronPacket) && x.DeclaringType != typeof(ExtendedBufferOperations) &&
                //Filter out any properties declared with TSOVoltronIgnorable
                x.GetCustomAttribute<TSOVoltronIgnorable>() == default
        );

        public override string ToString()
        {
            return $"{base.ToString()}\n " +
                $"Data1:{(HasData1 ? Data1 : "n/a")}\n " +
                $"Data2:{(HasData2 ? Data2 : "n/a")}\n " +
                $"Data3:{(HasData3 ? Data3 : "n/a")}\n " +
                $"Data4:{(HasData4 ? Data4 : "n/a")}\n " +
                $"Extra CLSID:{(HasExtraCLSID ? ExtraCLSID.ToString("X8") : "n/a")}\n " +
                $"Extra String:{(HasString ? MessageString : "n/a")}\n ";
        }
    }
}
