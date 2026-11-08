using nio2so.Data.Common.Serialization.Voltron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.Core.TSO.Serialization.Types
{
    public static class TSOVoltronStringConverter
    {
        public static string[] ConvertVoltronStr23Format(byte[] Bytes, out int EndIndex)
        {
            byte length = Bytes[0];
            int index = 1;
            List<string> strings = new List<string>();
            while (index < Bytes.Length)
            {
                int nullTerminatorIndex = Array.IndexOf(Bytes, (byte)0x0A, index);
                if (nullTerminatorIndex >= 0)
                {
                    strings.Add(Encoding.UTF8.GetString(Bytes, index, nullTerminatorIndex - index));
                    index = nullTerminatorIndex + 1;
                }
                else
                {
                    strings.Add(Encoding.UTF8.GetString(Bytes, index, Bytes.Length - index));
                    break;
                }
            }
            EndIndex = Bytes.Length;
            return strings.ToArray();
        }

        public static ReadOnlySpan<byte> GetVoltronStr23FormatBytes(string[] stringContent)
        {
            byte[] result = new byte[1 +stringContent.Sum(s => Encoding.UTF8.GetByteCount(s) + 1)];
            result[0] = (byte)(result.Length - 1);
            int index = 1;
            foreach (string s in stringContent)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(s);
                Array.Copy(bytes, 0, result, index, bytes.Length);
                index += bytes.Length;
                result[index] = 0x0A;
                index++;
            }
            return new ReadOnlySpan<byte>(result);
        }
    }
}
