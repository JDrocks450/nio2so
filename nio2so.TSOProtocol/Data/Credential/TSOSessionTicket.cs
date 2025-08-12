namespace nio2so.TSOHTTPS.Protocol.Data.Credential
{
    /// <summary>
    /// Awarded by the Auth Server, is the session cookie for the current TSOClient.
    /// </summary>
    public struct TSOSessionTicket
    {
        public TSOSessionTicket(string Ticket)
        {
            this.Ticket = Ticket;
        }
        public static TSOSessionTicket Error => new TSOSessionTicket("0");
        public static TSOSessionTicket GetNext() => new(Guid.NewGuid().ToString());

        public string Ticket { get; set; }
        public override string ToString() => Ticket;
    }
}
