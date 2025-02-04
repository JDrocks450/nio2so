﻿using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU
{
    /// <summary>
    /// Sent from the client when a new avatar is created
    /// </summary>
    [TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes.TRANSMIT_CREATEAVATARNOTIFICATION_PDU)]
    public class TSOCreateAvatarNotificationPDU : TSOVoltronPacket
    {
        public override ushort VoltronPacketType => (ushort)TSO_PreAlpha_VoltronPacketTypes.TRANSMIT_CREATEAVATARNOTIFICATION_PDU;

        /// <summary>
        /// The ID of the Avatar that was just created
        /// </summary>
        [TSOVoltronString]
        public string AvatarID { get; set; }
        /// <summary>
        /// The name of the avatar that was just created
        /// </summary>
        [TSOVoltronString]
        public string AvatarName { get; set; } = "";
        /// <summary>
        /// The ID of the Avatar that was just created... again?
        /// <para/> Could be the account ID this avatar was created under? Requires testing.
        /// </summary>
        [TSOVoltronString]
        public string AvatarID2 { get; set; }

        public TSOCreateAvatarNotificationPDU() : base()
        {
            MakeBodyFromProperties();
        }
    }
}
