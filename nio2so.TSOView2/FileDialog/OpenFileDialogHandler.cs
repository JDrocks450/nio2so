namespace nio2so.TSOView2.FileDialog
{
    /// <summary>
    /// Provides functions to open/save files from disk
    /// </summary>
    internal static class FileDialogHandler
    {
        public static bool? ShowUser_SelectCityFolder(out string? FileName, string? DefaultDirectory = default)
        {
            TSOViewCitySelectorDialog dlg = new();
            dlg.ShowDialog();            
            FileName = dlg.FileName;
            return dlg.DialogResult;
        }
    }
}
