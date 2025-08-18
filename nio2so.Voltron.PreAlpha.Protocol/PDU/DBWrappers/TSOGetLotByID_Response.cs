using nio2so.Data.Common.Serialization.Voltron;
using nio2so.DataService.Common.Types.Lot;
using nio2so.Voltron.Core.TSO.Util;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{
    /// <summary>
    /// not implemented yet -- testing only
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU((uint)TSO_PreAlpha_DBActionCLSIDs.GetLotByID_Response)] 
    public class TSOGetLotByID_Response : TSODBRequestWrapper
    {
        /// <summary>
        /// The Database ID of the Lot we're setting data for
        /// </summary>
        [TSOVoltronDBWrapperField] public uint LotID { get; set; } // 1338, for example
        /// <summary>
        /// The name of this lot, set by the owner
        /// </summary>
        [TSOVoltronDBWrapperField]
        [TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)]
        public string LotName { get; set; } // "This is bisquick's property" for instance
        /// <summary>
        /// The name of the owner avatar
        /// <para/>This is purely speculation
        /// </summary>
        [TSOVoltronDBWrapperField]
        [TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)]
        public string OwnerName { get; set; } 
        /// <summary>
        /// The grid position of this lot in the TSO World View
        /// <para/> This <b>needs</b> to be correct or else you will only get "Loading..." on the LotPage, for example
        /// </summary>
        [TSOVoltronDBWrapperField]
        public LotPosition LotPosition { get; set; }
        /// <summary>
        /// The description of the lot
        /// </summary>
        [TSOVoltronDBWrapperField]
        [TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)]
        public string LotDescription { get; set; } // "Home sweet home!" for instance

        [TSOVoltronDBWrapperField]
        public uint Param1 { get; set; } = 0x01;

        [TSOVoltronDBWrapperField]
        public uint Param2 { get; set; } = 0x01;

        [TSOVoltronDBWrapperField]
        public uint Param3 { get; set; } = 0x01;

        [TSOVoltronDBWrapperField] 
        [TSOVoltronBodyArray]
        public byte[] Garbage1 => (new byte[50]).TSOFillArray();

        public TSOGetLotByID_Response() :
            base(
                    TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetLotByID_Response
                )
        {

        }

        /// <summary>
        /// Creates a new <see cref="TSOGetLotByID_Response"/> with the given parameters
        /// </summary>
        /// <param name="LotID"></param>
        /// <param name="Name"></param>
        /// <param name="Owner"></param>
        /// <param name="Description"></param>
        /// <param name="Position"></param>
        public TSOGetLotByID_Response(uint LotID, string Name, string Owner, string Description, LotPosition Position) : this()
        {
            this.LotID = LotID;
            LotName = Name;
            LotDescription = Description;
            LotPosition = Position;
            OwnerName = Owner;
            
            MakeBodyFromProperties();            
        }
    }
}
