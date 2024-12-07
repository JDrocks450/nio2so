using MiscUtil.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QuazarAPI.Networking.Data.ExtendedBufferOperationsExtensions;

namespace nio2so.TSOTCP.City.TSO.Voltron.Util
{
    internal class TSOVoltronBinaryReader
    {
        public static string ReadString(TSOVoltronValueTypes StringType, Stream Stream, int NullTerminatedMaxLength = 255)
        {
            string destValue = "Error.";
            switch (StringType)
            {
                case TSOVoltronValueTypes.Pascal:
                    {
                        ushort strHeader = Stream.ReadBodyUshort(Endianness.LittleEndian);
                        if (strHeader != 0x80)
                            throw new Exception("This is supposed to be a string but I don't think it is one...");
                        ushort len = Stream.ReadBodyUshort(Endianness.BigEndian);
                        byte[] strBytes = Stream.ReadBodyByteArray((int)len);
                        destValue = Encoding.UTF8.GetString(strBytes);
                    }
                    break;
                case TSOVoltronValueTypes.NullTerminated:
                    destValue = Stream.ReadBodyNullTerminatedString(NullTerminatedMaxLength);
                    break;
                case TSOVoltronValueTypes.Length_Prefixed_Byte:
                    {
                        int len = Stream.ReadBodyByte();
                        byte[] strBytes = Stream.ReadBodyByteArray((int)len);
                        destValue = Encoding.UTF8.GetString(strBytes);
                    }
                    break;
            }
            return destValue;
        }
    }
}
