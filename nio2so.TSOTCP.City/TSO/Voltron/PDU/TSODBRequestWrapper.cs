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
    /// Describes how to write the header info of a DBWrapperPDU
    /// </summary>
    internal class TSODBWrapperPDUHeader
    {
        public string AriesID { get; set; } = "";
        public string MasterID { get; set; } = "";
        public ushort Arg1 { get; set; } = 0x00;
        public uint MessageLength { get; set; }
        public TSO_PreAlpha_DBStructCLSIDs StructType { get; set; } = TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage;
        public byte HeaderLength { get; set; } = 0x21;
        public TSO_PreAlpha_kMSGs kMSGID { get; set; } = TSO_PreAlpha_kMSGs.kDBServiceResponseMsg;
        public TSO_PreAlpha_DBActionCLSIDs ActionType { get; set; }
    }

    internal class TSODBWrapperMessageSize
    {
        public TSODBWrapperMessageSize(uint Size) => this.Size = Size;
        /// <summary>
        /// Use this in the <see cref="TSODBRequestWrapper"/> constructor for MessageSize to have
        /// the packet autosize using the <see cref="TSODBRequestWrapper.DBMessageBody"/> property
        /// </summary>
        public static TSODBWrapperMessageSize AutoSize => 0xFFFFFFFF;
        public bool IsAutoSize => Size == 0xFFFFFFFF;
        public uint Size { get; set; } = 0xFFFFFFFF;

        public static implicit operator TSODBWrapperMessageSize(uint Other) => new TSODBWrapperMessageSize(Other);
        public static implicit operator uint(TSODBWrapperMessageSize Other) => Other.Size;
    }

    /// <summary>
    /// A class for cTSONetMessageStandard structs wrapped inside a <see cref="TSODBRequestWrapper"/> PDU
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.DB_REQUEST_WRAPPER_PDU)]
    internal class TSODBRequestWrapper : TSOVoltronPacket
    {
        /// <summary>
        /// The distance from <see cref="MessageLength"/> -> the start of <see cref="DBMessageBody"/>. Used here: <see cref="TSODBRequestWrapper()"/>
        /// </summary>
        protected const uint DBWRAPPER_MESSAGESIZE_TO_BODY_DISTANCE = (sizeof(uint) * 3) + 1;
        /// <summary>
        /// This is only true if AriesID and MasterID are blank.
        /// </summary>
        protected const uint DBWRAPPER_MESSAGESIZE_INDEX = 0x10;
        /// <summary>
        /// This is only true if AriesID and MasterID are blank.
        /// </summary>
        public const uint DB_WRAPPER_ACTIONCLSID_INDEX = 0x1D;

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.DB_REQUEST_WRAPPER_PDU;

        protected TSODBWrapperPDUHeader Header { get; } = new();

        [TSOVoltronString]
        public string AriesID {
            get => Header.AriesID;
            set => Header.AriesID = value;
        }
        [TSOVoltronString]
        public string MasterID
        {
            get => Header.MasterID;
            set => Header.MasterID = value;
        }
        public ushort Bitfield_Arg1
        {
            get => Header.Arg1;
            set => Header.Arg1 = value;
        }
        /// <summary>
        /// The distance (in bytes) from the end of this specific DWORD to the end of the packet. 
        /// <para>Basically, all other fields after this one are included in the "Body" of the packet.</para>
        /// <para>For clarity and usability, values stored in the Body have been pulled-up to the class level,
        /// such as <see cref="TSOPacketFormatCLSID"/></para>
        /// </summary>
        public uint MessageLength
        {
            get => Header.MessageLength;
            set => Header.MessageLength = value;
        }
        /// <summary>
        /// TSO has different classes in the library that correspond with the structure of these requests.
        /// <para>This is the identifier for which class should be created to house the data.</para>
        /// </summary>
        public uint TSOPacketFormatCLSID
        {
            get => (uint)Header.StructType;
            set => Header.StructType = (TSO_PreAlpha_DBStructCLSIDs)value;
        }
        /// <summary>
        /// The length of the header, is usually <c>0x21</c>
        /// </summary>
        public byte HeaderLength
        {
            get => Header.HeaderLength;
            set => Header.HeaderLength = value; 
        }
        public uint kMSGID
        {
            get => (uint)Header.kMSGID;
            set => Header.kMSGID = (TSO_PreAlpha_kMSGs)value;
        }
        /// <summary>
        /// Beneath the overall packet type there is a CLSID for the individual request being made.
        /// </summary>
        public uint TSOSubMsgCLSID
        {
            get => (uint)Header.ActionType;
            set => Header.ActionType = (TSO_PreAlpha_DBActionCLSIDs)value;
        }

        [TSOVoltronIgnorable] public uint? Data1 { get; protected set; }
        [TSOVoltronIgnorable] public uint? Data2 { get; protected set; }
        [TSOVoltronIgnorable] public uint? Data3 { get; protected set; }
        [TSOVoltronIgnorable] public uint? Data4 { get; protected set; }
        [TSOVoltronIgnorable] public uint? ExtraCLSID { get; protected set; }

        [TSOVoltronIgnorable]        
        public string MessageString => (Strings != null && Strings.Any()) ?
            new StringBuilder().AppendJoin(", ", Strings).ToString() : "";
        [TSOVoltronIgnorable] public List<string> Strings { get; protected set; } = new();

        /// <summary>
        /// This is the default, parameterless constuctor.
        /// <para/>You should use this in two scenarios: Using reflection to instantiate an instance of a type of 
        /// <see cref="TSODBRequestWrapper"/> OR writing a <see cref="TSODBRequestWrapper"/> that has a <see cref="TSODBRequestWrapper.kMSGID"/>
        /// that is: <see cref="TSO_PreAlpha_kMSGs.kDBServiceRequestMsg"/> as this is sent from the Client and the structure is not generated by the
        /// nio2so <see cref="TSOVoltronPacket"/> API        
        /// </summary>
        public TSODBRequestWrapper() : base() 
        {
            MakeBodyFromProperties();
        }

        /// <summary>
        /// Creates a new <see cref="TSODBRequestWrapper"/> PDU where the argument list has been
        /// simplified to automate more of the properties that can be confusing otherwise.
        /// </summary>
        /// <param name="StructCLSID"></param>
        /// <param name="kMSG_ID"></param>
        /// <param name="DBAction"></param>
        /// <param name="Header"></param>
        /// <param name="Payload"></param>
        public TSODBRequestWrapper(TSO_PreAlpha_DBStructCLSIDs StructCLSID,                                   
                                   TSO_PreAlpha_kMSGs kMSG_ID,
                                   TSO_PreAlpha_DBActionCLSIDs DBAction, uint MessageLength = 0xFFFFFFFF)
        {
            TSOPacketFormatCLSID = (uint)StructCLSID;
            kMSGID = (uint)kMSG_ID;
            TSOSubMsgCLSID = (uint)DBAction;

            this.MessageLength = MessageLength;
            MakeBodyFromProperties();                                 
        }

        /// <summary>
        /// Creates a new <see cref="TSODBRequestWrapper"/> PDU where all properties must be set manually.
        /// <para/> Data1,2,3 and 4 plus extra clsid will need to be set manually if applicable by using the 
        /// <paramref name="body"/> parameter and <paramref name="flags"/> will also need to be set manually.
        /// </summary>
        [Obsolete] 
        public TSODBRequestWrapper(string ariesID,
                                   string masterID,
                                   ushort bitfield_Arg1,
                                   uint messageSize,
                                   uint tSOPacketFormatCLSID,
                                   byte headerLength,
                                   uint kMSGID,
                                   uint tSOSubMsgCLSID,
                                   byte[] body)
            : this()
        {
            AriesID = ariesID;
            MasterID = masterID;
            Bitfield_Arg1 = bitfield_Arg1;
            if (((TSODBWrapperMessageSize)messageSize).IsAutoSize)
                messageSize = (uint)(DBWRAPPER_MESSAGESIZE_TO_BODY_DISTANCE + body.Length);
            if (messageSize > short.MaxValue)
                throw new OverflowException($"DBRequestWrapperPDU::MessageSize ({messageSize}) is way too large. (max: {short.MaxValue})");
            MessageLength = messageSize;
            TSOPacketFormatCLSID = (uint)tSOPacketFormatCLSID;
            this.kMSGID = (uint)kMSGID;
            HeaderLength = headerLength;
            TSOSubMsgCLSID = (uint)tSOSubMsgCLSID;
            FillPacketToAvailableSpace();
            MakeBodyFromProperties();
        }

        public override void MakeBodyFromProperties()
        {
            base.MakeBodyFromProperties();
            //**RE-EVALUATE SIZE
            if (MessageLength == TSODBWrapperMessageSize.AutoSize || MessageLength < BodyLength)
                MessageLength = BodyLength - (HeaderLength - DBWRAPPER_MESSAGESIZE_TO_BODY_DISTANCE);
            FillPacketToAvailableSpace();
            SetPosition((int)DBWRAPPER_MESSAGESIZE_INDEX);
            EmplaceBody(MessageLength);
        }

        public override int ReflectFromBody(Stream BodyStream)
        {
            //**these packets can have a lot of junk at the end of them
            //this function will trim the excess of the end of the packet by using the 
            //message length property.
            //btw, if the header size isn't 0x21, this function is liable to cause major issues.
            //so... should check that.

            //Read message size property from message body
            BodyStream.Seek(DBWRAPPER_MESSAGESIZE_INDEX, SeekOrigin.Begin);
            byte[] sizeBytes = new byte[sizeof(uint)];
            BodyStream.Read(sizeBytes, 0, sizeBytes.Length);
            uint msgSize = EndianBitConverter.Big.ToUInt32(sizeBytes,0);
            BodyStream.Seek(0, SeekOrigin.Begin);

            //read body bytes
            byte[] bodyBytes = new byte[DBWRAPPER_MESSAGESIZE_INDEX + sizeof(uint) + msgSize];
            BodyStream.Read(bodyBytes);
            using (MemoryStream ms = new MemoryStream(bodyBytes)) {
                return base.ReflectFromBody(ms);
            }
        }

        private IEnumerable<PropertyInfo> GetDBWrapperProperties()
        {
            return GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
            .Where(
                //Hard-Coded absolutely avoided names
                x => x.Name != "VoltronPacketType" && x.Name != "FriendlyPDUName" &&
                //Get only DBWrapperField attributed properties
                x.GetCustomAttribute<TSOVoltronDBWrapperField>() != default
            );
        }

        protected override IEnumerable<PropertyInfo> GetPropertiesToCopy()
        {
            List<PropertyInfo> properties = new List<PropertyInfo>();
            properties.AddRange(base.GetPropertiesToCopy());
            properties.AddRange(GetDBWrapperProperties());
            return properties;
        }

        /// <summary>
        /// For writing into the DBMessageHeader property of this <see cref="TSODBRequestWrapper"/> packet,
        /// you can use this function to move the position of the buffer to right after the <see cref="TSOSubMsgCLSID"/> property
        /// <para/> This is where <see cref="Data1"/> is located.
        /// </summary>
        protected void MoveBufferPositionToDBMessageHeader() => SetPosition(HeaderLength);

        /// <summary>
        /// Use this function to extend the Body property to the <see cref="MessageLength"/> parameter.
        /// <para>See: <see cref="MessageLength"/> property for more info.</para>
        /// </summary>
        protected void FillPacketToAvailableSpace()
        {
            //TSOPacketFormat + Flags + TransID + SubMSgCLSID + CurrentBodySize
            long currentSize = BodyLength;
            int delta = (int)(MessageLength - currentSize); // size diff
            if (delta <= 0) return; // yikes, not necessary to even do this.
            long newSize = delta + BodyLength; 
            byte[] trash = new byte[delta];
            ReallocateBody((uint)newSize);
            EmplaceBodyAt((int)BodyLength - delta, trash);
            currentSize = (int)DBWRAPPER_MESSAGESIZE_TO_BODY_DISTANCE + BodyLength;
            if (currentSize != MessageLength)
                throw new Exception("This should never happen.");
        }



        public override string ToString()
        {
            return ToShortString();
        }

        public override string ToShortString(string Arguments = "")
        {
            StringBuilder sb = new StringBuilder();
            foreach (var property in GetDBWrapperProperties())
                sb.Append($"{property.Name}: {property.GetValue(this)}, ");
            string text = sb.ToString();
            if (text.Length > 1)
                text = text.Remove(text.Length - 2);
            return $"{(TSO_PreAlpha_kMSGs)kMSGID}->{GetDBWrapperName()}({text})";
        }

        public string GetDBWrapperName() => $"{(TSO_PreAlpha_DBStructCLSIDs)TSOPacketFormatCLSID}::{(TSO_PreAlpha_DBActionCLSIDs)TSOSubMsgCLSID}";

        protected static byte[] CombineArrays(params byte[][] arrays)
        {
            if (!arrays.Any()) throw new ArgumentException("No arrays provided to combine!!!");
            if (arrays.Length < 2) throw new ArgumentException("Only one array provided to combine!!!");

            byte[] current = arrays[0];
            for (int i = 1; i < arrays.Length; i++)
            {
                byte[] next = arrays[i];

                int index = current.Length;
                Array.Resize(ref current, current.Length + next.Length);
                next.CopyTo(current, index);
            }
            return current;
        }        
    }
}
