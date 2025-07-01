namespace nio2so.Formats.DB
{
    /// <summary>
    /// An X and Y coordinate position
    /// </summary>
    public class TSODBLotPosition
    {
        public TSODBLotPosition(uint x, uint y)
        {
            X = x;
            Y = y;
        }

        public uint X { get; set; }
        public uint Y { get; set; }
    }
}
