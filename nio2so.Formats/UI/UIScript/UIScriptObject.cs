namespace nio2so.Formats.UI.UIScript
{
    /// <summary>
    /// Simply used to denote a Property should be visible to editors that are working with these items.
    /// <para>Can also dictate that a property is explicitly not indended to be visible unless special 
    /// attention is given to making them easy to view.</para>
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class TSOUIScriptEditorVisible : Attribute
    {
        public TSOUIScriptEditorVisible(bool Visible = true)
        {
            this.Visible = Visible;
        }

        public bool Visible { get; }
    }

    /// <summary>
    /// A control in the User Interface of The Sims Online.
    /// <para>Typical controls are things like Buttons, Scrollable text, Labels, etc.</para>
    /// </summary>
    public class UIScriptObject : UIScriptComponentBase, IUIScriptNamedComponent
    {
        private string _type;

        public UIScriptObject(string type,string name)
        {
            Name = name;
            Type = type;
        }

        public string KnownType
        {
            get
            {
                if (Enum.TryParse<TSOUIsObjectTypes>(Type,true, out var result))
                    return result.ToString();
                if (Enum.TryParse<TSOUIsDefineTypes>(Type, true, out var result2))
                    return result2.ToString();
                return TSOUIsObjectTypes.None.ToString();
            }
        }

        public override string ToString()
        {
            return $"Add{Type} \"{Name}\" {PropertiesToString(MyProperties)}";
        }

        public string Name { get; set; }
        public string Type {
            get => _type.ToLower();
            set => _type = value.ToLower();
        }
    }
}
