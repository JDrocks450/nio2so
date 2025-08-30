using nio2so.Formats.CST;
using nio2so.Formats.UI.TSOTheme;
using nio2so.Formats.UI.UIScript;
using nio2so.TSOView2.Util;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace nio2so.TSOView2.Formats.UIs.Subpages
{
    /// <summary>
    /// Interaction logic for UIAssetReferencesPage.xaml
    /// </summary>
    public partial class UIAssetReferencesPage : Page
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

        public UIAssetReferencesPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //LOAD ASSET REFS
            ImagePropertiesBin.Items.Clear();
            foreach (var define in CurrentUIScriptFile.Defines)
            {
                if (define.Type.ToLower() == "image")
                {
                    var control = new HeaderedContentControl() { Header = define.Name, Tag = define };
                    //ASSET ID
                    if (define.TryGetReference(UIsHandler.Current.CurrentTheme, out TSOThemeDefinition? ThemeDefinition, out ulong assetID))
                    {
                        var img = ThemeDefinition?.TextureRef;
                        control.Content = img?.Convert(true);
                    }

                    ImagePropertiesBin.Items.Add(control);
                    control.PreviewMouseLeftButtonUp += ImageBin_SwitchReference;
                }
            }

            //CSTS
            OptionsTabViewer.Items.Clear();
            OptionsTabViewer.SelectionChanged -= ChangeStringTab;
            StringReferencesGrid.ItemsSource = null;

            foreach(var cstFile in //Select only files that are referenced by the current document
                UIsHandler.Current.StringTables.Where(x => CurrentUIScriptFile.ReferencedCSTFiles.Contains(x.Key)))
            {
                var newItem = new TabItem()
                {
                    Header = System.IO.Path.GetFileNameWithoutExtension(cstFile.Value.FilePath),
                    Tag = cstFile.Value
                };                
                OptionsTabViewer.Items.Add(newItem);
            }
            ChangeStringTab(null, null);
            OptionsTabViewer.SelectionChanged += ChangeStringTab;
        }

        private void ChangeStringTab(object sender, SelectionChangedEventArgs e)
        {            
            var tabItem = (TabItem)OptionsTabViewer.SelectedItem;
            if (sender == null || tabItem == null) return;
            var stringFile = (CSTFile)tabItem.Tag;
            StringReferencesGrid.ItemsSource = stringFile;
        }

        private void ImageBin_SwitchReference(object sender, MouseButtonEventArgs e)
        {
            var control = (HeaderedContentControl)sender;
            //control.IsSelected = true;
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
            else ResourceToolWindow.SpawnWithImageStream(currentDefinitionForDefine.TextureRef.Convert(true),
                currentDefinitionForDefine.FilePath == default ? null : System.IO.Path.GetFileName(currentDefinitionForDefine.FilePath));
                 
            OnSaveCallback = Callback;
            SaveDefinePropertiesButton.IsEnabled = true;            

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
                    MessageBox.Show($"An error has occured on save!\n\n{e}", e.Message);
                }
                UIsHandler.Current.CurrentTheme.Remove(assetID); // just in case B)
                UIsHandler.Current.CurrentTheme.Add(assetID, currentDefinitionForDefine);
                UIsHandler.Current.CurrentTheme.Save(TSOViewConfigHandler.CurrentConfiguration.TheSimsOnlinePreAlpha_ThemePath);

                ImageBin_SwitchReference(sender, e);
            }
            return;
        }

        private void SaveDefinePropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            if (OnSaveCallback != null)
                OnSaveCallback();
            else SaveDefinePropertiesButton.IsEnabled = false;
        }
    }
}
