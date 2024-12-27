using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.City.TSO;
using nio2so.TSOTCP.City.TSO.Aries;
using nio2so.TSOTCP.City.TSO.Voltron;
using nio2so.TSOTCP.City.TSO.Voltron.PDU.Datablob;
using nio2so.TSOTCP.City.TSO.Voltron.PDU.Datablob.Structures;
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

            while(Console.ReadLine() != "shutdown")
            {
                var housestatepacket = new TSOBroadcastDatablobPacket(
                    //new Struct.TSOAriesIDStruct("A 1337", ""),
                    TSO_PreAlpha_MasterConstantsTable.GZCLSID_cCrDMStandardMessage,
                    new TSOStandardMessageContent(TSO_PreAlpha_MasterConstantsTable.kMSGID_MessageHouseOccupants,
                    new byte[] { 0x00,0x00,0x00,0x01 })
                )
                {
                    CurrentSessionID = new TSO.Voltron.Struct.TSOAriesIDStruct("A 1337", TestingConstraints.MyAvatarName)
                };
                housestatepacket.MakeBodyFromProperties();
                cityServer.SendPacket(null,housestatepacket);
            }
        }
    }
}