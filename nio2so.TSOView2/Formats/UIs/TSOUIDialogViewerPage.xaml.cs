﻿using nio2so.Formats.UI.TSOTheme;
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
            //CHECK IF GAME PATH IS SET
            if (!UIsHandler.Current.EnsureSetGameDirectoryFirstRun()) return;
            string? basePath = TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_BaseDirectory;            
            // BASE PATH IS NOT NULL
            // THE SIMS ONLINE DIRECTORY ISN'T THE SAME AS THE UI SCRIPT
            // -- could be wrong, allow the user to bypass if false positive, obviously.
            if (basePath.Substring(0, Math.Min(basePath.Length, UIsHandler.Current.CurrentFilePath.Length)) !=
                UIsHandler.Current.CurrentFilePath.Substring(0, Math.Min(basePath.Length, UIsHandler.Current.CurrentFilePath.Length)))
            {
                if (MessageBox.Show("The Sims Online directory chosen isn't the same as where your UI Script is. Want to select a new " +
                    "The Sims Online directory? Failure to do so will probably cause the import to fail.",
                    "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    //sneaky trick to make it convenient for the user
                    string initialDirectory = System.IO.Path.GetDirectoryName(UIsHandler.Current.CurrentFilePath);
                    while (true)
                    {
                        TSOViewConfigHandler.Directory_PromptAndSaveResult("Select a The Sims Online Directory",
                            ref initialDirectory);
                        if (MessageBox.Show($"You selected: {initialDirectory}. Is this correct?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            break;
                    }
                    basePath = initialDirectory;
                }
            }
            if (basePath == null) return;  
            //Ensure if this game directory changed, reload necessary components
            UIsHandler.Current.ChangeGameDirectory(basePath);

            //DEREFERNCE CONTENT FIRST
            bool successful = theme.Initialize(basePath,                
                CurrentUIScriptFile,
                out string[] Missing);            

            if (!successful)
            {
                //MISSING REFS!                
                if (!retriedOnce)
                {
                    //warn user this may take a very long time
                    if (MessageBox.Show($"Missing ({Missing.Length}) references. Reload packingslips from The Sims Online installation? " +
                    $"This may take a while.", "Mrs. Shipper has a message", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    {
                        //skip reloading the packingslips
                        goto skip;
                    }
                    theme.UpdateDatabaseWithMrsShipper(TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_BaseDirectory);
                    theme.Save(TSOViewConfigHandler.CurrentConfiguration.TheSimsOnlinePreAlpha_ThemePath);
                    retriedOnce = true;

                    goto retry;
                }
                MessageBox.Show("Tried to use Mrs. Shipper to read AssetID database. Failed. Check your TSO installation.\n" +
                    $"We're missing these assets: \n{string.Join(", ", Missing)}");
            }            
        skip:
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
