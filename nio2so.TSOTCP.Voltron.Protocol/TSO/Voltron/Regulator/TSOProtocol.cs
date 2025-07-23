using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.Voltron.Protocol.Factory;
using nio2so.TSOTCP.Voltron.Protocol.Telemetry;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.Datablob.Structures;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Serialization;
using System.Reflection;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Regulator
{

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class TSOProtocolHandler : Attribute
    {
        public TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes packetType)
        {
            PacketType = packetType;
        }

        public TSO_PreAlpha_VoltronPacketTypes PacketType { get; set; }
    }
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class TSOProtocolDatabaseHandler : Attribute
    {
        public TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs DatabaseAction)
        {
            ActionType = DatabaseAction;
        }

        public TSO_PreAlpha_DBActionCLSIDs ActionType { get; set; }
    }
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class TSOProtocolDatablobHandler : Attribute
    {
        public TSOProtocolDatablobHandler(TSO_PreAlpha_MasterConstantsTable CLS_ID)
        {
            ActionType = CLS_ID;
        }

        public TSO_PreAlpha_MasterConstantsTable ActionType { get; set; }
    }

    /// <summary>
    /// A base class for <see cref="ITSOProtocolRegulator"/> objects that provides functionality for mapping 
    /// an incoming PDU type to a method that will handle the incoming PDU.
    /// </summary>
    public abstract class TSOProtocol : ITSOProtocolRegulator
    {
        public delegate void VoltronInvokationDelegate(TSOVoltronPacket PDU);
        private Dictionary<TSO_PreAlpha_VoltronPacketTypes, VoltronInvokationDelegate> voltronMap = new();

        public delegate void VoltronDatabaseInvokationDelegate(TSODBRequestWrapper PDU);
        private Dictionary<TSO_PreAlpha_DBActionCLSIDs, VoltronDatabaseInvokationDelegate> databaseMap = new();

        public delegate void VoltronDataBlobInvokationDelegate(ITSODataBlobPDU PDU);
        private Dictionary<TSO_PreAlpha_MasterConstantsTable, VoltronDataBlobInvokationDelegate> dataBlobMap = new();

        protected TSOProtocolRegulatorResponse? CurrentResponse = null;

        private TSOServerServiceManager _serviceManager => Server.Services;

        /// <summary>
        /// The underlying <see cref="ITSOServer"/> instance used for sending/receiving PDUs/other network traffic
        /// </summary>
        public ITSOServer Server { get; set; }

        

        /// <summary>
        /// The name of this regulator
        /// </summary>
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

        /// <summary>
        /// Maps all <see cref="TSOProtocolHandler"/>, <see cref="TSOProtocolDatabaseHandler"/> and <see cref="TSOProtocolDatablobHandler"/> adorned methods as handlers to this regulator
        /// </summary>
        private void MapMe()
        {
            voltronMap.Clear();
            foreach (var function in GetType().GetMethods().Where(x => x.GetCustomAttribute<TSOProtocolHandler>() != null))
            {
                var attribute = function.GetCustomAttribute<TSOProtocolHandler>();
                var packetType = attribute.PacketType;
                VoltronInvokationDelegate action = function.CreateDelegate<VoltronInvokationDelegate>(this);
                bool result = voltronMap.TryAdd(packetType, action);
                if (result) TSOServerTelemetryServer.LogConsole(new(TSOServerTelemetryServer.LogSeverity.Message,
                    nameof(TSOProtocol), $"MAPPED {packetType} to handler {function.Name}"));
            }

            databaseMap.Clear();
            foreach (var function in GetType().GetMethods().Where(x => x.GetCustomAttribute<TSOProtocolDatabaseHandler>() != null))
            {
                var attribute = function.GetCustomAttribute<TSOProtocolDatabaseHandler>();
                var packetType = attribute.ActionType;
                VoltronDatabaseInvokationDelegate action = function.CreateDelegate<VoltronDatabaseInvokationDelegate>(this);
                bool result = databaseMap.TryAdd(packetType, action);
                if (result) TSOServerTelemetryServer.LogConsole(new(TSOServerTelemetryServer.LogSeverity.Message,
                    nameof(TSOProtocol), $"MAPPED {packetType} to DB handler {function.Name}"));
            }

            dataBlobMap.Clear();
            foreach (var function in GetType().GetMethods().Where(x => x.GetCustomAttribute<TSOProtocolDatablobHandler>() != null))
            {
                var attribute = function.GetCustomAttribute<TSOProtocolDatablobHandler>();
                var packetType = attribute.ActionType;
                VoltronDataBlobInvokationDelegate action = function.CreateDelegate<VoltronDataBlobInvokationDelegate>(this);
                bool result = dataBlobMap.TryAdd(packetType, action);
                if (result) TSOServerTelemetryServer.LogConsole(new(TSOServerTelemetryServer.LogSeverity.Message,
                    nameof(TSOProtocol), $"MAPPED {packetType} to DATABLOB handler {function.Name}"));
            }
        }

        public virtual bool HandleIncomingDBRequest(TSODBRequestWrapper PDU, out TSOProtocolRegulatorResponse Response)
        {
            if (Server == null) throw new NullReferenceException("No server instance!!!");

            Response = null;
            if (!databaseMap.TryGetValue((TSO_PreAlpha_DBActionCLSIDs)PDU.TSOSubMsgCLSID, out var action))
                return false;

            Response = CurrentResponse = new(new List<TSOVoltronPacket>(), new List<TSOVoltronPacket>(), new List<TSOVoltronPacket>());
            action.Invoke(PDU);
            return true;
        }

        protected virtual bool OnUnknownDataBlobPDU(ITSODataBlobPDU PDU)
        {
            return false;
        }

        public virtual bool HandleIncomingPDU(TSOVoltronPacket PDU, out TSOProtocolRegulatorResponse Response)
        {
            if (Server == null) throw new NullReferenceException("No server instance!!!");

            Response = CurrentResponse = new(new List<TSOVoltronPacket>(), new List<TSOVoltronPacket>(), new List<TSOVoltronPacket>());
            switch (PDU.KnownPacketType)
            {
                case TSO_PreAlpha_VoltronPacketTypes.TRANSMIT_DATABLOB_PDU:
                case TSO_PreAlpha_VoltronPacketTypes.BROADCAST_DATABLOB_PDU:
                    {
                        var broadcastPDU = (ITSODataBlobPDU)PDU;
                        if (dataBlobMap.TryGetValue(broadcastPDU.SubMsgCLSID, out var action))
                        {
                            action(broadcastPDU);
                            return true;
                        }
                        if (OnUnknownDataBlobPDU(broadcastPDU)) return true;
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

        /// <summary>
        /// Tries to get the requested <typeparamref name="T"/> Service from the <see cref="ITSOServer"/> that this regulator is attached to
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Service"></param>
        /// <returns></returns>
        protected bool TryGetService<T>(out T Service) where T : ITSOService => Server.Services.TryGet<T>(out Service);

        /// <summary>
        /// Sends this packet to the remote connection(s) at the end of this Aries frame.
        /// <para/>Splitting into a <see cref="TSOSplitBufferPDU"/> is handled automatically by the <see cref="TSOVoltronBasicServer"/>
        /// </summary>
        /// <param name="ResponsePacket"></param>
        protected void RespondWith(TSOVoltronPacket ResponsePacket) =>
            ((List<TSOVoltronPacket>)CurrentResponse.ResponsePackets).Add(ResponsePacket);
        /// <summary>
        /// Sends this packet to the remote connection(s) at the end of this Aries frame.
        /// <para/>This function copies the <see cref="ITSOVoltronAriesMasterIDStructure"/> data to the <paramref name="ResponsePacket"/> before then calling <see cref="RespondWith(TSOVoltronPacket)"/>
        /// <para/>Splitting into a <see cref="TSOSplitBufferPDU"/> is handled automatically by the <see cref="TSOVoltronBasicServer"/>
        /// </summary>
        /// <param name="ResponsePacket"></param>
        protected void RespondTo<T>(ITSOVoltronAriesMasterIDStructure DBPacket, T ResponsePacket) where T : TSOVoltronPacket, ITSOVoltronAriesMasterIDStructure
        {
            ResponsePacket.SenderSessionID = new(
                DBPacket.SenderSessionID.AriesID,
                DBPacket.SenderSessionID.MasterID
            );
            ResponsePacket.MakeBodyFromProperties();
            RespondWith(ResponsePacket);
        }
        /// <summary>
        /// Puts this packet to the end of the current <see cref="ITSOServer"/> Receive Queue
        /// <para/>This works by using the <see cref="TSOProtocolRegulatorResponse"/> which is used by internal logic in <see cref="ITSOServer"/> implementations
        /// </summary>
        /// <param name="EnqueuePacket"></param>
        protected void EnqueueOne(TSOVoltronPacket EnqueuePacket) => ((List<TSOVoltronPacket>)CurrentResponse.EnqueuePackets).Add(EnqueuePacket);
        /// <summary>
        /// Puts this packet to the beginning of the current <see cref="ITSOServer"/> Receive Queue. Generally this means it will be processed directly after this <see cref="TSOVoltronPacket"/> is processed
        /// <para/>This is primarily used by internal logic for the <see cref="TSOSplitBufferPDU"/>
        /// <para/>This works by using the <see cref="TSOProtocolRegulatorResponse"/> which is used by internal logic in <see cref="ITSOServer"/> implementations
        /// </summary>
        /// <param name="InsertionPacket"></param>
        protected void InsertOne(TSOVoltronPacket InsertionPacket) => ((List<TSOVoltronPacket>)CurrentResponse.InsertionPackets).Add(InsertionPacket);
    }
}
