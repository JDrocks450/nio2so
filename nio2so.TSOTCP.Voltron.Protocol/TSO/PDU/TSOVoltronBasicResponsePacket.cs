using nio2so.TSOTCP.Voltron.Protocol.TSO.Struct;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.PDU
{
    public abstract class TSOVoltronBasicResponsePacket : TSOVoltronPacket
    {
        public TSOStatusReasonStruct StatusReason { get; set; } = TSOStatusReasonStruct.Online;
        public bool Value { get; set; } = true;
        
        protected TSOVoltronBasicResponsePacket() : this(true) { }

        protected TSOVoltronBasicResponsePacket(bool Value, TSOStatusReasonStruct? StatusReason = default) : base()
        {
            if (StatusReason == default) StatusReason = TSOStatusReasonStruct.Default;
            this.Value = Value;
            this.StatusReason = StatusReason;
            MakeBodyFromProperties();            
        }
    }
}
