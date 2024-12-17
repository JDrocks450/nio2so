/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Linq;
using nio2so.Formats.Util.Endian;
/*
 * THIS FILE CAN BE FOUND AT THE FREESO REPOSITORY AUTHORED BY RHYS
 * https://github.com/riperiperi/FreeSO
 */
namespace nio2so.Formats.FAR3
{
    /// <summary>
    /// Represents a decompresser that can decompress files in a FAR3
    /// archive. If you have some kind of need to understand this code, go to:
    /// http://wiki.niotso.org/RefPack
    /// The code in this class was ported from DBPF4J:
    /// http://sc4dbpf4j.cvs.sourceforge.net/viewvc/sc4dbpf4j/DBPF4J/
    /// </summary>
    public class Decompresser
    {
        private long m_CompressedSize = 0;
        private long m_DecompressedSize = 0;

        public long DecompressedSize
        {
            get { return m_DecompressedSize; }
            set { m_DecompressedSize = value; }
        }

        public long CompressedSize
        {
            get { return m_CompressedSize; }
            set { m_CompressedSize = value; }
        }

        /// <summary>
        ///  Copies data from source to destination array.<br>
        ///  The copy is byte by byte from srcPos to destPos and given length.
        /// </summary>
        /// <param name="Src">The source array.</param>
        /// <param name="SrcPos">The source Position.</param>
        /// <param name="Dest">The destination array.</param>
        /// <param name="DestPos">The destination Position.</param>
        /// <param name="Length">The length.</param>
        private void ArrayCopy2(byte[] Src, int SrcPos, ref byte[] Dest, int DestPos, long Length)
        {
            if (Dest.Length < DestPos + Length)
            {
                byte[] DestExt = new byte[(int)(DestPos + Length)];
                Array.Copy(Dest, 0, DestExt, 0, Dest.Length);
                Dest = DestExt;
            }

            for (int i = 0; i < Length/* - 1*/; i++)
                Dest[DestPos + i] = Src[SrcPos + i];
        }

        /// <summary>
        /// Copies data from array at destPos-srcPos to array at destPos.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="srcPos">The Position to copy from (reverse from end of array!)</param>
        /// <param name="destPos">The Position to copy to.</param>
        /// <param name="length">The length of data to copy.</param>
        private void OffsetCopy(ref byte[] array, int srcPos, int destPos, long length)
        {
            srcPos = destPos - srcPos;

            if (array.Length < destPos + length)
            {
                byte[] NewArray = new byte[(int)(destPos + length)];
                Array.Copy(array, 0, NewArray, 0, array.Length);
                array = NewArray;
            }

            for (int i = 0; i < length /*- 1*/; i++)
            {
                array[destPos + i] = array[srcPos + i];
            }
        }

