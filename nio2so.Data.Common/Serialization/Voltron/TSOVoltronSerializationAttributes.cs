namespace nio2so.Data.Common.Serialization.Voltron
{
    /// <summary>
    /// Attributes for Data Members intended to be used in with a <c>TSOVoltronSerializer</c>
    /// </summary>
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
            public char NullTerminatorChar { get; set; } = '\0';
            public int NullTerminatedMaxLength { get; set; } = 255;            

            /// <summary>
            /// Sets this string to be a <see cref="TSOVoltronValueTypes.Pascal"/> string
            /// <para>Looks like: <c>{ 0x8000 [WORD LENGTH] [UTF-8 *LENGTH* byte array] }</c></para>
            /// </summary>
            public TSOVoltronString() : base(TSOVoltronValueTypes.Pascal) { }
            /// <summary>
            /// Creates a <see cref="TSOVoltronString"/> for use with a <see cref="TSOVoltronValueTypes.Pascal"/> string
            /// </summary>
            /// <param name="Type"></param>            
            public TSOVoltronString(TSOVoltronValueTypes Type, int PascalLengthValueLengthBytes = 4) : base(Type)
            {
                this.PascalLengthValueLengthBytes = PascalLengthValueLengthBytes;
            }
            /// <summary>
            /// Creates a <see cref="TSOVoltronString"/> for use with a <see cref="TSOVoltronValueTypes.NullTerminated"/> string
            /// </summary>
            /// <param name="Type"></param>
            /// <param name="NullTerminator"></param>
            /// <param name="NullTerminatedMaxLength"></param>
            public TSOVoltronString(TSOVoltronValueTypes Type, char NullTerminator, int NullTerminatedMaxLength = 255) : base(Type)
            {
                this.PascalLengthValueLengthBytes = 4;
                NullTerminatorChar = NullTerminator;
                this.NullTerminatedMaxLength = NullTerminatedMaxLength;
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
        /// Dictates to the <see cref="TSOVoltronPacket"/> serializer that this property should be set to the <see cref="Array.Length"/> property of the given
        /// Property on this object that matches the <see cref="ArrayPropertyName"/> value of this <see cref="TSOVoltronArrayLength"/> attribute
        /// <para/>Perfect for <c>ArrayName</c>Count properties!
        /// <para/>Can be used on any <b>Numeric</b> data type
        /// </summary>
        [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
        public sealed class TSOVoltronArrayLength : Attribute
        {
            public string ArrayPropertyName { get; set; }

            public TSOVoltronArrayLength(string arrayPropertyName)
            {
                ArrayPropertyName = arrayPropertyName;
            }
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
