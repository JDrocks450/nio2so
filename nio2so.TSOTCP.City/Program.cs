using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.City.TSO;
using nio2so.TSOTCP.City.TSO.Aries;
using System.Diagnostics;

namespace nio2so.TSOTCP.City
{
    internal class Program
    {
        static void InvokeNewHSBClient()
        {
            string fname = @"E:\Games\TSO Pre-Alpha\TSO - Patched HouseSimServer\TSOClient.exe";
            Process? proc = Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = false,
                FileName = fname,
                Arguments = "-w -debug_objects -nosound -hsb_mode -playerid:1220 -vport:49 -vhost:localhost -city:1",
                WorkingDirectory = Path.GetDirectoryName(fname)
            });
        }

        static void Main(string[] args)
        {
            //**INIT HSB CLIENT FOR ROOM HOSTING**
            InvokeNewHSBClient();


            TSOCityServer cityServer = new(TestingConstraints.ListenPort); // 49000 for City Server || HouseSimServer testing is 49101
            cityServer.Start();
        }
    }
}