using nio2so.Formats.UI.UIScript;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Formats.UI.TSOTheme
{
    /// <summary>
    /// Dereferences Image Asset links automatically.
    /// <para/>A reference to Mr.Shipper used in FreeSO/TSO.     
    /// </summary>
    internal static class MrsShipper
    {
        private static string[] Extensions =
        {
            ".bmp", ".tga"
        };

        /// <summary>
        /// Yeah... definitely just noticed that packingslips is a thing.
        /// <para>This function will read the packingslips file and build a database.</para>
        /// </summary>
        public static void BreakdownPackingslips(string TSODirectory, TSOThemeFile File)
        {
            //TRY PRE-ALPHA FIRST
            string packingPath = System.IO.Path.Combine(TSODirectory, "packingslips", "packingslip.log");
            TSOThemeFile.ThemeVersionNames versionName = TSOThemeFile.ThemeVersionNames.PreAlpha;
            //Not found, try N&I
            if (!System.IO.Path.Exists(packingPath))
            {
                packingPath = System.IO.Path.Combine(TSODirectory, "packingslips", "packingslips.txt");
                if (!System.IO.Path.Exists(packingPath))
                    throw new FileNotFoundException("Could not locate packingslips in the The Sims Online directory.");
                versionName = TSOThemeFile.ThemeVersionNames.NandI;
            }
            if (File.GetVersionName() != versionName)
            {
                Debug.WriteLine($"[TSOTheme] WARNING! TSOTheme file was CLEARED due to version mis-match! Rebuilding packingslips...");
                File.Clear(); // Yikes! This is not a good way to do this. It should have multiple theme files but too much to do now
            }
            switch (versionName)
            {
                case TSOThemeFile.ThemeVersionNames.NandI:
                    DoNIPackingslips(File, packingPath);
                    break;
                case TSOThemeFile.ThemeVersionNames.PreAlpha:
                    DoPreAlphaPackingslips(File, packingPath);
                    break;
            }
            File.SetVersionName(versionName);
        }

        private static void DoNIPackingslips(TSOThemeFile File, string packingPath)
        {
            //Open file for reading
            using (StreamReader sr = new StreamReader(packingPath))
            {
                while (!sr.EndOfStream)
                {
                    string currentLine = sr.ReadLine();
                    if (!currentLine.StartsWith("LoadPackingSlips"))
                        continue;
                    break;
                }
                while (!sr.EndOfStream)
                {
                    //read each line
                    string lineText = sr.ReadLine();
                    lineText = lineText.TrimStart();
                    if (lineText.StartsWith("LoadPackingSlips"))
                        break;
                    char escapeChar = (char)0x09;
                    string hexText = lineText.Substring(0, lineText.IndexOf(' '));
                    string pathText = lineText.Substring(lineText.IndexOf(' '));
                    pathText = pathText.TrimStart().TrimEnd();
                    ulong assetID = Convert.ToUInt64(hexText, 16);
                    if (File.TryGetValue(assetID, out _))
                    {
                        Debug.WriteLine($"[TSOTheme] Packingslip {assetID} was ignored as it already exists.");
                        continue;
                    }
                    Debug.WriteLine($"[TSOTheme] Packingslip {assetID} was added to the current theme.");
                    File.Add(assetID, new(pathText));
                }
            }
        }

        private static void DoPreAlphaPackingslips(TSOThemeFile File, string packingPath)
        {
            //Open file for reading
            using (StreamReader sr = new StreamReader(packingPath))
            {
                string lineAmount = sr.ReadLine();
                uint linesCount = uint.Parse(lineAmount);
                //read each line
                for (int line = 0; line < linesCount; line++)
                {
                    string lineText = sr.ReadLine();
                    char escapeChar = (char)0x09;
                    string hexText = lineText.Substring(0, 18);
                    string pathText = lineText.Substring(23);
                    ulong assetID = Convert.ToUInt64(hexText, 16);
                    if (File.TryGetValue(assetID, out _))
                    {
                        Debug.WriteLine($"[TSOTheme] Packingslip {assetID} was ignored as it already exists.");
                        continue;
                    }
                    Debug.WriteLine($"[TSOTheme] Packingslip {assetID} was added to the current theme.");
                    File.Add(assetID, new(pathText));
                }
            }
        }

        /// <summary>
        /// Populates the <see cref="TSOThemeDefinition.FilePath"/> property using comments found in <see cref="UIScript.UIScriptFile"/>
        /// </summary>
        [Obsolete] 
        public static void DereferenceImageDefines(UIScriptFile File, TSOThemeFile Theme, out int Completes)
        {
            bool MatchExtension(string CommentText, out string? Match)
            {
                foreach(var extension in Extensions)
                {
                    Match = extension;
                    if (CommentText.Contains(extension)) return true;
                }
                Match = default;
                return false;
            }
            void Sanitize(ref string codeText)
            {
                codeText = codeText.Replace("path", "");
                codeText = codeText.Replace(" ", "").Replace("=", "");
                codeText = codeText.Replace("\"", "");
                codeText = codeText.Replace("./uigraphics", "");
            }

            var items = File.NestedSearch();
            int runningCount = -1;
            Completes = 0;
            UIScriptDefineComponent? currentDefine = default;
            foreach(var item in items)
            {
                runningCount++;
                if (item is UIScriptDefineComponent define)
                {
                    currentDefine = define;
                    continue;
                }
                if (currentDefine != null)
                {
                    if (item is UICommentComponent comment)
                    { // possible match here
                        string codeText = comment.Text;
                        if ((codeText.Contains('\\') || codeText.Contains('/')) && MatchExtension(codeText, out string? extension))
                        {
                            Sanitize(ref codeText);
                            codeText = codeText.Remove(codeText.IndexOf(extension) + extension.Length);
                            ulong AssetID = currentDefine.GetAssetID();
                            if (Theme.TryGetValue(AssetID, out TSOThemeDefinition? definition))
                            {
                                definition.FilePath = codeText;
                                Completes++;
                            }
                            else
                            {
                                Theme.Add(AssetID, new TSOThemeDefinition(codeText));
                                Completes++;
                            }
                        }
                    }
                    currentDefine = null;
                }
                else continue;
            }
        }
    }
}
