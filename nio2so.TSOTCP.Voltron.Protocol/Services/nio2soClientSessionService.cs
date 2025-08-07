using nio2so.TSOTCP.Voltron.Protocol.TSO;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Struct;
using System.Collections.Concurrent;

namespace nio2so.TSOTCP.Voltron.Protocol.Services
{
    /// <summary>
    /// Maps a Quazar Connection ID to a <see cref="TSOAriesIDStruct"/>
    /// </summary>
    public class nio2soClientSessionService : ITSOService
    {
        private readonly ConcurrentDictionary<uint, TSOAriesIDStruct> _sessions = new();
        private readonly HashSet<uint> _casClients = new();

        public bool AddClientInCAS(uint QuazarID) => _casClients.Add(QuazarID);
        public void AddClient(uint QuazarID, TSOAriesIDStruct VoltronID)
        {
            if (QuazarID == 0)
                throw new InvalidDataException(nameof(QuazarID) + $" is {QuazarID} which is invalid. (thrown at: {nameof(nio2soClientSessionService)})"); 
            if (!_sessions.TryAdd(QuazarID, VoltronID))
                _sessions[QuazarID] = VoltronID;
        }
        public bool RemoveClient(uint QuazarID, out TSOAriesIDStruct? VoltronID) => _sessions.TryRemove(QuazarID, out VoltronID);
        public bool IsInCAS(uint QuazarID) => _casClients.Contains(QuazarID);
        public bool TryIdentify(uint QuazarID, out TSOAriesIDStruct? VoltronID)
        {
            bool result = _sessions.TryGetValue(QuazarID, out VoltronID);
            return result;
        }
        public bool TryReverseSearch(TSOAriesIDStruct VoltronID, out uint QuazarID)
        {
            QuazarID = 0;
            var result = _sessions.Where(x => ((ITSONumeralStringStruct)x.Value).NumericID == ((ITSONumeralStringStruct)VoltronID).NumericID);
            if (!result.Any()) return false;
            QuazarID = result.First().Key;
            return true;    
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
            
        }
    }
}
