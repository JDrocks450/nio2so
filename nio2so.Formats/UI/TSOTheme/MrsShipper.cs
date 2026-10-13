using nio2so.Data.Common;
using nio2so.Formats.UI.UIScript;
using System.Diagnostics;

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
        public static Task BreakdownPackingslips(string TSODirectory, TSOThemeFile File, Action<AsyncStatusCompletion> AsyncCallback, CancellationToken? Token = default)
        {
            return Task.Run(async delegate
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
                        await DoNIPackingslips(File, packingPath, AsyncCallback);
                        break;
                    case TSOThemeFile.ThemeVersionNames.PreAlpha:
                        await DoPreAlphaPackingslips(File, packingPath);
                        break;
                }
                File.SetVersionName(versionName);
            }, Token.HasValue ? Token.Value : CancellationToken.None);
        }

        private static Task DoNIPackingslips(TSOThemeFile File, string packingPath, Action<AsyncStatusCompletion> Callback)
        {
            return Task.Run(delegate
            {
                //Open file for reading
                using (StreamReader sr = new StreamReader(packingPath))
                {
                    bool functionNameFound = false;
                    long startOffset = -1;
                    int totalDataAmount = 1, currentIndex = -1;

                    sr.BaseStream.Seek(0, SeekOrigin.Begin);
                    sr.DiscardBufferedData();

                    // ** find total data amount (attempt to any way)

                    while (!sr.EndOfStream) // LoadPackingSlips log entry in packingslips.txt indicates file start
                    { // we can sniff the end of the file for the total amount of data to load
                        string currentLine = sr.ReadLine();
                        if (currentLine.StartsWith("LoadPackingSlips"))
                        {                            
                            if (currentLine.ToLower().Contains("assets")) // number of data elements
                            {
                                string numberOfEntries = "";
                                for (int i = 0; i < currentLine.Length; i++)
                                {
                                    if (char.IsDigit(currentLine[i]))
                                        numberOfEntries += currentLine[i];                                    
                                }
                                int.TryParse(numberOfEntries, out totalDataAmount);
                                break;
                            }
                        }
                    }

                    // **                     
                    
                    // rewind and discard buffers

                    sr.BaseStream.Seek(0, SeekOrigin.Begin);
                    sr.DiscardBufferedData();

                    while (!sr.EndOfStream)
                    {
                        //read each line
                        string lineText = sr.ReadLine();

                        //** if the packing slips log hasn't started again, throw away this line
                        // packing slips start with LoadPackingSlips : begin
                        //this code detects that ... StreamReader is so bad, that i dont feel like moving to a better solution, just check thru 
                        // the beginning of the file again
                        if (!functionNameFound && lineText.StartsWith("LoadPackingSlips") &&
                            lineText.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains("start"))
                        {
                            functionNameFound = true;
                            startOffset = sr.BaseStream.Position;
                            continue;
                        }

                        if (!functionNameFound) continue;
                        //**

                        lineText = lineText.TrimStart();
                        if (lineText.StartsWith("LoadPackingSlips"))
                            break;
                        char escapeChar = (char)0x09;
                        string hexText = lineText.Substring(0, lineText.IndexOf(' '));
                        string pathText = lineText.Substring(lineText.IndexOf(' '));
                        pathText = pathText.TrimStart().TrimEnd();
                        ulong assetID = Convert.ToUInt64(hexText, 16);                        
                        bool result = File.TryAdd(assetID, new(pathText));

                        Callback?.Invoke(new AsyncStatusCompletion("Mrs. Shipper", pathText, currentIndex / (double)totalDataAmount, false));

                        if (!result)                        
                            Debug.WriteLine($"[TSOTheme] Packingslip {assetID} was ignored as it already exists.");                        
                        else 
                            Debug.WriteLine($"[TSOTheme] Packingslip {assetID} was added to the current theme.");                                               
                    }

                    Callback?.Invoke(new AsyncStatusCompletion("Mrs. Shipper", "done", currentIndex / (double)totalDataAmount, true));
                    // ** if we got this far without these fields, something is really really bad
                    if (!functionNameFound || startOffset == -1)
                        throw new InvalidDataException("This packingslips.txt file doesn't seem formatted correctly.");
                }
            });
        }

        private static Task DoPreAlphaPackingslips(TSOThemeFile File, string packingPath)
        {
            return Task.Run(delegate
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
            });
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
