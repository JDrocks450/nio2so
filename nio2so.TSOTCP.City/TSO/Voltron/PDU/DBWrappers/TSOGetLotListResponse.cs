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
        [TSOVoltronDBWrapperField] public uint LotCount {  get; set; }
        [TSOVoltronDBWrapperField] public byte[] LotIDList { get; set; }

        public TSOGetLotListResponse(params uint[] LotIDs) : base(
                TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                TSO_PreAlpha_DBActionCLSIDs.GetLotList_Response
            )
        {
            Arg1 = 0x10111213;
            LotCount = (uint)LotIDs.Length;
            LotIDList = new byte[sizeof(uint) * LotCount];
            int index = -1;
            foreach (uint LotID in LotIDs)
            {
                index++;
                byte[] lotIdBytes = EndianBitConverter.Big.GetBytes(LotID);
                LotIDList[index * sizeof(uint)] = lotIdBytes[0];
                LotIDList[index * sizeof(uint) + 1] = lotIdBytes[1];
                LotIDList[index * sizeof(uint) + 2] = lotIdBytes[2];
                LotIDList[index * sizeof(uint) + 3] = lotIdBytes[3];
            }
            MakeBodyFromProperties();
        }
    }
}
