using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.NewImproved.Protocol.PDU.DataService;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.NewImproved.Protocol.PDU
{
    /// <summary>
    /// Type for TSODataServiceWrapper <see cref="TSO_NewImproved_VoltronPacketTypes.DataServiceWrapperPDU"/>
    /// </summary>
    public class TSODataServiceWrapperPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_NewImproved_VoltronPacketTypes.DataServiceWrapperPDU;

        //**start schema

        /// <summary>
        /// The AvatarID of the player who sent this <see cref="TSODataServiceWrapperPDU"/>
        /// </summary>
        public uint SendingAvatarID { get; set; }

        /// <summary>
        /// The ID of the string in the TSODataDefinition.dat this data service query structure        
        /// </summary>
        public TSO_NewImproved_TSOData_StructIDs DefinitionStringID { get; set; }

        /// <summary>
        /// The length of the <see cref="ContentBytes"/> property
        /// </summary>
        [TSOVoltronDistanceToEnd]
        public uint ContentLength { get; set; }

        /// <summary>
        /// The message contained in this <see cref="TSODataServiceWrapperPDU"/>
        /// </summary>
        public TSOServiceMessage Message { get; set; }

        public TSODataServiceWrapperPDU() : base()
        {
            MakeBodyFromProperties();
        }

        public TSODataServiceWrapperPDU(uint SendingAvatarID, TSO_NewImproved_TSOData_StructIDs TSODataDefinitionID, TSOServiceMessage MessageContent) : this()
        {
            this.SendingAvatarID = SendingAvatarID;
            DefinitionStringID = TSODataDefinitionID;
            Message = MessageContent;

            MakeBodyFromProperties();
        }
    }
}
