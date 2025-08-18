namespace nio2so.Data.Common
{
    /// <summary>
    /// A type accepting an enum type parameter or a uint value that is implicitly convertible to and from uint.
    /// <para/>This type is useful for representing enum values as unsigned integers while providing type safety.
    /// Additionally, it can be used to create enum-like structures that are not strictly tied to a specific enum type.
    /// </summary>
    public struct UIntEnum
    {
        public uint Value { get; }
        public UIntEnum(uint value)
        {
            Value = value;
        }
        public UIntEnum(object EnumValue) : this(Convert.ToUInt32(EnumValue))
        {
            if (!EnumValue.GetType().IsEnum)
                throw new ArgumentException("EnumValue must be an enum type");
        }
        public static UIntEnum Create<T>(T value) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enum type");
            return new UIntEnum(Convert.ToUInt32(value));
        }
        public static implicit operator uint(UIntEnum u) => u.Value;
        public static implicit operator UIntEnum(uint u) => new(u);
        public override string ToString() => Value.ToString();
        public override bool Equals(object? obj)
        {
            return obj is UIntEnum other && Value == other.Value;
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
