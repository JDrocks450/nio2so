namespace nio2so.Formats.UI.UIScript
{
    public interface ITSOUIScriptValueType<T>
    {
        public T Parse(string Value, bool ignoreFormat = false);
    }
    public record UIScriptNumber(int Value) : ITSOUIScriptValueType<UIScriptNumber>
    {
        public static implicit operator int(UIScriptNumber Other) => Other.Value;
        public static implicit operator UIScriptNumber(int Other) => new(Other);
        public static implicit operator UIScriptNumber(string Other) => new(int.Parse(Other));
        public UIScriptNumber Parse(string Value, bool ignoreFormat = false) => Value;
    }
    public class UIScriptString : ITSOUIScriptValueType<UIScriptString>
    {
        public string Value { get; set; }
        public UIScriptString(string Value)
        {
            this.Value = Value.Replace("\"", "");
        }
        public static implicit operator string(UIScriptString Other) => Other.Value;
        public static implicit operator UIScriptString(string Other) => new(Other);
        public UIScriptString Parse(string Value, bool ignoreFormat = false) => Value;
    }
    public class UIScriptValueTuple : ITSOUIScriptValueType<UIScriptValueTuple>
    {
        public int Value1 => Values.ElementAt(0);
        public int Value2 => Values.ElementAt(1);
        public int Value3 => Values.ElementAt(2);

        public IEnumerable<int> Values { get; private set; }
        public UIScriptValueTuple(params int[] Values) => this.Values = Values;
        
        public UIScriptValueTuple Parse(string Value, bool ignoreFormat = false)
        {
            if (!ignoreFormat)
                if (!Value.StartsWith('(') || !Value.EndsWith(')') || !Value.Contains(','))
                    throw new InvalidDataException($"This value is not formatted correctly. {Value}");
            IEnumerable<int> values = Value.Replace("(", "").Replace(")", "").Split(',').Select(x => int.Parse(x));
            return new UIScriptValueTuple(values.ToArray());
        }
    }
    public record UIScriptComponentPropertyValue(string? Value)
    {
        public T? GetValue<T>() where T : class, ITSOUIScriptValueType<T>
        {
            if (typeof(T) == typeof(UIScriptValueTuple)) return new UIScriptValueTuple().Parse(Value) as T;
            if (typeof(T) == typeof(UIScriptString)) return new UIScriptString(Value) as T;
            if (typeof(T) == typeof(UIScriptNumber)) return new UIScriptNumber(int.Parse(Value)) as T;
            return default;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
