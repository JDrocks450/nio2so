namespace nio2so.Data.Common.Serialization.Voltron
{
    public class TSOVoltronSerializationAttributes
    {
        /// <summary>
        /// Dictates how this general value should be handled by the parser/decoder logic in <see cref="TSOVoltronPacket"/>
        /// <para/>For strings specifically, you can get a bit more granular with <see cref="TSOVoltronString"/> but this attribute will also
        /// work for changing between Pascal and NullTerminated
        /// </summary>
        [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
        public class TSOVoltronValue : Attribute
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
        public sealed class TSOVoltronString : TSOVoltronValue
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
        public sealed class TSOVoltronDistanceToEnd : Attribute
        {
            public TSOVoltronDistanceToEnd() { }
        }
        /// <summary>
        /// Tells the parser/decoder logic in <see cref="TSOVoltronPacket"/> to just ignore this variable
        /// </summary>
        [System.AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
        public class TSOVoltronIgnorable : Attribute
        {
            public TSOVoltronIgnorable()
            {

            }
        }        
        /// <summary>
        /// This property will terminate parsing the <see cref="TSOVoltronPacket"/> and put all remaining bytes until the end of the packet into this property.
        /// </summary>
        [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
        public sealed class TSOVoltronBodyArray : Attribute
        {
            public TSOVoltronBodyArray(int MaxLength = -1)
            {

            }
        }
    }
}
