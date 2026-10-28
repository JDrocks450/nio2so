using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Aries;
using nio2so.Voltron.Core.TSO.Regulator;
using nio2so.Voltron.Core.TSO.Serialization;
using nio2so.Voltron.Core.TSO.Struct;
using nio2so.Voltron.NewImproved.Protocol.PDU;
using nio2so.Voltron.NewImproved.Protocol.PDU.Database;
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
                throw new InvalidOperationException($"Invalid PDU type for {nameof(DATABASE_REQUEST)} handler.");

            ITSOMessage tsoMsg = dbPDU.Message.GetMessageAs();

            if (tsoMsg is TSONetMessageStandard stdMsg)
            {
                if ((uint)stdMsg.DataDefinitionID == 0x4E2DF83B) // LoadAvatarByID
                {                    
                    var message = TSOVoltronSerializer.Deserialize<TSOLoadAvatarByIDRequestMessage>(stdMsg.MessageBytes);
                    foreach (var chunk in message.Chunks)
                    {
                        if (chunk.Request == Constants.TSO_NewImproved_DBServiceConstants.LoadAvatarByID_Request || 
                            chunk.Request == Constants.TSO_NewImproved_DBServiceConstants.LoadAvatarByID_Response)
                            if (HandleLoadAvatarByID(out TSOVoltronPacket? response))
                                RespondWith(response);
                    }
                }
            }
        }

        [TSOProtocolHandler((ushort)TSO_NewImproved_VoltronPacketTypes.List20RoomsResponsePDU)]
        public void LIST_ROOMS_REQUEST(TSOVoltronPacket PDU)
        {
            RespondWith(new TSOListRoomsResponsePDU(new Core.TSO.Struct.TSORoomInfoStruct(
                new TSORoomIDStruct(12058837,"Bisquick's Testing Zone"), new TSOAriesIDStruct(1337,"Bisquick"), 1)));
            return;
            RespondWith(new TSOList20RoomsResponsePDU(new Core.TSO.Struct.TSORoomInfoStruct(
                new TSORoomIDStruct(12058837, "Bisquick's Testing Zone"), new TSOAriesIDStruct(1337, "Bisquick"), 1)));
        }

        [TSOProtocolHandler((ushort)TSO_NewImproved_VoltronPacketTypes.RSGZWrapperPDU)]
        public void RSGZ_REQUEST(TSOVoltronPacket PDU)
        {
            if (PDU is not TSORSGZWrapperPDU rsgzPDU)
                throw new InvalidOperationException($"Invalid PDU type for {nameof(RSGZ_REQUEST)} handler.");

            ITSOMessage msg = rsgzPDU.Message.GetMessageAs();
            if (msg is TSONetMessageStandard stdMsg)
            {
                if (stdMsg.HasEmbeddedMessage)
                {
                    var subMsg = stdMsg.EmbeddedServiceMessage.GetMessageAs();
                    if (subMsg is TSOAvatarCreationRequest stream)
                        File.WriteAllBytes("dumb.dat", stream.AvatarDataStream);
                }
            }
            RespondWith(PDU);
        }

        bool HandleLoadAvatarByID(out TSOVoltronPacket Response)
        {
            TSOTCPPacket response = PacketBase.Parse<TSOTCPPacket>(File.ReadAllBytes(@"C:\nio2so\const\loadavatarbyid.dat"), out _);
            TSONewImprovedPDUFactory factory = GetService<TSONewImprovedPDUFactory>();
            TSODBRequestWrapperPDU? voltronResponse = (TSODBRequestWrapperPDU)factory.CreatePacketObjectsFromAriesPacket(response).FirstOrDefault();
            voltronResponse.SenderAvatarID = 1337;
            voltronResponse.MakeBodyFromProperties();
            ITSOMessage stdMsg1 = voltronResponse.Message.GetMessageAs();
            Response = voltronResponse;
            return Response != null;
        }
    }
}
