using nio2so.Data.Common.Testing;
using nio2so.Formats.Util.Endian;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Util;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.SearchExactMatch_Response)]
    public class TSOExactSearchResponse : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint SearchType { get; set; }
        [TSOVoltronDBWrapperField] public uint NumResults { get; set; }
        [TSOVoltronDBWrapperField] public byte[] ResultsVec { get; set; }

        /// <summary>
        /// Default parameterless constructor. Please use overload for programmatically creating PDUs.
        /// </summary>
        public TSOExactSearchResponse() : base() { }

        public TSOExactSearchResponse(TSO_PreAlpha_SearchCategories SearchType, params uint[] ResultIDs) : base(
                TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                TSO_PreAlpha_DBActionCLSIDs.SearchExactMatch_Response
            )
        {
            this.SearchType = (uint)SearchType;
            var array = new byte[]
            {
                0x01,0x01,0x41
            };
            Array.Resize(ref array, 16);
            ResultsVec = array;
            NumResults = 1;
            MakeBodyFromProperties();
            return;
            NumResults = (uint)ResultIDs.Length * sizeof(uint);
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
