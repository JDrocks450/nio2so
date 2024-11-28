using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// Nothing is known about this PDU... this is a placeholder file
    /// </summary>
    internal class TSOGetHouseLeaderByIDResponse : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; }
        [TSOVoltronDBWrapperField] public uint Filler { get; set; } = 0x0;
        [TSOVoltronDBWrapperField] public uint LeaderID { get; set; }

        public TSOGetHouseLeaderByIDResponse(uint HouseID, uint LeaderID) :
            base(
                    TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Response
                )
        {
            MakeBodyFromProperties();
        }
    }
}
