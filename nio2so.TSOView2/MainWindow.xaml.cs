using nio2so.TSOView2.Formats.Compressor;
using nio2so.TSOView2.Formats.Terrain;
using nio2so.TSOView2.Formats.TSOData;
using nio2so.TSOView2.Formats.UIs;
using nio2so.TSOView2.Formats.UIs.Subpages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace nio2so.TSOView2
{
    public interface ITSOView2Window
    {
        void MainWindow_ShowPlugin(Page NewPage);
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ITSOView2Window
    {
        /// <summary>
        /// Maps <see cref="MenuItem"/> controls defined in XAML to an action in code behind.
        /// </summary>
        private Dictionary<MenuItem, Action> uiInvokableActionMap { get; set; } = default;

        public MainWindow()
        {
            InitializeComponent();            
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {           
            //Setup the UIsHandler
            if (UIsHandler.Current == null)
                new UIsHandler().Initialize();
            //HOOK Click Event for all MenuItems with a Name property
            UIWireUp_HookEvents();
        }

        public void MainWindow_ShowPlugin(Page NewPage)
        {
            MainPageContent.Content = NewPage;
        }

        private void UIWireUp_HookEvents()
        {
            //SET THE INVOKABLE MAP
            uiInvokableActionMap = new Dictionary<MenuItem, Action>()
            {
                { ExitItem, Application.Current.Shutdown },
                { ConfigMenuItem, TSOViewConfigHandler.InvokeConfigViewerDialog },
                { ViewTGAItem, ResourceToolWindow.SpawnWithPrompt },
                { OpenUIsItem, UIsHandler.Current.PromptUserOpenFile },
                { OpenTSOPreAlphaWorldItem, CityTerrainHandler.PromptUserShowCityPlugin },
                { OpenNIWorldItem, async () => await CityTerrainHandler.PromptUserShowCityPlugin(TSOVersion.NewImproved) },
                { EnumPluginItem, Plugins.EnumFixerUpperPlugin.Do },
                { CityPluginItem, Plugins.TSOCityTransmogrifier.Do },
                { AboutItem, AboutWindow.ShowAboutBox },
                { UpdatesItem, CheckForUpdates },
                { WikiItem, () => OpenWebsite(@"https://github.com/JDrocks450/nio2so/wiki/TSOView2") },
                { MyselfItem, () => OpenWebsite(@"https://github.com/JDrocks450/") },
                { GithubItem, () => OpenWebsite(@"https://github.com/JDrocks450/nio2so") },
                { OpenTSODataFileItem, TSODataDefinitionExplorerWindow.ShowExplorer },
                { RefPackItem, DecompressorWindow.ShowDecompressor },
            };
            //Set all named MenuItems to be included in the system
            void SearchChildren(MenuItem MenuItem)
            {
                if (!string.IsNullOrWhiteSpace(MenuItem.Name))
                    MenuItem.Click += UIDefaultMenuStrip_InvokeAction;
                foreach (var subItem in MenuItem.Items)
                {
                    if (subItem is MenuItem mItem)                    
                        SearchChildren(mItem);                    
                }
            }
            foreach (var item in DefaultMenu.Items)
            {
                if (item is MenuItem mItem)
                    SearchChildren(mItem);
            }
        }

        private void OpenWebsite(string Website) => Process.Start("explorer", Website);
        private void CheckForUpdates() => OpenWebsite(@"https://github.com/JDrocks450/nio2so/releases");

        private void UIDefaultMenuStrip_InvokeAction(object sender, RoutedEventArgs e)
        {
            //If this named MenuItem has an attached handler, it will be invoked here
            if (uiInvokableActionMap.TryGetValue((MenuItem)sender, out Action? member))
#if !DEBUG
                try
                {
                    member(); // run while catching unhandled exceptions
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"An error occured in the plugin: {member.Method.DeclaringType.Name}.\r\n{ex.Message}");
                }
#else
                member(); // run without exception catching
#endif
        }
    }
}
