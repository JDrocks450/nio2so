namespace nio2so.Voltron.NewImproved.Protocol.Types.TSOValue
{
    internal sealed class TSOValue_MapKeyValuePair<TValue>
    {
        public TSOValue_MapKeyValuePair()
        {
        }

        public TSOValue_MapKeyValuePair(uint key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public uint Key { get; set; }
        public TValue Value { get; set; }
    }
}
