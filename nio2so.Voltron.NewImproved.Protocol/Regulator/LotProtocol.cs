using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Regulator;
using nio2so.Voltron.NewImproved.Protocol.PDU;
using nio2so.Voltron.PreAlpha.Protocol.PDU;

namespace nio2so.Voltron.NewImproved.Protocol.Regulator
{
    [TSORegulator("LotProtocol")]
    internal class LotProtocol : TSOProtocol
    {
        static uint tso_entry_test_i = 0;

        [TSOProtocolHandler((uint)TSO_NewImproved_VoltronPacketTypes.LotEntryRequestPDU)]
        public void LOT_ENTRY_REQUEST(TSOVoltronPacket PDU)
        {
            if (PDU is not TSOLotEntryRequestPDU lotEntry) return;
            //TEST -- each time clicking join, a new error window will appear testing each case
            TSOUpdateRoomPDU.ReasonCodes[] values = Enum.GetValues<TSOUpdateRoomPDU.ReasonCodes>();

            TSOUpdateRoomPDU.ReasonCodes code = values[tso_entry_test_i++];
            RespondWith(new TSOUpdateRoomPDU(new Core.TSO.Struct.TSORoomInfoStruct(
                new Core.TSO.Struct.TSORoomIDStruct(lotEntry.GridXY, "Bloaty Land"),
                new Core.TSO.Struct.TSOAriesIDStruct(1337,"Bisquick"),1)));
        }
    }
}
