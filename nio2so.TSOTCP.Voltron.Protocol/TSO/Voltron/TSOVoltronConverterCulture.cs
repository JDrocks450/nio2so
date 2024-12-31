using nio2so.Formats.Util.Endian;
using nio2so.TSOTCP.Voltron.Protocol.Factory;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron
{
    //**** CLASS LEVEL****

    /// <summary>
    /// Allows this <see cref="TSOVoltronPacket"/> to be mapped to a <see cref="TSO_PreAlpha_VoltronPacketTypes"/> to be
    /// created using Reflection when it is sent/received.
    /// <para/>See <see cref="TSOPDUFactory.CreatePacketObjectByPacketType(TSO_PreAlpha_VoltronPacketTypes, Stream, bool)"/> for implementation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    class TSOVoltronPDU : Attribute
    {
        public TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes Type)
        {
            this.Type = Type;
        }

        public TSO_PreAlpha_VoltronPacketTypes Type { get; }
    }
    /// <summary>
    /// Similar to <see cref="TSOVoltronPDU"/>. When a packet is created using Reflection using a mapped <see cref="TSOVoltronPDU"/> - if it
    /// is a <see cref="TSO_PreAlpha_VoltronPacketTypes.DB_REQUEST_WRAPPER_PDU"/> it will have its header tested against this <see cref="TSO_PreAlpha_DBActionCLSIDs"/>
    /// provided to match this <see cref="TSODBRequestWrapper"/> implementation to the incoming/outgoing data.
    /// <para>See <see cref="TSOPDUFactory.CreatePacketObjectByPacketType(TSO_PreAlpha_VoltronPacketTypes, Stream, bool)"/> for implementation</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    class TSOVoltronDBRequestWrapperPDU : Attribute
    {
        public TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs ActionCLSID)
        {
            Type = ActionCLSID;
        }

        public TSO_PreAlpha_DBActionCLSIDs Type { get; }
    }
    /// <summary>
    /// Similar to <see cref="TSOVoltronPDU"/>. When a packet is created using Reflection using a mapped <see cref="TSOVoltronPDU"/> - if it
    /// is a <see cref="TSO_PreAlpha_VoltronPacketTypes.BROADCAST_DATABLOB_PDU"/> it will have its header tested against this <see cref="TSO_PreAlpha_MasterConstantsTable"/>
    /// provided to match this <see cref="TSOVoltronDatablobContent"/> implementation to the incoming/outgoing data.
    /// <para>See <see cref="TSOPDUFactory.CreatePacketObjectByPacketType(TSO_PreAlpha_VoltronPacketTypes, Stream, bool)"/> for implementation</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    class TSOVoltronDatablobContent : Attribute
    {
        public TSOVoltronDatablobContent(TSO_PreAlpha_MasterConstantsTable ActionCLSID)
        {
            Type = ActionCLSID;
        }

        public TSO_PreAlpha_MasterConstantsTable Type { get; }
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
