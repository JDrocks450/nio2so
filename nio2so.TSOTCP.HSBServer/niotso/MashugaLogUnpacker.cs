using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.HSBServer.niotso
{
    internal class MashugaLogUnpacker
    {
        public record MashugaLogFrame
        {
            public string Sender => Caption.Substring(Caption.LastIndexOf(' ')).Replace(":", "");
            public int DataLength => int.Parse((Caption.Substring(9)).Substring(0, Caption.Substring(9).IndexOf(' ')));
            public string Caption { get; set; }
            public byte[] DumpedData { get; set; }
            public long FileOffset { get; set; }
        }

        public List<MashugaLogFrame> Frames { get; } = new();

        public MashugaLogUnpacker(string PathToLog)
        {
            this.LogPath = PathToLog;
            Unpack();
        }

        public string LogPath { get; }

        private void Unpack()
        {
            using(FileStream fs = File.OpenRead(LogPath))
            {
                while (fs.Position < fs.Length)
                {
                    long offset = fs.Position;
                    byte b = (byte)fs.ReadByte();
                    if (b != 0x0A) throw new Exception("Invalid Data!");
                    byte[] readFrameChunk(int DataLength = -1)
                    {
                        using (MemoryStream commentStream = new MemoryStream())
                        {
                            if (DataLength > 0)
                            {
                                byte[] buffer = new byte[DataLength];
                                fs.ReadExactly(buffer,0,DataLength);
                                commentStream.Write(buffer, 0, DataLength);
                            }
                            else
                            {
                                do
                                {
                                    b = (byte)fs.ReadByte();
                                    if (b != 0x0A)
                                        commentStream.Write([b]);
                                    else break;
                                }
                                while (true);
                            }
                            return commentStream.ToArray();
                        }
                    }
                    MashugaLogFrame frame = new MashugaLogFrame();
                    frame.Caption = Encoding.UTF8.GetString(readFrameChunk());
                    frame.DumpedData = readFrameChunk(frame.DataLength);
                    frame.FileOffset = offset;
                    Frames.Add(frame);
                }
            }
        }
    }
}
