using nio2so.TSOTCP.City.TSO.Voltron.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.Serialization
{
    /// <summary>
    /// A type with AriesID and MasterID properties.
    /// </summary>
    public interface ITSOVoltronAriesMasterIDStructure
    {
        /// <summary>
        /// The current AriesID/MasterID combination that denotes the current Client
        /// </summary>
        public TSOAriesIDStruct CurrentSessionID { get; set; }
    }
}
