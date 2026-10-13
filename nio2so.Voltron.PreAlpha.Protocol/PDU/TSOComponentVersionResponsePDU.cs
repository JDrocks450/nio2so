using nio2so.Voltron.Core.TSO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU
{
    [TSOVoltronPDU(ID)]
    internal class TSOComponentVersionResponsePDU : TSOVoltronPacket
    {
        private const ushort ID = (ushort)TSO_PreAlpha_VoltronPacketTypes.COMPONENT_VERSION_RESPONSE_PDU;

        public const string MyComponentName = "Nio2soVoltronRuntime";
        public const string PreAlphaRuntimeVersion = "1.3.1.56";
        /// <summary>
        /// Creates a new <see cref="TSOComponentVersionResponsePDU"/> with default values for the component name and version.
        /// </summary>
        public TSOComponentVersionResponsePDU() {
            MakeBodyFromProperties();
        }
        /// <summary>
        /// Creates a new <see cref="TSOComponentVersionResponsePDU"/> with the given component name and version.
        /// </summary>
        /// <param name="componentName"></param>
        /// <param name="componentVersion"></param>
        public TSOComponentVersionResponsePDU(string componentName, string componentVersion) : this() {
            ComponentName = componentName;
            ComponentVersion = componentVersion;
            MakeBodyFromProperties();
        }

        public override ushort VoltronPacketType => ID;
        /// <summary>
        /// Not sure what this does yet, but appears as 0x8001 in the packets I've seen. It may be a component ID for the given component.
        /// </summary>
        public ushort ComponentID { get; set; } = 0x8001;
        /// <summary>
        /// The name of the component, usually "ServerRuntime"
        /// </summary>
        [TSOVoltronString]
        public string ComponentName { get; set; } = MyComponentName;
        /// <summary>
        /// The version of the component. Matches the game build version string
        /// </summary>
        [TSOVoltronString]
        public string ComponentVersion { get; set; } = PreAlphaRuntimeVersion;
    }
}
