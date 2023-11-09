using System.Data;
using System.Text;

namespace nio2so.Formats.UI.UIScript
{
    public abstract class UIScriptComponentBase : IUIScriptComponentProperties
    {
        public UIScriptComponentBase() { }
        public UIScriptComponentBase(Dictionary<string, UIScriptComponentPropertyValue> myProperties) : this() => MyProperties = myProperties;
        public UIScriptComponentBase(Dictionary<string, UIScriptComponentPropertyValue> myProperties, Dictionary<string,
            UIScriptComponentPropertyValue> inheritedProperties) : this(myProperties) => InheritedProperties = inheritedProperties;

        [TSOUIScriptEditorVisible(false)]
        public Dictionary<string, UIScriptComponentPropertyValue> MyProperties { get; } = new();

        [TSOUIScriptEditorVisible(false)]
        public Dictionary<string, UIScriptComponentPropertyValue> InheritedProperties { get; } = new();

        public UIScriptComponentPropertyValue? GetProperty(string Name)
        {
            if (InheritedProperties.TryGetValue(Name, out var value))
                return value;
            else if (MyProperties.TryGetValue(Name, out value))
                return value;
            return default;
        }

        public bool TryGetProperty(string Name, out UIScriptComponentPropertyValue? Value) => (Value = GetProperty(Name)) != default;

        public string PropertiesToString(Dictionary<string, UIScriptComponentPropertyValue> Properties)
        {
            StringBuilder builder = new StringBuilder();
            foreach(var property in Properties)
                builder.AppendLine($"{property.Key}={property.Value}");
            return builder.ToString();
        }

        /// <summary>
        /// Gets all properties added to this <see cref="UIScriptComponentBase"/> - Inherited first, MyProperties second. No duplicates.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UIScriptComponentProperty> GetProperties()
        {
            Dictionary<string, UIScriptComponentPropertyValue> newList = new();
            foreach (var prop in InheritedProperties)
                newList.Add(prop.Key, prop.Value);
            foreach (var prop in MyProperties)
                newList.TryAdd(prop.Key, prop.Value);
            return newList.Select(x => new UIScriptComponentProperty(x.Key, x.Value));
        }

        public void CombineProperties(Dictionary<string, UIScriptComponentPropertyValue> DestinationGroup,
            IEnumerable<UIScriptComponentProperty> Properties)
        {
            foreach (var prop in Properties)
                DestinationGroup.TryAdd(prop.Name, prop.Value);
        }
    }
}
