using nio2so.TSOView2.Formats;
using nio2so.TSOView2.Formats.Compressor;
using nio2so.TSOView2.Formats.Cst;
using nio2so.TSOView2.Formats.FAR3;
using nio2so.TSOView2.Formats.Network;
using nio2so.TSOView2.Formats.Terrain;
using nio2so.TSOView2.Formats.TSOData;
using nio2so.TSOView2.Formats.UIs;
using nio2so.TSOView2.Formats.UIs.Subpages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace nio2so.TSOView2
{
    public interface ITSOView2Window
    {
        void ShowPlugin(Page NewPage);
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ITSOView2Window
    {
        private Dictionary<int, Process> _processes = new();
        /// <summary>
        /// Maps <see cref="MenuItem"/> controls defined in XAML to an action in code behind.
        /// </summary>
        private Dictionary<MenuItem, Action> uiInvokableActionMap { get; set; } = default;

        public string VersionString => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
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
            //reset ui
            ClosePlugin();
        }

        public void ShowPlugin(Page NewPage)
        {
            ProjectLogo.Visibility = Visibility.Collapsed;
            Background = (Brush)FindResource("TSOWindowBackgroundBrush");
            MainPageContent.Content = NewPage;
            ClosePluginItem.Visibility = Visibility.Visible;
        }

        public void ClosePlugin()
        {
            ProjectLogo.Visibility = Visibility.Visible;
            ClosePluginItem.Visibility = Visibility.Collapsed;
            Background = Brushes.White;
            MainPageContent.Content = null;
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
                { VoltronPacketOpenItem, () => TSOVoltronPacketPropertiesWindow.TryPromptUserAndShowDialog(out _) },
                { VoltronDirectoryOpenItem, () => TSOVoltronPacketDirectoryWindow.TryPromptUserAndShowDialog(out _) },
                { MaxisProtocolItem, () => ShowPlugin(new TSOPacketLibraryControl()) },
                { ConstantsBrowser, () => new TSOConstantsTableWindow(this).Show() },
                { HexDumperEdithPluginItem, () => new HexDumpWindow(this).Show() },
                { OpenCSTDirectoryItem, () => ShowPlugin(new CSTDirectoryControl()) },
                { OpenFAR3ArchiveItem, () => FARShowExplorer(FAR3Control.FARMode.FAR3) },
                { OpenFAR1ArchiveItem, () => FARShowExplorer(FAR3Control.FARMode.FAR1) },
                { OpenFARV1BArchiveItem, () => FARShowExplorer(FAR3Control.FARMode.FAR1_v1B) },
                { ClosePluginItem, ClosePlugin },
                { NewWindowItem, CreateNewInstance },
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
        private void FARShowExplorer(FAR3Control.FARMode Mode) => ShowPlugin(new FAR3Control(Mode));
        private void OpenWebsite(string Website) => Process.Start("explorer", Website);
        private void CheckForUpdates() => OpenWebsite(@"https://github.com/JDrocks450/nio2so/releases");
        private void CreateNewInstance()
        {
            string path = Environment.ProcessPath;
            if (string.IsNullOrWhiteSpace(path)) return;
            Process p = Process.Start(path);
            _processes.Add(p.Id, p);
        }
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

        private void HOST_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //todo: make ui for this that is cancellable and forceable

            //CLOSE ALL PROCESSES
            for (int i = 0; i < _processes.Count; i++)
            {
                var proc = _processes.Values.ElementAt(i);
                proc.CloseMainWindow(); // close cleanly to prevent data loss in case any pages override closing() behavior
                proc.WaitForExit(); // wait for exit ... this certainly won't cause hangs. nope.
            }
        }
    }
}
