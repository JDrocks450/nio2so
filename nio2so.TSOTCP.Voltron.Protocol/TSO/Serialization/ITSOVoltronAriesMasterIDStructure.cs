using nio2so.TSOTCP.Voltron.Protocol.TSO.Struct;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Serialization
{
    /// <summary>
    /// A type with AriesID and MasterID properties.
    /// </summary>
    public interface ITSOVoltronAriesMasterIDStructure
    {
        /// <summary>
        /// The current AriesID/MasterID combination that denotes the current Client
        /// </summary>
        public TSOAriesIDStruct SenderSessionID { get; set; }
    }
}
