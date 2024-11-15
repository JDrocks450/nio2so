using nio2so.TSOTCP.City.Telemetry;
using nio2so.TSOTCP.City.TSO.Voltron.PDU;
using QuazarAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.Regulator
{
    /// <summary>
    /// A response to a <see cref="ITSOProtocolRegulator"/> Handle() method
    /// </summary>
    /// <param name="ResponsePackets"> These are packets that the server should send to the client in response to the incoming data </param>
    /// <param name="InsertionPackets"> These are packets that should be immediately inserted into the Server receive queue for processing </param>
    /// <param name="EnqueuePackets"> These are packets that should be added to the end of the Server's receive queue for processing </param>
    internal record TSOProtocolRegulatorResponse(IEnumerable<TSOVoltronPacket> ResponsePackets, IEnumerable<TSOVoltronPacket> InsertionPackets, IEnumerable<TSOVoltronPacket> EnqueuePackets);

    /// <summary>
    /// Inheritors of this class will handle incoming PDUs (and DB Requests).
    /// <para>You should make use of the <see cref="TSORegulator"/> attribute to mark one as a Regulator.</para>
    /// </summary>
    internal interface ITSOProtocolRegulator
    {
        string RegulatorName { get; }
        /// <summary>
        /// Handles an incoming PDU.
        /// <para>Returns <see langword="false"/> if this particulator <see cref="ITSOProtocolRegulator"/> cannot handle that request.</para>
        /// </summary>
        /// <param name="PDU">The PDU the client sent</param>
        /// <param name="ResponsePackets">Packets sent in response to this PDU.</param>
        /// <returns></returns>
        bool HandleIncomingPDU(TSOVoltronPacket PDU, out TSOProtocolRegulatorResponse Response);
        /// <summary>
        /// Handles an incoming <see cref="TSODBRequestWrapper"/> PDU.
        /// <para>Returns <see langword="false"/> if this particular <see cref="ITSOProtocolRegulator"/> cannot handle that request.</para>
        /// </summary>
        /// <param name="PDU">The PDU the client sent</param>
        /// <param name="ResponsePackets">Packets sent in response to this PDU.</param>
        /// <returns></returns>
        bool HandleIncomingDBRequest(TSODBRequestWrapper PDU, out TSOProtocolRegulatorResponse Response);
    }

    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    internal sealed class TSORegulator : Attribute
    {
        public string Name { get; set; }
        public TSORegulator(string name)
        {
            Name = name;
        }
    }
    /// <summary>
    /// Handles all regulators defined in this assembly. 
    /// <para>When a <see cref="ITSOProtocolRegulator"/> has the attribute <see cref="TSORegulator"/>
    /// it is added to this object's map of regulators.</para>
    /// <para>Incoming packets will be tested against any of these regulators, and if any of them can handle 
    /// the supplied packet, the response packets will be given back to the caller. </para>
    /// </summary>
    internal static class TSORegulatorManager
    {
        private static HashSet<ITSOProtocolRegulator> typeMap = new();

        static TSORegulatorManager()
        {
            foreach (var type in typeof(TSOPDUFactory).Assembly.GetTypes())
            {
                var attribute = type.GetCustomAttribute<TSORegulator>();
                if (attribute != null)
                {
                    var instance = type.Assembly.CreateInstance(type.FullName);
                    if (instance != null)
                    {
                        bool value = typeMap.Add((ITSOProtocolRegulator)instance);
                        if (value)
                            TSOCityTelemetryServer.Global.OnConsoleLog(new(TSOCityTelemetryServer.LogSeverity.Message, 
                                "cTSORegulatorManager", $"Mapped {type.Name}!"));
                    }
                    //TSOCityTelemetryServer.Global.OnConsoleLog(new(TSOCityTelemetryServer.LogSeverity.Errors, 
                      //  "cTSORegulatorManager", $"Error when mapping {type.Name}! (Already added?)"));
                }
            }
        }

        public static bool HandleIncomingPDU(TSOVoltronPacket Incoming, out TSOProtocolRegulatorResponse Outgoing)
        {
            foreach(var regulator in  typeMap)            
                if (regulator.HandleIncomingPDU(Incoming, out Outgoing)) return true;
            Outgoing = null;
            return false;
        }
        public static bool HandleIncomingDBRequest(TSODBRequestWrapper Incoming, out TSOProtocolRegulatorResponse Outgoing)
        {            
            foreach (var regulator in typeMap)
                if (regulator.HandleIncomingDBRequest(Incoming, out Outgoing)) return true;
            Outgoing = null;
            return false;
        }
    }
}
