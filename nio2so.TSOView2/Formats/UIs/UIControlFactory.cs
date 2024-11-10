using nio2so.Formats.UI.UIScript;
using nio2so.TSOView2.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using nio2so.Formats.UI.TSOTheme;
using System.Xml.Linq;
using System.ComponentModel;
using nio2so.TSOView2.Formats.UIs.Controls;

namespace nio2so.TSOView2.Formats.UIs
{
    internal static class UIControlFactory
    {
        /// <summary>
        /// Gets the <see cref="UIsHandler.CurrentFile"/>
        /// </summary>
        private static UIScriptFile CurrentUIScriptFile => UIsHandler.Current.CurrentFile;
        public static BitmapSource GetManaged(System.Drawing.Image image)
        {
            if (image == null) image = new System.Drawing.Bitmap(1, 1);
            using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(image))
            {
                bmp.MakeTransparent(System.Drawing.Color.FromArgb(255, 255, 0, 255)); // magenta
                return bmp.Convert(true);
            }
        }
        public static BitmapSource GetManaged(ulong AssetID)
        {
            var theme = UIsHandler.Current.CurrentTheme;
            var image = theme[AssetID].TextureRef;
            bool disposeMe = false;
            if (image == null)
            {
                image = new System.Drawing.Bitmap(1, 1);
                disposeMe = true;
            }
            using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(image))
            {
                bmp.MakeTransparent(System.Drawing.Color.FromArgb(255, 255, 0, 255)); // magenta                
                var managed = bmp.Convert(true);
                if (disposeMe) image.Dispose();
                return managed;
            }
        }
        public static ImageBrush MakeImage(ulong AssetID)
        {
            var managed = GetManaged(AssetID);
            return new ImageBrush(managed);
        }
        public static ImageBrush? MakeImageName(string Name)
        {
            var define = CurrentUIScriptFile.GetDefineByName(Name);
            if (define == default) return default;
            var assetID = define.GetAssetID();
            return MakeImage(assetID);
        }
        public static Image? MakeImageFromDefinition(TSOThemeDefinition Definition)
        {
            if (Definition?.TextureRef == default) return default;
            return new Image()
            {
                Name = System.IO.Path.GetFileNameWithoutExtension(Definition.FilePath).Replace("-","").Replace(" ",""),
                Source = GetManaged(Definition.TextureRef),
                Width = Definition.TextureRef.Width,
                Height = Definition.TextureRef.Height,
            };
        }
        public static void ApplyUniversalControlProperties(UIScriptObject Control, FrameworkElement Element)
        {
            foreach (var property in Control.GetProperties())
                ApplyUniversalControlProperty(property, Element);
        }
        public static void ApplyUniversalControlProperty(UIScriptComponentProperty Property, FrameworkElement Element)
        {
            switch (Property.Name.ToLower())
            {
                case "id":
                    {
                        var value = Property.Value.GetValue<UIScriptString>();
                        Element.Tag = value;
                    }
                    break;
                case "size":
                    {
                        var size = Property.Value.GetValue<UIScriptValueTuple>();
                        Element.Width = size.Values.ElementAt(0);
                        Element.Height = size.Values.ElementAt(1);
                    }
                    break;
                case "position":
                    {
                        var position = Property.Value.GetValue<UIScriptValueTuple>();
                        Canvas.SetLeft(Element, position.Values.ElementAt(0));
                        Canvas.SetTop(Element, position.Values.ElementAt(1));
                    }
                    break;
                case "image":
                    {
                        var name = Property.Value.GetValue<UIScriptString>();
                        var brush = MakeImageName(name);
                        if (brush != null)
                        {
                            RenderOptions.SetBitmapScalingMode(brush, BitmapScalingMode.NearestNeighbor);                            
                            Element.Width = brush.ImageSource.Width;
                            Element.Height = brush.ImageSource.Height;
                            Element.SetValue(Control.BackgroundProperty, brush);
                        }
                    }
                    break;
                case "tooltip":
                    {
                        var tip = Property.Value.GetValue<UIScriptString>();
                        Element.ToolTip = new TextBlock
                        {
                            Text = (string)tip,
                            Style = null,
                            FontFamily = new FontFamily("Comic Sans MS"),
                            Foreground = Brushes.Black
                        };
                    }
                    break;
            }
        }

        public static void ApplyTextEditProperties(UIScriptObject Control, TextBox Text)
        {
            var control = Control;
            Text.Text = Control.Name;            

            foreach (var property in control.GetProperties())
            {
                switch (property.Name.ToLower())
                {
                    case "font":
                        Text.FontSize = property.Value.GetValue<UIScriptNumber>() + 3;
                        break;
                    case "color":
                    case "textcolor":
                        var colorValues = property.Value.GetValue<UIScriptValueTuple>();
                        var color = Color.FromRgb((byte)colorValues.Value1, (byte)colorValues.Value2, (byte)colorValues.Value3);
                        Text.Foreground = new SolidColorBrush(color);
                        break;
                    case "text":
                        var text = property.Value.GetValue<UIScriptString>();
                        Text.Text = (string)text;
                        break;
                    default: 
                        ApplyUniversalControlProperty(property, Text);
                        break;
                }
            }
        }

        public static void ApplyTextControlProperties(UIScriptObject Control, TextBlock Text)
        {
            var control = Control;
            Text.Text = Control.Name;

            foreach (var property in control.GetProperties())
            {
                switch (property.Name.ToLower())
                {
                    case "font":
                        Text.FontSize = property.Value.GetValue<UIScriptNumber>() + 3;
                        break;
                    case "color":
                        var colorValues = property.Value.GetValue<UIScriptValueTuple>();
                        var color = Color.FromRgb((byte)colorValues.Value1, (byte)colorValues.Value2, (byte)colorValues.Value3);
                        Text.Foreground = new SolidColorBrush(color);
                        break;
                    case "opaque":
                        double opacity = property.Value.GetValue<UIScriptNumber>();
                        if (opacity > 0) Text.Opacity = .5;
                        break;
                    case "wrapped":
                        int wrapping = property.Value.GetValue<UIScriptNumber>();
                        if (wrapping > 0) Text.TextWrapping = TextWrapping.Wrap;
                        break;
                    case "alignment":
                        int alignment = property.Value.GetValue<UIScriptNumber>();
                        switch (alignment)
                        {
                            case 0: //Left
                                break;
                            case 1:
                                Text.TextAlignment = TextAlignment.Center;
                                break;
                            case 2:
                                Text.TextAlignment = TextAlignment.Right;
                                break;
                            case 3:
                                Text.TextAlignment = TextAlignment.Center;
                                break;
                        }
                        break;
                    case "text":                        
                        var text = property.Value.GetValue<UIScriptString>();
                        Text.Text = (string)text;
                        break;
                    default:
                        ApplyUniversalControlProperty(property, Text);
                        break;
                }
            }
        }

        public static void ApplyButtonControlProperties(UIScriptObject Control, TSOButton Button)
        {
            var control = Control;
            var btn = Button;

            foreach (var property in control.GetProperties())
            {
                switch (property.Name.ToLower())
                {
                    case "buttonimage":
                        goto case "image"; // makes this scenario debuggable
                    case "image":
                        var name = property.Value.GetValue<UIScriptString>();
                        var brush = MakeImageName(name);
                        if (brush != null)
                        {                            
                            RenderOptions.SetBitmapScalingMode(brush, BitmapScalingMode.NearestNeighbor);
                            btn.Background = brush;
                            btn.Width = brush.ImageSource.Width / 4;
                            btn.Height = brush.ImageSource.Height;
                        }
                        else btn.Background = Brushes.Red;
                        break;
                    case "text":
                        {
                            var text = property.Value.GetValue<UIScriptString>();
                            Button.Content = (string)text;
                        }
                        break;
                    default:
                        ApplyUniversalControlProperty(property, btn);
                        break;
                }
            }
        }

        public static T? MakeControl<T>(UIScriptObject Control) where T : FrameworkElement
        {
            var control = Control;
            FrameworkElement element = default;
            switch (control.Type.ToLower())
            {
                case "button":
                    TSOButton btn = new TSOButton()
                    {
                        Background = null,
                        BorderBrush = null,
                    };
                    ApplyButtonControlProperties(control, btn);
                    btn.Reset();
                    Panel.SetZIndex(btn, 2);
                    element = btn;
                    break;
                case "text":
                    TextBlock block = new TextBlock()
                    {
                        FontFamily = new FontFamily("Comic Sans MS"),
                        ClipToBounds = false,
                    };
                    ApplyTextControlProperties(control, block);
                    block.Height = double.NaN;
                    Panel.SetZIndex(block, 3);
                    element = block;
                    break;
                case "genericcontrol":
                    Border contentControl = new Border()
                    {

                    };
                    ApplyUniversalControlProperties(control, contentControl);
                    element = contentControl;
                    break;
                case "scrollabletext":
                    {
                        TextBox textBox = new TextBox()
                        {
                            IsReadOnly = true,
                            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                        };
                        ApplyTextEditProperties(control, textBox);
                        element = textBox;
                    }
                    break;
                case "textedit":
                    {
                        TextBox textBox = new TextBox()
                        {
                            IsReadOnly = true,
                        };
                        ApplyTextEditProperties(control, textBox);
                        element = textBox;
                    }
                    break;
            }
            if (element != null)
                element.Name = Control.Name;
            return element as T;
        }
    }
}
