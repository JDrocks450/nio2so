namespace nio2so.DataService.Common.Tokens
{
    /// <summary>
    /// Represents the Version of The Sims Online
    /// </summary>
    public struct TSOVersionToken
    {
        public string Version { get; set; }
        public TSOVersionToken(string version) { Version = version; }
    }
}