        /// <summary>
        /// Compresses data and returns it as an array of bytes.
        /// Assumes that the array of bytes passed contains 
        /// uncompressed data.
        /// </summary>
        /// <param name="Data">The data to be compressed.</param>
        /// <returns>An array of bytes with compressed data.</returns>
        public byte[] Compress(byte[] Data, bool WriteCompressedSizeBeforeMagicNumber = true)
        {
            // if data is big enough for compress
            if (Data.Length > 6)
            {
                // some Compression Data
                const int MAX_OFFSET = 0x20000;
                const int MAX_COPY_COUNT = 0x404;
                // used to finetune the lookup (small values increase the
                // compression for Big Files)
                const int QFS_MAXITER = 0x80;

                // contains the latest offset for a combination of two
                // characters
                Dictionary<int, ArrayList> cmpmap2 = new Dictionary<int, ArrayList>();

                // will contain the compressed data (maximal size =
                // uncompressedSize+MAX_COPY_COUNT)
                byte[] cData = new byte[Data.Length + MAX_COPY_COUNT];

                // init some vars
                int writeIndex = 9; // leave 9 bytes for the header
                int lastReadIndex = 0;
                ArrayList? indexList = null;
                int copyOffset = 0;
                int copyCount = 0;
                int index = -1;
                bool end = false;

                // begin main compression loop
                while (index < Data.Length - 3)
                {
                    // get all Compression Candidates (list of offsets for all
                    // occurances of the current 3 bytes)
                    do
                    {
                        index++;
                        if (index >= Data.Length - 2)
                        {
                            end = true;
                            break;
                        }
                        int mapindex = Data[index] + (Data[index + 1] << 8)
                                + (Data[index + 2] << 16);

                        _ = cmpmap2.TryGetValue(mapindex, out indexList);
                        if (indexList == null)
                        {
                            indexList = new ArrayList();
                            cmpmap2.Add(mapindex, indexList);
                        }
                        indexList.Add(index);
                    } while (index < lastReadIndex);
                    if (end)
                        break;

                    // find the longest repeating byte sequence in the index
                    // List (for offset copy)
                    int offsetCopyCount = 0;
                    int loopcount = 1;
                    while (loopcount < indexList.Count && loopcount < QFS_MAXITER)
                    {
                        int foundindex = (int)indexList[indexList.Count - 1 - loopcount];
                        if (index - foundindex >= MAX_OFFSET)
                        {
                            break;
                        }

                        loopcount++;
                        copyCount = 3;

                        while (Data.Length > index + copyCount && Data[index + copyCount] == Data[foundindex + copyCount] && copyCount < MAX_COPY_COUNT)
                        {
                            copyCount++;
                        }

                        if (copyCount > offsetCopyCount)
                        {
                            offsetCopyCount = copyCount;
                            copyOffset = index - foundindex;
                        }
                    }

                    // check if we can compress this
                    // In FSH Tool stand additionally this:
                    if (offsetCopyCount > Data.Length - index)
                    {
                        offsetCopyCount = index - Data.Length;
                    }
                    if (offsetCopyCount <= 2)
                    {
                        offsetCopyCount = 0;
                    }
                    else if (offsetCopyCount == 3 && copyOffset > 0x400)
                    { // 1024
                        offsetCopyCount = 0;
                    }
                    else if (offsetCopyCount == 4 && copyOffset > 0x4000)
                    { // 16384
                        offsetCopyCount = 0;
                    }

                    // this is offset-compressable? so do the compression
                    if (offsetCopyCount > 0)
                    {
                        // plaincopy

                        // In FSH Tool stand this (A):
                        while (index - lastReadIndex >= 4)
                        {
                            copyCount = (index - lastReadIndex) / 4 - 1;
                            if (copyCount > 0x1B)
                            {
                                copyCount = 0x1B;
                            }
                            cData[writeIndex++] = (byte)(0xE0 + copyCount);
                            copyCount = 4 * copyCount + 4;

                            ArrayCopy2(Data, lastReadIndex, ref cData, writeIndex, copyCount);
                            lastReadIndex += copyCount;
                            writeIndex += copyCount;
                        }

                        // offsetcopy
                        copyCount = index - lastReadIndex;
                        copyOffset--;
                        if (offsetCopyCount <= 0x0A && copyOffset < 0x400)
                        {
                            cData[writeIndex++] = (byte)((copyOffset >> 8 << 5)
                                    + (offsetCopyCount - 3 << 2) + copyCount);
                            cData[writeIndex++] = (byte)(copyOffset & 0xff);
                        }
                        else if (offsetCopyCount <= 0x43 && copyOffset < 0x4000)
                        {
                            cData[writeIndex++] = (byte)(0x80 + (offsetCopyCount - 4));
                            cData[writeIndex++] = (byte)((copyCount << 6) + (copyOffset >> 8));
                            cData[writeIndex++] = (byte)(copyOffset & 0xff);
                        }
                        else if (offsetCopyCount <= MAX_COPY_COUNT && copyOffset < MAX_OFFSET)
                        {
                            cData[writeIndex++] = (byte)(0xc0
                                    + (copyOffset >> 16 << 4)
                                    + (offsetCopyCount - 5 >> 8 << 2) + copyCount);
                            cData[writeIndex++] = (byte)(copyOffset >> 8 & 0xff);
                            cData[writeIndex++] = (byte)(copyOffset & 0xff);
                            cData[writeIndex++] = (byte)(offsetCopyCount - 5 & 0xff);
                        }

                        // do the offset copy
                        ArrayCopy2(Data, lastReadIndex, ref cData, writeIndex, copyCount);
                        writeIndex += copyCount;
                        lastReadIndex += copyCount;
                        lastReadIndex += offsetCopyCount;
                    }
                }

                // add the End Record
                index = Data.Length;
                // in FSH Tool stand the same as above (A)
                while (index - lastReadIndex >= 4)
                {
                    copyCount = (index - lastReadIndex) / 4 - 1;

                    if (copyCount > 0x1B)
                        copyCount = 0x1B;

                    cData[writeIndex++] = (byte)(0xE0 + copyCount);
                    copyCount = 4 * copyCount + 4;

                    ArrayCopy2(Data, lastReadIndex, ref cData, writeIndex, copyCount);
                    lastReadIndex += copyCount;
                    writeIndex += copyCount;
                }

                copyCount = index - lastReadIndex;
                cData[writeIndex++] = (byte)(0xfc + copyCount);
                ArrayCopy2(Data, lastReadIndex, ref cData, writeIndex, copyCount);
                writeIndex += copyCount;
                lastReadIndex += copyCount;

                MemoryStream DataStream = new MemoryStream();
                BinaryWriter Writer = new BinaryWriter(DataStream);

                int writeStart = 0;
                for (int i = 0; i < cData.Length; i++)
                {
                    writeStart = i;
                    if (cData[i] != 0) break;                    
                }
                writeIndex -= writeStart;
                cData = cData.Skip(writeStart).Take(writeIndex).ToArray();

                // write the header for the compressed data
                // set the compressed size
                if (WriteCompressedSizeBeforeMagicNumber)
                    Writer.Write(EndianBitConverter.Big.GetBytes((uint)writeIndex + 9));
                m_CompressedSize = writeIndex;
                // set the MAGICNUMBER
                Writer.Write((ushort)0xFB10);
                // set the decompressed size

                //little endian
                //byte[] revData = BitConverter.GetBytes(Data.Length);
                //Big endian
                byte[] revData = EndianBitConverter.Big.GetBytes(Data.Length);
                Writer.Write(revData,1,3);                

                Writer.Write(cData);

                //Avoid nasty swearing here!
                Writer.Flush();

                m_DecompressedSize = Data.Length;

                return DataStream.ToArray();
            }

            return Data;
        }

