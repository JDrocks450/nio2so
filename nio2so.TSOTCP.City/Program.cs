using nio2so.Data.Common.Testing;
using nio2so.DataService.Common.Types;
using OpenSSL.Crypto;
using OpenSSL.X509;
using System.Diagnostics;

namespace nio2so.TSOTCP.Voltron.Server
{
    internal class Program
    {
        private const string DisableCachingName = @"TestSwitch.LocalAppContext.DisableCaching";
        private const string DontEnableSchUseStrongCryptoName = @"Switch.System.Net.DontEnableSchUseStrongCrypto";
        static int Main(string[] args)
        {
            AppContext.SetSwitch(DisableCachingName, true);
            AppContext.SetSwitch(DontEnableSchUseStrongCryptoName, true);

            //VersionInfo is a special color
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{nameof(TSONeoVol2ronServer)} is starting up...\n\nVERSION INFO:\n{Protocol.TSOVoltronBasicServer.ServerVersionInfoString}");

            //Init is a special color
            Console.ForegroundColor = ConsoleColor.Yellow;

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
                settings = TSONeoVol2ronServer.DownloadSettings().Result;
                if (settings == null)
                    throw new InvalidOperationException("Please check your LocalServerSettings.config file to ensure your API Data Service URL is accurate.\n" +
                        "Could not download " + nameof(VoltronServerSettings));
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
                File.WriteAllBytes("crashdump.log", System.Text.Encoding.UTF8.GetBytes(ex.ToString()));
                return 1;
            }            

            Console.WriteLine("\nStarting engines! Settings: " + settings);
            Console.ResetColor();

            //OPEN SSL CERT IF ONE IS AVAILABLE
            X509Certificate? voltronCertificate = default;
            string certFile = @"C:\nio2so\voltron.pfx";
            if (settings.SSLEnabled)
            {
                Console.WriteLine("SSL is enabled, loading certificate...");
                string certPath = @"c:\nio2so\cert.pem";
                //NIO2SO
                if (File.Exists(certFile))
                {
                    //var results = X509CertificateLoader.LoadPkcs12CollectionFromFile(certFile, settings.SSLCertificatePassword);
                    //voltronCertificate = results.First();
                    //Console.WriteLine($"Verified Certificate: {voltronCertificate.Verify()} Matches Hostname: {voltronCertificate.MatchesHostname(settings.ServerIPAddress)}");
                }
                //NIOTSO COMPATIBILITY
                else if (File.Exists(certPath)) 
                {                    
                    var chain = new X509Chain(File.ReadAllText(certPath));
                    var keyPath = @"c:\nio2so\key.pem";
                    var cert = voltronCertificate = chain[0];
                    var key = CryptoKey.FromPrivateKey(File.ReadAllText(keyPath), settings.SSLCertificatePassword);
                    if (!cert.CheckPrivateKey(key))
                        Console.WriteLine("Private key failed!");
                    bool verified = cert.Verify(key);
                    cert.PrivateKey = key;
                }
                //failed to load cert
                if (voltronCertificate == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Could not load the certificate file. Please ensure voltron.pfx (or niotso cert.pem) is in the working directory. Disabling SSL...");
                    Console.ResetColor();
                }
            }

            //START THE CITY SERVER
            TSONeoVol2ronServer cityServer = new TSONeoVol2ronServer(settings, voltronCertificate); // 49000 for City Server || HouseSimServer testing is 49101            
            cityServer.Start();

            //Go is a special color
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n\n{nameof(TSONeoVol2ronServer)} Shard: \"{cityServer.Name}\" is: ONLINE ({settings.ServerConnectionAddress}) and" +
                $" nio2so DataService is: CONNECTED ({LocalServerSettings.Default.APIUrl})\n\n");
            Console.ResetColor();

            while (Console.ReadLine() != "shutdown")
            {
                
            }

            return 0;
        }
    }
}