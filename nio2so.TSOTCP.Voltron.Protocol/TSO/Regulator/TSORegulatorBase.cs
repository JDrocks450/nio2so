using nio2so.Data.Common;
using nio2so.DataService.Common;
using nio2so.Voltron.Core.Services;
using nio2so.Voltron.Core.Telemetry;
using nio2so.Voltron.Core.TSO.PDU;
using nio2so.Voltron.Core.TSO.Struct;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;

namespace nio2so.Voltron.Core.TSO.Regulator
{
    public interface ITSOProtocolHandlerAttribute
    {
        public uint ItemID { get; }
    }
    /// <summary>
    /// Marks this method as a handler for a specific <see cref="TSOVoltronPacket"/> type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class TSOProtocolHandler : Attribute, ITSOProtocolHandlerAttribute
    {
        public TSOProtocolHandler(uint packetType)
        {
            PacketType = packetType;
        }

        public UIntEnum PacketType { get; set; }

        uint ITSOProtocolHandlerAttribute.ItemID => (uint)PacketType;
    }
    
    /// <summary>
    /// Defines a matching parameters for a <see cref="ITSOProtocolRegulator"/> to use when mapping its methods to the PDU types it handles.
    /// </summary>
    public class TSOProtocolMatchingOption
    {
        /// <summary>
        /// The type of the <see cref="Enum"/> that will be used to map the PDU type to the handler function.
        /// </summary>
        public Type EnumType { get; private set; }
        /// <summary>
        /// The <see cref="Type"/> of the PDU that this regulator will handle.
        /// <para/>This is used to map the PDU type to the handler function.
        /// <para/>This should be a type that implements <see cref="TSOVoltronPacket"/> or a derived type.
        /// </summary>
        public Type PDUType { get; private set; }
        /// <summary>
        /// The <see cref="Type"/> of the attribute that will be used to mark the handler function.
        /// <para/>Examples include <see cref="TSOProtocolHandler"/>, <see cref="TSOProtocolDatabaseHandler"/> and <see cref="TSOProtocolDatablobHandler"/>.
        /// </summary>
        public Type AttributeType { get; private set; }
        public Type HandlerFunctionSignature { get; }

        public TSOProtocolMatchingOption(Type enumType, Type pDUType, Type attributeType, Type handlerFunctionSig)
        {
            EnumType = enumType;
            PDUType = pDUType;
            AttributeType = attributeType;
            HandlerFunctionSignature = handlerFunctionSig;
        }

        public static TSOProtocolMatchingOption Create<TEnum, TPDUType, TAttribute, TDelegate>()
            where TEnum : Enum            
            where TAttribute : Attribute, ITSOProtocolHandlerAttribute
            where TDelegate : Delegate
        {
            return new TSOProtocolMatchingOption(
                typeof(TEnum),
                typeof(TPDUType),
                typeof(TAttribute),
                typeof(TDelegate)
            );
        }
        public static TSOProtocolMatchingOption CreateVoltron<TEnum>()
            where TEnum : Enum => Create<TEnum, TSOVoltronPacket, TSOProtocolHandler, TSOProtocolBase.VoltronInvokationDelegate>();
    }

    /// <summary>
    /// <inheritdoc cref="ITSOProtocolRegulator"/>
    /// </summary>
    public abstract class TSOProtocolBase : ITSOProtocolRegulator
    {
        /// <summary>
        /// A <see cref="TSOVoltronPacket"/> function handler delegate that will be used to handle incoming <see cref="TSOVoltronPacket"/> types
        /// </summary>
        /// <param name="PDU"></param>
        public delegate void VoltronInvokationDelegate(TSOVoltronPacket PDU);

        /// <summary>
        /// Maps the <see cref="TSOProtocolMatchingOption.PDUType"/> property to a dictionary of <see cref="TSOProtocolMatchingOption.EnumType"/> value to the <see cref="Delegate"/> function that will handle it
        /// </summary>
        private Dictionary<Type,Dictionary<uint,Delegate>> _map = new();

