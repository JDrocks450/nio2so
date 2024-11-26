using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.SetHouseBlobByID_Request)]
    internal class TSOSetHouseBlobByIDRequest
    {
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }
        /// <summary>
        /// 0x00000001 ?? no idea
        /// </summary>
        [TSOVoltronDBWrapperField] public uint CompressionType { get; set; } = 0x1;
        /// <summary>
        /// Length to end of payload
        /// </summary>
        [TSOVoltronDBWrapperField] public uint DataLength { get; set; }
        [TSOVoltronDBWrapperField] public uint SARHeader { get; set; } = 0x5F534152; // "_SAR"
        [TSOVoltronDBWrapperField] public uint UncompressedSize { get; set; }
        [TSOVoltronDBWrapperField] public byte[] HouseBlobStream { get; set; }

        //from here, should start with 0x02?

        public TSOSetHouseBlobByIDRequest() : base() { }
    }
}
