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
    /// <para/>A reference to Mr.Shipper used in FreeSO.     
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
            string packingPath = System.IO.Path.Combine(TSODirectory, "packingslips", "packingslip.log");
            //Open file for reading
            using (StreamReader sr = new StreamReader(packingPath))
            {
                string lineAmount = sr.ReadLine();
                uint linesCount = uint.Parse(lineAmount);
                //read each line
                for(int line = 0; line < linesCount; line++)
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