        protected TSOServerServiceManager? ServiceManager => Server?.Services;
        private TSOProtocolRegulatorResponse? CurrentResponse { get; set; } = null;

        private readonly TSOProtocolMatchingOption[] options;

        /// <summary>
        /// The underlying <see cref="ITSOServer"/> instance used for sending/receiving PDUs/other network traffic
        /// </summary>
        public ITSOServer? Server { get; set; }
        protected TSOLoggerServiceBase? Logger => Server?.Logger;

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

        /// <summary>
        /// Creates a new <see cref="TSOProtocolBase"/> instance with the provided <paramref name="Options"/>.
        /// <para/>Please ensure that the <paramref name="Options"/> are unique and in order of most generic to most specific.
        /// <para/>As in, you should position <see cref="TSOVoltronPacket"/> as the first option, and then more specific PDUs/interfaces after that.
        /// </summary>
        /// <param name="Options">Please ensure that the <paramref name="Options"/> are unique and in order of most generic to most specific.
        /// <para/>As in, you should position <see cref="TSOVoltronPacket"/> as the first option, and then more specific PDUs/interfaces after that.</param>
        protected TSOProtocolBase(params TSOProtocolMatchingOption[] Options)
        {
            options = Options;
        }

        public virtual void Init(ITSOServer Server)
        {
            this.Server = Server;
            MapInternal(options);
            LogConsole($"Initialized {GetType().Name} Regulator", "Initialization");
        }

