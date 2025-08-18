using nio2so.Data.Common.Serialization.Voltron;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{
    /// <summary>
    /// Sends a log to the remote server console log
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU((uint)TSO_PreAlpha_DBActionCLSIDs.InsertGenericLog_Request)]
    public class TSOInsertGenericLogRequest : TSODBRequestWrapper
    {
        /// <summary>
        /// Not sure needs more testing
        /// </summary>
        [TSOVoltronDBWrapperField] public uint LogType { get; set; }
        [TSOVoltronDBWrapperField] public uint Arg1 { get; set; }
        [TSOVoltronDBWrapperField] public uint Arg2 { get; set; }
        /// <summary>
        /// The game has multiple areas to log information to the console.
        /// <para/> This dictates the "probe" that is sending the telemetry data
        /// <para>See: <seealso cref="TSO_PreAlpha_GZPROBE"/> for guidance on Probe types</para>
        /// </summary>
        [TSOVoltronDBWrapperField] public uint ProbeCLSID { get; set; } = (uint)TSO_PreAlpha_DBStructCLSIDs.GZPROBEID_cEAS;

        //**EVERYTHING AFTER HERE MAY BE PROBE-TYPE SPECIFIC!!!
        //**THIS IS ONLY BASED OF EAS PROBE!!

        [TSOVoltronDBWrapperField] public uint Arg3 { get; set; }
        [TSOVoltronDBWrapperField] public uint Arg4 { get; set; }
        /// <summary>
        /// The text to log
        /// </summary>
        [TSOVoltronDBWrapperField][TSOVoltronString(TSOVoltronValueTypes.Length_Prefixed_Byte)] public string ConsoleLog { get; set; } = "ERROR NO TEXT";

        public TSOInsertGenericLogRequest() : base() { }
    }
}
