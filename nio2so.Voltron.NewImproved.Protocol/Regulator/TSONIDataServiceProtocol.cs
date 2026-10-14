using nio2so.Voltron.Core.Services;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Aries;
using nio2so.Voltron.Core.TSO.Regulator;
using nio2so.Voltron.NewImproved.Protocol.PDU;
using nio2so.Voltron.NewImproved.Protocol.PDU.MessageFormat;
using nio2so.Voltron.NewImproved.Protocol.Services;
using nio2so.Voltron.NewImproved.Protocol.Types.TSOValue;
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
    public class TSONIDataServiceProtocol : TSOProtocol
    {
        byte[] packet_data = {
		            /* Aries header */
		            0x00,0x00,0x00,0x00,
                    0xA1,0xA2,0xA3,0xA4,
                    0x37,0x01,0x00,0x00, /* hex: 40+50+6c+3b= 137 */

		            /* MyAvatar.Avatar_Name packet */
		            0x27,0x34,
                    0x00,0x00,0x00,0x40,
                    0xFE,0xED,0xF0,0x0D,
                    0xA4,0x6E,0x47,0xDC,
                    0x00,0x00,0x00,0x2E,
                    0x09,0x73,0x60,0x27,
                    0x0F,0xE8,0x80,0x3B,
                    0xA9,0x73,0x60,0xC5,
                    0x00,0x00,0x00,0x00,
                    0x00,0x00,0x00,0x03,
                    0x05,0x60,0x03,0x32,
                    0x00,0x00,0x05,0x39,
                    0x4F,0xFE,0x27,0x0C, // MyAvatar_Avatar_Name
                    0x89,0x6D,0x16,0x88, // string
                    0x08,0x4A,0x6F,0x6C,0x6C,0x79,0x53,0x69,0x6D, /* "JollySim" */
		            0x00,

		            /* MyAvatar.Avatar_Description packet */
		            0x27,0x34,
                    0x00,0x00,0x00,0x50,
                    0xFE,0xED,0xF0,0x0D,
                    0xA4,0x6E,0x47,0xDC, //MyAvatar
                    0x00,0x00,0x00,0x3E, // data length
                    0x09,0x73,0x60,0x27, // cTSOTopicUpdateMessage
                    0x0F,0xE8,0x80,0x3B, // update counter
                    0xA9,0x73,0x60,0xC5, // GZMSGID_cITSOApprovedTopicUpdate
                    0x00,0x00,0x00,0x00, // status code?
                    0x00,0x00,0x00,0x03, // items in this vector
                    0x05,0x60,0x03,0x32,
                    0x00,0x00,0x05,0x39,
                    0x54,0xB9,0x9C,0x9A, // avatar description
                    0x89,0x6D,0x16,0x88, // string
                    0x18,0x42,0x65,0x61,0x72,0x2D,0x79,0x20,0x6e,0x69,0x63,0x65,0x20,0x74,0x6f,0x20,
                    0x6d,0x65,0x65,0x74,0x20,0x79,0x6f,0x75,0x21, /* "Bear-y nice to meet you!" */
		            0x00,

		            /* MyAvatar.Avatar_Appearance packet */
		            0x27,0x34,
                    0x00,0x00,0x00,0x6C,
                    0xFE,0xED,0xF0,0x0D,
                    0xA4,0x6E,0x47,0xDC, // myAvatar
                    0x00,0x00,0x00,0x5A,
                    0x09,0x73,0x60,0x27,
                    0x0F,0xE8,0x80,0x3B,
                    0xA9,0x73,0x60,0xC5,
                    0x00,0x00,0x00,0x00,
                    0x00,0x00,0x00,0x03, // items in this vector
                    0x05,0x60,0x03,0x32,
                    0x00,0x00,0x05,0x39,
                    0xF1,0xB9,0x2C,0x99, // Variable: AvatarAppearance Avatar_Appearance
                    0xA9,0x6E,0x7E,0x5B,
                    0x89,0x73,0x9A,0x79,
                    0x3B,0x04,0x30,0xBF, // AvatarAppearance (type)
                    0x00,0x00,0x00,0x03,
                    0x1D,0x53,0x02,0x75, // BODYOUTFITID
                    0x69,0xD3,0xE3,0xDB, // uint64
                    0x00,0x00,0x01,0x60,0x00,0x00,0x00,0x0D, /* body outfit */
		            0x25,0x11,0xB2,0x9C, // SKIN TONE
                    0xC9,0x76,0x08,0x7C, // byte
                    0x00,
                    0x68,0x74,0x75,0xBF, // HEADID
                    0x69,0xD3,0xE3,0xDB, // uint64
                    0x00,0x00,0x04,0x0F,0x00,0x00,0x00,0x0D, /* head outfit */
		            0x00,

		            /* MyAvatar.Avatar_LotGridXY packet */
		            0x27,0x34,
                    0x00,0x00,0x00,0x3B,
                    0xFE,0xED,0xF0,0x0D,
                    0xA4,0x6E,0x47,0xDC, // MyAvatar
                    0x00,0x00,0x00,0x29,
                    0x09,0x73,0x60,0x27,
                    0x0F,0xE8,0x80,0x3B,
                    0xA9,0x73,0x60,0xC5,
                    0x00,0x00,0x00,0x00,
                    0x00,0x00,0x00,0x03,
                    0x05,0x60,0x03,0x32,
                    0x00,0x00,0x05,0x39,
                    0x7E,0x0D,0xD3,0x5C, // Avatar_LotGridXY
                    0x69,0x6D,0x11,0x89, // uint32
                    0x00,0xB8,0x00,0xD5,
                    0x00
            };

        [TSOProtocolHandler((ushort)TSO_NewImproved_VoltronPacketTypes.DataServiceWrapperPDU)]
        public void DATA_SERVICE_REQUEST(TSOVoltronPacket PDU)
        {
            if (PDU is not TSODataServiceWrapperPDU dataService)
                throw new InvalidOperationException("Invalid PDU type for DATA_SERVICE_REQUEST handler.");

            var stdMsg = dataService.Message.GetMessageAs<TSONetMessageStandard>();

            if (!TryGetService<TSODataDefinitionService>(out TSODataDefinitionService? service))
                throw new InvalidOperationException("Failed to get TSODataDefinitionService.");

            if (dataService.DefinitionStringID != 0xA46E47DC) return;

            object obj = service.GetObject(dataService.DefinitionStringID, out string structName);

            TSOTopicUpdateMessage avatarName = new(new TSOValue_String("Bisquick"), 0x4FFE270C, 0x05600332, 0x00000539);
            TSOTopicUpdateMessage avatarDescription = new(new TSOValue_String("Cheese'd to meet you."), 0x54B99C9A, 0x05600332, 0x00000539);

            RespondWith(new TSODataServiceWrapperPDU(dataService.SendingAvatarID, dataService.DefinitionStringID, new PDU.DataService.TSOServiceMessage(avatarName)));
            RespondWith(new TSODataServiceWrapperPDU(dataService.SendingAvatarID, dataService.DefinitionStringID, new PDU.DataService.TSOServiceMessage(avatarDescription)
            /*{
                Unknown_Flags = 0x0FE8,
                ParameterLength = 0x803B
            }*/));

            //TSOTopicUpdateMessage avatarName = new(new TSOValue_String("Bloaty"), 0x4FFE270C, 0x05600332, 0x00000539);            

            TSOTCPPacket packet = TSOTCPPacket.Parse<TSOTCPPacket>(packet_data, out _);

            var testPackets = GetService<TSONewImprovedPDUFactory>().CreatePacketObjectsFromAriesPacket(packet);            

            RespondWithAll(testPackets.Skip(2).ToArray());
        }
    }
}
