using Microsoft.Win32;
using nio2so.Formats.TSOData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace nio2so.TSOView2.Formats.TSOData
{
    /// <summary>
    /// Interaction logic for TSODataDefinitionExplorerWindow.xaml
    /// </summary>
    public partial class TSODataDefinitionExplorerWindow : Window
    {
        enum UI_TSODATADEF_PAGE
        {
            LevelOne,
            LevelTwo,
            Derived,
            Strings
        }
        TSODataFile? CurrentFile;

        public TSODataDefinitionExplorerWindow()
        {
            InitializeComponent();

            if (!string.IsNullOrWhiteSpace(TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_BaseDirectory))
                TryAutoFindAndOpenTSODataDefinition(TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_BaseDirectory);
        }

        private bool TryAutoFindAndOpenTSODataDefinition(string BaseDirectory)
        {
            if (string.IsNullOrWhiteSpace(BaseDirectory)) return false;
            var results = Directory.GetFiles(BaseDirectory, "*DataDefinition.dat");
            string path = results.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(path)) return false;
            if (!File.Exists(path)) return false;
            OpenFile(path);
            return true;
        }

        private void OpenFile(string FilePath)
        {
            string selectedFile = FilePath;
            if (string.IsNullOrWhiteSpace(selectedFile)) return; // ??
            if (System.IO.Path.GetExtension(selectedFile).EndsWith("json"))
                CurrentFile = OpenJson(selectedFile);
            else if (System.IO.Path.GetExtension(selectedFile).EndsWith("dat"))
                CurrentFile = OpenDat(selectedFile);
            else throw new InvalidDataException("File extension can only be: json or dat");            
            InvokeUIRedraw(UI_TSODATADEF_PAGE.LevelOne);
        }
        private TSODataFile? OpenJson(string FilePath) => JsonSerializer.Deserialize<TSODataFile>(FilePath);
        private TSODataFile OpenDat(string FilePath) => TSODataImporter.Import(FilePath);

        //**UI EVENTS

        private void InvokeUIRedraw(UI_TSODATADEF_PAGE Page, string? SearchFilter = default)
        {
            if (string.IsNullOrWhiteSpace(SearchFilter))
                SearchFilter = null;            
            FilterNotification.Visibility = SearchFilter != null ? Visibility.Visible : Visibility.Collapsed;
            FilterLabel.Text = SearchFilter;

            TreeViewItem ShowBasicStructs(IEnumerable<TSODataStruct> Types)
            {
                TypeViewer.Items.Clear();
                TreeViewItem parentItem = new()
                {
                    Header = "Types",
                    IsExpanded = true
                };
                TypeViewer.Items.Add(parentItem);
                foreach (var type in Types.Where(x => SearchFilter == null || x.NameString.Contains(SearchFilter, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var typeItem = new TreeViewItem()
                    {
                        Header = $"{type.NameString} ({type.FieldCount})",
                        IsExpanded = true
                    };
                    typeItem.Selected += delegate
                    {
                        DataViewer.ItemsSource = type.Fields;
                    };
                    parentItem.Items.Add(typeItem);
                    foreach (var field in type.Fields)
                    {
                        typeItem.Items.Add(new TreeViewItem()
                        {
                            Header = $"{field.TypeString} {field.NameString} ({field.Classification})"
                        });
                    }                    
                }
                return parentItem;
            }
            TreeViewItem ShowDerivedStructs(IEnumerable<TSODerivedStruct> Types)
            {
                TypeViewer.Items.Clear();
                TreeViewItem parentItem = new()
                {
                    Header = "Types",
                    IsExpanded = true
                };
                TypeViewer.Items.Add(parentItem);
                foreach (var type in Types.Where(x => SearchFilter == null || x.NameString.Contains(SearchFilter, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var typeItem = new TreeViewItem()
                    {
                        Header = $"{type.NameString} ({type.ParentString})",
                        IsExpanded = true
                    };
                    typeItem.Selected += delegate
                    {
                        DataViewer.ItemsSource = type.FieldMasks;
                    };
                    parentItem.Items.Add(typeItem);
                    foreach (var field in type.FieldMasks)
                    {
                        typeItem.Items.Add(new TreeViewItem()
                        {
                            Header = $"{field.Values} {field.NameString}"
                        });
                    }
                }
                return parentItem;
            }

            StructTabViewer.IsEnabled = CurrentFile != null;
            if (CurrentFile == null) return;

            Title = "TSODataFile - " + CurrentFile.TimeStamp;

            StructTabViewer.SelectionChanged -= StructTabViewer_SelectionChanged;
            StructTabViewer.SelectedIndex = (int)Page;
            StructTabViewer.SelectionChanged += StructTabViewer_SelectionChanged;
            
            TypeViewer.Items.Clear();
            TreeViewItem? ParentItem = null;

            switch (Page)
            {
                case UI_TSODATADEF_PAGE.LevelOne:
                case UI_TSODATADEF_PAGE.LevelTwo:
                    ParentItem = ShowBasicStructs(Page switch
                    {
                        UI_TSODATADEF_PAGE.LevelOne => CurrentFile.LevelOneStructs,
                        UI_TSODATADEF_PAGE.LevelTwo => CurrentFile.LevelTwoStructs,
                    });
                    break;
                case UI_TSODATADEF_PAGE.Derived:
                    ParentItem = ShowDerivedStructs(CurrentFile.DerivedStructs);
                    break;
                case UI_TSODATADEF_PAGE.Strings:
                    Dictionary<TSODataStringCategories, TreeViewItem> categoryGroup = new();
                    string getSafeName(KeyValuePair<uint,TSODataString> stringRef) => $"{stringRef.Key:X4}: \"{stringRef.Value.Value ?? "null"}\"";
                    foreach (var stringRef in CurrentFile.Strings.Where(x => SearchFilter == null || getSafeName(x).Contains(SearchFilter,StringComparison.InvariantCultureIgnoreCase)))
                    {
                        if (!categoryGroup.TryGetValue(stringRef.Value.Category, out var TreeItem)) {
                            TreeItem = new TreeViewItem()
                            {
                                Header = stringRef.Value.Category.ToString(),
                                IsExpanded = true
                            };
                            TreeItem.Selected += delegate
                            {
                                DataViewer.ItemsSource = CurrentFile.Strings.Where(x => x.Value.Category == stringRef.Value.Category).Select(
                                    y => y.Value);
                            };
                            TypeViewer.Items.Add(TreeItem);
                            categoryGroup.Add(stringRef.Value.Category, TreeItem);
                        }
                        TreeItem.Items.Add(new TreeViewItem()
                        {
                            Header = getSafeName(stringRef)
                        });
                    }
                    break;
            }

            if (ParentItem == null) return;

            //**stylize
            //risky uses reflection
            //oh well
            try
            {
                int index = -1;
                foreach (TreeViewItem item in ParentItem.Items)
                {
                    index++;
                    SolidColorBrush color = (SolidColorBrush)(typeof(Brushes).GetProperties()[index + 27].GetValue(null));
                    item.Foreground = color;
                }
            }
            catch { }
        }

        private void OpenFileItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UI_PromptOpenFile();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }        

        void UI_PromptOpenFile()
        {
            string initialDir = TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_BaseDirectory ??
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            OpenFileDialog dialog = new()
            {
                Title = "Select your TSODataDefinition file",
                InitialDirectory = initialDir,
                CheckFileExists = true,
                Multiselect = false,
                DereferenceLinks = true,
            };
            if (!dialog.ShowDialog() ?? true) return; // USER CANCELLED
            //Open file now
            OpenFile(dialog.FileName);
        }
        void UI_PromptExportFile()
        {
            string initialDir = 
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            SaveFileDialog dialog = new()
            {
                Title = "Export TSODataDefinition file",
                InitialDirectory = initialDir,
                DereferenceLinks = true,
                Filter = "JSON File|*.json",
                FileName = System.IO.Path.Combine(initialDir, "TSODataDefinition.json")
            };
            if (!dialog.ShowDialog() ?? true) return; // USER CANCELLED
            //export file now
            File.WriteAllText(dialog.FileName, 
                JsonSerializer.Serialize<TSODataFile>(CurrentFile, new JsonSerializerOptions()
                {
                    WriteIndented = true
                }));
        }


        private void CloseItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StructTabViewer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UI_TSODATADEF_PAGE page = UI_TSODATADEF_PAGE.LevelOne;
            page = (UI_TSODATADEF_PAGE)StructTabViewer.SelectedIndex;
            InvokeUIRedraw(page);
        }

        internal static void ShowExplorer()
        {
            new TSODataDefinitionExplorerWindow().Show();
        }

        private void ExportItem_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFile == null)
                return;
            try
            {
                UI_PromptExportFile();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SearchItem_Click(object sender, RoutedEventArgs e)
        {
            InvokeUIRedraw((UI_TSODATADEF_PAGE)StructTabViewer.SelectedIndex, SearchBoxItem.Text);
            SearchBoxItem.Text = "";
        }

        private void ResetSearchItem_Click(object sender, RoutedEventArgs e)
        {
            SearchBoxItem.Text = "";
            SearchItem_Click(sender, e);
        }

        private void FilterNotification_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) => ResetSearchItem_Click(sender, e);
    }
}
