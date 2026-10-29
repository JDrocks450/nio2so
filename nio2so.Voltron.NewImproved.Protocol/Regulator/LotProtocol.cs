using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Regulator;
using nio2so.Voltron.PreAlpha.Protocol.PDU;

namespace nio2so.Voltron.NewImproved.Protocol.Regulator
{
    [TSORegulator("LotProtocol")]
    internal class LotProtocol : TSOProtocol
    {
        static uint tso_entry_test_i = 0;

        [TSOProtocolHandler((uint)TSO_NewImproved_VoltronPacketTypes.LotEntryRequestPDU)]
        public void LOT_ENTRY_REQUEST()
        {
            //TEST -- each time clicking join, a new error window will appear testing each case
            TSOUpdateRoomPDU.ReasonCodes[] values = Enum.GetValues<TSOUpdateRoomPDU.ReasonCodes>();

            TSOUpdateRoomPDU.ReasonCodes code = values[tso_entry_test_i];
            RespondWith(new TSOUpdateRoomPDU(code,Core.TSO.Struct.TSORoomInfoStruct.NoRoom));
        }
    }
}
