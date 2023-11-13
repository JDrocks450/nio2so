using nio2so.Formats.CST;
using System.Data;

namespace nio2so.Formats.UI.UIScript
{
    public class UIScriptFile : UIScriptGroup, ITSOImportable
    {
        /// <summary>
        /// After importing the <see cref="UIScriptFile"/> using the <see cref="TSOUIScriptImporter.Import(Stream)"/>
        /// method, this is populated with all <see cref="CSTFile"/>s actually referenced by a control in this document. 
        /// </summary>
        public HashSet<uint> ReferencedCSTFiles { get; } = new();
        /// <summary>
        /// Be careful with this -- uses a nested search algorithm. Need to optimze this later.
        /// </summary>
        public IEnumerable<UIScriptDefineComponent> Defines => GetItems<UIScriptDefineComponent>();
        public UIScriptDefineComponent? GetDefineByName(string Name) => Defines.FirstOrDefault(x => x.Name.ToLowerInvariant() == Name.Replace("\"","").ToLowerInvariant());
        /// <summary>
        /// Returns all comments that are likely referencing a file name
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UICommentComponent> Intelligence_ReturnPossibleFileNames()
        {
            var comments = Comments;
            return comments.Where(x => x.Text.Contains('\\') || x.Text.Contains('.'));
        }
    }
}
