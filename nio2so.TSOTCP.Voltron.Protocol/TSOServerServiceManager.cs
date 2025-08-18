using nio2so.Voltron.Core.TSO;
using System.Diagnostics;

namespace nio2so.Voltron.Core
{
    /// <summary>
    /// Services can be added to a <see cref="ITSOServer"/> which can then be used by <see cref="ITSOProtocolRegulator"/> instances
    /// </summary>
    public interface ITSOService : IDisposable
    {
        /// <summary>
        /// The <see cref="ITSOServer"/> this <see cref="ITSOService"/> is registered to.
        /// </summary>
        ITSOServer Parent { get; set; }
        /// <summary>
        /// This is called when the <see cref="ITSOService"/> is registered to a <see cref="ITSOServer"/> using the <see cref="TSOServerServiceManager.Register{T}(T)"/> function
        /// </summary>
        /// <param name="Server"></param>
        void Init(ITSOServer Server);
    }

    /// <summary>
    /// Contains <see cref="ITSOService"/> instances for use with <see cref="TSORegulator"/>
    /// </summary>
    public sealed class TSOServerServiceManager
    {
        private readonly Dictionary<Type, ITSOService> _services = new();
        private readonly ITSOServer server;

        public TSOServerServiceManager(ITSOServer Server)
        {
            server = Server;
        }
        /// <summary>
        /// Registers the provided <see cref="ITSOService"/> to the <see cref="TSOServerServiceManager"/> for use with <see cref="TSORegulator"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        /// <returns></returns>
        public bool Register<T>(T service) where T : ITSOService
        {
            if (server == null)
            {
                Debug.WriteLine("TSOServerServiceManager: Cannot register service, server is null.");
                return false;
            }
            if (service == null)
            {
                Debug.WriteLine("TSOServerServiceManager: Cannot register service, service is null.");
                return false;
            }
            service.Parent = server;
            service.Init(server);
            return _services.TryAdd(typeof(T), service);
        }
        /// <summary>
        /// Gets the <see cref="ITSOService"/> by <see cref="Type"/> <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        /// <returns></returns>
        public bool TryGet<T>(out T? service) where T : ITSOService
        {
            var result = GetByType<T>();
            service = default;
            if (!result.Any())
                return false;
            service = (T)result.First().Value;
            return true;
        }
        private IEnumerable<KeyValuePair<Type, ITSOService>> GetByType<T>() where T : ITSOService => _services.Where(x => x.Key.IsAssignableTo(typeof(T)));
        public T Get<T>() where T : ITSOService => (T)GetByType<T>().First().Value;
        /// <summary>
        /// Removes the <see cref="ITSOService"/> by <see cref="Type"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Deregister<T>() where T : ITSOService
        {
            if (_services.TryGetValue(typeof(T), out var service))
                service.Parent = null; // clear parent reference
            return _services.Remove(typeof(T));
        }
    }
}
