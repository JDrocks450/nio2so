using nio2so.DataService.API.Databases.Libraries;
using nio2so.DataService.Common.Tokens;

namespace nio2so.DataService.API.Databases
{
    internal class LotsDataService : DataServiceBase
    {
        public LotsDataService() : base()
        {

        }

        protected override void AddLibraries()
        {
            ServerSettings settings = ServerSettings.Current;
            Libraries.Add("THUMBNAILS", new FileObjectLibrary(Path.Combine(settings.DatabaseDirectory, "thumbs"),
                "thumb", "png", GetDefaultThumbnail));
        }

        Task<byte[]> GetDefaultThumbnail() => File.ReadAllBytesAsync(ServerSettings.Current.DefaultThumbnailPath);

        /// <summary>
        /// Returns the PNG Image for the given <see cref="HouseIDToken"/> in binary
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public Task<byte[]> GetThumbnailByHouseID(HouseIDToken HouseID) => GetLibrary<FileObjectLibrary>("THUMBNAILS").GetDataByID(HouseID);
        /// <summary>
        /// Sets the PNG Image for the given <see cref="HouseIDToken"/> in binary
        /// </summary>
        /// <param name="HouseID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public Task SetThumbnailByHouseID(HouseIDToken HouseID, byte[] PNGStream, bool overwrite = true) =>
            GetLibrary<FileObjectLibrary>("THUMBNAILS").SetDataByIDToDisk(HouseID, PNGStream, overwrite);
    }
}
