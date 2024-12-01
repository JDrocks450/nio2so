using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.City.TSO;
using nio2so.TSOTCP.City.TSO.Aries;

namespace nio2so.TSOTCP.City
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TSOCityServer cityServer = new(TestingConstraints.ListenPort); // 49000 for City Server || HouseSimServer testing is 49101
            cityServer.Start();
        }
    }
}