        /// <summary>
        /// This will assess the incoming data stream to see if it is in a valid format to be decompressed.
        /// <para/>Valid format means: starting with 0x10FB and having the <see cref="m_DecompressedSize"/> field
        /// set before decompression can begin.
        /// <para/>Warning: this will ensure that a magic number is present at the beginning of the datastream. If it is not,
        /// it will advance the <paramref name="Reader"/> until the magic number is found or the end of file is reached.
        /// </summary>
        private void PreprocessDecompress(BinaryReader Reader)
        {
            uint read_u24(in BinaryReader _reader)
            {
                byte[] u24bytes = new byte[4];
                _reader.Read(u24bytes, 1, 3);
                return EndianBitConverter.Big.ToUInt32(u24bytes, 0);
            }

            Reader.BaseStream.Seek(0, SeekOrigin.Begin);
            while (Reader.ReadByte() != 0xFB) ;
            if (Reader.BaseStream.Position < 2) throw new InvalidDataException("Header must start with <byte> 0xFB.");
            if (Reader.BaseStream.Position == Reader.BaseStream.Length)
                throw new InvalidDataException("Read through entire stream, cannot find the RefPack header anywhere.");
            Reader.BaseStream.Seek(-2, SeekOrigin.Current);

            ushort signature;
            uint compressed_size, decompressed_size;
            signature = Reader.ReadUInt16();
            compressed_size = ((signature & 0x0100) == 0) ? read_u24(Reader) : 0;
            //compressed size is not used.

            decompressed_size = read_u24(Reader);
            if (m_DecompressedSize == 0)
                m_DecompressedSize = decompressed_size;            
        }

