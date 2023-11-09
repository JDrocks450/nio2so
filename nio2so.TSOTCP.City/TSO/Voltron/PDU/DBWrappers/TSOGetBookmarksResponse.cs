using nio2so.TSOTCP.City.TSO.Voltron.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.TSO.Voltron.PDU.DBWrappers
{
    internal class TSODBWrapperMessageSize
    {
        public TSODBWrapperMessageSize(uint Size) => this.Size = Size;
        /// <summary>
        /// Use this in the <see cref="TSODBRequestWrapper"/> constructor for MessageSize to have
        /// the packet autosize using the <see cref="TSODBRequestWrapper.DBMessageBody"/> property
        /// </summary>
        public static TSODBWrapperMessageSize AutoSize => 0xFFFFFFFF;
        public bool IsAutoSize => Size == 0xFFFFFFFF;
        public uint Size { get; set; } = 0xFFFFFFFF;

        public static implicit operator TSODBWrapperMessageSize(uint Other) => new TSODBWrapperMessageSize(Other);
        public static implicit operator uint(TSODBWrapperMessageSize Other) => Other.Size;
    }
    /// <summary>
    /// The response packet structure to <see cref="TSO_PreAlpha_DBActionCLSIDs.GetBookmarksQuery"/>
    /// </summary>
    internal class TSOGetBookmarksResponse : TSODBRequestWrapper
    {
        /// <summary>
        /// Makes a default response packet using the supplied parameters.
        /// </summary>
        /// <param name="AriesID"></param>
        /// <param name="MasterID"></param>
        public TSOGetBookmarksResponse(string AriesID, string MasterID, uint TransactionID) :
            base(AriesID,
                MasterID,
                0x0000,
                0x00000041,
                (uint)TSO_PreAlpha_DBStructCLSIDs.cTSONetMessageStandard,
                0x21,
                TransactionID,
                (uint)TSO_PreAlpha_DBActionCLSIDs.GetBookmarksResponse,
                new byte[]
                {
                    // ** Emplace this data **
                    0x00,0x00,0x05,0x3A,
                    0x00,0x00,0x00,0x01,
                    0x00,0x00,0x00,0xA1,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    0x21,0x22,0x23,0x24,
                    // ** Then use the API to fill the rest with garbage
                    // ** Please note, see: TSODBWrapperPDU.FillAvailableSpace()
                })
        {

        }
    }
}
