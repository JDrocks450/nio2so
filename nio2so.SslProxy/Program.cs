using OpenSSL.Core;
using OpenSSL.Crypto;
using OpenSSL.SSL;
using OpenSSL.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace nio2so.SslProxy
{
    internal class Program
    {
        static TcpListener listener;
        static X509Chain chain;

        static void Main(string[] args)
        {            
            chain = new X509Chain(File.ReadAllText(@"c:\nio2so\cert.pem"));                        

            listener = new TcpListener(System.Net.IPAddress.Loopback, 49100);
            listener.Start();
            Console.WriteLine("SSL Proxy is running on port 49100...");
            BeginAccept();
            Console.ReadKey();
        }

        private static void BeginAccept()
        {
            listener.BeginAcceptTcpClient(new AsyncCallback(AcceptTcpClientCallback), listener);
        }

        private static void AcceptTcpClientCallback(IAsyncResult ar)
        {
            TcpClient client = listener.EndAcceptTcpClient(ar);
            Console.WriteLine("Client accepted, attempting authentication...");

            var cert = chain[0];
            var key = CryptoKey.FromPrivateKey(File.ReadAllText(@"c:\nio2so\key.pem"), "");
            if (!cert.CheckPrivateKey(key))
                Console.WriteLine("Private key failed!");
            bool verified = cert.Verify(key);
            cert.PrivateKey = key;

            using (SslStream sslStream = new SslStream(client.GetStream(), false))
            {
                try
                {
                    
                    // Authenticate the server certificate
                    sslStream.AuthenticateAsServer(cert,false,chain,SslProtocols.Tls|SslProtocols.Ssl2|SslProtocols.Ssl3,SslStrength.All,false);
                    
                    Console.WriteLine("Client connection completed.");
                    
                    // Handle client communication here
                    // For example, read/write data using sslStream.Read and sslStream.Write
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex}");
                }
                finally
                {
                    sslStream.Close();
                    client.Close();
                }
            }
            BeginAccept();
        }
    }
}
