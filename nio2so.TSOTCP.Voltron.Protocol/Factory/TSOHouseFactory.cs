using nio2so.Data.Common.Testing;
using nio2so.DataService.Common.Types.Lot;
using nio2so.Formats.DB;
using nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron;

namespace nio2so.TSOTCP.Voltron.Protocol.Factory
{
    /// <summary>
    /// Serves data in relation to House requests
    /// </summary>
    [TSOFactory]
    internal class TSOHouseFactory : TSOFactoryBase
    {
        const string HOUSE_DIR = TSOVoltronConst.HouseDataDirectory;
        protected override string MY_DIR => HOUSE_DIR;
        protected override string MY_ITEMNAME => "house";
        protected override string MY_EXT => ".houseblob";

        public TSOHouseFactory() : base()
        {

        }

        public string GetHouseURI(uint HouseID) => GetObjectURI(HouseID);

        protected override byte[] OnFileNotFound() => File.ReadAllBytes(Path.Combine(TSOVoltronConst.WorkspaceDirectory,
            "const", "default_house.dat")).Skip(0xD).ToArray();

        public uint Create()
        {
            uint NewHouseID = TestingConstraints.MyHouseLotID;
            byte[] data = OnFileNotFound();
            SetHouseBlobByIDToDisk(NewHouseID, new(data));
            return NewHouseID;
        }

        public TSODBHouseBlob GetHouseBlobByID(uint HouseID) => GetDataObjectByID<TSODBHouseBlob>(HouseID);

        /// <summary>
        /// Writes the <see cref="TSODBHouseBlob"/> to the disk at <see cref="HOUSE_DIR"/>
        /// </summary>
        /// <param name="houseID"></param>
        /// <param name="houseBlob"></param>
        public void SetHouseBlobByIDToDisk(uint houseID, TSODBHouseBlob HouseBlob) => SetDataObjectByIDToDisk(houseID, HouseBlob, false);
    }
}
