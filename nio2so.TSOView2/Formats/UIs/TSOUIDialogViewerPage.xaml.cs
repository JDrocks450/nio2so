using nio2so.Formats.UI.TSOTheme;
using nio2so.Formats.UI.UIScript;
using nio2so.TSOView2.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
            bool successful = theme.LoadReferencedImages(
                TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_UIGraphicsDirectory,
                CurrentUIScriptFile
            );
            if (!successful)
                return;

            BitmapSource GetManaged(ulong AssetID)
            {
                var image = theme[AssetID].TextureRef;
                if (image == null) image = new Bitmap(1,1);
                using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(image))
                {
                    bmp.MakeTransparent(System.Drawing.Color.FromArgb(255, 255, 0, 255)); // magenta
                    return bmp.Convert(true);
                }
            }
            ImageBrush MakeImage(ulong AssetID)
            {
                var managed = GetManaged(AssetID);                
                return new ImageBrush(managed);
            }
            ImageBrush MakeImageName(string Name)
            {
                var define = CurrentUIScriptFile.GetDefineByName(Name);
                var assetIDprop = define.GetProperty("assetID");
                return MakeImage(Convert.ToUInt64(assetIDprop.GetValue<UIScriptString>(),16));
            }

            // BACKGROUND IMAGE
            var imgBrush = MakeImageName("BackgroundImage");
            UICanvas.Background = imgBrush;
            UICanvas.Width = imgBrush.ImageSource.Width;
            UICanvas.Height = imgBrush.ImageSource.Height;
            
            //CONTROLS
            foreach(var control in CurrentUIScriptFile.Controls)
            {
                UIElement element = default;
                switch (control.Type.ToLower())
                {
                    case "button":
                        Button btn = new Button()
                        {
                            Background = null,
                            Style = null,
                            BorderBrush = null,                            
                        };
                        var prop = control.GetProperty("image");
                        if (prop != null)
                        {
                            var name = prop.GetValue<UIScriptString>();
                            var brush = MakeImageName(name);
                            brush.ViewportUnits = BrushMappingMode.Absolute;
                            brush.Viewport = new Rect(0,0, brush.ImageSource.Width, brush.ImageSource.Height);
                            RenderOptions.SetBitmapScalingMode(brush, BitmapScalingMode.NearestNeighbor);
                            btn.Background = brush;
                            btn.Width = brush.ImageSource.Width / 4;
                            btn.Height = brush.ImageSource.Height;
                        }
                        prop = control.GetProperty("position");
                        if (prop != null)
                        {
                            var position = prop.GetValue<UIScriptValueTuple>();
                            Canvas.SetLeft(btn, position.Values.ElementAt(0));
                            Canvas.SetTop(btn, position.Values.ElementAt(1));
                        }
                        element = btn;
                        break;
                }
                if (element == default) continue;
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
                currentDefinitionForDefine = new(); // blank definition here
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
