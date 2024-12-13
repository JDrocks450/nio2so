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
    internal interface ITSOVoltronAriesMasterIDStructure
    {
        public string AriesID { get; set; }
        public string MasterID { get; set; }
    }
}
