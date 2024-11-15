namespace nio2so.Protocol.Data.Credential
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
        public static TSOSessionTicket LoginDefault => new TSOSessionTicket("N2-LOGIN-D");
        public static TSOSessionTicket CityDefault => new TSOSessionTicket("N2-CITY-D");
        public static TSOSessionTicket Empty => new TSOSessionTicket("0");
        public string Ticket { get; set; }
        public override string ToString() => Ticket;
    }
}
