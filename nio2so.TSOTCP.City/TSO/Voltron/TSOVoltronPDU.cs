namespace nio2so.TSOTCP.City.TSO.Voltron
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    class TSOVoltronPDU : Attribute
    {
        public TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes Type)
        {
            this.Type = Type;
        }

        public TSO_PreAlpha_VoltronPacketTypes Type { get; }
    }
    class TSOVoltronDBRequestWrapperPDU : Attribute
    {
        public TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs ActionCLSID)
        {
            this.Type = ActionCLSID;
        }

        public TSO_PreAlpha_DBActionCLSIDs Type { get; }
    }
}
