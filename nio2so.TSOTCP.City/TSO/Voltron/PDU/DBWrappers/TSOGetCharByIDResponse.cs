using nio2so.Formats.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    /// <summary>
    /// The response PDU to a <see cref="TSOGetCharByIDRequest"/> PDU
    /// <para/>It contains extremely surface-level info about an Avatar, like their Name and Description
    /// </summary>
    [TSOVoltronDBRequestWrapperPDU(TSO_PreAlpha_DBActionCLSIDs.GetCharByID_Response)]
    internal class TSOGetCharByIDResponse : TSODBRequestWrapper
    {
        static byte[] reference_material = new byte[]
        {
            #region DATA
            0x00,0x00,0x05,0x39, // <-- avatar id?
            0xBA,0xAD,0xF0,0x0D,
            0x08,0x4A,0x6F,0x6C,0x6C,0x79,0x53,0x69,0x6D, /* Name: JollySim */
		    0x01,0x42, /* Description: B */
            #endregion
        };
        static byte[] niotso_research_data = new byte[] {
            #region DATA            
		    0x00,0x01,0x02,0x03,
            0x00,0x00,0x05,0x3A,
            0x04,0x05,0x06,0x07,
            0x08,0x09,0x0A,0x0B,
            0x0C,0x0D,0x0E,0x0F,
            0x10,0x11,0x12,0x13,
            0x14,0x15,0x16,0x17,
            0x01,0x43,
            0x01,0x44,
            0x18,0x19,0x1A,0x1B,
            0x1C,0x1D,0x1E,0x1F,
            0x20,0x21,0x22,0x23,
            0x24,0x25,0x26,0x27,
            0x28,0x29,0x2A,0x2B,
            0x2C,0x2D,0x2E,0x2F,
            0x30,0x31,0x32,0x33,
            0x34,0x35,0x36,0x37,
            0x38,0x39,0x3A,0x3B,
            0x3C,0x3D,0x3E,0x3F,
            0x40,0x41,0x42,0x43,
            0x44,0x45,0x46,0x47,
            0x48,0x49,0x4A,0x4B,
            0x4C,0x4D,0x4E,0x4F,
            0x50,0x51,0x52,0x53,
            0x54,0x55,0x56,0x57,
            0x58,0x59,0x5A,0x5B,
            0x5C,0x5D,0x5E,0x5F,
            0x60,0x61,0x62,0x63,
            0x64,0x65,0x66,0x67,
            0x68,0x69,0x6A,0x6B
            #endregion
        };
        /// <summary>
        /// The Avatar this packet is in reference to
        /// </summary>
        [TSOVoltronDBWrapperField] public uint AvatarID { get; set; }
        /// <summary>
        /// Not sure what this is used for yet, probably 0x00000000 most of the time?
        /// </summary>
        [TSOVoltronDBWrapperField] public uint Filler { get; set; } = 0xBAADF00D;
        /// <summary>
        /// The <see cref="TSODBChar"/> data to send, in bytes
        /// </summary>
        [TSOVoltronDBWrapperField] [TSOVoltronBodyArray] public byte[] CharDataBytes { get; set; }                

        /// <summary>
        /// Creates a new <see cref="TSOGetCharByIDResponse"/> packet with the provided <see cref="TSODBChar"/>
        /// </summary>
        /// <param name="AriesID"></param>
        /// <param name="MasterID"></param>
        public TSOGetCharByIDResponse(uint AvatarID, TSODBChar CharData) :
            base(
                    TSO_PreAlpha_DBStructCLSIDs.cCrDMStandardMessage,
                    TSO_PreAlpha_kMSGs.kDBServiceResponseMsg,
                    TSO_PreAlpha_DBActionCLSIDs.GetCharByID_Response
                )
        {
            this.AvatarID = AvatarID;
            CharDataBytes = CharData.BlobData;

            MakeBodyFromProperties();                                           
        }
    }
}
