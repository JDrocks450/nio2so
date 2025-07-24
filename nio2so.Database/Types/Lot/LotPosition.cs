namespace nio2so.DataService.Common.Types.Lot
{
    /// <summary>
    /// A Grid Position in the TSO World View
    /// <code>uint X, uint Y</code>
    /// </summary>
    public class LotPosition
    {
        public LotPosition() { }
        public LotPosition(uint x, uint y) : this()
        {
            X = x;
            Y = y;
        }

        public uint X { get; set; } = 0;
        public uint Y { get; set; } = 0;

        public override string ToString()
        {
            return $"{X},{Y}";
        }
    }
}
