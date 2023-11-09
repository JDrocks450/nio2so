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
        public UIScriptObject(string type,string name)
        {
            Name = name;
            Type = type;
        }

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
            return $"Add{Type} \"{Name}\" {PropertiesToString(MyProperties)}";
        }

        public string Name { get; set; }
        public string Type { get; set; }
    }
}
