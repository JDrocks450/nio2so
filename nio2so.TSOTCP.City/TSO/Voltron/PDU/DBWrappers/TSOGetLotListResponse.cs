using nio2so.Formats.Util.Endian;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetLotList_Response)]
    internal class TSOGetLotListResponse : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint Arg1 { get; set; }
        [TSOVoltronDBWrapperField] public uint LotCount { get; set; } = 0x03;

        [TSOVoltronDBWrapperField] public byte[] BodyArray { get; set; }

        public TSOGetLotListResponse(params uint[] LotIDs) : base(
                TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                TSO_PreAlpha_DBActionCLSIDs.GetLotList_Response
            )
        {
            Arg1 = 0x10111213;
            byte startingNum = 10;
            BodyArray = new byte[128];
            for(int i = 0; i < BodyArray.Length; i += 4)
            {
                BodyArray[i + 0] = 0;
                BodyArray[i + 1] = 0;
                BodyArray[i + 2] = startingNum++;
                BodyArray[i + 3] = startingNum++;
            }

            MakeBodyFromProperties();
        }
    }
}
