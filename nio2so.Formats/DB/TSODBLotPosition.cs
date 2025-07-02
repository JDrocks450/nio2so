namespace nio2so.Formats.DB
{
    /// <summary>
    /// A Grid Position in the TSO World View
    /// </summary>
    public class TSODBLotPosition
    {
        public TSODBLotPosition() { }
        public TSODBLotPosition(uint x, uint y) : this()
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
