using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers
{
    /// <summary>
    /// A blank <see cref="TSODBRequestWrapper"/> PDU structure that simply takes a <see cref="byte"/>[] as a payload
    /// </summary>
    public class TSODebugWrapperPDU : TSODBRequestWrapper
    {
        /// <summary>
        /// The payload of this PDU
        /// </summary>
        [TSOVoltronDBWrapperField]
        [TSOVoltronBodyArray]
        public byte[] PDUBytes { get; set; }

        /// <summary>
        /// Creates a new <see cref="TSODebugWrapperPDU"/> with a <paramref name="BodyArray"/> acting as the data following the <see cref="TSODBRequestWrapper"/> header structure
        /// </summary>
        /// <param name="BodyArray">See: <see cref="PDUBytes"/></param>
        /// <param name="Action">See: <see cref="TSODBRequestWrapper.TSOPacketFormatCLSID"/></param>
        /// <param name="kMSG">See: <see cref="TSODBRequestWrapper.kMSGID"/></param>
        /// <param name="Struct">See: <see cref="TSODBRequestWrapper.TSOSubMsgCLSID"/></param>
        public TSODebugWrapperPDU(byte[] BodyArray, TSO_PreAlpha_DBActionCLSIDs Action,
            TSO_PreAlpha_kMSGs kMSG = TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
            TSO_PreAlpha_DBStructCLSIDs Struct = TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage
            )
            : base(Struct, kMSG, Action)
        {
            PDUBytes = BodyArray;
            MakeBodyFromProperties();
        }
        /// <summary>
        /// Creates a new <see cref="TSODebugWrapperPDU"/> with a <paramref name="BodyArray"/> acting as the data following the <see cref="TSODBRequestWrapper"/> header structure
        /// </summary>
        /// <param name="BodyArray">See: <see cref="PDUBytes"/></param>
        /// <param name="Action">See: <see cref="TSODBRequestWrapper.TSOPacketFormatCLSID"/></param>
        /// <param name="kMSG">See: <see cref="TSODBRequestWrapper.kMSGID"/></param>
        /// <param name="Struct">See: <see cref="TSODBRequestWrapper.TSOSubMsgCLSID"/></param>
        public TSODebugWrapperPDU(byte[] BodyArray, TSO_PreAlpha_DBActionCLSIDs Action,
            uint kMSG = (uint)TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
            TSO_PreAlpha_DBStructCLSIDs Struct = TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage
            )
            : this(BodyArray, Action, (TSO_PreAlpha_kMSGs)kMSG, Struct) { }
        /// <summary>
        /// Creates a new <see cref="TSODebugWrapperPDU"/> with a Body Array acting as the data following the <see cref="TSODBRequestWrapper"/> header structure
        /// </summary>
        /// <param name="BodyArray">Reads a file into the <see cref="PDUBytes"/> property</param>
        /// <param name="Action">See: <see cref="TSODBRequestWrapper.TSOPacketFormatCLSID"/></param>
        /// <param name="kMSG">See: <see cref="TSODBRequestWrapper.kMSGID"/></param>
        /// <param name="Struct">See: <see cref="TSODBRequestWrapper.TSOSubMsgCLSID"/></param>
        public static TSODebugWrapperPDU FromFile(string FName, TSO_PreAlpha_DBActionCLSIDs Action,
            TSO_PreAlpha_kMSGs kMSG = TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
            TSO_PreAlpha_DBStructCLSIDs Struct = TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage
            )
        {
            var bytes = File.ReadAllBytes(FName);
            return new TSODebugWrapperPDU(bytes, Action, kMSG, Struct);
        }
    }
}
