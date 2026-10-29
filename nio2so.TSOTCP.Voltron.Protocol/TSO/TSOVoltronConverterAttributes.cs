using nio2so.Data.Common;

namespace nio2so.Voltron.Core.TSO
{
    //**** CLASS LEVEL****

    /// <summary>
    /// Allows this <see cref="TSOVoltronPacket"/> to be mapped to a <see cref="TSO_PreAlpha_VoltronPacketTypes"/> to be
    /// created using Reflection when it is sent/received.
    /// <para/>See <see cref="TSOPDUFactory.CreatePacketObjectByPacketType(TSO_PreAlpha_VoltronPacketTypes, Stream, bool)"/> for implementation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class TSOVoltronPDU : Attribute
    {
        public TSOVoltronPDU(uint Type)
        {
            this.Type = Type;
        }

        public UIntEnum Type { get; }
    }

    /// <summary>
    /// Marks a type as a duplicate of a PDU found in another version TargetingPack.
    /// <para/>Has no affect on serialization or any behavior, just used for the IDE.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class TSOVoltronDuplicate : Attribute
    {
        public TSOVoltronDuplicate(params string[] Versions) { }
    }
}
