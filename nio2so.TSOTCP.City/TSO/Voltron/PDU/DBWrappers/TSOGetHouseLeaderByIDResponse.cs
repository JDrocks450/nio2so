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
        
        public TSOGetHouseLeaderByIDResponse(string AriesID, string MasterID, uint HouseID, uint LeaderID) : 
            base(AriesID,
                MasterID,
                    0x00,
                    TSODBWrapperMessageSize.AutoSize,
                    TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                    0x21,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetHouseBlobByID_Response,
                    new byte[]
                    {
                        0x00,0x00,0x05,0x3A, // <--- HOUSEID
                        0x01,
                        0x00,0x00,0x00,0x00,
                    }
                )
        {            
            MoveBufferPositionToDBMessageBody();
            EmplaceBody(HouseID);
            Advance(4);
            EmplaceBody(LeaderID);
            ReadAdditionalMetadata();
        }
    }
}
