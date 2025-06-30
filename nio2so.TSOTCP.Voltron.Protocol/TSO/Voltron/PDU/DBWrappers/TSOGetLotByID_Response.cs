using nio2so.Data.Common.Serialization.Voltron;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// not implemented yet -- testing only
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetLotByID_Response)] 
    public class TSOGetLotByID_Response : TSODBRequestWrapper
    {
        /// <summary>
        /// The Database ID of the Lot we're setting data for
        /// </summary>
        [TSOVoltronDBWrapperField] public uint LotID { get; set; } // 1338, for example
        [TSOVoltronDBWrapperField] public uint Param1 { get; set; } = 1; // 1 in testing
        /// <summary>
        /// The name of the lot
        /// </summary>
        [TSOVoltronDBWrapperField]
        [TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)]
        public string LotName { get; set; } // "This is bisquick's property" for instance
        /// <summary>
        /// The description of the lot
        /// </summary>
        [TSOVoltronDBWrapperField]
        [TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)]
        public string LotDescription { get; set; } // "This is bisquick's property" for instance

        public TSOGetLotByID_Response(uint LotID, string Name, string Desc) :
            base(
                    TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetLotByID_Response
                )
        {
            this.LotID = LotID;
            LotName = Name;
            LotDescription = Desc;
            
            MakeBodyFromProperties();            
        }
    }
}
