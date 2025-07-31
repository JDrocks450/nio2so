using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.Datablob.Structures;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Serialization;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.Datablob
{
    public class TSOTransmitDatablobPDUHeader : ITSOVoltronSpecializedPDUHeader
    {
        public TSOPlayerInfoStruct SenderInfo { get; set; } = new();
        public ushort Arg1 { get; set; } = 0x4101;
        public TSOAriesIDStruct DestinationSessionID { get; set; } = new();
        public uint MessageLength { get; set; }
        public TSO_PreAlpha_MasterConstantsTable SubMsgCLSID { get; set; }
        public TSOGenericDataBlobContent DataBlobContentObject { get; set; }

        public void EnsureNoErrors()
        {
            if (SubMsgCLSID == TSO_PreAlpha_MasterConstantsTable.key_mResourceType) // 0x00000000!!!!
                throw new InvalidDataException($"{nameof(SubMsgCLSID)} is {SubMsgCLSID} (INVALID)");
        }
    }

    /// <summary>
    /// This <see cref="TSOVoltronSpecializedPacket{TAttribute, THeader}"/> is intended to transmit data to only the specified Client
    /// by their <see cref="TSOAriesIDStruct"/>.
    /// </summary>    
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.TRANSMIT_DATABLOB_PDU)]
    public class TSOTransmitDataBlobPacket : TSOVoltronSpecializedPacket<TSOVoltronBroadcastDatablobPDUField, TSOTransmitDatablobPDUHeader>,
        ITSOVoltronAriesMasterIDStructure, ITSODataBlobPDU
    {
        public TSOPlayerInfoStruct SenderInfo
        {
            get => Header.SenderInfo;
            set => Header.SenderInfo = value;
        }
        TSOAriesIDStruct ITSOVoltronAriesMasterIDStructure.SenderSessionID { get => SenderInfo.PlayerID; set => SenderInfo.PlayerID = value; }
        public TSOAriesIDStruct DestinationSessionID
        {
            get => Header.DestinationSessionID;
            set => Header.DestinationSessionID = value;
        }

        [TSOVoltronDistanceToEnd]
        public uint MessageLength
        {
            get => Header.MessageLength;
            set => Header.MessageLength = value;
        }
        public TSO_PreAlpha_MasterConstantsTable SubMsgCLSID
        {
            get => Header.SubMsgCLSID;
            set => Header.SubMsgCLSID = value;
        }
        public TSOGenericDataBlobContent DataBlobContentObject
        {
            get => Header.DataBlobContentObject;
            set
            {
                Header.DataBlobContentObject = value;
                Header.DataBlobContentObject?.SetCLSID(SubMsgCLSID); // used for ToString() mainly
            }
        }

        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.TRANSMIT_DATABLOB_PDU;
        protected override TSOTransmitDatablobPDUHeader Header { get; } = new();        

        /// <summary>
        /// This is the default, parameterless constuctor.       
        /// </summary>
        public TSOTransmitDataBlobPacket() : base()
        {
            MakeBodyFromProperties();
        }

        /// <summary>
        /// Creates a new <see cref="TSOTransmitDataBlobPacket"/> PDU where the argument list has been
        /// simplified to automate more of the properties that can be confusing otherwise.
        /// </summary>
        /// <param name="StructCLSID"></param>
        /// <param name="kMSG_ID"></param>
        /// <param name="DBAction"></param>
        /// <param name="Header"></param>
        /// <param name="Payload"></param>
        public TSOTransmitDataBlobPacket(TSOAriesIDStruct DestinationAddress,
            TSO_PreAlpha_MasterConstantsTable SubMsgCLSID,
            ITSODataBlobContentObject? Content = default,
            uint MessageLength = 0xFFFFFFFF) : base()
        {
            DestinationSessionID = DestinationAddress;
            this.SubMsgCLSID = SubMsgCLSID;
            DataBlobContentObject = new TSOGenericDataBlobContent(Content);

            this.MessageLength = MessageLength;
            MakeBodyFromProperties();
        }        
    }
}
