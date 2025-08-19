using nio2so.Data.Common.Testing;
using nio2so.DataService.Common.Types.Avatar;
using nio2so.DataService.Common.Types.Lot;
using nio2so.Voltron.Core;
using nio2so.Voltron.Core.Telemetry;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.PreAlpha.Protocol.PDU;
using nio2so.Voltron.PreAlpha.Protocol.PDU.Datablob.Structures;

namespace nio2so.Voltron.PreAlpha.Protocol
{    
    public class TSOPreAlphaLoggerService : TSOLoggerServiceBase
    {
        public TSOPreAlphaLoggerService(string SysLogPath = default) : base(SysLogPath) { }

        public override void OnVoltronPacket(NetworkTrafficDirections Direction, DateTime Time, TSOVoltronPacket PDU, uint? ClientID = null)
        {
            //**Auto-Redirect**
            if (PDU is TSODBRequestWrapper dbWrapper)
            {
                OnVoltron_DBWrapperPDU(Direction, Time, dbWrapper, ClientID);
                return;
            }
            //filter out dumb stuff
            if (!TestingConstraints.VerboseLogging)
            {
                if (PDU is TSOPreAlphaSplitBufferPDU)
                    return; // skip these
                if (PDU is ITSODataBlobPDU standardMsg &&
                    standardMsg.DataBlobContentObject.GetAs<TSOStandardMessageContent>().
                    Match(TSO_PreAlpha_MasterConstantsTable.kServerTickConfirmationMsg))
                    return; // this is a server confirmation message, they spam. Do not log these.
            }
            base.OnVoltronPacket(Direction, Time, PDU, ClientID);
        }

        public void OnVoltron_DBWrapperPDU(NetworkTrafficDirections Direction, DateTime Time, TSODBRequestWrapper PDU, uint? ClientID = null)
        {
            Console.ForegroundColor = Direction switch
            {
                NetworkTrafficDirections.INBOUND => ConsoleColor.Green,
                NetworkTrafficDirections.OUTBOUND => ConsoleColor.Cyan,
                _ => ConsoleColor.Cyan
            };
            Log($"{Time.ToLongTimeString()} - *VOLTRON_DATABASE* [{Direction}] {PDU.ToString()}");

            //**LOG PDU TO DISK
            if (Direction == NetworkTrafficDirections.INBOUND || Direction == NetworkTrafficDirections.OUTBOUND)
                PDU.WritePDUToDisk(Direction == NetworkTrafficDirections.INBOUND);
        }    

        public void OnHouseBlob(NetworkTrafficDirections Direction, uint HouseID, TSODBHouseBlob HouseBlob) => OnBlobBase(Direction, HouseID, "House File Stream");
        public void OnCharBlob(NetworkTrafficDirections Direction, uint AvatarID, TSODBCharBlob charBlob) => OnBlobBase(Direction, AvatarID, "Character File Stream");
        private void OnBlobBase(NetworkTrafficDirections Direction, uint ID, string Type)
        {
            Console.ForegroundColor = Direction switch
            {
                NetworkTrafficDirections.INBOUND => ConsoleColor.DarkCyan,
                NetworkTrafficDirections.OUTBOUND => ConsoleColor.DarkCyan,
                _ => ConsoleColor.Cyan
            };
            Log($"{DateTime.Now.ToLongTimeString()} - *{Type}* [{Direction}] ObjectID: {ID}");
        }
        public void OnCharData(NetworkTrafficDirections Direction, uint AvatarID, TSODBChar CharData)
        {
            OnBlobBase(Direction, AvatarID, "Character Profile");
        }
    }
}
