using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.City;
using nio2so.TSOTCP.City.TSO;
using nio2so.TSOTCP.Voltron.Protocol;
using System.Diagnostics;

namespace nio2so.TSOTCP.HSBServer
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
            //InvokeNewHSBClient();

            //**AWAIT ARRIVAL OF THE HSB
            HSBHouseServer roomServer = new(TestingConstraints.Room_ListenPort); // Room server
            roomServer.IsRunning = false;
            roomServer.HSB_ImReady += delegate { CreateCityServer(); };
            roomServer.Start();
            HSBSession.RoomServer = roomServer;

            Console.WriteLine("Waiting for the HSB server to arrive...");
        }

        static void CreateCityServer()
        {
            //START THE CITY SERVER
            TSOCityServer cityServer = new(TestingConstraints.City_ListenPort); // 49000 for City Server || HouseSimServer testing is 49101
            cityServer.Start();

            HSBSession.CityServer = cityServer;            
            HSBSession.HSB_Activated = true;

            Console.WriteLine("Server is ready! Join as your Avatar onto a new lot.");
        }
    }
}
