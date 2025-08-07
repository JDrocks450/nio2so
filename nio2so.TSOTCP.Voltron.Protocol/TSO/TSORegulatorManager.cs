using nio2so.TSOTCP.Voltron.Protocol.Factory;
using nio2so.TSOTCP.Voltron.Protocol.Telemetry;
using nio2so.TSOTCP.Voltron.Protocol.TSO.PDU;
using System.Reflection;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO
{
    /// <summary>
    /// A response to a <see cref="ITSOProtocolRegulator"/> Handle() method
    /// </summary>
    /// <param name="ResponsePackets"> These are packets that the server should send to the client in response to the incoming data </param>
    /// <param name="InsertionPackets"> These are packets that should be immediately inserted into the Server receive queue for processing </param>
    /// <param name="EnqueuePackets"> These are packets that should be added to the end of the Server's receive queue for processing </param>
    /// <param name="SessionPackets"> These are packets that will be sent to the Quazar Session ID (connection) you select.</param>
    public record TSOProtocolRegulatorResponse(
        IEnumerable<TSOVoltronPacket> ResponsePackets, 
        IEnumerable<TSOVoltronPacket> InsertionPackets, 
        IEnumerable<TSOVoltronPacket> EnqueuePackets,
        IEnumerable<(uint Session, TSOVoltronPacket Packet)> SessionPackets);

    /// <summary>
    /// Inheritors of this class will handle incoming PDUs (and DB Requests).
    /// <para>You should make use of the <see cref="TSORegulator"/> attribute to mark one as a Regulator.</para>
    /// </summary>
    public interface ITSOProtocolRegulator
    {
        ITSOServer Server { get; set; }
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

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class TSORegulator : Attribute
    {
        public string Name { get; set; } = "";
        public TSORegulator() { }
        public TSORegulator(string name) : this()
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
    public class TSORegulatorManager
    {
        private HashSet<ITSOProtocolRegulator> typeMap = new();
        private readonly ITSOServer server;

        /// <summary>
        /// Attaches a <see cref="TSORegulatorManager"/> to the current <see cref="ITSOServer"/> instance provided by <paramref name="Server"/>
        /// <para/>All registered <see cref="ITSOProtocolRegulator"/> types will be added using functionality provided by <see cref="RegisterProtocols(Assembly, Type[])"/>
        /// which itself is usually invoked through <see cref="RegisterDefaultProtocols(Type[])"/> which takes an OmissionList if needed
        /// </summary>
        /// <param name="Server"></param>
        public TSORegulatorManager(ITSOServer Server)
        {
            server = Server;
        }

        /// <summary>
        /// Registers all default protocols implemented in TSOTCP.Voltron (this assembly)
        /// <para/><paramref name="OmissionList"/> will omit any types specified from this mapping process.
        /// </summary>
        public void RegisterDefaultProtocols(params Type[] OmissionList) =>
            RegisterProtocols(typeof(TSOPDUFactory).Assembly, OmissionList);

        /// <summary>
        /// Registers all <see cref="TSORegulator"/> types in the given assembly.
        /// </summary>
        /// <param name="DLL"></param>
        public void RegisterProtocols(Assembly DLL, params Type[] OmissionList)
        {
            foreach (var type in DLL.GetTypes())
            {
                if (OmissionList.Select(x => x.AssemblyQualifiedName).Contains(type.AssemblyQualifiedName))
                    continue; // omitted
                //not omitted ... map this type.
                var attribute = type.GetCustomAttribute<TSORegulator>();
                if (attribute != null)
                { // is this type a TSORegulator?
                    var instance = type.Assembly.CreateInstance(type.FullName);
                    if (instance != null)
                    {
                        bool value = typeMap.Add((ITSOProtocolRegulator)instance);
                        if (value)
                        {
                            TSOServerTelemetryServer.Global.OnConsoleLog(new(TSOServerTelemetryServer.LogSeverity.Message,
                                "cTSORegulatorManager", $"Mapped {type.Name}!"));
                            ((ITSOProtocolRegulator)instance).Server = server;
                        }
                    }
                }
            }
        }

        public bool HandleIncomingPDU(TSOVoltronPacket Incoming, out TSOProtocolRegulatorResponse Outgoing)
        {
            foreach (var regulator in typeMap)
                if (regulator.HandleIncomingPDU(Incoming, out Outgoing)) return true;
            Outgoing = null;
            return false;
        }

        public bool HandleIncomingDBRequest(TSODBRequestWrapper Incoming, out TSOProtocolRegulatorResponse Outgoing)
        {
            foreach (var regulator in typeMap)
                if (regulator.HandleIncomingDBRequest(Incoming, out Outgoing)) return true;
            Outgoing = null;
            return false;
        }

        public T Get<T>()
        {
            foreach (var regulator in typeMap)
                if (regulator.GetType() == typeof(T)) return (T)regulator;
            throw new Exception($"Regulator {typeof(T).Name} is not found.");
        }
    }
}
