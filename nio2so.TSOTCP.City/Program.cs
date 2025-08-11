using nio2so.Data.Common.Testing;
using nio2so.DataService.Common.Types;
using System.Diagnostics;

namespace nio2so.TSOTCP.Voltron.Server
{
    internal class Program
    {
        static TSOCityServer cityServer;

        static int Main(string[] args)
        {
            //Init is a special color
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine("nio2so Voltron Server is starting up...\n");
            //CLEAR PREVIOUS PACKETS
            string clearDirProcName = Path.Combine(TestingConstraints.WorkspaceDirectory, @"cleandir.bat");
            if (File.Exists(clearDirProcName))
            {
                Console.WriteLine("Cleaning up prior session...");
                Process.Start(new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    WorkingDirectory = Path.GetDirectoryName(clearDirProcName),
                    FileName = clearDirProcName
                })?.WaitForExit();
            }
            else Console.WriteLine("Clean up script could not be found. Please purge tsotcppackets often to avoid disk usage...");

            //PING DATA SERVICE
            Console.WriteLine("Downloading server settings...");
            VoltronServerSettings? settings;
            try
            {
                settings = TSOCityServer.DownloadSettings().Result;
                if (settings == null)
                    throw new InvalidOperationException("Please check your LocalServerSettings.config file to ensure your API Data Service URL is accurate.\n" +
                        "Could not download " + nameof(VoltronServerSettings));
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
                return 1;
            }            

            Console.WriteLine("\nStarting engines! Settings: " + settings);
            Console.ResetColor();

            //START THE CITY SERVER
            cityServer = new TSOCityServer(settings); // 49000 for City Server || HouseSimServer testing is 49101            
            cityServer.Start();

            while(Console.ReadLine() != "shutdown")
            {
                
            }

            return 0;
        }
    }
}