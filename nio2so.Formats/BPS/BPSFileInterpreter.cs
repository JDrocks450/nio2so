using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Formats.BPS
{
    public enum BPSActions
    {
        SourceRead,
        TargetRead,
        SourceCopy,
        TargetCopy
    }

    public class BPSAction
    {
        private static Dictionary<BPSActions, string> friendlyNames = new()
        {
            { BPSActions.SourceRead, "Copy 0x{0:X16}h bytes from source file to target." },
            { BPSActions.TargetRead, "Copy 0x{0:X16}h bytes from the patch file to target." },
            { BPSActions.TargetCopy, "TGTCOPY" },{ BPSActions.SourceCopy, "SRCCOPY" },
        };

        public BPSAction(ulong data, BPSActions command, ulong length)
        {
            Data = data;
            Command = command;
            Length = length;
        }
        public override string ToString()
        {
            return string.Format(friendlyNames[Command], Length);
        }
        public ulong Data { get; internal set; }
        public BPSActions Command { get; internal set; }
        public ulong Length { get; internal set; }
    }

    public class BPSFile
    {
        public string Header { get; internal set; }
        public ulong SourceSize { get; internal set; }
        public ulong TargetSize { get; internal set; }
        public ulong MetadataSize { get; internal set; }
        public string Metadata { get; internal set; }
        public List<BPSAction> Actions { get; } = new();
        public uint SourceChecksum { get; internal set; }
        public uint TargetChecksum { get; internal set; }
        public uint PatchChecksum { get; internal set; }
    }

    public class BPSFileInterpreter
    {        
        public static BPSFile Interpret(string BPSFile)
        {
            Stream fs = default;

            string readStr(int length)
            {
                byte[] sourceBuff = new byte[length];
                fs.Read(sourceBuff, 0, length);
                return Encoding.UTF8.GetString(sourceBuff);
            }
            uint readUint32()
            {
                byte[] sourceBuff = new byte[sizeof(uint)];
                fs.Read(sourceBuff, 0, sourceBuff.Length);
                return BitConverter.ToUInt32(sourceBuff);
            }
            ulong decode()
            {
                ulong data = 0, shift = 1;
                while (true)
                {
                    byte x = (byte)fs.ReadByte();
                    data += (ulong)(x & 0x7f) * shift;
                    if ((x & 0x80) != 0) break;
                    shift <<= 7;
                    data += shift;
                }
                return data;
            }
            BPSFile bps = new();
            using (var stream = File.OpenRead(BPSFile))
            {
                fs = stream;

                bps.Header = readStr(4);
                bps.SourceSize = decode();
                bps.TargetSize = decode();
                bps.MetadataSize = decode();
                bps.Metadata = readStr((int)bps.MetadataSize);
                while (stream.Position < (stream.Length-12))
                {
                    ulong data = decode();
                    ulong command = data & 3;
                    UInt64 length = (data >> 2) + 1;
                    BPSAction action = new(data, (BPSActions)command, length);
                    bps.Actions.Add(action);
                }
                bps.SourceChecksum = readUint32();
                bps.SourceChecksum = readUint32();
                bps.SourceChecksum = readUint32();
            }
            return bps;
        }
    }
}
