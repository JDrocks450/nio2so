using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.City;
using nio2so.TSOTCP.City.Factory;
using nio2so.TSOTCP.City.TSO;
using nio2so.TSOTCP.City.TSO.Aries;
using nio2so.TSOTCP.City.TSO.Voltron;
using nio2so.TSOTCP.HSBServer.niotso;
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
            //NIOTSO UNPACK            

            //**INIT HSB CLIENT FOR ROOM HOSTING**
            //InvokeNewHSBClient();

            //**AWAIT ARRIVAL OF THE HSB
            HSBHouseServer roomServer = new(TestingConstraints.Room_ListenPort); // Room server
            roomServer.IsRunning = true;
            roomServer.HSB_ImReady += delegate { CreateCityServer(); };
            roomServer.Start();
            HSBSession.RoomServer = roomServer;

            MashugaLogUnpacker unpacker = new(@"E:\Games\TSO Pre-Alpha\niotso\mashuga-2016-08-13\log.dat");            
            foreach (var frame in unpacker.Frames)
            {
                using (MemoryStream stream = new MemoryStream(frame.DumpedData))
                using (TSOVoltronPacket packet = TSOPDUFactory.CreatePacketObjectFromDataBuffer(stream))
                {
                    if (packet == null) continue;
                    Console.WriteLine($"[{frame.Sender}, {frame.FileOffset:X4}]" + ": " + packet.ToString());
                    Console.ReadKey();
                }
            }

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
            cityServer.HSB_ImReady += delegate
            {
                Console.WriteLine("Join quickly!");
                HSBSession.RoomServer.IsRunning = true;
            };
        }
    }
}
