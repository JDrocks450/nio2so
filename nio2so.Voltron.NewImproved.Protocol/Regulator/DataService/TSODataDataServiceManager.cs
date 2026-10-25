using nio2so.Formats.TSOData;
using nio2so.Voltron.Core;
using nio2so.Voltron.Core.Telemetry;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.NewImproved.Protocol.PDU;
using nio2so.Voltron.NewImproved.Protocol.PDU.MessageFormat;
using nio2so.Voltron.NewImproved.Protocol.Types.DataService;
using System;
using System.Reflection;
using static nio2so.Voltron.NewImproved.Protocol.Regulator.DataService.TSODataServiceControllerBase;

namespace nio2so.Voltron.NewImproved.Protocol.Regulator.DataService
{
    internal abstract class TSODataServiceControllerBase
    {
        internal enum TSODataServiceHandlerDataDirection
        {
            None,
            FromDataService,
            FromTSOClient,
        }

        public class HandlerContext
        {
            public TSODataServiceWrapperPDU Query { get; }
            public TSODataRuntimeObject DataObject { get; }
            public TSODataServiceHandlerDataDirection Direction { get; }
            public bool Handled { get; set; }
            public uint? ObjectID
            {
                get
                {
                    var stdMsg = GetMessageAs<TSONetMessageStandard>();
                    return (stdMsg != null && stdMsg.HasData1 && stdMsg.Data1.HasValue) ? stdMsg.Data1.Value : null;
                }
            }

            private List<TSODataRuntimeObject> _responseObjects = new();

            public HandlerContext(TSODataServiceWrapperPDU query, TSODataRuntimeObject dataObject, TSODataServiceHandlerDataDirection Direction)
            {
                Query = query;
                Handled = false;
                DataObject = dataObject;
                this.Direction = Direction;
            }

            /// <summary>
            /// <inheritdoc cref="TSOMessageFormatBase.GetMessageAs{T}(byte[])"/>
            /// <para/> See: <see cref="TSOMessageFormatBase.GetMessageAs{T}(byte[])"/>
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public T GetMessageAs<T>() where T : TSOMessageFormatBase, new() => Query.Message.GetMessageAs<T>();
            /// <summary>
            /// <inheritdoc cref="TSOMessageFormatBase.GetMessageAs(TSO_NewImproved_TSOMessageType_CLSIDs, byte[])"/>
            /// <para/> See: <see cref="TSOMessageFormatBase.GetMessageAs(TSO_NewImproved_TSOMessageType_CLSIDs, byte[])"/>
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public ITSOMessage GetMessageAs() => Query.Message.GetMessageAs();

            internal void RespondWithObject(TSODataRuntimeObject Object) => _responseObjects.Add(Object);
        }

        /// <summary>
        /// The signature of a Handler function for a TSOData object
        /// </summary>
        /// <param name="ctx"></param>
        public delegate void HandlerFunction(TSOProtocol Sender, HandlerContext ctx);
    }
    /// <summary>
    /// <see cref="ITSOService"/> that maps all <see cref="TSODataServiceTypeHandler"/> functions in the current <see cref="Assembly"/>
    /// </summary>
    internal class TSODataDataServiceManager : ITSOService
    {
        Dictionary<Type, object> _instances = new();
        Dictionary<string, HandlerFunction> _handlerFunctionsByName = new();
        Dictionary<uint, HandlerFunction> _handlerFunctionsByID = new();

        public TSODataDataServiceManager()
        {
            
        }

        public ITSOServer Parent { get; set; } = null;

        public void Dispose()
        {
            _instances.Clear();
            _handlerFunctionsByName.Clear();
            _handlerFunctionsByID.Clear();
        }

        public void Init(ITSOServer Server)
        {
            MapInternal();
        }

        /// <summary>
        /// Maps all HandlerFunctions to an object map
        /// </summary>
        void MapInternal()
        {
            Dispose();

            Assembly thisAssembly = typeof(TSODataServiceControllerBase).Assembly;
            foreach (var type in thisAssembly.GetTypes())
            {
                if (!type.IsClass) continue;

                IEnumerable<MethodInfo> handlers = type.GetMethods().Where(x => x.GetCustomAttribute<TSODataServiceTypeHandler>() != null);

                if (!handlers.Any()) continue;

                if (!_instances.TryGetValue(type, out object Instance))
                {
                    Instance = Activator.CreateInstance(type);
                    _instances.Add(type, Instance);
                    LogConsole($"Mapping {type.Name}...");
                }

                foreach (var handlerFunction in handlers)
                {
                    TSODataServiceTypeHandler? attribute = handlerFunction.GetCustomAttribute<TSODataServiceTypeHandler>();

                    if (attribute == null)
                        throw new NullReferenceException(nameof(handlerFunction));

                    HandlerFunction signature = null;
                    try
                    {
                        signature = (HandlerFunction)handlerFunction.CreateDelegate(typeof(HandlerFunction), Instance);
                    }
                    catch (Exception ex)
                    {
                        LogConsole($"ERROR: [{handlerFunction.Name}] Your function declaration is likely incorrect. See: {nameof(HandlerFunction)}\n\n{ex}",TSOLoggerServiceBase.LogSeverity.Errors);
                    }
                    if (signature == null)
                        continue;

                    _handlerFunctionsByName.Add(attribute.StructName, signature);
                    if (attribute.NameID != null)
                        _handlerFunctionsByID.Add(attribute.NameID.Value, signature);

                    LogConsole($"Mapped Function {handlerFunction.Name} -> Struct {attribute.StructName}" + (attribute.NameID.HasValue ? $" {attribute.NameID:X}" : ""));
                }
            }
        }

        protected void LogConsole(string Message, TSOLoggerServiceBase.LogSeverity Severity = TSOLoggerServiceBase.LogSeverity.Message) =>
           Parent?.Logger?.LogConsole(new TSOLoggerServiceBase.ConsoleLogEntry(Severity, GetType().Name, Message));

        /// <summary>
        /// Invokes the corresponding <see cref="HandlerFunction"/> for the given <paramref name="ctx"/> <see cref="HandlerContext.DataObject"/> Structure
        /// <para/>Use: <paramref name="ForceExplicitHandlers"/> to dictate whether the protocol will peel back a <see cref="TSODerivedStruct"/> and attempt to invoke the base type handler, if one exists
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="ctx"></param>
        /// <param name="ForceExplicitHandlers"></param>
        /// <exception cref="InvalidDataException"></exception>
        internal void Invoke(TSOProtocol Sender, HandlerContext ctx, bool ForceExplicitHandlers = false)
        {
            string name = ctx.DataObject.Structure.NameString;
            uint id = ctx.DataObject.Structure.NameID;

            bool attemptInvoke()
            {
                if (_handlerFunctionsByID.TryGetValue(id, out HandlerFunction handlerFunction))
                    handlerFunction.Invoke(Sender, ctx);
                else if (_handlerFunctionsByName.TryGetValue(name, out handlerFunction))
                    handlerFunction.Invoke(Sender, ctx);
                else return false;

                return true;
            }

            if (attemptInvoke() && ctx.Handled)
                return;
            
            if (!ForceExplicitHandlers && ctx.DataObject.Structure.IsDerived)
            { // attempt parent invoke
                TSODerivedStruct? newType = (ctx.DataObject.Structure as TSODerivedStruct);
                if (newType != null)
                {
                    name = newType.ParentString;
                    id = newType.NameID;
                    if (attemptInvoke() && ctx.Handled)
                        return;
                }
            }

            throw new InvalidDataException($"Handler function for {ctx.DataObject.Structure} not found, or the function did not set Handled!");
        }
    }
}
