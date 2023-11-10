using nio2so.Formats.UI.UIScript;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Formats.UI.TSOTheme
{
    public static class TSOThemeExtensions
    {
        public static bool TryGetReference(this UIScriptDefineComponent DefineComponent, TSOThemeFile Theme, out TSOThemeDefinition? Definition, out ulong AssetID)
        {
            Definition = default;
            AssetID = 0;
            string message = "OK";
            try
            {
                AssetID = GetAssetID(DefineComponent);
            }
            catch (Exception e)
            {
                // can't parse value!
                message = e.Message;
                goto catastrophic;
            }
            if (!Theme.TryGetValue(AssetID, out Definition) || Definition == default)
                goto skip; // Theme definition not found or was null!
            if (Definition.FilePath == null)
                goto skip; // file uri is null!            
            return true;
        skip:
            return false;
        //can't recover!
        catastrophic:
            throw new InvalidDataException($"An error has occured! {message}");
        }

        public static ulong GetAssetID(this UIScriptDefineComponent DefineComponent)
        {
            string message = "OK";
            string? assetIdStr = DefineComponent.GetProperty("assetID")?.GetValue<UIScriptString>()?.Value; // avoid implicit here
            if (assetIdStr == null) // no property found!
            {
                message = "AssetID Property not found!!";
                goto catastrophic;
            }
            try
            {
                return Convert.ToUInt64(assetIdStr, 16);
            }
            catch
            {
                // can't parse value!
                message = $"Can't parse the value: {assetIdStr}!!";
                goto catastrophic;
            }
        catastrophic:
            throw new InvalidDataException($"An error has occured! {message}");
        }
    }
}
