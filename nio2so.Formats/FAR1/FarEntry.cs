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

namespace nio2so.Formats.FAR1
{
    /// <summary>
    /// Represents an entry in a FAR1 archive.
    /// </summary>
    public class FarEntry : IFileEntry
    {
        //Decompressed data size - A 4-byte unsigned integer specifying the uncompressed size of the file.
        public int DataLength { get; internal set; }
        //A 4-byte unsigned integer specifying the compressed size of the file; if this and the previous field are the same, 
        //the file is considered uncompressed. (It is the responsibility of the archiver to only store data compressed when 
        //its size is less than the size of the original data.) Note that The Sims 1 does not actually support any form 
        //of compression.
        public int DataLength2 { get; internal set; }
        //A 4-byte unsigned integer specifying the offset of the file from the beginning of the archive.
        public int DataOffset { get; internal set; }
        //A 4-byte unsigned integer specifying the length of the filename field that follows.
        public short FilenameLength { get; internal set; }
        //Filename - The name of the archived file; size depends on the previous field.
        public string? Filename { get; internal set; }

        uint IFileEntry.DecompressedFileSize => (uint)DataLength;
        uint IFileEntry.CompressedFileSize => (uint)DataLength2;
        uint IFileEntry.DataOffset => (uint)DataOffset;
        ushort IFileEntry.FilenameLength => (ushort)FilenameLength;

        public override string ToString() => !string.IsNullOrWhiteSpace(Filename) ? Filename : "no name"; // handle NULL
    }
}
