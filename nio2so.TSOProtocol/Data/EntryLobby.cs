using nio2so.DataService.Common.Tokens;
using nio2so.Formats;
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
        /// <param name="GameClientVersion">nio2so supports multiple game client versions, use this to determine what version of the client this connection is</param>
        public record LobbyWaiter(IPAddress IP, int Port, UserToken Token, TSOVersions GameClientVersion);
        private static ConcurrentDictionary<LobbyWaiter, UserToken> Awaiters { get; } = new();

        /// <summary>
        /// Adds a new connection to the <see cref="EntryLobby"/>
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="IP"></param>
        /// <param name="PORT"></param>
        /// <returns></returns>
        public static bool Add(UserToken Token, IPAddress IP, int PORT, TSOVersions GameClientVersion) => Awaiters.TryAdd(new(IP, PORT, Token, GameClientVersion), Token);

        /// <summary>
        /// <inheritdoc cref="Get(IPAddress, int, out LobbyWaiter?)"/>
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="PORT"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        public static bool GetUser(IPAddress IP, int PORT, out UserToken? Token)
        {
            Token = default;
            if (!Get(IP, PORT, out LobbyWaiter? waiter) || waiter == default)
                return false;
            Token = waiter.Token;
            return true;
        }
        
        /// <summary>
        /// Returns a <see cref="TSOVersions"/> of the connection from the <see cref="EntryLobby"/>, if found in the lobby.
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="PORT"></param>
        /// <param name="GameVersion"></param>
        /// <returns></returns>
        public static bool GetVersion(IPAddress IP, int PORT, out TSOVersions? GameVersion)
        {
            GameVersion = default;
            if (!Get(IP,PORT,out LobbyWaiter? waiter) || waiter == default)
                return false;
            GameVersion = waiter.GameClientVersion;
            return true;
        }

        /// <summary>
        /// Returns a connection from the <see cref="EntryLobby"/>, if found in the lobby.
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="PORT"></param>
        /// <param name="GameVersion"></param>
        /// <returns></returns>
        public static bool Get(IPAddress IP, int PORT, out LobbyWaiter? Waiter)
        {
            lock (Awaiters)
            {
                Waiter = null;
                foreach (var waiter in Awaiters)
                {
                    if (waiter.Key.IP == IP && waiter.Key.Port == PORT)
                    {
                        Waiter = waiter.Key;
                        break;
                    }
                }
                return Waiter != null;
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
