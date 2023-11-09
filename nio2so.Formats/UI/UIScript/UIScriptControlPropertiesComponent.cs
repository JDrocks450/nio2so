using System.ComponentModel;

namespace nio2so.Formats.UI.UIScript
{
    [DisplayName("Set Control Properties")]
    public class UIScriptControlPropertiesComponent : UIScriptComponentBase, IUIScriptNamedComponent
    {
        public string Name { get; set; }
    }
}