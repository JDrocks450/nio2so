namespace nio2so.Voltron.NewImproved.Protocol.Types.TSOValue
{
    internal class TSOValue_Boolean : TSOValueBase<bool>
    {
        const TSO_NewImproved_TSOValueBase_CLSIDs MYID = TSO_NewImproved_TSOValueBase_CLSIDs.cTSOValue_Bool;

        public TSOValue_Boolean()
        {
        }

        public TSOValue_Boolean(bool Value) : base(Value)
        {
        }

        public override TSO_NewImproved_TSOValueBase_CLSIDs TypeClassID { get; set; } = MYID;
    }
}