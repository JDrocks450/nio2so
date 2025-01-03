namespace nio2so.TSOHTTPS.Protocol.Data
{
    internal record UserAccount(string UserName, string Password)
    {
        public IEnumerable<uint> AvatarIDs { get; } = new List<uint>();
    }
}
