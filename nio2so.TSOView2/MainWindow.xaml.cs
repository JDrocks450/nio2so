using nio2so.Formats.UI.UIScript;
using nio2so.TSOView2.Formats.UIs;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
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
                { OpenUIsItem, UIsHandler.Current.PromptUserOpenFile }
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

        private void UIDefaultMenuStrip_InvokeAction(object sender, RoutedEventArgs e)
        {
            //If this named MenuItem has an attached handler, it will be invoked here
            if (uiInvokableActionMap.TryGetValue((MenuItem)sender, out Action? member))
                member();
        }
    }
}
