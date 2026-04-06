using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.DataService.Common.Types.Top100
{
    /// <summary>
    /// Schema for Data Service-supplied Top 100 list info.
    /// <para/>This is a version-independent data structure for Top 100 list information, which then used on a per-protocol basis for Pre-Alpha or Play-Test.
    /// </summary>
    public class Top100ListInfo
    {
        public Top100ListInfo() { }
        public Top100ListInfo(uint listID, string listType, string listName, string iconResourceURI) : base()
        {
            ListID = listID;
            ListType = listType;
            ListName = listName;
            IconResourceURI = iconResourceURI;
        }
        /// <summary>
        /// The ID of the list in the database
        /// </summary>
        public uint ListID { get; set; }
        /// <summary>
        /// The name of the Top 100 list type, e.g. "Avatars", "Houses", "Clubs", "Neighborhoods"
        /// </summary>
        public string ListType { get; set; } = "Avatars";
        /// <summary>
        /// The name of this list to be displayed in the UI, e.g. "Bisquick's Top Picks", "Splash Zone", "questionable sandwich"
        /// </summary>
        public string ListName { get; set; } = "";
        /// <summary>
        /// The file path on the server file system to the icon for this Top 100 list, 
        /// which should be then converted to a BMP RLE8 INDEXED format and sent to the client as a byte array in the protocol handler.
        /// </summary>
        public string IconResourceURI { get; set; } = "";
    }
}
