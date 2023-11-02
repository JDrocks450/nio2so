using nio2so.TSOTCP.City.TSO;
using nio2so.TSOTCP.City.TSO.Aries;

namespace nio2so.TSOTCP.City
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TSOCityServer cityServer = new(36100);
            cityServer.Start();
        }
    }
}