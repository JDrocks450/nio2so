﻿using nio2so.TSOTCP.City.TSO.Voltron.PDU;
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
        bool HandleIncomingPDU(TSOVoltronPacket PDU, out IEnumerable<TSOVoltronPacket> ResponsePackets);
        /// <summary>
        /// Handles an incoming <see cref="TSODBRequestWrapper"/> PDU.
        /// <para>Returns <see langword="false"/> if this particular <see cref="ITSOProtocolRegulator"/> cannot handle that request.</para>
        /// </summary>
        /// <param name="PDU">The PDU the client sent</param>
        /// <param name="ResponsePackets">Packets sent in response to this PDU.</param>
        /// <returns></returns>
        bool HandleIncomingDBRequest(TSODBRequestWrapper PDU, out IEnumerable<TSOVoltronPacket> ResponsePackets);
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
                            QConsole.WriteLine("cTSORegulatorManager", $"Mapped {type.Name}!");
                    }
                    QConsole.WriteLine("cTSORegulatorManager", $"Error when mapping {type.Name}! (Already added?)");
                }
            }
        }

        public static bool HandleIncomingPDU(TSOVoltronPacket Incoming, out IEnumerable<TSOVoltronPacket> Outgoing)
        {
            foreach(var regulator in  typeMap)            
                if (regulator.HandleIncomingPDU(Incoming, out Outgoing)) return true;
            Outgoing = null;
            return false;
        }
        public static bool HandleIncomingDBRequest(TSODBRequestWrapper Incoming, out IEnumerable<TSOVoltronPacket> Outgoing)
        {            
            foreach (var regulator in typeMap)
                if (regulator.HandleIncomingDBRequest(Incoming, out Outgoing)) return true;
            Outgoing = null;
            return false;
        }
    }
}
