namespace nio2so.Voltron.Core.TSO.Struct
{
    /// <summary>
    /// A type with AriesID and MasterID properties.
    /// </summary>
    public interface ITSOVoltronAriesMasterIDStructure
    {
        /// <summary>
        /// The current AriesID/MasterID combination that denotes the current Client
        /// </summary>
        public TSOPlayerInfoStruct SenderSessionID { get; set; }
    }
}
