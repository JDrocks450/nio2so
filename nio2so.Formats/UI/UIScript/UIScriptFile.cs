using System.Data;

namespace nio2so.Formats.UI.UIScript
{
    public class UIScriptFile : UIScriptGroup, ITSOImportable
    {
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
