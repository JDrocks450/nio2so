using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace nio2so.TSOView2.Plugins
{
    /// <summary>
    /// This plugin will take in a constants table formatted like so:
    /// <code>0xFFFFFFFF: ConstantName</code>
    /// and instead make it like this:
    /// <code>ConstantName = 0xFFFFFFFF,</code>
    /// because these can then be used as <see langword="enum"/> in the codebase
    /// </summary>
    internal static class EnumFixerUpperPlugin
    {
        const string MYMSG = "This plugin will take in a constants table formatted like so:\r\n" +
            "\"0xFFFFFFFF: ConstantName\"\r\n" +
            "and instead make it like this:\r\n" +
            "\"ConstantName = 0xFFFFFFFF,\"\r\n" +
            "because these can then be used as enums in the codebase.";
        const string THANKYOUMSG = "Here you go :)";

        /// <summary>
        /// Prompts the user for text, spits out the formatted version
        /// </summary>
        public static void Do()
        {
            var window = makeWindow("Enter Text", MYMSG, out var TextBox);
            if (!window.ShowDialog() ?? true) return;
            string formatString = TextBox.Text;
            if (string.IsNullOrWhiteSpace(formatString)) return;
            formatString = FormatText(formatString);
            window = makeWindow("Thank me later", THANKYOUMSG, out _, formatString);
            window.Show();
        }
        private static Window makeWindow(string title, string caption, out TextBox ContentBox, string? content = default)
        {
            if (content == null) content = "";
            Window blankWindow = new Window()
            {
                Title = title,
            };
            DockPanel dockPanel = new DockPanel();
            var titleBlock = new TextBlock()
            {
                Text = MYMSG
            };
            DockPanel.SetDock(titleBlock, Dock.Top);
            dockPanel.Children.Add(titleBlock);
            TextBox spitText = new()
            {
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                AcceptsTab = true,
                Text = content
            };
            Button dismissButton = new Button()
            {
                Content = "OK",
                IsDefault = true,
            };
            dismissButton.Click += delegate
            {                
                blankWindow.DialogResult = true;
                blankWindow.Close();
            };
            DockPanel.SetDock(dismissButton, Dock.Bottom);
            dockPanel.Children.Add(dismissButton);
            dockPanel.Children.Add(spitText);
            blankWindow.Content = dockPanel;
            ContentBox = spitText;

            return blankWindow;
        }
        private static string FormatText(string RawText)
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<string, string> mapValues = new();
            using StringReader reader = new StringReader(RawText);
            int skips = 0, duplicates = 0;
            while (reader.Peek() > 0)
            {
                string? line = reader.ReadLine();
                if (line == null) break;
                line = line.Trim();
                if (!line.StartsWith("0x")) goto skip; // comment? 
                if (!line.Contains(':')) goto skip; // not formatted correctly?
                string value = line.Substring(0, line.IndexOf(':'));
                string name = line.Substring(line.IndexOf(":") + 1);
                //**sanitize a little bit
                name = name.Replace('.', '_'); // add more as needed
                //**
                if (mapValues.ContainsKey(name)) goto dup; // duplicate
                mapValues.Add(name, value);
                sb.AppendLine($"{name} = {value},");
                continue;
                dup:
                duplicates++;
                skip:
                skips++;
            }
            if (skips > 0) MessageBox.Show($"Skipped {skips} line(s). Found {duplicates} duplicate entry(s).");
            return sb.ToString();
        }
    }
}
