using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.NewImproved.Protocol.PDU.DataService;

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
        public uint DefinitionStringID { get; set; }

        /// <summary>
        /// The message contained in this <see cref="TSODataServiceWrapperPDU"/>
        /// </summary>
        public TSOServiceMessage Message { get; set; }

        public TSODataServiceWrapperPDU() : base()
        {
            
        }
    }
}
