using nio2so.TSOTCP.Voltron.Protocol.TSO;
using nio2so.TSOTCP.Voltron.Protocol.TSO.PDU.Datablob;
using nio2so.TSOTCP.Voltron.Protocol.TSO.PDU.Datablob.Structures;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Regulator;

namespace nio2so.TSOTCP.City.TSO.Voltron.Regulator
{

    [TSORegulator]
    internal class CityRoomClientProtocol : TSOProtocol
    {
#if false

            var stdMessagePDU = PDU.DataBlobContentObject.GetAs<TSOStandardMessageContent>();
            if (stdMessagePDU.kMSG == TSO_PreAlpha_MasterConstantsTable.kMSGID_RequestAvatarID)
            {
                //AVATARID
                RespondWith((TSOVoltronPacket)PDU);
            }
            else if (stdMessagePDU.kMSG == TSO_PreAlpha_MasterConstantsTable.kMSGID_AvatarID)
            { // do nothing
                RespondWith((TSOVoltronPacket)PDU);
                donoPacket = (TSOVoltronPacket)PDU;
                RespondWith(new TSOOccupantArrivedPDU(new(EndianBitConverter.Big.ToUInt32(PDU.DataBlobContentObject.ContentBytes,0), "bisquick")));
            }
            else if (stdMessagePDU.kMSG == TSO_PreAlpha_MasterConstantsTable.kMSGID_HouseData)
            {
                //HOUSE DATA
                RespondTo((ITSOVoltronAriesMasterIDStructure)PDU, GetHouseData());
                //MESSAGE HOUSE OCCUPANTS
                RespondTo((ITSOVoltronAriesMasterIDStructure)PDU, GetHouseOccupants());
            }
            else if (stdMessagePDU.kMSG == TSO_PreAlpha_MasterConstantsTable.kMSGID_MessageHouseOccupants)
            {
                //MESSAGE HOUSE OCCUPANTS
                RespondTo((ITSOVoltronAriesMasterIDStructure)PDU, GetHouseOccupants());
            }
            else if (stdMessagePDU.kMSG == TSO_PreAlpha_MasterConstantsTable.kMSGID_JoinHouseResponse)
            {
                RespondTo((ITSOVoltronAriesMasterIDStructure)PDU, GetJoinResponse());                
            }
            else
            {
                if (PDU is TSOTransmitDataBlobPacket)
                    RespondTo((ITSOVoltronAriesMasterIDStructure)PDU,
                        new TSOBroadcastDatablobPacket(
                            PDU.SubMsgCLSID,
                            PDU.DataBlobContentObject.ContentBytes
                        ));
                else return;                    
            } 
        }
#endif

        private TSOBroadcastDatablobPacket GetJoinResponse() =>
            new TSOBroadcastDatablobPacket(
                    //new Struct.TSOAriesIDStruct("A 1337", ""),
                    TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage,
                    new TSOStandardMessageContent(TSO_PreAlpha_MasterConstantsTable.kMSGID_JoinHouseResponse,
                    new byte[4] { 0x00, 0x03, 0x4D, 0x61 })
                );

        private TSOBroadcastDatablobPacket GetHouseOccupants() =>
            //MESSAGE HOUSE OCCUPANTS
            new TSOBroadcastDatablobPacket(
                //new Struct.TSOAriesIDStruct("A 1337", ""),
                TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage,
                new TSOStandardMessageContent(
                    TSO_PreAlpha_MasterConstantsTable.kMSGID_MessageHouseOccupants,
                    File.ReadAllBytes(@"E:\packets\house\HOUSEOCCUPANTS.dat")
                )
                {
                    BufferStartByte = 0x43
                });

        private TSOBroadcastDatablobPacket GetHouseData() =>
            new TSOBroadcastDatablobPacket(
                //new Struct.TSOAriesIDStruct("A 1337", ""),
                TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage,
                new TSOStandardMessageContent(TSO_PreAlpha_MasterConstantsTable.kMSGID_HouseData,
                File.ReadAllBytes(@"E:\packets\house\HOUSESTATE.dat"))
                {
                    BufferStartByte = 0x3F
                }
            );        
    }
}
