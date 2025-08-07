using nio2so.DataService.Common.Tokens;
using System.Collections.Concurrent;
using System.Net;

namespace nio2so.TSOHTTPS.Protocol.Data
{
    /// <summary>
    /// Stores a list of connections that are consulting with TSOHTTPS and tracks the <see cref="UserToken"/> to identify the connection
    /// </summary>
    internal static class EntryLobby
    {
        /// <summary>
        /// A connection waiting for service from TSOHTTPS
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="Port"></param>
        public record LobbyWaiter(IPAddress IP, int Port);
        private static ConcurrentDictionary<LobbyWaiter, UserToken> Awaiters { get; } = new();

        /// <summary>
        /// Adds a new connection to the <see cref="EntryLobby"/>
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="IP"></param>
        /// <param name="PORT"></param>
        /// <returns></returns>
        public static bool Add(UserToken Token, IPAddress IP, int PORT) => Awaiters.TryAdd(new(IP, PORT), Token);

        /// <summary>
        /// Returns a connection from the <see cref="EntryLobby"/>, if found in the lobby.
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="PORT"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        public static bool Get(IPAddress IP, int PORT, out UserToken Token)
        {
            Token = default;
            lock (Awaiters)
            {
                LobbyWaiter? foundWaiter = null;
                UserToken? found = null;
                foreach (var waiter in Awaiters)
                {
                    if (waiter.Key.IP == IP && waiter.Key.Port == PORT)
                    {
                        found = waiter.Value;
                        foundWaiter = waiter.Key;
                        break;
                    }
                }
                Token = found.Value;
                return found.HasValue;
            }
        }

        /// <summary>
        /// Removes a connection from the <see cref="EntryLobby"/>, if found in the lobby.
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="PORT"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        public static bool Serve(IPAddress IP, int PORT, out UserToken Token)
        {
            Token = default;
            lock (Awaiters)
            {
                LobbyWaiter? foundWaiter = null;
                UserToken? found = null;
                foreach (var waiter in Awaiters)
                {
                    if (waiter.Key.IP == IP && waiter.Key.Port == PORT)
                    {
                        found = waiter.Value;
                        foundWaiter = waiter.Key;
                        break;
                    }
                }
                if (foundWaiter != null)                
                    Awaiters.Remove(foundWaiter, out _);
                if (found.HasValue)
                    Token = found.Value;
                return found.HasValue;
            }
        }
    }
}
