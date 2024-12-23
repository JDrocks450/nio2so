using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.City.Factory;
using nio2so.TSOTCP.City.TSO.Voltron.PDU;
using nio2so.TSOTCP.City.TSO.Voltron.PDU.Datablob;
using nio2so.TSOTCP.City.TSO.Voltron.PDU.Datablob.Structures;
using nio2so.TSOTCP.City.TSO.Voltron.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.Regulator
{
    [TSORegulator]
    internal class RoomProtocol : TSOProtocol
    {
        protected override bool OnUnknownDataBlobPDU(ITSODataBlobPDU PDU)
        {
            //RespondWith(PDU);
            return true;
        }

        [TSOProtocolDatablobHandler(TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage)]
        public void OnStandardMessage(ITSODataBlobPDU PDU)
        {
            var stdMessagePDU = PDU.DataBlobContentObject.GetAs<TSOStandardMessageContent>();
            if (stdMessagePDU.kMSG == TSO_PreAlpha_MasterConstantsTable.kMSGID_RequestAvatarID)
            {
                var kClientConnectedMsg = new TSOBroadcastDatablobPacket(
                    //new Struct.TSOAriesIDStruct("A 1337", ""),
                    TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage,
                    new TSOStandardMessageContent(TSO_PreAlpha_MasterConstantsTable.kMSGID_RequestAvatarID,
                    new byte[4] { 0x00,0x00,0x05,0x39 })
                );
                RespondTo((ITSOVoltronAriesMasterIDStructure)PDU, kClientConnectedMsg);
                return;
            }
            if (stdMessagePDU.kMSG == TSO_PreAlpha_MasterConstantsTable.kMSGID_JoinHouseResponse)
            {
                //00 03 4D 61
                var joinHouseResponse = new TSOBroadcastDatablobPacket(
                    //new Struct.TSOAriesIDStruct("A 1337", ""),
                    TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage,
                    new TSOStandardMessageContent(TSO_PreAlpha_MasterConstantsTable.kMSGID_JoinHouseResponse,
                    new byte[4] { 0x00, 0x00, 0x05, 0x3A })//{ 0x00, 0x03, 0x4D, 0x61 })
                );
                RespondTo((ITSOVoltronAriesMasterIDStructure)PDU, joinHouseResponse);
                return;
                //HOUSE DATA
                var housestatepacket = new TSOBroadcastDatablobPacket(
                    //new Struct.TSOAriesIDStruct("A 1337", ""),
                    TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage,
                    new TSOStandardMessageContent(TSO_PreAlpha_MasterConstantsTable.kMSGID_HouseData,
                    File.ReadAllBytes(@"E:\packets\house\HOUSESTATE.dat"))
                );
                RespondTo((ITSOVoltronAriesMasterIDStructure)PDU, housestatepacket);
                //MESSAGE HOUSE OCCUPANTS
                var msgHouseOccupants = new TSOBroadcastDatablobPacket(
                    //new Struct.TSOAriesIDStruct("A 1337", ""),
                    TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage,
                    new TSOStandardMessageContent(TSO_PreAlpha_MasterConstantsTable.kMSGID_MessageHouseOccupants,
                    File.ReadAllBytes(@"E:\packets\house\HOUSEOCCUPANTS.dat"))
                );
                RespondTo((ITSOVoltronAriesMasterIDStructure)PDU, msgHouseOccupants);
                return;
            }
            ;
            RespondWith((TSOVoltronPacket)PDU);
        }

        [TSOProtocolDatablobHandler(TSO_PreAlpha_MasterConstantsTable.GZCLSID_cTSOSimEvent)]
        public void OnSimEvent(ITSODataBlobPDU PDU)
        {
            return;
            /*var simEventPDU = PDU.DataBlobContentObject.GetAs<TSOSimEventContent>();
            simEventPDU.RefPackDataStream.FlipEndian();
            RespondTo((ITSOVoltronAriesMasterIDStructure)PDU, (ITSOVoltronAriesMasterIDStructure)PDU); // forces PDU to be reserialized with flipped endian numbers
            TSOFactoryBase.Get<TSOHouseFactory>().Debug_SetCustomDataToDisk((uint)simEventPDU.Simulator_kMSG, "simevent", 
                simEventPDU.RefPackDataStream.DecompressRefPack());*/
            
        }
    }
}
