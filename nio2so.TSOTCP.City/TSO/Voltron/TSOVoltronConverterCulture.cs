namespace nio2so.TSOTCP.City.TSO.Voltron
{
    /// <summary>
    /// Dictates how this string should be handled by the parser/decoder logic in <see cref="TSOVoltronPacket"/>
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    sealed class TSOVoltronString : Attribute
    {
        public TSOVoltronValueTypes Type { get; }
        public int PascalLengthValueLengthBytes { get; set; }
        public int NullTerminatedMaxLength { get; set; }

        /// <summary>
        /// Sets this string to be a <see cref="TSOVoltronValueTypes.Pascal"/> string
        /// <para>Looks like: <c>{ 0x8000 [WORD LENGTH] [UTF-8 *LENGTH* byte array] }</c></para>
        /// </summary>
        public TSOVoltronString() : this(TSOVoltronValueTypes.Pascal) { }
        public TSOVoltronString(TSOVoltronValueTypes Type, int PascalLengthValueLengthBytes = 4)
        {
            this.Type = Type;
            this.PascalLengthValueLengthBytes = PascalLengthValueLengthBytes;
        }
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
