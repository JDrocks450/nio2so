using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.Voltron.Protocol;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.Datablob;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.Datablob.Structures;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Regulator;

namespace nio2so.TSOTCP.HSBServer.Protocol
{
    [TSORegulator]
    internal class HSBRoomHostProtocol : TSOProtocol
    {
        /// <summary>
        /// Uses the RoomServer in the <see cref="HSBSession"/> to figure out the incoming packet
        /// </summary>
        private void IncomingConsultHSB(ITSODataBlobPDU PDU)
        {
            if (PDU is TSOTransmitDataBlobPacket transmitPDU)
                PDU = new TSOBroadcastDatablobPacket(transmitPDU);                
            HSBSession.CityServer.SendPacket(HSBSession.RoomServer, (TSOVoltronPacket)PDU);
        }

        protected override bool OnUnknownDataBlobPDU(ITSODataBlobPDU PDU)
        {
            IncomingConsultHSB(PDU);
            return true;
        }

        [TSOProtocolDatablobHandler(TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage)]
        public void OnStandardMessage(ITSODataBlobPDU PDU)
        {
            IncomingConsultHSB(PDU);
        }

        [TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes.LOAD_HOUSE_RESPONSE_PDU)]
        public void LOAD_HOUSE_RESPONSE_PDU(TSOVoltronPacket PDU)
        {
            //HOST PROTOCOL SHOULD ALWAYS TELL THE CLIENT THIS
            RespondWith(new TSOHouseSimConstraintsResponsePDU(TSOVoltronConst.MyHouseID)); // dictate what lot to load here.
            RespondWith(new TSOOccupantArrivedPDU(TestingConstraints.MyAvatarID, TestingConstraints.MyAvatarName));
        }
    }
}
