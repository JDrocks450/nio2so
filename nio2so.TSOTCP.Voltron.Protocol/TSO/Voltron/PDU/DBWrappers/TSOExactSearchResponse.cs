using nio2so.Formats.Util.Endian;
using nio2so.TSOTCP.City.TSO.Voltron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.SearchExactMatch_Response)]
    internal class TSOExactSearchResponse : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint SearchType { get; set; }
        [TSOVoltronDBWrapperField] public uint NumResults { get; set; }
        [TSOVoltronDBWrapperField] public byte[] ResultsVec { get; set; }

        public TSOExactSearchResponse(TSO_PreAlpha_SearchCategories SearchType, params uint[] ResultIDs) : base(
                TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                TSO_PreAlpha_DBActionCLSIDs.SearchExactMatch_Response
            )
        {
            this.SearchType = (uint)SearchType;
            NumResults = (uint)ResultIDs.Length;
            ResultsVec = new byte[sizeof(uint) * ResultIDs.Length];
            int index = -1;
            foreach (uint DB_ID in ResultIDs)
            {
                index++;
                byte[] lotIdBytes = EndianBitConverter.Big.GetBytes(DB_ID);
                ResultsVec[index * sizeof(uint)] = lotIdBytes[0];
                ResultsVec[index * sizeof(uint) + 1] = lotIdBytes[1];
                ResultsVec[index * sizeof(uint) + 2] = lotIdBytes[2];
                ResultsVec[index * sizeof(uint) + 3] = lotIdBytes[3];
            }
            MakeBodyFromProperties();
        }
    }
}
