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
    public interface ITSOCustomSerializableType : ITSOCustomSerialize, ITSOCustomDeserialize { }
    /// <summary>
    /// A <see cref="Delegate"/> that is the method signature for a <see cref="TSOVoltronCustomDeserializeBehavior"/>
    /// <para/>Name of function should match "OnDeserialize" with exactly matching signature.
    /// </summary>
    /// <param name="Stream"></param>
    /// <returns></returns>
    public delegate object TSOVoltron_OnDeserializeDelegate(Stream Stream);
}
