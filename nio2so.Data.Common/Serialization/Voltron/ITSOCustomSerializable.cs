namespace nio2so.Data.Common.Serialization.Voltron
{
    public interface ITSOCustomSerialize
    {
        public byte[] OnSerialize();        
    }
    public interface ITSOCustomDeserialize
    {
        public void OnDeserialize(Stream Stream);
    }
}
