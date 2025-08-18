namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{
    [TSOVoltronDBRequestWrapperPDU((uint)TSO_PreAlpha_DBActionCLSIDs.GetCharByID_Request)]
    public class TSOGetCharByIDRequest : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }

        public TSOGetCharByIDRequest() : base() { }
    }
}
