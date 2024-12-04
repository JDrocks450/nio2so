using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    internal class TSODebugWrapperPDU : TSODBRequestWrapper
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

        public static TSODebugWrapperPDU FromFile(string FName, TSO_PreAlpha_DBActionCLSIDs Action,
            TSO_PreAlpha_kMSGs kMSG = TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
            TSO_PreAlpha_DBStructCLSIDs Struct = TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage
            )
        {
            var bytes = File.ReadAllBytes( FName );
            return new TSODebugWrapperPDU(bytes, Action, kMSG, Struct);
        }
    }
}
