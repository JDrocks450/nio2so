using nio2so.TSOTCP.City.TSO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.Voltron.Protocol
{
    public static class HSBSession
    {
        public static bool HSB_Activated = false;
        public static ITSOServer CityServer { get; set; }
        public static ITSOServer RoomServer { get; set; }
    }
}
