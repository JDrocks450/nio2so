namespace nio2so.Voltron.Core.TSO.Serialization
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
