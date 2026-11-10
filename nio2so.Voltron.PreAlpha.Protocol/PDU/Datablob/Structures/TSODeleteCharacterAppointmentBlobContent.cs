using nio2so.Voltron.Core.TSO.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU.Datablob.Structures
{
    [TSOVoltronDatablobContent((uint)TSO_PreAlpha_MasterConstantsTable.GZCLSID_cDeleteCharacterAppointment)]
    internal class TSODeleteCharacterAppointmentBlobContent : ITSODataBlobContentObject
    {
        public TSODeleteCharacterAppointmentBlobContent() { }
        public TSODeleteCharacterAppointmentBlobContent(uint AvatarID) : this()
        {
            this.AvatarID = AvatarID;
        }

        /// <summary>
        /// AvatarID to delete
        /// </summary>
        public uint AvatarID { get; set; }
        /// <summary>
        /// Unsure about this but it is 0x0
        /// </summary>
        public uint StatusCode { get; set; } = 0x00000000;
        /// <summary>
        /// Could be a timestamp of some sort? AppointmentID? Unsure.
        /// </summary>
        public uint Unsure1 { get; set; } = 0x000354AC;
        /// <summary>
        /// Unsure again but is 0x01
        /// </summary>
        public uint Parameter1 { get; set; } = 0x01;
        /// <summary>
        /// Set to <see cref="TSO_PreAlpha_MasterConstantsTable.kLegacyAppointmentEvent"/>
        /// </summary>
        public TSO_PreAlpha_MasterConstantsTable kAppointmentTypeCLSID { get; set; } = TSO_PreAlpha_MasterConstantsTable.kLegacyAppointmentEvent;
        /// <summary>
        /// Set to <see cref="TSO_PreAlpha_kMSGs.kSimRequestMsg"/>
        /// </summary>
        public TSO_PreAlpha_kMSGs kAppointmentTypeMSG { get; set; } = TSO_PreAlpha_kMSGs.kSimRequestMsg;
        /// <summary>
        /// Remaining payload data which amounts to pretty much all zeros
        /// </summary>
        [Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes.TSOVoltronBodyArray]
        public byte[] SimEventData { get; set; } = new byte [] { 00, 00, 00, 00, 01, 00, 00, 00, 00, 00, 00, 00, 00, 00 };
    }
}
