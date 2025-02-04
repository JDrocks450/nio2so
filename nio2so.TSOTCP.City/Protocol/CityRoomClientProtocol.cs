﻿using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.Voltron.Protocol;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.Datablob;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.Datablob.Structures;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Regulator;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Serialization;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;

namespace nio2so.TSOTCP.City.TSO.Voltron.Regulator
{
    [TSORegulator]
    internal class CityRoomClientProtocol : TSOProtocol
    {
        /// <summary>
        /// Uses the RoomServer in the <see cref="HSBSession"/> to figure out the incoming packet
        /// </summary>
        private void IncomingConsultHSB(ITSODataBlobPDU PDU)
        {
            if (PDU is TSOTransmitDataBlobPacket transmitPDU)
                PDU = new TSOBroadcastDatablobPacket(transmitPDU);
            if(PDU.SubMsgCLSID == TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage)
            {
                if (PDU.DataBlobContentObject.GetAs<TSOStandardMessageContent>().kMSG == TSO_PreAlpha_MasterConstantsTable.kMSGID_RequestAvatarID)
                {
                    ;                                       
                }
            }
            HSBSession.RoomServer?.SendPacket(HSBSession.CityServer, (TSOVoltronPacket)PDU);
            if (HSBSession.RoomServer == null)
                RespondTo((ITSOVoltronAriesMasterIDStructure)PDU, (TSOBroadcastDatablobPacket)PDU);
        }       

        protected override bool OnUnknownDataBlobPDU(ITSODataBlobPDU PDU)
        {
            RespondWith((TSOVoltronPacket)PDU);
            //IncomingConsultHSB(PDU);
            return true;
        }

        TSOVoltronPacket donoPacket = null;

        [TSOProtocolDatablobHandler(TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage)]
        public void OnStandardMessage(ITSODataBlobPDU PDU)
        {
            if (HSBSession.RoomServer != null)
            {
                IncomingConsultHSB(PDU);
                return;
            }

            if (PDU is TSOTransmitDataBlobPacket transmitPDU)
                PDU = new TSOBroadcastDatablobPacket(transmitPDU);
#if true
            var stdMessagePDU = PDU.DataBlobContentObject.GetAs<TSOStandardMessageContent>();
            if (stdMessagePDU.kMSG == TSO_PreAlpha_MasterConstantsTable.kMSGID_RequestAvatarID)
            {
                var kClientConnectedMsg = new TSOBroadcastDatablobPacket(
                    //new Struct.TSOAriesIDStruct("A 1337", ""),
                    TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage,
                    new TSOStandardMessageContent(TSO_PreAlpha_MasterConstantsTable.kMSGID_AvatarID,
                    new byte[4] { 0x00, 0x00, 0x04, 0xC4 })
                );
                //RespondTo((ITSOVoltronAriesMasterIDStructure)PDU, kClientConnectedMsg);
                //AVATARID
                RespondWith((TSOVoltronPacket)PDU);
            }
            else if (stdMessagePDU.kMSG == TSO_PreAlpha_MasterConstantsTable.kMSGID_AvatarID)
            { // do nothing
                //RespondWith((TSOVoltronPacket)PDU);
                donoPacket = (TSOVoltronPacket)PDU;
            } 
            else if (stdMessagePDU.kMSG == TSO_PreAlpha_MasterConstantsTable.kMSGID_HouseData)
            {
                RespondWith(new TSOOccupantArrivedPDU(TestingConstraints.MyFriendAvatarID, TestingConstraints.MyFriendAvatarName));
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
                else
                {
                    return;
                    RespondWith((TSOVoltronPacket)PDU);
                }
            }        
#endif
        }

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

        int _attempts = 0;

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.LOAD_HOUSE_RESPONSE_PDU)]
        public void LOAD_HOUSE_RESPONSE_PDU(TSOVoltronPacket PDU)
        {
            /* From niotso:                  hello, fatbag - bisquick :]
             * TODO: It is known that sending HouseSimConstraintsResponsePDU (before the
            ** GetCharBlobByID response and other packets) is necessary for the game to post
            ** kMSGID_LoadHouse and progress HouseLoadRegulator from kStartedState to kLoadingState.
            ** However, the game appears to never send HouseSimConstraintsPDU to the server at
            ** any point.
            **
            ** The question is:
            ** Is there a packet that we can send to the game to have it send us
            ** HouseSimConstraintsPDU?
            ** Actually, (in New & Improved at least), manually sending a kMSGID_LoadHouse packet
            ** to the client (in an RSGZWrapperPDU) will cause the client to send
            ** HouseSimConstraintsPDU to the server.
            ** It is not known at this point if that is the "correct" thing to do.
            **
            ** So, for now, we will send the response packet to the game, without it having explicitly
            ** sent us a request packet--just like (in response to HostOnlinePDU) the game sends us a
            ** LoadHouseResponsePDU without us ever having sent it a LoadHousePDU. ;)
            */

            _attempts++;

            var houseID = ((TSOLoadHouseResponsePDU)PDU).HouseID;

            //The client will always send a LoadHouseResponsePDU with a blank houseID, so this can be ignored
            //when that happens
            if (houseID == 0)
            {
                if (!TestingConstraints.LOTTestingMode)
                    if (_attempts < 2) return; // break; 
            }
            
            //im in a lot -- send it the lot im supposed to be in
            RespondWith(new TSOHouseSimConstraintsResponsePDU(TSOVoltronConst.MyHouseID)); // dictate what lot to load here.
            return;

            RespondWith(
                new TSOBroadcastDatablobPacket(TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage,
                new TSOStandardMessageContent(TSO_PreAlpha_MasterConstantsTable.kMSGID_LoadHouse))
                {
                    CurrentSessionID = new TSOAriesIDStruct(
                        TestingConstraints.MyAvatarID,
                        TestingConstraints.MyAvatarName)
                });

            return;
            //HOUSE DATA
            RespondTo((ITSOVoltronAriesMasterIDStructure)donoPacket, GetHouseData());
            //MESSAGE HOUSE OCCUPANTS
            RespondTo((ITSOVoltronAriesMasterIDStructure)donoPacket, GetHouseOccupants());
            RespondTo((ITSOVoltronAriesMasterIDStructure)donoPacket,
                new TSOBroadcastDatablobPacket(TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage,
                new TSOStandardMessageContent(TSO_PreAlpha_MasterConstantsTable.kMSGID_LoadHouse)));
        }
    }
}
