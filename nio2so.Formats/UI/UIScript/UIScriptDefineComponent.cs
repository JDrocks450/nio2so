using System.ComponentModel;

namespace nio2so.Formats.UI.UIScript
{
    public interface IUIScriptNamedComponent
    {
        public string Name { get; set; }
    }
    [DisplayName("Define Constant")]
    public class UIScriptDefineComponent : UIScriptComponentBase, IUIScriptNamedComponent
    {
        public UIScriptDefineComponent(string type, string name)
        {
            Type = type;
            Name = name;
        }

        public string Type { get; set; }
        public string Name { get; set; }

        public TSOUIsDefineTypes KnownType
        {
            get
            {
                if (Enum.GetNames<TSOUIsDefineTypes>().Contains(Type))
                    return Enum.Parse<TSOUIsDefineTypes>(Type);
                return 0;
            }
        }

        public override string ToString()
        {
            return $"Define{Type} \"{Name}\" {PropertiesToString(MyProperties)}";
        }
    }
}
