using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types.Lot;
using nio2so.DataService.Common.Types.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace nio2so.DataService.Common.Types
{
    /// <summary>
    /// Data file for storing information on Lots
    /// </summary>
    public class LotInfo : ISearchableItem
    {
        public LotInfo() { }

        public LotProfile Profile { get; set; } = new();
        /// <summary>
        /// The owner of the house. To alter this, see: <see cref="Profile"/> and <see cref="LotProfile.OwnerAvatar"/>
        /// </summary>
        public AvatarIDToken Owner => Profile.OwnerAvatar;
        /// <summary>
        /// List of Roommates <i>without</i> the house owner included. Use <see cref="GetRoommates"/> to get complete list with owner.
        /// <para/>See: <see cref="Owner"/>
        /// </summary>
        public List<AvatarIDToken> Roommates { get; set; } = new();
        /// <summary>
        /// Dictates whether this house can be overwritten when the game is saved or not
        /// </summary>
        public bool IsReadOnly { get; set; } = false;

        IEnumerable<string> ISearchableItem.SearchableKeywords => [Profile.Name,Profile.Description];

        /// <summary>
        /// Gets a complete list of all roommates in the house combining <see cref="Roommates"/> and <see cref="Owner"/>. <see cref="Owner"/> is first in the returned list.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AvatarIDToken> GetRoommates()
        {
            return [Owner, ..Roommates];
        }
    }
}
