using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Struct;
using nio2so.Voltron.NewImproved.Protocol.PDU.DataService;
using nio2so.Voltron.NewImproved.Protocol.PDU.MessageFormat;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.NewImproved.Protocol.PDU
{
    [TSOVoltronPDU((uint)TSO_NewImproved_VoltronPacketTypes.DBRequestWrapperPDU)]
    public class TSODBRequestWrapperPDU : TSOVoltronPacket
    {
        public TSODBRequestWrapperPDU() : base()
        {
            MakeBodyFromProperties();
        }

        public TSODBRequestWrapperPDU(TSOPlayerInfoStruct playerInfo, TSOServiceMessage message) : this()
        {
            SenderAvatarID = playerInfo.PlayerID.AvatarID;
            PlayerInfo = playerInfo;
            Message = message;

            MakeBodyFromProperties();
        }
        public TSODBRequestWrapperPDU(TSOPlayerInfoStruct playerInfo, ITSODatabaseNetMessageStandard message) :
            this(playerInfo, new TSOServiceMessage(message))
        {
        
        }

        public override ushort VoltronPacketType => (ushort)TSO_NewImproved_VoltronPacketTypes.DBRequestWrapperPDU;

        /// <summary>
        /// <inheritdoc cref="TSODataServiceWrapperPDU.SendingAvatarID"/>
        /// </summary>
        public uint SenderAvatarID { get; set; }
        /// <summary>
        /// m_PlayerInfo
        /// </summary>
        public TSOPlayerInfoStruct PlayerInfo { get; set;  }
        /// <summary>
        /// The length of the <see cref="ContentBytes"/> property
        /// </summary>
        [TSOVoltronDistanceToEnd]
        public uint ContentLength { get; set; }
        /// <summary>
        /// <inheritdoc cref="TSODataServiceWrapperPDU.Message"/>
        /// </summary>
        public TSOServiceMessage Message { get; set; }

        /// <summary>
        /// If the <see cref="Message"/> is <see cref="TSONetMessageStandard"/> then casts it to <see cref="ITSODatabaseNetMessageStandard"/>, otherwise null.
        /// <para/>This is helpful for peeking at what the <see cref="ITSODatabaseNetMessageStandard.FormatID"/> would be a named constant.
        /// </summary>
        public ITSODatabaseNetMessageStandard? DataServiceNetMessage => Message.GetMessageAs() as ITSODatabaseNetMessageStandard;
    }
}
