using QuazarAPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class TSOVoltronPDU : Attribute
    {
        public TSOVoltronPDU(TSO_PreAlpha_VoltronPacketTypes Type)
        {
            this.Type = Type;
        }

        public TSO_PreAlpha_VoltronPacketTypes Type { get; }
    }

    internal static class TSOPDUFactory
    {
        private static Dictionary<TSO_PreAlpha_VoltronPacketTypes, Type> typeMap = new();

        static TSOPDUFactory()
        {
            foreach(var type in typeof(TSOPDUFactory).Assembly.GetTypes())
            {
                var attribute = type.GetCustomAttribute<TSOVoltronPDU>();
                if (attribute != null)
                {
                    bool value = typeMap.TryAdd(attribute.Type, type);
                    QConsole.WriteLine("cTSOPDUFactory", $"Mapped {attribute.Type} to {type.Name}");
                }
            }
        }
        /// <summary>
        /// Instantiates the corresponding <see cref="TSOVoltronPacket"/> type with a given <see cref="TSO_PreAlpha_VoltronPacketTypes"/> value
        /// </summary>
        /// <param name="PacketType"></param>
        /// <param name="MakeBlankPacketOnFail"></param>
        /// <returns></returns>
        public static TSOVoltronPacket? CreatePacketObjectByPacketType(TSO_PreAlpha_VoltronPacketTypes PacketType, bool MakeBlankPacketOnFail = true)
        {           
            TSOVoltronPacket? getReturnValue() => (MakeBlankPacketOnFail ? new TSOBlankPDU(PacketType) : default);
            if (typeMap.TryGetValue(PacketType,out var type))            
                return (TSOVoltronPacket)type?.Assembly?.CreateInstance(type.FullName) ?? getReturnValue();            
            switch (PacketType)
            {
                case TSO_PreAlpha_VoltronPacketTypes.HOST_ONLINE_PDU:
                    return new TSOHostOnlinePDU();
                case TSO_PreAlpha_VoltronPacketTypes.CLIENT_ONLINE_PDU:
                    return new TSOClientOnlinePDU();
            }
            return getReturnValue();
        }

        public static void LogDiscoveryPacketToDisk(ushort VoltronPacketType, byte[] PacketData)
        {
            string? displayName = Enum.GetName<TSO_PreAlpha_VoltronPacketTypes>((TSO_PreAlpha_VoltronPacketTypes)VoltronPacketType) ??
                            VoltronPacketType.ToString("X4");
            Directory.CreateDirectory("/packets/discoveries");
            string fileName = $"/packets/discoveries/cTSOPDU [{displayName}].dat";
            if (!File.Exists(fileName))
            {
                File.WriteAllBytes(fileName, PacketData);
                QConsole.WriteLine("TSO PDU Discovery", $"Discovered the {displayName} PDU with: {PacketData.Length} bytes. Add it to constants!");
            }
            else
                QConsole.WriteLine("TSO PDU Discovery", $"Found the {displayName} PDU with: {PacketData.Length} bytes. Make a class for it. ");
        }
    }
}
