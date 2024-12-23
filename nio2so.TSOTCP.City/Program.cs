using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.City.TSO;
using nio2so.TSOTCP.City.TSO.Aries;
using System.Diagnostics;

namespace nio2so.TSOTCP.City
{
    internal class Program
    {
        static TSOCityServer cityServer;

        static void Main(string[] args)
        {
            //CLEAR PREVIOUS PACKETS
            string clearDirProcName = @"E:\packets\cleandir.bat";
            if (File.Exists(clearDirProcName))
            {
                Process.Start(new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    WorkingDirectory = Path.GetDirectoryName(clearDirProcName),
                    FileName = clearDirProcName
                })?.WaitForExit();
            }

            //START THE CITY SERVER
            cityServer = new(TestingConstraints.City_ListenPort); // 49000 for City Server || HouseSimServer testing is 49101
            cityServer.Start();
        }
    }
}