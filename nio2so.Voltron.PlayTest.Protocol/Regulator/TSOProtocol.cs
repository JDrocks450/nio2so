using nio2so.Data.Common;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Aries;
using nio2so.Voltron.Core.TSO.Regulator;
using nio2so.Voltron.PlayTest.Protocol.PDU;
using nio2so.Voltron.PlayTest.Protocol.PDU.MessageFormat;
using nio2so.Voltron.PlayTest.Protocol.Services;
using QuazarAPI.Networking.Data;
using System.Reflection;

namespace nio2so.Voltron.PlayTest.Protocol.Regulator
{
    /// <summary>
    /// For use with The Sims Online Play-test Voltron Server.<para/>
    /// <inheritdoc cref="TSOProtocolBase"/>
    /// </summary>
    public abstract class TSOProtocol : TSOProtocolBase
    {
        /// <summary>
        /// <inheritdoc cref="TSOProtocolBase.TSOProtocolBase(TSOProtocolMatchingOption[])"/>
        /// </summary>
        protected TSOProtocol() : base(
                TSOProtocolMatchingOption.CreateVoltron<TSO_PlayTest_VoltronPacketTypes>()
            )
        { }

        protected override bool TryHandleSpecialVoltronPDU(TSOVoltronPacket PDU, ref TSOProtocolRegulatorResponse Response)
        {
            // Special VoltronPDU handling for TSO_PlayTest_Packets
            if (PDU is TSODataServiceWrapperPDU dataService)
            {
                var stdMsg = dataService.Message.GetMessageAs<TSONetMessageStandard>();
                return true;
            }
            if (PDU is TSODBRequestWrapperPDU dbPDU)
            {
                var stdMsg = dbPDU.Message.GetMessageAs<TSONetMessageStandard>();
                TSOTCPPacket response = PacketBase.Parse<TSOTCPPacket>(File.ReadAllBytes(@"C:\nio2so\const\loadavatarbyid.dat"), out _);
                TSOVoltronPacket? voltronResponse = GetService<TSOPlayTestPDUFactory>().CreatePacketObjectsFromAriesPacket(response).FirstOrDefault();
                var stdMsg1 = (voltronResponse as TSODBRequestWrapperPDU).Message.GetMessageAs<TSONetMessageStandard>();
                if (voltronResponse != null)
                    RespondWith(voltronResponse);
                return true;
            }
            return false;
        }
    }
}
