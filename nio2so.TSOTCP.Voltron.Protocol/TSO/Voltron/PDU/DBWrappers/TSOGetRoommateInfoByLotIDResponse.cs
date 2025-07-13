using QuazarAPI.Networking.Data;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// The response packet structure to <see cref="TSO_PreAlpha_DBActionCLSIDs.GetRoommateInfoByLotID_Request"/>
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetRoommateInfoByLotID_Response)]
    public class TSOGetRoommateInfoByLotIDResponse : TSODBRequestWrapper
    {
        /// <summary>
        /// A byte array for the body of the DBRequestWrapper that seems to work?
        /// </summary>
        private byte[] DBBody_research = new byte[]
        {
            // ** Emplace this data **
            0x00,0x00,0x05,0x3A, //<---HOUSEID
            0x00,0x00,0x00,0x01,
            0x00,0x00,0x00,0xA1
            // ** Then use the API to fill the rest with garbage
            // ** Please note, see: TSODBWrapperPDU.FillAvailableSpace()
        };
        /// <summary>
        /// Represents a roommate in the <see cref="TSOGetRoommateInfoByLotIDResponse"/>
        /// </summary>
        public record TSORoommate
        {
            public TSORoommate()
            {
            }

            public TSORoommate(uint avatarID) : this()
            {
                AvatarID = avatarID;
            }

            public uint AvatarID { get; set; }
        }

        /// <summary>
        /// The ID of the House we're getting information on
        /// </summary>
        [TSOVoltronDBWrapperField] public uint HouseID { get; set; } = 0x0;
        [TSOVoltronDBWrapperField][TSOVoltronArrayLength(nameof(Roommates))] public uint NumberOfRoommates { get; set; } = 0x01;
        [TSOVoltronDBWrapperField] public TSORoommate[] Roommates { get; set; } = new TSORoommate[0];

        /// <summary>
        /// Default parameterless constructor. Please use overload for programmatically creating PDUs.
        /// </summary>
        public TSOGetRoommateInfoByLotIDResponse() : base(
                    TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetRoommateInfoByLotID_Response
                ) { }        

        /// <summary>
        /// Creates a <see cref="TSOGetRoommateInfoByLotIDResponse"/> using the supplied parameters.
        /// </summary>
        /// <param name="AriesID"></param>
        /// <param name="MasterID"></param>
        public TSOGetRoommateInfoByLotIDResponse(uint HouseID, params uint[] Roommates) : this()
        {
            this.HouseID = HouseID;
            this.Roommates = Roommates.Select(x => new TSORoommate(x)).ToArray();            
            MakeBodyFromProperties();
        }
    }
}
