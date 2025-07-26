using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.Voltron.Protocol.Services
{
    /// <summary>
    /// Maps a Quazar Connection ID to a <see cref="TSOAriesIDStruct"/>
    /// </summary>
    public class nio2soClientSessionService : ITSOService
    {
        private readonly Dictionary<uint, TSOAriesIDStruct> _sessions = new();

        public void AddClient(uint QuazarID, TSOAriesIDStruct VoltronID)
        {
            if (!_sessions.TryAdd(QuazarID, VoltronID))
                _sessions[QuazarID] = VoltronID;
        }
        public bool RemoveClient(uint QuazarID) => _sessions.Remove(QuazarID);
        public bool TryIdentify(uint QuazarID, out TSOAriesIDStruct? VoltronID)
        {
            bool result = _sessions.TryGetValue(QuazarID, out VoltronID);
            CurrentClient = VoltronID;
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
        public void ClearClientProperty() => CurrentClient = null;
        public TSOAriesIDStruct? CurrentClient { get; private set; }

        public void Dispose()
        {
            
        }
    }
}
