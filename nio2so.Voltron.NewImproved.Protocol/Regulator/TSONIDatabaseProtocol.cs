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

            ITSOMessage stdMsg = dbPDU.Message.GetMessageAs();

            TSOTCPPacket response = PacketBase.Parse<TSOTCPPacket>(File.ReadAllBytes(@"C:\nio2so\const\loadavatarbyid.dat"), out _);
            TSONewImprovedPDUFactory factory = GetService<TSONewImprovedPDUFactory>();
            TSODBRequestWrapperPDU? voltronResponse = (TSODBRequestWrapperPDU)factory.CreatePacketObjectsFromAriesPacket(response).FirstOrDefault();
            voltronResponse.SenderAvatarID = 1337;
            ITSOMessage stdMsg1 = voltronResponse.Message.GetMessageAs();
            if (voltronResponse != null)
                RespondWith(voltronResponse);
        }
    }
}
