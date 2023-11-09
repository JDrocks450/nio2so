using nio2so.Formats.UI.UIScript;
using nio2so.TSOView2.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace nio2so.TSOView2.Formats.UIs.Subpages
{    
    /// <summary>
    /// Interaction logic for TabUIScriptTreeSubPage.xaml
    /// </summary>
    public partial class TabUIScriptTreeSubPage : Page
    {
        /// <summary>
        /// Gets the <see cref="UIsHandler.CurrentFile"/>
        /// </summary>
        public UIScriptFile CurrentUIScriptFile => UIsHandler.Current.CurrentFile;

        public TabUIScriptTreeSubPage()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private Brush GetColorByComponentType(UIScriptComponentBase Component)
        {
            Brush brush = OtherPreview.Fill;
            if (Component is UIScriptDefineComponent)
                brush = DefinePreview.Fill;
            if (Component is UIScriptObject)
                brush = ObjectPreview.Fill;
            if (Component is UIScriptControlPropertiesComponent)
                brush = ControlPropertiesPreview.Fill;
            if (Component is UIScriptGroup)
                brush = GroupPreview.Fill;
            if (Component is UICommentComponent)
                brush = CommentPreview.Fill;
            return brush;

        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // SET TREE OBJECT VIEWER
            void AddGroup(TreeViewItem Node, UIScriptGroup Current)
            {
                foreach (var comp in Current.Items)
                {
                    var name = comp.ToString();
                    var displayName = comp.GetType().GetCustomAttribute<DisplayNameAttribute>();
                    if (displayName != default)
                        name = displayName.DisplayName;
                    if (comp is UIScriptObject obj)
                        name = obj.Type;
                    var compNode = new TreeViewItem()
                    {
                        Header = name,
                        Foreground = GetColorByComponentType(comp),
                        Tag = comp
                    };
                    compNode.Selected += ObjectTreeNode_Selected;
                    Node.Items.Add(compNode);
                    if (comp is UIScriptGroup group)
                        AddGroup(compNode, group);
                }
            }
            var node = new TreeViewItem()
            {
                Header = "Root"
            };
            AddGroup(node, CurrentUIScriptFile);
            UiScriptsTreeObject.Items.Add(node);
        }

        private void ObjectTreeNode_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem? node = (sender as TreeViewItem);
            UIScriptComponentBase? component = (node.Tag as UIScriptComponentBase);

            TitleBlock.Text = node?.Header.ToString() ?? "None selected";

            //---POPULATE FIELDS (readonly ; _ ;)
            FieldsGrid.Children.Clear();
            PropertyUtil.MakePropertyControlsToPanel(FieldsGrid, component);

            //--POPULATE PROPERTIES
            MyPropertiesDataGrid.ItemsSource = component?.MyProperties;
            InheritPropertiesDataGrid.ItemsSource = component?.InheritedProperties;

            //crucial
            e.Handled = true;
        }
    }
}