        /// <summary>
        /// Decompresses data and returns it as an
        /// uncompressed array of bytes.
        /// </summary>
        /// <param name="Data">The data to decompress.</param>
        /// <returns>An uncompressed array of bytes.</returns>
        public byte[] Decompress(byte[] Data)
        {
            //** MODIFIED BY BISQUICK
            // Add signature processing: Read 0x10 0xFB **u24 m_DecompressedSize** before reading RefPack bitstream            
            
            byte byte_0, byte_1, byte_2, byte_3;
            uint proc_len, ref_dis, ref_len;

            MemoryStream MemData = new MemoryStream(Data);
            BinaryReader Reader = new BinaryReader(MemData);

            PreprocessDecompress(Reader);
            if(m_DecompressedSize == 0)
                throw new InvalidDataException("Decompressed Size is zero. This file may be corrupt.");

            byte[] ndata = new byte[Data.Length - MemData.Position];
            //**The preprocess decompress function will ensure MemData is at the correct position.
            MemData.Read(ndata,0,ndata.Length);
            MemData.Dispose();
            Reader.Dispose();
            Data = ndata;

            MemData = new MemoryStream(Data);
            Reader = new BinaryReader(MemData);

            if (Data.Length > 6)
            {
                byte[] DecompressedData = new byte[(int)m_DecompressedSize];
                int DataPos = 0;

                int Pos = 0;
                long Control1 = 0;

                while (Control1 != 0xFC && Pos < Data.Length)
                {
                    Control1 = Data[Pos];
                    Pos++;

                    if (Pos == Data.Length)
                        break;

                    if (Control1 >= 0 && Control1 <= 127)
                    {
                        // 0x00 - 0x7F
                        long control2 = Data[Pos];
                        Pos++;
                        long numberOfPlainText = Control1 & 0x03;
                        ArrayCopy2(Data, Pos, ref DecompressedData, DataPos, numberOfPlainText);
                        DataPos += (int)numberOfPlainText;
                        Pos += (int)numberOfPlainText;

                        if (DataPos == DecompressedData.Length)
                            break;

                        int offset = (int)(((Control1 & 0x60) << 3) + control2 + 1);
                        long numberToCopyFromOffset = ((Control1 & 0x1C) >> 2) + 3;
                        OffsetCopy(ref DecompressedData, offset, DataPos, numberToCopyFromOffset);
                        DataPos += (int)numberToCopyFromOffset;

                        if (DataPos == DecompressedData.Length)
                            break;
                    }
                    else if (Control1 >= 128 && Control1 <= 191)
                    {
                        // 0x80 - 0xBF
                        long control2 = Data[Pos];
                        Pos++;
                        long control3 = Data[Pos];
                        Pos++;

                        long numberOfPlainText = control2 >> 6 & 0x03;
                        ArrayCopy2(Data, Pos, ref DecompressedData, DataPos, numberOfPlainText);
                        DataPos += (int)numberOfPlainText;
                        Pos += (int)numberOfPlainText;

                        if (DataPos == DecompressedData.Length)
                            break;

                        int offset = (int)(((control2 & 0x3F) << 8) + control3 + 1);
                        long numberToCopyFromOffset = (Control1 & 0x3F) + 4;
                        OffsetCopy(ref DecompressedData, offset, DataPos, numberToCopyFromOffset);
                        DataPos += (int)numberToCopyFromOffset;

                        if (DataPos == DecompressedData.Length)
                            break;
                    }
                    else if (Control1 >= 192 && Control1 <= 223)
                    {
                        // 0xC0 - 0xDF
                        long numberOfPlainText = Control1 & 0x03;
                        long control2 = Data[Pos];
                        Pos++;
                        long control3 = Data[Pos];
                        Pos++;
                        long control4 = Data[Pos];
                        Pos++;
                        ArrayCopy2(Data, Pos, ref DecompressedData, DataPos, numberOfPlainText);
                        DataPos += (int)numberOfPlainText;
                        Pos += (int)numberOfPlainText;

                        if (DataPos == DecompressedData.Length)
                            break;

                        int offset = (int)(((Control1 & 0x10) << 12) + (control2 << 8) + control3 + 1);
                        long numberToCopyFromOffset = ((Control1 & 0x0C) << 6) + control4 + 5;
                        OffsetCopy(ref DecompressedData, offset, DataPos, numberToCopyFromOffset);
                        DataPos += (int)numberToCopyFromOffset;

                        if (DataPos == DecompressedData.Length)
                            break;
                    }
                    else if (Control1 >= 224 && Control1 <= 251)
                    {
                        // 0xE0 - 0xFB
                        long numberOfPlainText = ((Control1 & 0x1F) << 2) + 4;
                        ArrayCopy2(Data, Pos, ref DecompressedData, DataPos, numberOfPlainText);
                        DataPos += (int)numberOfPlainText;
                        Pos += (int)numberOfPlainText;

                        if (DataPos == DecompressedData.Length)
                            break;
                    }
                    else
                    {
                        long numberOfPlainText = Control1 & 0x03;
                        ArrayCopy2(Data, Pos, ref DecompressedData, DataPos, numberOfPlainText);

                        DataPos += (int)numberOfPlainText;
                        Pos += (int)numberOfPlainText;

                        if (DataPos == DecompressedData.Length)
                            break;
                    }
                }

                return DecompressedData;
            }

            //No data to decompress
            return Data;
        }
    }
}