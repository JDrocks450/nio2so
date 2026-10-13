using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Aries;
using nio2so.Voltron.Core.TSO.Regulator;
using nio2so.Voltron.NewImproved.Protocol.PDU;
using nio2so.Voltron.NewImproved.Protocol.PDU.MessageFormat;
using nio2so.Voltron.NewImproved.Protocol.Services;
using QuazarAPI.Networking.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.NewImproved.Protocol.Regulator
{
    /// <summary>
    /// Handles all incoming DataService requests from the TSOClient
    /// </summary>
    [TSORegulator]
    public class TSONIDatabaseProtocol : TSOProtocol
    {
        [TSOProtocolHandler((ushort)TSO_NewImproved_VoltronPacketTypes.DBRequestWrapperPDU)]
        public void DATABASE_REQUEST(TSOVoltronPacket PDU)
        {
            if (PDU is not TSODBRequestWrapperPDU dbPDU)
                throw new InvalidOperationException("Invalid PDU type for DATABASE_REQUEST handler.");

            var stdMsg = dbPDU.Message.GetMessageAs<TSONetMessageStandard>();

            TSOTCPPacket response = PacketBase.Parse<TSOTCPPacket>(File.ReadAllBytes(@"C:\nio2so\const\loadavatarbyid.dat"), out _);
            var factory = GetService<TSONewImprovedPDUFactory>();
            TSOVoltronPacket? voltronResponse = factory.CreatePacketObjectsFromAriesPacket(response).FirstOrDefault();
            var stdMsg1 = (voltronResponse as TSODBRequestWrapperPDU).Message.GetMessageAs<TSONetMessageStandard>();
            if (voltronResponse != null)
                RespondWith(voltronResponse);
        }
    }
}
