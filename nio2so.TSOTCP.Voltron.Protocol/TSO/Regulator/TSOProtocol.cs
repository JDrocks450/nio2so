using nio2so.DataService.Common;
using nio2so.TSOTCP.Voltron.Protocol.Services;
using nio2so.TSOTCP.Voltron.Protocol.Telemetry;
using nio2so.TSOTCP.Voltron.Protocol.TSO.PDU;
using nio2so.TSOTCP.Voltron.Protocol.TSO.PDU.Datablob.Structures;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Serialization;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Struct;
using System.Reflection;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Regulator
{
    public interface ITSOProtocolHandlerAttribute
    {
        public uint ItemID { get; }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class TSOProtocolHandler : Attribute, ITSOProtocolHandlerAttribute
    {
        public TSOProtocolHandler(TSO_PreAlpha_VoltronPacketTypes packetType)
        {
            PacketType = packetType;
        }

        public TSO_PreAlpha_VoltronPacketTypes PacketType { get; set; }

        uint ITSOProtocolHandlerAttribute.ItemID => (uint)PacketType;
    }
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class TSOProtocolDatabaseHandler : Attribute, ITSOProtocolHandlerAttribute
    {
        public TSOProtocolDatabaseHandler(TSO_PreAlpha_DBActionCLSIDs DatabaseAction)
        {
            ActionType = DatabaseAction;
        }

        public TSO_PreAlpha_DBActionCLSIDs ActionType { get; set; }
        uint ITSOProtocolHandlerAttribute.ItemID => (uint)ActionType;
    }
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class TSOProtocolDatablobHandler : Attribute, ITSOProtocolHandlerAttribute
    {
        public TSOProtocolDatablobHandler(TSO_PreAlpha_MasterConstantsTable CLS_ID)
        {
            ActionType = CLS_ID;
        }

        public TSO_PreAlpha_MasterConstantsTable ActionType { get; set; }
        uint ITSOProtocolHandlerAttribute.ItemID => (uint)ActionType;
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
            void DoMappingFunction<TAttribute, TDelegate>(MethodInfo Target, Func<uint, TDelegate, bool> TryAdd) 
                where TAttribute : Attribute, ITSOProtocolHandlerAttribute
                where TDelegate : Delegate
            {
                string message = "success.";
                try
                {
                    var attribute = Target.GetCustomAttribute<TAttribute>();
                    var packetType = attribute.ItemID;
                    TDelegate action = Target.CreateDelegate<TDelegate>(this);
                    bool result = TryAdd(packetType, action);
                    if (result)
                    {
                        LogConsole($"MAPPED Handler {Target.Name}(PacketType: {packetType:X8})", typeof(TAttribute).Name);
                        return;
                    }
                    else 
                        message = $"{packetType} is already mapped!";
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }
                LogConsole($"FAILED {Target.Name}(): {message}", typeof(TAttribute).Name, TSOServerTelemetryServer.LogSeverity.Errors);
            }

            voltronMap.Clear();
            foreach (var function in GetType().GetMethods().Where(x => x.GetCustomAttribute<TSOProtocolHandler>() != null))
            {
                DoMappingFunction<TSOProtocolHandler,VoltronInvokationDelegate>(function, (VoltronPacketType, Delegate) => 
                    voltronMap.TryAdd((TSO_PreAlpha_VoltronPacketTypes)VoltronPacketType,Delegate));
            }

            databaseMap.Clear();
            foreach (var function in GetType().GetMethods().Where(x => x.GetCustomAttribute<TSOProtocolDatabaseHandler>() != null))
            {
                DoMappingFunction<TSOProtocolDatabaseHandler, VoltronDatabaseInvokationDelegate>(function, (CLSID, Delegate) =>
                    databaseMap.TryAdd((TSO_PreAlpha_DBActionCLSIDs)CLSID, Delegate));
            }

            dataBlobMap.Clear();
            foreach (var function in GetType().GetMethods().Where(x => x.GetCustomAttribute<TSOProtocolDatablobHandler>() != null))
            {
                DoMappingFunction<TSOProtocolDatablobHandler, VoltronDataBlobInvokationDelegate>(function, (ActionType, Delegate) =>
                    dataBlobMap.TryAdd((TSO_PreAlpha_MasterConstantsTable)ActionType, Delegate));
            }
        }

        public virtual bool HandleIncomingDBRequest(TSODBRequestWrapper PDU, out TSOProtocolRegulatorResponse Response)
        {
            if (Server == null) throw new NullReferenceException("No server instance!!!");

            Response = null;
            if (!databaseMap.TryGetValue(PDU.TSOSubMsgCLSID, out var action))
                return false;

            Response = CurrentResponse = new(new List<TSOVoltronPacket>(), new List<TSOVoltronPacket>(), new List<TSOVoltronPacket>(), new List<(uint, TSOVoltronPacket)>());
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

            Response = CurrentResponse = new(new List<TSOVoltronPacket>(), new List<TSOVoltronPacket>(), new List<TSOVoltronPacket>(), new List<(uint, TSOVoltronPacket)>());
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
        protected bool TryGetService<T>(out T Service) where T : ITSOService => Server.Services.TryGet(out Service);
        /// <summary>
        /// Tries to get the requested <typeparamref name="T"/> Service from the <see cref="ITSOServer"/> that this regulator is attached to
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Service"></param>
        /// <returns></returns>
        protected T GetService<T>() where T : ITSOService => Server.Services.Get<T>();
        /// <summary>
        /// Tries to get the requested <typeparamref name="T"/> other <see cref="ITSOProtocolRegulator"/> from the <see cref="ITSOServer"/> that this regulator is attached to
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T GetRegulator<T>() where T : ITSOProtocolRegulator => Server.Regulators.Get<T>();

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
        /// Queues this <paramref name="Packet"/> to be sent to the <paramref name="QuazarID"/> at the end of this Aries frame
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="QuazarID"></param>
        /// <param name="Packet"></param>
        protected void SendTo<T>(uint QuazarID, T Packet) where T : TSOVoltronPacket => ((List<(uint,TSOVoltronPacket)>)CurrentResponse.SessionPackets).Add((QuazarID,Packet));
        /// <summary>
        /// Attempts to locate the <paramref name="VoltronID"/> and if successful, sends this <paramref name="Packet"/> to that connection at the end of this Aries frame.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="VoltronID"></param>
        /// <param name="Packet"></param>
        /// <returns></returns>
        protected bool TrySendTo<T>(TSOAriesIDStruct VoltronID, T Packet) where T : TSOVoltronPacket
        {
            //**find the client session service
            nio2soClientSessionService sessionService = GetService<nio2soClientSessionService>();
            if (!sessionService.TryReverseSearch(VoltronID, out uint Session))
            {
#if false
                throw new Exception($"[DEBUG] {nameof(TrySendTo)} FAILED. Tried to send to {VoltronID} was not connected to Voltron.");
#endif
                return false;
            }
            SendTo(Session, Packet);
            return true;
        }
        /// <summary>
        /// <inheritdoc cref="TrySendTo{T}(TSOAriesIDStruct, T)"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="AvatarID"></param>
        /// <param name="Packet"></param>
        /// <returns></returns>
        protected bool TrySendTo<T>(uint AvatarID, T Packet) where T : TSOVoltronPacket => TrySendTo(new TSOAriesIDStruct(AvatarID,""), Packet);
        /// <summary>
        /// Broadcasts the <typeparamref name="T"/> <paramref name="Packet"/> to <b>all</b> connected clients to this server.
        /// <para/>I shouldn't have to say this, but use this cautiously.
        /// <paramref name="ExcludeQuazarIDs"/>A list of Quazar Connection IDs to NOT send this packet to.
        /// </summary>
        protected void BroadcastToServer<T>(T Packet, params uint[] ExcludeQuazarIDs) where T : TSOVoltronPacket
        {
            //**find the client session service
            nio2soClientSessionService sessionService = GetService<nio2soClientSessionService>();
            foreach (uint QuazarID in sessionService.GetConnectedQuazarIDs())
                if(!ExcludeQuazarIDs.Contains(QuazarID))
                    SendTo(QuazarID, Packet);
        }
        protected nio2soVoltronDataServiceClient GetDataService()
        {
            if (!TryGetService<nio2soVoltronDataServiceClient>(out var dataServiceClient))
                throw new NullReferenceException(nameof(nio2soVoltronDataServiceClient));
            return dataServiceClient;
        }
        protected bool TryDataServiceQuery<T>(Func<Task<HTTPServiceClientBase.HTTPServiceResult<T>>> DataServiceQuery, out T? Result, out string FailureMessage)
        {
            var result = DataServiceQuery().Result;
            FailureMessage = result.FailureReason;
            Result = result.Result;
            return result.IsSuccessful;
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
        /// <summary>
        /// Logs the console from this <see cref="TSORegulator"/> with the message you provide
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="Caption"></param>
        /// <param name="Severity"></param>
        protected void LogConsole(string Message, string Caption = "", TSOServerTelemetryServer.LogSeverity Severity = TSOServerTelemetryServer.LogSeverity.Message) => 
            TSOServerTelemetryServer.LogConsole(new(TSOServerTelemetryServer.LogSeverity.Message, RegulatorName, $"[{Caption ?? ""}] {Message}"));
    }
}
