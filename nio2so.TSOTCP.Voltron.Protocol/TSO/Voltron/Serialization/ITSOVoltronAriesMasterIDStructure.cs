using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Serialization
{
    /// <summary>
    /// A type with AriesID and MasterID properties.
    /// </summary>
    public interface ITSOVoltronAriesMasterIDStructure
    {
        /// <summary>
        /// The current AriesID/MasterID combination that denotes the current Client
        /// </summary>
        public TSOAriesIDStruct CurrentSessionID { get; set; }
    }
}
