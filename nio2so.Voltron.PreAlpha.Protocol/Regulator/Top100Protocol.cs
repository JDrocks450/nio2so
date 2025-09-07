using nio2so.Formats.Img.BMP;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.PreAlpha.Protocol.PDU;
using nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.Voltron.PreAlpha.Protocol.Regulator
{
    /// <summary>
    /// Handles incoming requests related to the Avatars and their DB operations
    /// </summary>
    [TSORegulator("Top 100 Protocol")]
    internal class Top100Protocol : TSOProtocol
    {
        [TSOProtocolDatabaseHandler((uint)TSO_PreAlpha_DBActionCLSIDs.GetTopList_Request)]
        public void GET_TOP_LIST_REQUEST(TSODBRequestWrapper DBPDU)
        {
            //trigger exp to find spot to breakpoint in the disassembly
            //RespondTo(DBPDU, new TSODebugWrapperPDU(new byte[0], TSO_PreAlpha_DBActionCLSIDs.GetTopList_Response));
            //return;

            string path = @"C:\nio2so\const\top100_1.bmp";
            byte[] iconBytes = new byte[0];

            if (File.Exists(path))
            { // convert user image to BMP RLE8 INDEXED
                try
                {
                    using (Bitmap bmp = (Bitmap)Image.FromFile(path))
                    {
                        var destinationFormat = PixelFormat.Format8bppIndexed;
                        LogConsole($"Starting conversion program ({bmp.PixelFormat} -> {destinationFormat} as {nameof(RLE8Bitmap)} compression)");
                        using (Bitmap top100listIcon = bmp.Clone(new Rectangle(0, 0, Math.Min(96, bmp.Width), Math.Min(23, bmp.Height)), destinationFormat))
                            iconBytes = RLE8Bitmap.RunLengthEncodeBitmap(top100listIcon);
                    }
                }
                catch (Exception error)
                {
                    LogError(error);
                }
            }

            var list = new TSOGetTopListResponse.TSOTop100List(0x03EA, 0x0001, "Bisquick's Top Picks", iconBytes);
            var list2 = new TSOGetTopListResponse.TSOTop100List(0x001C, 0x0003, "Splash Zone", iconBytes);
            var list3 = new TSOGetTopListResponse.TSOTop100List(0x001B, 0x0003, "questionable sandwich", iconBytes);
            //respond with test pdu
            RespondTo(DBPDU, new TSOGetTopListResponse(list,list2,list3));
            return;
            RespondTo(DBPDU, TSODebugWrapperPDU.FromFile(@"C:\nio2so\const\gettop100list.dat", TSO_PreAlpha_DBActionCLSIDs.GetTopList_Response));
        }
    }
}
