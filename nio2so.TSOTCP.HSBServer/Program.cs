using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.City.TSO;
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

        static TSOCityServer roomServer, cityServer;
        static int readyClients = 0;

        static void Main(string[] args)
        {
            //**INIT HSB CLIENT FOR ROOM HOSTING**
            InvokeNewHSBClient();

            //**AWAIT ARRIVAL OF THE HSB
            roomServer = new(TestingConstraints.Room_ListenPort); // Room server
            roomServer.IsRunning = false;
            roomServer.HSB_ImReady += Component_HSBReady;
            roomServer.Start();

            //START THE CITY SERVER
            cityServer = new(TestingConstraints.City_ListenPort); // 49000 for City Server || HouseSimServer testing is 49101
            cityServer.IsRunning = false;
            cityServer.HSB_ImReady += Component_HSBReady;
            cityServer.Start();
        }

        private static void Component_HSBReady(object? sender, EventArgs e)
        {
            readyClients++;
            if (readyClients == 1)
                cityServer.IsRunning = roomServer.IsRunning = true;
        }
    }
}
