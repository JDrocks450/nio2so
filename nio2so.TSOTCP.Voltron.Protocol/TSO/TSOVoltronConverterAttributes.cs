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
}
