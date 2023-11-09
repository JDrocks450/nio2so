using System.ComponentModel;

namespace nio2so.Formats.UI.UIScript
{
    [DisplayName("Script Comment")]
    public class UICommentComponent : UIScriptComponentBase
    {
        public UICommentComponent(string text)
        {
            Text = text;
        }

        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}