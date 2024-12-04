using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Formats.DB
{
    /// <summary>
    /// Taken from the TSODataDefinition (see: <see cref="TSOData.TSODataFile"/>) this is the structure for a lot
    /// data struct
    /// </summary>
    public class TSOLotDataDefinitionStructure
    {
        /// <summary>
        /// In TSODataDefinition, Location is defined as <see cref="ushort"/> X and <see cref="ushort"/> Y
        /// </summary>
        public struct Location
        {
            public UInt16 Location_X { get; set; }
            public UInt16 Location_Y { get; set; }
        }
        /// <summary>
        /// The location of the lot on the World Map
        /// </summary>
        public Location Lot_Location {  get; set; } // 4
        /// <summary>
        /// The ID of this Lot in the database
        /// </summary>
        public uint Lot_DatabaseID { get; set; } // 8
        /// <summary>
        /// Lot Online Status
        /// </summary>
        public bool Lot_IsOnline {  get; set; } // 9
        /// <summary>
        /// The number of people in the Lot, if any.
        /// </summary>
        public byte Lot_NumOccupants { get; set; } // 13
        /// <summary>
        /// Defined as iunknown, this is likely to be a BMPChunk found in an IFF file.
        /// </summary>
        public byte[] Lot_Thumbnail {  get; set; } // ??
        /// <summary>
        /// The buildable area of the lot, not sure how this figures yet.
        /// </summary>
        public uint Lot_BuildableArea { get; set; } // ??
    }
}
