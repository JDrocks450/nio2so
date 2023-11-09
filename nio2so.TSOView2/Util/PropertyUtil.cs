using nio2so.Formats.UI.UIScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace nio2so.TSOView2.Util
{
    internal static class PropertyUtil
    {
        /// <summary>
        /// Reads all Properties on <paramref name="Source"/> and makes controls, puts the controls into <paramref name="Parent"/>
        /// </summary>
        /// <param name="Parent"></param>
        /// <param name="Source"></param>
        /// <param name="InputTag">Will add this to the <see cref="TextBox.Tag"/> property for ya ;)</param>
        public static void MakePropertyControlsToPanel(Panel Parent, object Source, object InputTag = default)
        {
            foreach (var property in Source.GetType().GetProperties())
            {
                var propertyVisible = property.GetCustomAttribute<TSOUIScriptEditorVisible>();
                if (propertyVisible != default && !propertyVisible.Visible) continue;

                TextBlock titleBlock = new TextBlock()
                {
                    Text = property.Name,
                };
                TextBox input = new TextBox()
                {
                    IsReadOnly = true,
                    Margin = new Thickness(0, 0, 0, 5),
                    Text = property.GetValue(Source)?.ToString(),
                    Tag = InputTag,
                    Height = 22
                };
                Parent.Children.Add(titleBlock);
                Parent.Children.Add(input);
            }
        }
    }
}
