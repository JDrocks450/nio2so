using nio2so.DataService.Common.Types.Top100;
using nio2so.Formats.Img.BMP;
using nio2so.Voltron.Core.TSO;
using nio2so.Voltron.PreAlpha.Protocol.PDU;
using nio2so.Voltron.PreAlpha.Protocol.PDU.DBWrappers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
        private ConcurrentDictionary<string, byte[]> _resourceCache = new ConcurrentDictionary<string, byte[]>();        

        [TSOProtocolDatabaseHandler((uint)TSO_PreAlpha_DBActionCLSIDs.GetTopList_Request)]
        public void GET_TOP_LIST_REQUEST(TSODBRequestWrapper DBPDU)
        {
            //trigger exp to find spot to breakpoint in the disassembly
            //RespondTo(DBPDU, new TSODebugWrapperPDU(new byte[0], TSO_PreAlpha_DBActionCLSIDs.GetTopList_Response));
            //return;            

            //get all remote top 100 lists from the nio2so data service
            if (!TryDataServiceQuery(x => x.GetTop100Lists(), out IEnumerable<Top100ListInfo>? dbLists, out string ErrorMsg))
            {
                LogError(ErrorMsg);
                return;
            }

            if (dbLists == null || dbLists.Count() == 0)
            {
                LogError("Data Service returned null or no entries for Top 100 lists query.");
                //Respond with empty response
                RespondTo(DBPDU, new TSOGetTopListResponse());
                return;
            }

            //convert remote format top 100 list schema into TSO PA format
            TSOGetTopListResponse.TSOTop100List[] tsoPAListFormat = new TSOGetTopListResponse.TSOTop100List[dbLists.Count()];

            for(int i = 0; i < tsoPAListFormat.Length; i++)
            {
                var dataSource = dbLists.ElementAt(i);

                byte[] iconBytes = new byte[0];
                string path = dataSource.IconResourceURI;

                if (File.Exists(path))
                { // convert user image to BMP RLE8 INDEXED
                    try
                    {
                        iconBytes = TO_RLE8(path);
                    }
                    catch (Exception error)
                    {
                        LogError(error);
                    }
                }

                tsoPAListFormat[i] = new TSOGetTopListResponse.TSOTop100List(dataSource.ListID,
                    Enum.Parse<TSOGetTopListResponse.TSOTop100ListTypes>(dataSource.ListType),
                    dataSource.ListName, iconBytes);
            }

            //flush the cache
            FLUSH_CACHE();

            //Respond with top 100 listswhat
            RespondTo(DBPDU, new TSOGetTopListResponse(tsoPAListFormat));
        }

        [TSOProtocolDatabaseHandler((uint)TSO_PreAlpha_DBActionCLSIDs.GetTopResultSetByID_Request)]
        public void GET_TOP_RESULT_SET_BY_ID_REQUEST(TSODBRequestWrapper DBPDU)
        {
            if (DBPDU is not TSOGetTop100ListSetByIDRequest ListSetRequest)
            {
                LogError($"Received {nameof(TSODBRequestWrapper)} {DBPDU} but it was not the expected {nameof(TSOGetTop100ListSetByIDRequest)} type.");
                return;
            }

            RespondTo(DBPDU, new TSOGetTopResultSetByIDResponse(ListSetRequest.ListID, (uint)TSOGetTopListResponse.TSOTop100ListTypes.Houses,
                new TSOGetTopResultSetByIDResponse.TSOTopListResultStruct(1,6094983,"First House"),
                new TSOGetTopResultSetByIDResponse.TSOTopListResultStruct(2,161, "Friend"),
                new TSOGetTopResultSetByIDResponse.TSOTopListResultStruct(3,1337, "Bisquick")));
        }

        private byte[] TO_RLE8(string ResourceURI)
        {
            if (_resourceCache.TryGetValue(ResourceURI, out var resource))
                return resource;
            using (Bitmap bmp = (Bitmap)Image.FromFile(ResourceURI))
            {
                var destinationFormat = PixelFormat.Format8bppIndexed;
                LogConsole($"Starting conversion program ({bmp.PixelFormat} -> {destinationFormat} as {nameof(RLE8Bitmap)} compression)");
                using (Bitmap top100listIcon = bmp.Clone(new Rectangle(0, 0, Math.Min(96, bmp.Width), Math.Min(23, bmp.Height)), destinationFormat))
                {
                    byte[] encodedBytes = RLE8Bitmap.RunLengthEncodeBitmap(top100listIcon);
                    _resourceCache.TryAdd(ResourceURI, encodedBytes);
                    return encodedBytes;
                }
            }
        }

        private void FLUSH_CACHE()
        {
            _resourceCache.Clear();
        }
    }
}
