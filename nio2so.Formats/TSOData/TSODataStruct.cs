namespace nio2so.Formats.TSOData
{
    public class TSODataStruct : TSODataObject
    {
        public TSODataStruct(uint strID)
        {
            ParentFile = TSODataImporter.Current;
            NameID = strID;
        }
        public uint FieldCount => (uint)Fields.Count;
        public List<TSODataField> Fields { get; } = new();
    }
}
