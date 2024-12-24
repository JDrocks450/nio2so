using nio2so.Formats.Streams;
using nio2so.TSOTCP.City.TSO.Voltron.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.Datablob.Structures
{
    /// <summary>
    /// This is found when the <see cref="TSO_PreAlpha_MasterConstantsTable.GZIID_cITSOSimEvent"/> is received by the server
    /// <para/>Seems to be a wrapper for Simulator Events
    /// </summary>
    [TSOVoltronDatablobContent(TSO_PreAlpha_MasterConstantsTable.GZCLSID_cTSOSimEvent)]
    internal class TSOSimEventContent : ITSODataBlobContentObject, ITSOSerializableStreamPDU
    {
        /// <summary>
        /// The first parameter of the time struct, still unknown
        /// </summary>
        [TSOVoltronBroadcastDatablobPDUField] public uint Time_Ticks { get; set; } = 0x00034D45;
        /// <summary>
        /// The second parameter to the time struct, still unknown ((COULD ALSO BE SIZE)))
        /// </summary>
        [TSOVoltronBroadcastDatablobPDUField] public uint Time_Index { get; set; }
        /// <summary>
        /// The kMSG corresponding to what the simulator is notifying the room about
        /// </summary>
        [TSOVoltronBroadcastDatablobPDUField] public TSO_PreAlpha_MasterConstantsTable Simulator_kMSG { get; set; }
        /// <summary>
        /// Generally, if this is seen as a Response or a Request
        /// </summary>
        [TSOVoltronBroadcastDatablobPDUField] public TSO_PreAlpha_kMSGs Simulator_RequestType { get; set; }
        [TSOVoltronBroadcastDatablobPDUField] public uint SimEvent_RefPack_Header_Arg3 { get; set; } = 0x00000000;

        //**TSOSerializableStream**

        [TSOVoltronBroadcastDatablobPDUField] public TSOSerializableStream RefPackDataStream { get; set; }
        TSOSerializableStream ITSOSerializableStreamPDU.GetStream() => RefPackDataStream;

        public TSOSimEventContent() { }

        public TSOSimEventContent(TSO_PreAlpha_MasterConstantsTable CLSID,
            TSO_PreAlpha_MasterConstantsTable SimulatorkMSG, TSO_PreAlpha_kMSGs SimMessageType)
        {
            Simulator_RequestType = SimMessageType;
            Simulator_kMSG = SimulatorkMSG;
        }
    }
}
