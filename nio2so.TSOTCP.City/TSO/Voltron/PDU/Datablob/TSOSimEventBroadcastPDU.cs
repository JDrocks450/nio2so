﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.Datablob
{
    /// <summary>
    /// This is sent when the SimEvent kMSG is raised to the server
    /// </summary>
    internal class TSOSimEventBroadcastPDU
    {
        [TSOVoltronBroadcastDatablobPDUField] public uint SimEvent_Arg1 { get; set; } = 0x00034D45;
        [TSOVoltronBroadcastDatablobPDUField] public uint RefPack_Header_Size { get; set; }
        [TSOVoltronBroadcastDatablobPDUField] public uint SimEvent_RefPack_Header_Arg1 { get; set; } = 0xE980EE20;
        [TSOVoltronBroadcastDatablobPDUField] public uint SimEvent_RefPack_Header_Arg2 { get; set; } = 0x7B6F2221;
        [TSOVoltronBroadcastDatablobPDUField] public uint SimEvent_RefPack_Header_Arg3 { get; set; } = 0x00000000;

        //**Start RefPack**

        [TSOVoltronBroadcastDatablobPDUField] public byte RefPack_Byte { get; set; }
    }
}