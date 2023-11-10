namespace nio2so.Formats.UI.UIScript
{
    public enum TSOUIsDefineTypes
    {
        Other,
        Image,
        String
    }
    public enum TSOUIsObjectTypes
    {
        None,
        /// <summary>
        /// This is a special type for an object that isn't explicitly defined in the script, but referenced but it.
        /// <para>This is helpful when an object the GameClient usually generates at runtime is referenced by the script.</para>
        /// <para>For example, if a <see cref="UIScriptControlPropertiesComponent"/> references an object by name that isn't defined, a
        /// <see cref="UIScriptObject"/> with this type is created and the properties are applied to it.</para>
        /// <para>These objects have no other functionality.</para>
        /// </summary>
        n2_InferredObject,
        /// <summary>
        /// A button.
        /// </summary>
        Button,
        /// <summary>
        /// A textbox that can be scrolled when the text content overflows
        /// </summary>
        ScrollableText,
        /// <summary>
        /// A label with text applied
        /// </summary>
        Text,
    }
}
