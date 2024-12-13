using nio2so.Formats.Util.Endian;
using nio2so.TSOTCP.City.Factory;

namespace nio2so.TSOTCP.City.TSO.Voltron
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
            this.Type = ActionCLSID;
        }

        public TSO_PreAlpha_DBActionCLSIDs Type { get; }
    }
    /// <summary>
    /// Similar to <see cref="TSOVoltronPDU"/>. When a packet is created using Reflection using a mapped <see cref="TSOVoltronPDU"/> - if it
    /// is a <see cref="TSO_PreAlpha_VoltronPacketTypes.BROADCAST_DATABLOB_PDU"/> it will have its header tested against this <see cref="TSO_PreAlpha_MasterConstantsTable"/>
    /// provided to match this <see cref="TSOVoltronBroadcastDatablobPDU"/> implementation to the incoming/outgoing data.
    /// <para>See <see cref="TSOPDUFactory.CreatePacketObjectByPacketType(TSO_PreAlpha_VoltronPacketTypes, Stream, bool)"/> for implementation</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    class TSOVoltronBroadcastDatablobPDU : Attribute
    {
        public TSOVoltronBroadcastDatablobPDU(TSO_PreAlpha_MasterConstantsTable ActionCLSID)
        {
            this.Type = ActionCLSID;
        }

        public TSO_PreAlpha_MasterConstantsTable Type { get; }
    }

    //***PROPERTY LEVEL

    /// <summary>
    /// Dictates how this general value should be handled by the parser/decoder logic in <see cref="TSOVoltronPacket"/>
    /// <para/>For strings specifically, you can get a bit more granular with <see cref="TSOVoltronString"/> but this attribute will also
    /// work for changing between Pascal and NullTerminated
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    class TSOVoltronValue : Attribute
    {
        public TSOVoltronValueTypes Type { get; protected set; }

        public TSOVoltronValue(TSOVoltronValueTypes type)
        {
            Type = type;
        }
    }
    /// <summary>
    /// Dictates how this string should be handled by the parser/decoder logic in <see cref="TSOVoltronPacket"/>
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    sealed class TSOVoltronString : TSOVoltronValue
    {        
        public int PascalLengthValueLengthBytes { get; set; }
        public int NullTerminatedMaxLength { get; set; }

        /// <summary>
        /// Sets this string to be a <see cref="TSOVoltronValueTypes.Pascal"/> string
        /// <para>Looks like: <c>{ 0x8000 [WORD LENGTH] [UTF-8 *LENGTH* byte array] }</c></para>
        /// </summary>
        public TSOVoltronString() : base(TSOVoltronValueTypes.Pascal) { }
        public TSOVoltronString(TSOVoltronValueTypes Type, int PascalLengthValueLengthBytes = 4) : base(Type)
        {
            this.PascalLengthValueLengthBytes = PascalLengthValueLengthBytes;
        }
    }
    /// <summary>
    /// Dictates to the <see cref="TSOVoltronPacket"/> serializer that this property should be set to the distance from this property to 
    /// the end of the packet. 
    /// <para/>Perfect for MessageLength properties!
    /// <para/>Should only be used on <see cref="UInt32"/>
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    class TSOVoltronDistanceToEnd : Attribute
    {
        public TSOVoltronDistanceToEnd() { }                
    }
    /// <summary>
    /// Tells the parser/decoder logic in <see cref="TSOVoltronPacket"/> to just ignore this variable
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    class TSOVoltronIgnorable : Attribute
    {
        public TSOVoltronIgnorable()
        {

        }
    }
    /// <summary>
    /// Tells the parser/decoder logic in <see cref="TSOVoltronPacket"/> to just ignore this variable -- because it is in a <see cref="DBWrapper"/> packet
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    sealed class TSOVoltronDBWrapperField : TSOVoltronIgnorable
    {
        public TSOVoltronDBWrapperField()
        {

        }
    }
    /// <summary>
    /// Tells the parser/decoder logic in <see cref="TSOVoltronPacket"/> to just ignore this variable -- because it is in a <see cref="TSOBroadcastDatablobPDU"/> packet
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    sealed class TSOVoltronBroadcastDatablobPDUField : TSOVoltronIgnorable
    {
        public TSOVoltronBroadcastDatablobPDUField()
        {

        }
    }
    /// <summary>
    /// This property will terminate parsing the <see cref="TSOVoltronPacket"/> and put all remaining bytes until the end of the packet into this property.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    sealed class TSOVoltronBodyArray : Attribute
    {
        public TSOVoltronBodyArray(int MaxLength = -1)
        {

        }
    }
}
