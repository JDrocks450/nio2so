using System.ComponentModel;

namespace nio2so.Formats.UI.UIScript
{
    
    /// <summary>
    /// Denoted by a <c>Begin</c> tag, groups are used to apply properties to a group of <see cref="UIScriptComponentBase"/> items
    /// </summary>
    [DisplayName("Group")] public class UIScriptGroup : UIScriptComponentBase
    {
        /// <summary>
        /// Be careful with this -- uses a nested search algorithm. Need to optimze this later.
        /// </summary>
        public IEnumerable<UIScriptObject> Controls => GetItems<UIScriptObject>();
        /// <summary>
        /// Be careful with this -- uses a nested search algorithm. Need to optimze this later.
        /// </summary>
        public IEnumerable<UICommentComponent> Comments => GetItems<UICommentComponent>();
        public HashSet<UIScriptComponentBase> Items { get; } = new();
        public IEnumerable<T> GetItems<T>() where T : UIScriptComponentBase => NestedSearch().OfType<T>();
        public IEnumerable<UIScriptComponentBase> NestedSearch()
        {      
            List<UIScriptComponentBase> children = new();      
            void GetMyChildren(UIScriptGroup nUIComp)
            {                
                foreach (var uicomp in nUIComp.Items)
                {
                    children.Add(uicomp);
                    if (uicomp is UIScriptGroup group)
                        GetMyChildren(group);
                }
            }
            GetMyChildren(this);
            return children;
        }
    }
}