        /// <summary>
        /// Maps all <see cref="TSOProtocolMatchingOption.AttributeType"/> adorned methods as VoltronPDU handlers to this <see cref="TSOProtocolBase"/> instance.
        /// </summary>
        private void MapInternal(params TSOProtocolMatchingOption[] matchingConfigs)
        {
            void DoMappingFunction(Type TAttribute, Type TDelegate, MethodInfo Target, Func<uint, Delegate, bool> TryAdd)
            {
                string message = "success.";
                try
                {
                    ITSOProtocolHandlerAttribute attribute = (ITSOProtocolHandlerAttribute)Target.GetCustomAttribute(TAttribute);
                    var packetType = attribute.ItemID;
                    Delegate action = Target.CreateDelegate(TDelegate, this);
                    bool result = TryAdd(packetType, action);
                    if (result)
                    {
                        LogConsole($"MAPPED Function: void {Target.Name}() -> PacketType: {packetType:X8})", TAttribute.Name);
                        return;
                    }
                    else
                        message = $"{packetType} is already mapped!";
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }
                LogConsole($"FAILED {Target.Name}(): {message}", TAttribute.GetType().Name, TSOLoggerServiceBase.LogSeverity.Errors);
            }

            _map.Clear();
            HashSet<Type> usedAttributes = new();
            HashSet<MethodInfo> usedMethods = new();
            foreach (var mapping in matchingConfigs)
            {
                _map.Add(mapping.PDUType, new Dictionary<uint, Delegate>());
                if (!usedAttributes.Add(mapping.AttributeType))
                    throw new InvalidOperationException($"Mapping for {mapping.AttributeType.Name} is already defined in this regulator's Options list. Please ensure each mapping attribute type is unique.");
                foreach (var function in GetType().GetMethods().Where(x => x.GetCustomAttribute(mapping.AttributeType) != null))
                {
                    DoMappingFunction(mapping.AttributeType, mapping.HandlerFunctionSignature, function, (PacketType, Delegate) =>
                    {
                        if (!usedMethods.Add(function))
                            throw new InvalidOperationException($"Mapping for {function.Name} has already been used before on another PDU type. You cannot have multiple PDUs using the same handler.");
                        return _map[mapping.PDUType].TryAdd(PacketType, Delegate);
                    });

                }
            }
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
        protected void SendTo<T>(uint QuazarID, T Packet) where T : TSOVoltronPacket => ((List<(uint, TSOVoltronPacket)>)CurrentResponse.SessionPackets).Add((QuazarID, Packet));
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
        protected bool TrySendTo<T>(uint AvatarID, T Packet) where T : TSOVoltronPacket => TrySendTo(new TSOAriesIDStruct(AvatarID, ""), Packet);
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
                if (!ExcludeQuazarIDs.Contains(QuazarID))
                    SendTo(QuazarID, Packet);
        }
        protected nio2soVoltronDataServiceClient GetDataService()
        {
            if (!TryGetService<nio2soVoltronDataServiceClient>(out var dataServiceClient))
                throw new NullReferenceException(nameof(nio2soVoltronDataServiceClient));
            return dataServiceClient;
        }
        protected bool TryDataServiceQuery<T>(Func<nio2soVoltronDataServiceClient, Task<HTTPServiceClientBase.HTTPServiceResult<T>>> DataServiceQuery, out T? Result, out string FailureMessage)
        {
            var result = DataServiceQuery(GetDataService()).Result;
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
        protected void LogConsole(string Message, string Caption = "", TSOLoggerServiceBase.LogSeverity Severity = TSOLoggerServiceBase.LogSeverity.Message) =>
            GetService<TSOLoggerServiceBase>().LogConsole(new(TSOLoggerServiceBase.LogSeverity.Message, RegulatorName, $"[{Caption ?? ""}] {Message}"));

        /// <summary>
        /// Returns the type map for the provided <paramref name="PDUType"/>
        /// </summary>
        /// <param name="PDUType"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        protected Dictionary<uint, Delegate> GetMapFor<T>()
        {
            if (!_map.TryGetValue(typeof(T), out var map))
                throw new KeyNotFoundException($"No mapping found for PDU Type {typeof(T).Name} in Regulator {RegulatorName}");
            return map;
        }

        public bool HandleIncomingPDU(TSOVoltronPacket PDU, out TSOProtocolRegulatorResponse Response)
        {
            if (Server == null) throw new NullReferenceException("No server instance!!!");

            Response = CurrentResponse = new(new List<TSOVoltronPacket>(), new List<TSOVoltronPacket>(), new List<TSOVoltronPacket>(), new List<(uint, TSOVoltronPacket)>());
            
            //TRY GENERIC VOLTRON PACKET HANDLER
            if (GetMapFor<TSOVoltronPacket>().TryGetValue(PDU.VoltronPacketType, out Delegate? action) && action != null)
            {
                Invoke(action, PDU);
                return true;
            }
            //PASS TO PROTOCOL FOR SPECIFIC HANDLER
            else if (TryHandleSpecialVoltronPDU(PDU, ref Response))            
                return true;          
            //NO HANDLERS EXIST
            else if (TryHandleUnhandledVoltronPDU(PDU, ref Response))
                return true;
            //COMPLETELY UNHANDLED
            Response = null;
            return false;
        }
        /// <summary>
        /// Uses the <see cref="MethodInvoker"/> to invoke the <paramref name="Delegate"/> with the provided <paramref name="Parameters"/> list.
        /// </summary>
        /// <param name="Delegate"></param>
        /// <param name="Parameters"></param>
        protected void Invoke(Delegate Delegate, params object[] Parameters) => MethodInvoker.Create(Delegate.Method).Invoke(this, [..Parameters]);
        /// <summary>
        /// When overridden, this method will enable the specific protocols to handle special Voltron PDUs that are not handled by the generic <see cref="TSOVoltronPacket"/> handler.
        /// </summary>
        /// <param name="PDU"></param>
        /// <param name="Response"></param>
        /// <returns></returns>
        protected abstract bool TryHandleSpecialVoltronPDU(TSOVoltronPacket PDU, ref TSOProtocolRegulatorResponse Response);
        /// <summary>
        /// If all attempts to handle the <paramref name="PDU"/> fail, this method will be called to handle unhandled Voltron PDUs.
        /// </summary>
        /// <param name="PDU"></param>
        /// <param name="Response"></param>
        /// <returns></returns>
        protected virtual bool TryHandleUnhandledVoltronPDU(TSOVoltronPacket PDU, ref TSOProtocolRegulatorResponse Response) => false;
    }
}
