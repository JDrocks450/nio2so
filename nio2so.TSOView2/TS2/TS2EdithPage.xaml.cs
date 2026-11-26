using Microsoft.Win32;
using nio2so.Formats.TS2.Edith;
using nio2so.TSOView2.Util;
using System;
using System.Collections.Generic;
using System.IO;
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
using static nio2so.Formats.TS2.Edith.TREE;

namespace nio2so.TSOView2.TS2
{
    /// <summary>
    /// Interaction logic for TS2EdithPage.xaml
    /// </summary>
    public partial class TS2EdithPage : Page
    {
        public TS2EdithPage()
        {
            Loaded += TS2EdithPage_Loaded;
            InitializeComponent();
        }

        public bool PromptAndOpenArchive(string InitialDirectory = default)
        {
            string noun = "TREE Chunk binary data";

            string Prompt = $"Open a {noun}";

            if (InitialDirectory == null)
            {
                if (TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_BaseDirectory != null)
                    InitialDirectory = TSOViewConfigHandler.CurrentConfiguration.TheSimsOnline_BaseDirectory;
                else InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            }
            OpenFileDialog fileDialog = new()
            {
                Title = Prompt,
                InitialDirectory = InitialDirectory,
                Multiselect = false,
                DereferenceLinks = true,
                ValidateNames = true,
            };
            if (fileDialog.ShowDialog() == true)
            {
                try
                { // default error handler
                    Open(new FileInfo(fileDialog.FileName));
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open {noun} file:\n{ex.Message}", $"Error Opening {noun} file", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return false;
        }

        private void TS2EdithPage_Loaded(object sender, RoutedEventArgs e) => PromptAndOpenArchive();

        Dictionary<int, Control> visualMap = new();
        Dictionary<int, Control> virtualMap = new();

        private void Open(FileInfo fileInfo)
        {
            int YTransform = 0;

            Canvas p = MainRenderSpace;
            p.Children.Clear();

            nio2so.Formats.TS2.Edith.TREE tree = new();
            using (FileStream file = File.OpenRead(fileInfo.FullName))
            {
                tree.Read(file);
            }

            //**render tree
            Button AddNode(int X, int Y, int Width, int Height, Color Color, string Text, string ToolTip)
            {
                var shape = new Button()
                {
                    Background = new SolidColorBrush(Color),
                    Width = Width,
                    Height = Height,
                    ToolTip = ToolTip
                };
                Canvas.SetLeft(shape, X);
                Canvas.SetTop(shape, YTransform + Y);

                shape.Content = new TextBlock()
                {
                    TextWrapping = TextWrapping.WrapWithOverflow,
                    Foreground = Brushes.White,
                    Text = Text,
                };

                p.Children.Add(shape);
                return shape;
            }

            YTransform = tree.Min(x => x.Y) * -1;
            MainRenderSpace.Width = Math.Abs(Math.Abs(tree.Max(x => x.X))) + 500;
            MainRenderSpace.Height = Math.Abs(YTransform + Math.Abs(tree.Max(x => x.Y)) + 500);

            for (int i = 0; i < tree.Count; i++)
            {
                TREENodeEntry entry = tree[i];
                System.Windows.Media.Color[] colors =
                {
                    Colors.CornflowerBlue,
                    Colors.Green,
                    Colors.Red,
                    Colors.DarkGreen,
                    Colors.Blue,
                    Colors.Purple
                };
                Color getColor(ushort index) => colors.ElementAtOrDefault(index);
                var shape = AddNode(entry.X, entry.Y, entry.Width, entry.Height, getColor((ushort)entry.Type), $"[{i:X}] {entry}", entry.ToString());
                visualMap.Add(i, shape);
            }

            for (int i = 0; i < tree.Count; i++)
            {
                void ADDLINE(Control source, Control destination, Brush stroke, int Thickness = 4)
                {
                    p.Children.Add(new Line()
                    {
                        X1 = Canvas.GetLeft(source) + (source.Width / 2),
                        Y1 = Canvas.GetTop(source) + (source.Height),

                        X2 = Canvas.GetLeft(destination) + (destination.Width / 2),
                        Y2 = Canvas.GetTop(destination),

                        Stroke = stroke,
                        Fill = stroke,
                        StrokeThickness = Thickness,
                        StrokeEndLineCap = PenLineCap.Triangle,                        
                    });
                }

                TREENodeEntry entry = tree[i];
                Control source = visualMap[i];

                visualMap.TryGetValue(entry.TrueConnection, out Control? trueNode);

                if (trueNode != null)
                    ADDLINE(source, trueNode, Brushes.Green);
                if (entry.Type == TREEBoxType.Primitive) {
                    if (entry.FalseConnection != 0xFFFFFFFF)
                    {
                        visualMap.TryGetValue((ushort)entry.FalseConnection, out Control? overrideFalse);
                        if (overrideFalse == null)
                            throw new InvalidDataException("Unknown error FALSE connection.");
                        ADDLINE(source, overrideFalse, Brushes.Red);
                    }
                    else
                    {
                        const int excpW = 65, excpH = 25;
                        var exception = AddNode(entry.X + entry.Width + 20, entry.Y + ((entry.Height / 2) - (excpH / 2)), excpW, excpH, Colors.DarkRed, $"Exception", "Exception");
                        ADDLINE(source, exception, Brushes.DarkRed);
                    }
                }
            }

            //**update information            

            StringBuilder sbuilder = new();
            sbuilder.AppendLine($"Nodes: {tree.Count}");

            int special = 0;
            foreach(TREEBoxType value in Enum.GetValues<TREE.TREEBoxType>())
            {
                int amount = tree.Count(x => x.Type == value);
                sbuilder.AppendLine($"{value}: {amount}");
                special += value != TREEBoxType.Primitive ? amount : 0;
            }
            sbuilder.AppendLine($"Non-Primitive Nodes: {special}");

            InfoBlock.Text = sbuilder.ToString();
        }
    }
}
