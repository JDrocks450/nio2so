/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

/*
 * THIS FILE CAN BE FOUND AT THE FREESO REPOSITORY AUTHORED BY RHYS
 * https://github.com/riperiperi/FreeSO
 */
using nio2so.Formats.ARCHIVE;

namespace nio2so.Formats.FAR3
{
    /// <summary>
    /// Represents an entry in a FAR3 archive.
    /// </summary>
    public class Far3Entry : IFileEntry
    {
        //A 4-byte unsigned integer specifying the uncompressed size of the file.
        public uint DecompressedFileSize { get; internal set; }
        // A 3-byte unsigned integer specifying the compressed size of the file (including 
        //the Persist header); if the data is raw, this field is ignored (though TSO's game 
        //files have this set to the same first three bytes as the previous field).
        public uint CompressedFileSize { get; internal set; }
        //Data type - A single byte used to describe what type of data is pointed to by the Data offset field. 
        //The value can be 0x80 to denote that the data is a Persist container or 0x00 to denote that it is raw data.
        public byte DataType { get; internal set; }
        //A 4-byte unsigned integer specifying the offset of the file from the beginning of the archive.
        public uint DataOffset { get; internal set; }
        //A byte (can be either 0 or 1) specifying if this file is compressed.
        public byte IsCompressed { get; internal set; }
        //A byte specifying the number of files this time has been accessed?
        public byte AccessNumber { get; internal set; }
        //A 2-byte unsigned integer specifying the length of the filename field.
        public ushort FilenameLength { get; internal set; }
        //A 4-byte integer describing what type of file is held.
        public uint TypeID { get; internal set; }
        //A 4-byte ID assigned to the file which, together with the Type ID, is assumed to be unique all throughout the game.
        public uint FileID { get; internal set; }
        //The name of the archived file; size depends on the filename length field.
        public string? Filename { get; internal set; }

        public override string ToString() => !string.IsNullOrWhiteSpace(Filename) ? Filename : "no name"; // handle NULL
    }
}
