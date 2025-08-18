using QuazarAPI.Util.Endian;

namespace nio2so.Voltron.Core.TSO.Aries
{
    /// <summary>
    /// This Aries packet gets information about the client that's calling into the server. 
    /// <para>See: <see href:"https://wiki.niotso.org/Maxis_Protocol"/></para>
    /// </summary>
    /// <param name:"User"></param>
    /// <param name:"AriesClientVersion"></param>
    /// <param name:"Email"></param>
    /// <param name:"AuthServ"></param>
    /// <param name:"Product"></param>
    /// <param name:"Unknown"></param>
    /// <param name:"Password"></param>
    public record TSOAriesClientSessionInfo(string User,
                                              string AriesClientVersion,
                                              string Email,
                                              string AuthServ,
                                              ushort Product,
                                              int Unknown,
                                              string ServiceIdentifier,
                                              ushort ReconnectedPriorFlag,
                                              string Password)
    {
        public static TSOAriesClientSessionInfo FromPacket(TSOTCPPacket basePacket)
        {
            return new TSOAriesClientSessionInfo(
                User: basePacket.ReadBodyNullTerminatedString(0, 112),
                AriesClientVersion: basePacket.ReadBodyNullTerminatedString(80),
                Email: basePacket.ReadBodyNullTerminatedString(40),
                AuthServ: basePacket.ReadBodyNullTerminatedString(84),
                Product: basePacket.ReadBodyUshort(Endianness.LittleEndian),
                Unknown: basePacket.ReadBodyByte(),
                ServiceIdentifier: basePacket.ReadBodyNullTerminatedString(3),
                ReconnectedPriorFlag: basePacket.ReadBodyUshort(Endianness.BigEndian),
                Password: basePacket.ReadBodyNullTerminatedString((int)(basePacket.PayloadSize - 331))
            );
        }
    }
}
