using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.City.TSO.Voltron.PDU;
using nio2so.TSOTCP.City.TSO.Voltron.PDU.Datablob;
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
        [TSOProtocolBroadcastDatablobHandler(TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage)]
        public void OnStandardMessage(TSOBroadcastDatablobPacket PDU)
        {
            var stdMessagePDU = (TSOStandardMessageBroadcastPDU)PDU;
            ;
            RespondWith(PDU);
        }

        [TSOProtocolBroadcastDatablobHandler(TSO_PreAlpha_MasterConstantsTable.GZCLSID_cTSOSimEvent)]
        public void OnSimEvent(TSOBroadcastDatablobPacket PDU)
        {
            var simEventPDU = (TSOSimEventBroadcastPDU)PDU;
            ;
            return;
            File.WriteAllBytes(TestingConstraints.WorkspaceDirectory + "\\decompressed_simevent.dat", simEventPDU.RefPackDataStream.DecompressRefPack());
            RespondWith(new TSOBlankPDU(TSO_PreAlpha_VoltronPacketTypes.BROADCAST_DATABLOB_PDU,
                    File.ReadAllBytes(@"E:\packets\discoveries\BroadcastDataBlob(SimEvent).dat")));
        }
    }
}
