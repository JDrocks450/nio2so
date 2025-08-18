using nio2so.Data.Common.Serialization.Voltron;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{
    /// <summary>
    /// This PDU is used when the remote party wants to overwrite the Lot Profile data stored in 
    /// the Database for a given property
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU((uint)TSO_PreAlpha_DBActionCLSIDs.SetLotByID_Request)]
    public class TSOSetLotByIDRequest : TSODBRequestWrapper
    {
        /// <summary>
        /// The Database ID of the Lot we're setting data for
        /// </summary>
        [TSOVoltronDBWrapperField] public uint LotID { get; set; } // 1338, for example
        [TSOVoltronDBWrapperField] public uint Param1 { get; set; } // 1 in testing
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

        public TSOSetLotByIDRequest() : base(
                TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                TSO_PreAlpha_kMSGs.kDBServiceRequestMsg,
                TSO_PreAlpha_DBActionCLSIDs.SetLotByID_Request
            )
        {
            MakeBodyFromProperties();
        }
    }
}
