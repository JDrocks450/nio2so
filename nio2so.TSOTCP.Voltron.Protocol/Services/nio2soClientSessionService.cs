using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.Core.TSO.Struct;
using System.Collections.Concurrent;

namespace nio2so.Voltron.Core.Services
{
    /// <summary>
    /// Tracks current connections to Voltron by mapping a <i>Quazar Connection ID</i>(<see cref="UInt32"/>) to a <see cref="TSOAriesIDStruct"/>
    /// </summary>
    public class nio2soClientSessionService : ITSOService
    {
        /// <summary>
        /// Thread-safe store of incoming mapped connection ids to <see cref="TSOAriesIDStruct"/>
        /// </summary>
        private readonly ConcurrentDictionary<uint, TSOAriesIDStruct> _sessions = new();
        /// <summary>
        /// list of clients that are in Create-A-Avatar
        /// </summary>
        private readonly HashSet<uint> _casClients = new();

        public ITSOServer Parent { get; set; } = null!;
        /// <summary>
        /// Maps a new <paramref name="QuazarID"/> to a Create an Avatar instance of TSO
        /// </summary>
        /// <param name="QuazarID"></param>
        /// <returns></returns>
        public bool AddClientInCAS(uint QuazarID) => _casClients.Add(QuazarID);
        /// <summary>
        /// Maps a new <paramref name="QuazarID"/> to the <paramref name="VoltronID"/> in this service -- this allows this service to then be 
        /// used to retrieve either the QuazarID or VoltronID for a given connection using one of these two details.
        /// 
        /// <para/>Notice: If the requested <paramref name="QuazarID"/> is <b>already mapped</b>, it will be <b>overwritten</b> by this new mapping.
        /// If this is not desired, utilize the <paramref name="Overwrite"/> parameter, which will instead throw an <see cref="InvalidOperationException"/>
        /// </summary>
        /// <param name="QuazarID">The Quazar Connection ID of the remote party</param>
        /// <param name="VoltronID">The Voltron ID the client identifies with</param>
        /// <exception cref="InvalidDataException"></exception>
        public void AddClient(uint QuazarID, TSOAriesIDStruct VoltronID, bool Overwrite = true)
        {
            if (QuazarID == 0)
                throw new InvalidDataException(nameof(QuazarID) + $" is {QuazarID} which is invalid. (thrown at: {nameof(nio2soClientSessionService)})");
            if (!_sessions.TryAdd(QuazarID, VoltronID) && Overwrite)
                _sessions[QuazarID] = VoltronID;
            else throw new InvalidOperationException($"The given key ({QuazarID}) already exists in the " + nameof(nio2soClientSessionService));
        }
        /// <summary>
        /// Removes a connection from this <see cref="nio2soClientSessionService"/>
        /// </summary>
        /// <param name="QuazarID"></param>
        /// <param name="VoltronID"></param>
        /// <returns></returns>
        public bool RemoveClient(uint QuazarID, out TSOAriesIDStruct? VoltronID) => _sessions.TryRemove(QuazarID, out VoltronID);
        /// <summary>
        /// Returns whether this connection is in Create an Avatar
        /// </summary>
        /// <param name="QuazarID"></param>
        /// <returns></returns>
        public bool IsInCAS(uint QuazarID) => _casClients.Contains(QuazarID);
        /// <summary>
        /// Tries to return the connection mapped to the given <paramref name="QuazarID"/>
        /// </summary>
        /// <param name="QuazarID"></param>
        /// <param name="VoltronID"></param>
        /// <returns></returns>
        public bool TryIdentify(uint QuazarID, out TSOAriesIDStruct? VoltronID)
        {
            bool result = _sessions.TryGetValue(QuazarID, out VoltronID);
            return result;
        }
        /// <summary>
        /// Tries to return the <paramref name="QuazarID"/> mapped to the given <see cref="TSOAriesIDStruct"/>
        /// </summary>
        /// <param name="VoltronID"></param>
        /// <param name="QuazarID"></param>
        /// <returns></returns>
        public bool TryReverseSearch(TSOAriesIDStruct VoltronID, out uint QuazarID)
        {
            QuazarID = 0;
            lock (_sessions)
            {
                var result = _sessions.Where(x => ((ITSONumeralStringStruct)x.Value).NumericID == ((ITSONumeralStringStruct)VoltronID).NumericID);
                if (!result.Any()) return false;
                QuazarID = result.First().Key;
                return true;
            }             
        }

        /// <summary>
        /// Identifies the <see cref="TSOAriesIDStruct"/> of the sender of the provided <paramref name="PDU"/>.
        /// <para/>WARNING: The <see cref="ITSOVoltron.SenderQuazarConnectionID"/> property MUST have a value for this to work.
        /// </summary>
        /// <param name="PDU"></param>
        /// <param name="VoltronID"></param>
        /// <returns></returns>
        public bool GetVoltronClientByPDU(TSOVoltronPacket PDU, out TSOAriesIDStruct? VoltronID) => 
            TryIdentify(PDU.SenderQuazarConnectionID.GetValueOrDefault(), out VoltronID);

        /// <summary>
        /// Returns a list of the current session list, this will not be updated if more clients are added as it is copied to a new enumerable
        /// <para/>Therefore, it is safe to directly use in foreach statements, etc.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TSOAriesIDStruct> GetConnectedClients() => [.. _sessions.Values];
        /// <summary>
        /// Returns a list of the current session list, this will not be updated if more clients are added as it is copied to a new enumerable
        /// <para/>Therefore, it is safe to directly use in foreach statements, etc.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<uint> GetConnectedQuazarIDs() => [.. _sessions.Keys];

        public void Dispose()
        {
            _sessions.Clear();
            _casClients.Clear();
        }

        public void Init(ITSOServer Server)
        {
            
        }
    }
}
