﻿using nio2so.Data.Common.Testing;
using nio2so.TSOTCP.City.TSO;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.Datablob;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.Datablob.Structures;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Serialization;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace nio2so.TSOTCP.City
{
    internal class Program
    {
        static TSOCityServer cityServer;

        static void Main(string[] args)
        {
            //CLEAR PREVIOUS PACKETS
            string clearDirProcName = Path.Combine(TestingConstraints.WorkspaceDirectory, @"cleandir.bat");
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
                
            }
        }
    }
}