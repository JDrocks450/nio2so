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

        /// <summary>
        /// The function to call when the Save button is pressed. 
        /// <para>See: <see cref="ImageBin_SwitchReference(object, MouseButtonEventArgs)"/></para>
        /// </summary>
        private Action OnSaveCallback;

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
            //LOAD ASSET REFS
            ImagePropertiesBin.Items.Clear();
            foreach (var define in CurrentUIScriptFile.Defines)
            {
                if (define.Type.ToLower() == "image")
                {
                    var control = new ListViewItem() { Content = define.Name, Tag = define };
                    ImagePropertiesBin.Items.Add(control);
                    control.PreviewMouseLeftButtonUp += ImageBin_SwitchReference;
                }
            }
            //LOAD LIVE VIEW
            ReloadUIViewer();
        }
        
        private void ReloadUIViewer()
        {
            var theme = UIsHandler.Current.CurrentTheme;
            //DEREFERNCE CONTENT FIRST
            bool successful = theme.Initialize(
                TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_UIGraphicsDirectory,
                CurrentUIScriptFile,
                out string[] Missing);
            
            if (!successful)
            {
                if (MessageBox.Show("This script has missing Image references.\n" +
                    $"{string.Join(", ", Missing)}" +
                    "Want to try Mrs.Shipper?", "Mrs. Shipper", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    int completed = theme.TryMrsShipper(CurrentUIScriptFile);
                    MessageBox.Show($"Mrs.Shipper found {completed} references. Please validate.");
                }
                return;
            }            

            List<FrameworkElement> elements = new List<FrameworkElement>();
            ControlsStack.Children.Clear();
            UICanvas.Children.Clear();

            //UNREFERENCED IMAGES FIRST
            foreach (var definition in theme.Values)
            {
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
                    isVisibleCheck.IsChecked = !isVisibleCheck.IsChecked;
                };               
                decorButton.Content = isVisibleCheck;
                ControlsStack.Children.Add(decorButton);

                UICanvas.Children.Add(element);
            }
        }

        private void ImageBin_SwitchReference(object sender, MouseButtonEventArgs e)
        {
            var control = (ListViewItem)sender;
            control.IsSelected = true;
            var define = (UIScriptDefineComponent)control.Tag;

            ImagePropertiesBinGrid.Children.Clear();
            PropertyUtil.MakePropertyControlsToPanel(ImagePropertiesBinGrid, define);
            TSOThemeDefinition currentDefinitionForDefine = default;

            //ASSET ID
            string message = "OK.";
            bool assetFound = define.TryGetReference(UIsHandler.Current.CurrentTheme, out TSOThemeDefinition? ThemeDefinition, out ulong assetID);
            currentDefinitionForDefine = ThemeDefinition;

            //setup UI properties for ThemeDefinition
            if (currentDefinitionForDefine == null)
                currentDefinitionForDefine = new(default); // blank definition here
            PropertyUtil.MakePropertyControlsToPanel(ImagePropertiesBinGrid, currentDefinitionForDefine);
            TextBox URIBox = (TextBox)ImagePropertiesBinGrid.Children[ImagePropertiesBinGrid.Children.Count - 1];
            URIBox.IsReadOnly = false;

            if (!assetFound)
                URIBox.BorderBrush = System.Windows.Media.Brushes.Red;

            OnSaveCallback = Callback;

            //blessed C# is blessed
            void Callback()
            {
                string filePath = URIBox.Text;
                try
                {
                    currentDefinitionForDefine.FilePath = filePath;
                }
                catch (Exception e)
                {
                    message = e.Message;
                    MessageBox.Show($"An error has occured on save! {message}", "Big oopsies!");
                }
                UIsHandler.Current.CurrentTheme.Remove(assetID); // just in case B)
                UIsHandler.Current.CurrentTheme.Add(assetID, currentDefinitionForDefine);
                UIsHandler.Current.CurrentTheme.Save(TSOViewConfigHandler.CurrentConfiguration.TheSimsOnlinePreAlpha_ThemePath);
                
                ImageBin_SwitchReference(sender, e);
            }
            return;        
        }

        private void SaveDefinePropertiesButton_Click(object sender, RoutedEventArgs e) => OnSaveCallback();

        private void RefreshUIButton_Click(object sender, RoutedEventArgs e)
        {
            ReloadUIViewer();
        }
    }
}
