using nio2so.Data.Common;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.PreAlpha.Protocol.PDU;
using nio2so.Voltron.PreAlpha.Protocol.Services;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PreAlpha.Protocol
{
    //**** CLASS LEVEL****

    /// <summary>
    /// Similar to <see cref="TSOVoltronPDU"/>. When a packet is created using Reflection using a mapped <see cref="TSOVoltronPDU"/> - if it
    /// is a <see cref="TSO_PreAlpha_VoltronPacketTypes.DB_REQUEST_WRAPPER_PDU"/> it will have its header tested against this <see cref="TSO_PreAlpha_DBActionCLSIDs"/>
    /// provided to match this <see cref="TSODBRequestWrapper"/> implementation to the incoming/outgoing data.
    /// <para>See <see cref="TSOPreAlphaPDUFactory.CreatePacketObjectByPacketType(TSO_PreAlpha_VoltronPacketTypes, Stream, bool)"/> for implementation</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    class TSOVoltronDBRequestWrapperPDU : Attribute
    {
        public TSOVoltronDBRequestWrapperPDU(uint ActionCLSID)
        {
            Type = ActionCLSID;
        }

        public UIntEnum Type { get; }
    }
    /// <summary>
    /// Similar to <see cref="TSOVoltronPDU"/>. When a packet is created using Reflection using a mapped <see cref="TSOVoltronPDU"/> - if it
    /// is a <see cref="TSO_PreAlpha_VoltronPacketTypes.BROADCAST_DATABLOB_PDU"/> it will have its header tested against this <see cref="TSO_PreAlpha_MasterConstantsTable"/>
    /// provided to match this <see cref="TSOVoltronDatablobContent"/> implementation to the incoming/outgoing data.
    /// <para>See <see cref="TSOPreAlphaPDUFactory.CreatePacketObjectByPacketType(TSO_PreAlpha_VoltronPacketTypes, Stream, bool)"/> for implementation</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    class TSOVoltronDatablobContent : Attribute
    {
        public TSOVoltronDatablobContent(uint ActionCLSID)
        {
            Type = ActionCLSID;
        }

        public UIntEnum Type { get; }
    }

    //***PROPERTY LEVEL    

    /// <summary>
    /// Tells the parser/decoder logic in <see cref="TSOVoltronPacket"/> to just ignore this variable -- because it is in a <see cref="DBWrapper"/> packet
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public sealed class TSOVoltronDBWrapperField : TSOVoltronIgnorable
    {
        public TSOVoltronDBWrapperField()
        {

        }
    }
    /// <summary>
    /// Tells the parser/decoder logic in <see cref="TSOVoltronPacket"/> to just ignore this variable -- because it is in a <see cref="TSOBroadcastDatablobPDU"/> packet
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public sealed class TSOVoltronBroadcastDatablobPDUField : TSOVoltronIgnorable
    {
        public TSOVoltronBroadcastDatablobPDUField()
        {

        }
    }
}
