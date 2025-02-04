﻿using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    public class TSODebugWrapperPDU : TSODBRequestWrapper
    {
        [TSOVoltronDBWrapperField]
        [TSOVoltronBodyArray]
        public byte[] PDUBytes { get; set; }

        public TSODebugWrapperPDU(byte[] BodyArray, TSO_PreAlpha_DBActionCLSIDs Action,
            TSO_PreAlpha_kMSGs kMSG = TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
            TSO_PreAlpha_DBStructCLSIDs Struct = TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage
            )
            : base(Struct, kMSG, Action)
        {
            PDUBytes = BodyArray;
            MakeBodyFromProperties();
        }
        public TSODebugWrapperPDU(byte[] BodyArray, TSO_PreAlpha_DBActionCLSIDs Action,
            uint kMSG = (uint)TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
            TSO_PreAlpha_DBStructCLSIDs Struct = TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage
            )
            : this(BodyArray, Action, (TSO_PreAlpha_kMSGs)kMSG, Struct) { }

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
