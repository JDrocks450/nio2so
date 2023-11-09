namespace nio2so.Formats.UI.UIScript
{
    public interface IUIScriptComponentProperties
    {
        Dictionary<string, UIScriptComponentPropertyValue> MyProperties { get; }
        Dictionary<string, UIScriptComponentPropertyValue> InheritedProperties { get; }
        UIScriptComponentPropertyValue? GetProperty(string Name);
        bool TryGetProperty(string Name, out UIScriptComponentPropertyValue? Value);
    }
}
