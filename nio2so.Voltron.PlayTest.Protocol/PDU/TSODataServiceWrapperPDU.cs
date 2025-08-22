using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.PlayTest.Protocol.PDU.DataService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PlayTest.Protocol.PDU
{
    /// <summary>
    /// Type for TSODataServiceWrapper <see cref="TSO_PlayTest_VoltronPacketTypes.DataServiceWrapperPDU"/>
    /// </summary>
    public class TSODataServiceWrapperPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PlayTest_VoltronPacketTypes.DataServiceWrapperPDU;

        //**start schema

        /// <summary>
        /// The AvatarID of the player who sent this <see cref="TSODataServiceWrapperPDU"/>
        /// </summary>
        public uint SendingAvatarID { get; set; }
#if TSONANDI
        /// <summary>
        /// The ID of the string in the TSODataDefinition.dat this data service query structure        
        /// </summary>
        public uint DefinitionStringID { get; set; }
#endif
        /// <summary>
        /// The message contained in this <see cref="TSODataServiceWrapperPDU"/>
        /// </summary>
        public TSODataServiceMessage Message { get; set; }

        public TSODataServiceWrapperPDU() : base()
        {
            
        }
    }
}
