using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.City.Factory;
using nio2so.TSOTCP.City.Telemetry;
using nio2so.TSOTCP.City.TSO.Voltron.PDU;
using nio2so.TSOTCP.City.TSO.Voltron.PDU.Datablob;
using nio2so.TSOTCP.City.TSO.Voltron.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.Regulator
{

    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class TSOProtocolHandler : Attribute
    {
        public TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes packetType)
        {
            PacketType = packetType;
        }

        public TSO_PreAlpha_VoltronPacketTypes PacketType { get; set; }
    }
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class TSOProtocolDatabaseHandler : Attribute
    {
        public TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs DatabaseAction)
        {
            ActionType = DatabaseAction;
        }

        public TSO_PreAlpha_DBActionCLSIDs ActionType { get; set; }
    }
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class TSOProtocolBroadcastDatablobHandler : Attribute
    {
        public TSOProtocolBroadcastDatablobHandler(TSO_PreAlpha_MasterConstantsTable CLS_ID)
        {
            ActionType = CLS_ID;
        }

        public TSO_PreAlpha_MasterConstantsTable ActionType { get; set; }
    }

    /// <summary>
    /// A base class for <see cref="ITSOProtocolRegulator"/> objects that provides functionality for mapping 
    /// an incoming PDU type to a method that will handle the incoming PDU.
    /// </summary>
    internal abstract class TSOProtocol : ITSOProtocolRegulator
    {
        public delegate void VoltronInvokationDelegate(TSOVoltronPacket PDU);
        private Dictionary<TSO_PreAlpha_VoltronPacketTypes, VoltronInvokationDelegate> voltronMap = new();

        public delegate void VoltronDatabaseInvokationDelegate(TSODBRequestWrapper PDU);
        private Dictionary<TSO_PreAlpha_DBActionCLSIDs, VoltronDatabaseInvokationDelegate> databaseMap = new();

        public delegate void VoltronBroadcastInvokationDelegate(TSOBroadcastDatablobPacket PDU);
        private Dictionary<TSO_PreAlpha_MasterConstantsTable, VoltronBroadcastInvokationDelegate> broadcastMap = new();

        protected TSOProtocolRegulatorResponse? CurrentResponse = null;
        private TSOCityServer? _server;

        public TSOCityServer Server { set => _server = value; }

        public virtual string RegulatorName
        {
            get
            {
                var reg = GetType().GetCustomAttribute<TSORegulator>();
                if (string.IsNullOrWhiteSpace(reg?.Name))
                    return GetType().Name;
                return reg.Name;
            }
        }

        protected TSOProtocol()
        {
            MapMe();
        }

        private void MapMe()
        {
            voltronMap.Clear();
            foreach(var function in GetType().GetMethods().Where(x => x.GetCustomAttribute<TSOProtocolHandler>() != null))
            {
                var attribute = function.GetCustomAttribute<TSOProtocolHandler>();
                var packetType = attribute.PacketType;
                VoltronInvokationDelegate action = function.CreateDelegate<VoltronInvokationDelegate>(this);
                bool result = voltronMap.TryAdd(packetType, action);
                if (result) TSOCityTelemetryServer.LogConsole(new(TSOCityTelemetryServer.LogSeverity.Message,
                    nameof(TSOProtocol), $"MAPPED {packetType} to handler {function.Name}"));
            }

            databaseMap.Clear();
            foreach (var function in GetType().GetMethods().Where(x => x.GetCustomAttribute<TSOProtocolDatabaseHandler>() != null))
            {
                var attribute = function.GetCustomAttribute<TSOProtocolDatabaseHandler>();
                var packetType = attribute.ActionType;
                VoltronDatabaseInvokationDelegate action = function.CreateDelegate<VoltronDatabaseInvokationDelegate>(this);
                bool result = databaseMap.TryAdd(packetType, action);
                if (result) TSOCityTelemetryServer.LogConsole(new(TSOCityTelemetryServer.LogSeverity.Message,
                    nameof(TSOProtocol), $"MAPPED {packetType} to DB handler {function.Name}"));
            }

            broadcastMap.Clear();
            foreach (var function in GetType().GetMethods().Where(x => x.GetCustomAttribute<TSOProtocolBroadcastDatablobHandler>() != null))
            {
                var attribute = function.GetCustomAttribute<TSOProtocolBroadcastDatablobHandler>();
                var packetType = attribute.ActionType;
                VoltronBroadcastInvokationDelegate action = function.CreateDelegate<VoltronBroadcastInvokationDelegate>(this);
                bool result = broadcastMap.TryAdd(packetType, action);
                if (result) TSOCityTelemetryServer.LogConsole(new(TSOCityTelemetryServer.LogSeverity.Message,
                    nameof(TSOProtocol), $"MAPPED {packetType} to BROADCAST handler {function.Name}"));
            }
        }        

        public virtual bool HandleIncomingDBRequest(TSODBRequestWrapper PDU, out TSOProtocolRegulatorResponse Response)
        {
            if (_server == null) throw new NullReferenceException("No server instance!!!");

            Response = null;
            if (!databaseMap.TryGetValue((TSO_PreAlpha_DBActionCLSIDs)PDU.TSOSubMsgCLSID, out var action))
                return false;

            Response = CurrentResponse = new(new List<TSOVoltronPacket>(), new List<TSOVoltronPacket>(), new List<TSOVoltronPacket>());
            action.Invoke(PDU);
            return true;
        }

        protected virtual bool OnUnknownBroadcastPDU(TSOBroadcastDatablobPacket PDU)
        {
            return false;
        }

        public virtual bool HandleIncomingPDU(TSOVoltronPacket PDU, out TSOProtocolRegulatorResponse Response)
        {
            if (_server == null) throw new NullReferenceException("No server instance!!!");

            Response = CurrentResponse = new(new List<TSOVoltronPacket>(), new List<TSOVoltronPacket>(), new List<TSOVoltronPacket>());
            switch (PDU.KnownPacketType)
            {
                case TSO_PreAlpha_VoltronPacketTypes.BROADCAST_DATABLOB_PDU:
                    {
                        var broadcastPDU = (TSOBroadcastDatablobPacket)PDU;
                        if (broadcastMap.TryGetValue(broadcastPDU.SubMsgCLSID, out var action))
                        {
                            action(broadcastPDU);
                            return true;
                        }
                        if (OnUnknownBroadcastPDU(broadcastPDU)) return true;
                    }
                    break;
                default:
                    {
                        if (voltronMap.TryGetValue(PDU.KnownPacketType, out var action))
                        {
                            action(PDU);
                            return true;
                        }
                    }
                    break;
            }
            Response = null;
            return false;
        }

        protected IEnumerable<TSOVoltronPacket> SplitLargePDU(TSOVoltronPacket DBWrapper)
        {
            List<TSOVoltronPacket> packets = new();
            uint splitSize = TSOSplitBufferPDU.STANDARD_CHUNK_SIZE;
            if (DBWrapper.BodyLength > splitSize && TestingConstraints.SplitBuffersPDUEnabled)
                packets.AddRange(TSOPDUFactory.CreateSplitBufferPacketsFromPDU(DBWrapper));
            else 
                packets.Add(DBWrapper);
            return packets;
        }

        /// <summary>
        /// Will send this packet to the remote connection at the end of this Aries frame
        /// </summary>
        /// <param name="ResponsePacket"></param>
        protected void RespondWith(TSOVoltronPacket ResponsePacket) => 
            ((List<TSOVoltronPacket>)CurrentResponse.ResponsePackets).AddRange(SplitLargePDU(ResponsePacket));
        /// <summary>
        /// Will send this packet to the remote connection at the end of this Aries frame
        /// <para/>This will take the AriesID, MasterID fields from the packet given to the response
        /// </summary>
        /// <param name="ResponsePacket"></param>
        protected void RespondTo<T>(ITSOVoltronAriesMasterIDStructure DBPacket, T ResponsePacket) where T : TSOVoltronPacket, ITSOVoltronAriesMasterIDStructure
        {
            ResponsePacket.AriesID = DBPacket.AriesID;
            ResponsePacket.MasterID = DBPacket.MasterID;
            ResponsePacket.MakeBodyFromProperties();
            RespondWith(ResponsePacket);
        }
        /// <summary>
        /// Puts this packet to the end of the current <see cref="TSOCityServer"/> ReceiveQueue
        /// </summary>
        /// <param name="EnqueuePacket"></param>
        protected void EnqueueOne(TSOVoltronPacket EnqueuePacket) => ((List<TSOVoltronPacket>)CurrentResponse.EnqueuePackets).Add(EnqueuePacket);
        /// <summary>
        /// Puts this packet to the beginning of the current <see cref="TSOCityServer"/> ReceiveQueue
        /// </summary>
        /// <param name="EnqueuePacket"></param>
        protected void InsertOne(TSOVoltronPacket InsertionPacket) => ((List<TSOVoltronPacket>)CurrentResponse.InsertionPackets).Add(InsertionPacket);
    }
}
