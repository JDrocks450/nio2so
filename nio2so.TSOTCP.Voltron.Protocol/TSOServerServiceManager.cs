namespace nio2so.TSOTCP.Voltron.Protocol
{
    public interface ITSOService : IDisposable
    {

    }

    /// <summary>
    /// Contains <see cref="ITSOService"/> instances for use with <see cref="TSORegulator"/>
    /// </summary>
    public sealed class TSOServerServiceManager
    {
        private readonly Dictionary<Type, ITSOService> _services = new();

        public TSOServerServiceManager() { }
        /// <summary>
        /// Registers the provided <see cref="ITSOService"/> to the <see cref="TSOServerServiceManager"/> for use with <see cref="TSORegulator"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        /// <returns></returns>
        public bool Register<T>(T service) where T : ITSOService => _services.TryAdd(typeof(T), service);
        /// <summary>
        /// Gets the <see cref="ITSOService"/> by <see cref="Type"/> <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        /// <returns></returns>
        public bool TryGet<T>(out T? service) where T : ITSOService
        {
            service = default;
            if (!_services.TryGetValue(typeof(T), out var s))
                return false;
            service = (T)s;
            return true;
        }
        public T Get<T>() => (T)_services[typeof(T)];
        /// <summary>
        /// Removes the <see cref="ITSOService"/> by <see cref="Type"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Deregister<T>() where T : ITSOService => _services.Remove(typeof(T));
    }
}
