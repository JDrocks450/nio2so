using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Formats.ARCHIVE
{
    public interface IFileEntry
    {
        //Decompressed data size - A 4-byte unsigned integer specifying the uncompressed size of the file.
        public uint DecompressedFileSize { get;  }
        //A 4-byte unsigned integer specifying the compressed size of the file; if this and the previous field are the same, 
        //the file is considered uncompressed. (It is the responsibility of the archiver to only store data compressed when 
        //its size is less than the size of the original data.) Note that The Sims 1 does not actually support any form 
        //of compression.
        public uint CompressedFileSize { get;  }
        //A 4-byte unsigned integer specifying the offset of the file from the beginning of the archive.
        public uint DataOffset { get;  }
        //A 4-byte unsigned integer specifying the length of the filename field that follows.
        public ushort FilenameLength { get;  }
        //Filename - The name of the archived file; size depends on the previous field.
        public string? Filename { get;  }
    }
    public interface IFileArchive<TKey> : IDisposable
    {
        byte[] GetEntry(TKey FileName);
        IEnumerable<IFileEntry> GetAllFileEntries();
        byte[] this[TKey Filename] { get; }
    }
}
