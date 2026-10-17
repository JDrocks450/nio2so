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
        public class HandlerContext
        {
            public TSODataServiceWrapperPDU Query { get; }
            public TSODataServiceRuntimeObject DataObject { get; }
            public bool Handled { get; set; }
            public uint? ObjectID
            {
                get
                {
                    var stdMsg = GetMessageAs<TSONetMessageStandard>();
                    return (stdMsg != null && stdMsg.HasData1 && stdMsg.Data1.HasValue) ? stdMsg.Data1.Value : null;
                }
            }

            private List<TSODataServiceRuntimeObject> _responseObjects = new();

            public HandlerContext(TSODataServiceWrapperPDU query, TSODataServiceRuntimeObject dataObject)
            {
                Query = query;
                Handled = false;
                DataObject = dataObject;
            }

            public T GetMessageAs<T>() where T : class, ITSOMessage, new() => Query.Message.GetMessageAs<T>();
            public TSONetMessageStandard GetMessageAs() => GetMessageAs<TSONetMessageStandard>();

            internal void RespondWithObject(TSODataServiceRuntimeObject Object) => _responseObjects.Add(Object);
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

                IEnumerable<MethodInfo> handlers = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic).Where(x => x.GetCustomAttribute<TSODataServiceTypeHandler>() != null);
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

                    HandlerFunction signature = (HandlerFunction)handlerFunction.CreateDelegate(typeof(HandlerFunction), Instance);

                    _handlerFunctionsByName.Add(attribute.StructName, signature);
                    if (attribute.NameID != null)
                        _handlerFunctionsByID.Add(attribute.NameID.Value, signature);

                    LogConsole($"Mapped Function {handlerFunction.Name} -> Struct {attribute.StructName}");
                }
            }
        }

        protected void LogConsole(string Message, TSOLoggerServiceBase.LogSeverity Severity = TSOLoggerServiceBase.LogSeverity.Message) =>
           Parent?.Logger?.LogConsole(new TSOLoggerServiceBase.ConsoleLogEntry(Severity, GetType().Name, Message));

        internal void Invoke(TSOProtocol Sender, HandlerContext ctx)
        {
            string name = ctx.DataObject.Structure.NameString;
            uint id = ctx.DataObject.Structure.NameID;

            if (_handlerFunctionsByID.TryGetValue(id, out HandlerFunction handlerFunction))
                handlerFunction.Invoke(Sender,ctx);
            else if (_handlerFunctionsByName.TryGetValue(name, out handlerFunction))
                handlerFunction.Invoke(Sender,ctx);
            else
                throw new InvalidDataException($"Handler function for {ctx.DataObject.Structure} not found!");
        }
    }
}
