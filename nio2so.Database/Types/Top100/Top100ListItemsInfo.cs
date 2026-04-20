using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.DataService.Common.Types.Top100
{
    /// <summary>
    /// Nio2so Schema for an item added to a <see cref="Top100ListItemsInfo"/> body
    /// </summary>
    /// <param name="Rank">The rank of this item, 1-100</param>
    /// <param name="ItemID">The ID of the item in the Database, when clicked, this should open the displayed item.</param>
    /// <param name="ItemName">The name displayed in the list.</param>
    /// <param name="Caption">The subtitle shown on the right side after the name, e.g. a score, money, etc.</param>
    public record nio2soTop100Item(uint Rank, uint ItemID, string ItemName, string Caption);

    /// <param name="ListID"> <inheritdoc cref="Top100ListInfo.ListID"/> </param>
    /// <param name="ListType"> <inheritdoc cref="Top100ListInfo.ListType"/> </param>
    /// <param name="ListName"> <inheritdoc cref="Top100ListInfo.ListName"/> </param>
    /// <param name="ListItems"> List of items contained in this list.</param>
    public record Top100ListItemsInfo(uint ListID, string ListType, string ListName, IEnumerable<nio2soTop100Item> ListItems);
}
