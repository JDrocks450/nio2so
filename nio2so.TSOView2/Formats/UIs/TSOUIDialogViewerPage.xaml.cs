using nio2so.Formats.UI.TSOTheme;
using nio2so.Formats.UI.UIScript;
using nio2so.TSOView2.Formats.UIs.Controls;
using nio2so.TSOView2.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
using System.Xml.Linq;

namespace nio2so.TSOView2.Formats.UIs
{
    /// <summary>
    /// Interaction logic for TSOUIDialogViewerPage.xaml
    /// </summary>
    public partial class TSOUIDialogViewerPage : Page
    {
        /// <summary>
        /// Gets the <see cref="UIsHandler.CurrentFile"/>
        /// </summary>
        public UIScriptFile CurrentUIScriptFile => UIsHandler.Current.CurrentFile;        

        public TSOUIDialogViewerPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {            
            //LOAD LIVE VIEW
            ReloadUIViewer();
        }
        
        private void ReloadUIViewer()
        {
            UIGizmo.Visibility = Visibility.Collapsed;
            bool retriedOnce = false;

        retry:
            var theme = UIsHandler.Current.CurrentTheme;
            //DEREFERNCE CONTENT FIRST
            bool successful = theme.Initialize(
                TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_BaseDirectory,
                CurrentUIScriptFile,
                out string[] Missing);
            
            if (!successful)
            {
                //MISSING REFS!
                if (!retriedOnce)
                {
                    theme.UpdateDatabaseWithMrsShipper(TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_BaseDirectory);
                    theme.Save(TSOViewConfigHandler.CurrentConfiguration.TheSimsOnlinePreAlpha_ThemePath);
                    retriedOnce = true;
                    goto retry;
                }
                MessageBox.Show("Tried to use Mrs. Shipper to read AssetID database. Failed. Check your TSO installation.\n" +
                    $"We're missing these assets: \n{string.Join(", ", Missing)}");
                return;
            }            

            //SUCCESS
            List<FrameworkElement> elements = new List<FrameworkElement>();
            ControlsStack.Children.Clear();
            UICanvas.Children.Clear();

            //UNREFERENCED IMAGES FIRST
            foreach (var definition in theme.Values)
            {
                if (definition.TextureRef == null) continue;
                if (!definition.ReferencedBy.Any())
                {
                    var imgControl = UIControlFactory.MakeImageFromDefinition(definition);
                    if (imgControl == default) continue;
                    elements.Add(imgControl);
                }
            }
            
            //CONTROLS
            foreach(var control in CurrentUIScriptFile.Controls)
            {
                FrameworkElement? element = UIControlFactory.MakeControl<FrameworkElement>(control);                
                if (element == default) continue;
                elements.Add(element);
            }

            // MAKE CONTROLS NAV BAR
            foreach(var element in elements)
            {
                //CURSOR
                element.Cursor = Cursors.Hand;                

                UIViewExplorerItem decorButton = new UIViewExplorerItem()
                {
                    Header = $"{element.Name} ({element.GetType().Name})",
                    Margin = new Thickness(0,0,0,5)
                };
                CheckBox isVisibleCheck = new CheckBox()
                {
                    Content = "Visible",
                    IsChecked = true,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(10)
                };
                RoutedEventHandler action = delegate
                {
                    element.Visibility = isVisibleCheck.IsChecked ?? false ? Visibility.Visible : Visibility.Collapsed;
                };
                isVisibleCheck.Checked += action;
                isVisibleCheck.Unchecked += action;
                element.MouseLeftButtonUp += delegate
                {
                    ShowUIGizmoForControl(element.Name, element);
                    var gizmoVisCheck = new CheckBox()
                    {
                        Content = "Visible",
                        IsChecked = true,
                    };
                    RoutedEventHandler action1 = delegate
                    {
                        isVisibleCheck.IsChecked = !isVisibleCheck.IsChecked;
                    };
                    gizmoVisCheck.Checked += action1;
                    gizmoVisCheck.Unchecked += action1;
                    GizmoControlStack.Children.Add(gizmoVisCheck);
                };               
                decorButton.Content = isVisibleCheck;
                ControlsStack.Children.Add(decorButton);

                UICanvas.Children.Add(element);
            }
        }

        private void ShowUIGizmoForControl(string Title, UIElement Element)
        {
            GizmoName.Text = Title;

            void AddControllable(string Name, DependencyProperty e, Type PropertyType)
            {
                void ChangeValue(object sender)
                {
                    object value = null;
                    if (PropertyType == typeof(string))
                        value = (sender as TextBox).Text;
                    if (PropertyType == typeof(bool))
                        value = (sender as CheckBox).IsChecked;
                    Element.SetValue(e, value);
                }

                GizmoControlStack.Children.Add(new TextBlock()
                {
                    Text = Name,
                });
                if (PropertyType == typeof(string))
                {
                    var input = new TextBox()
                    {
                        Margin = new Thickness(0, 0, 0, 5),
                        Text = Element.GetValue(e).ToString()
                    };
                    TextChangedEventHandler textChangeValue = delegate
                    {
                        ChangeValue(input);
                    };
                    input.TextChanged += textChangeValue;
                    GizmoControlStack.Children.Add(input);
                    return;
                }
                if (PropertyType == typeof(bool))
                {
                    var input = new CheckBox()
                    {
                        Margin = new Thickness(0, 0, 0, 5),
                        Content = Element.GetValue(e).ToString(),
                        IsChecked = (bool)Element.GetValue(e)
                    };
                    RoutedEventHandler checkedChangeValue = delegate
                    {
                        ChangeValue(input);
                    };
                    input.Checked += checkedChangeValue; 
                    input.Unchecked += checkedChangeValue;
                    GizmoControlStack.Children.Add(input);
                    return;
                }
            }

            if (Element == null) return;
            GizmoControlStack.Children.Clear();

            if (Element is TextBlock)
                AddControllable("Display Text", TextBlock.TextProperty, typeof(string));
            if (Element is TextBox)
                AddControllable("Display Text", TextBox.TextProperty, typeof(string));
            if (Element is TSOButton)
                AddControllable("Enabled", IsEnabledProperty, typeof(bool));

            var position = Mouse.GetPosition(UICanvas);
            UIGizmo.Margin = new Thickness(position.X, position.Y, 0, 0);
            UIGizmo.Visibility = Visibility.Visible;
        }        

        private void RefreshUIButton_Click(object sender, RoutedEventArgs e)
        {
            ReloadUIViewer();
        }
    }
}
