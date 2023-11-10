using nio2so.Formats.UI.UIScript;
using System;
using System.Collections.Generic;
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
        /// Populates the <see cref="TSOThemeDefinition.FilePath"/> property using comments found in <see cref="UIScript.UIScriptFile"/>
        /// </summary>
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
