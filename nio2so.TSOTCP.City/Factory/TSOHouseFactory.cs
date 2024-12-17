using nio2so.Data.Common.Testing;
using nio2so.Formats.DB;
using nio2so.Formats.Util.Endian;
using nio2so.TSOTCP.City.TSO.Voltron;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.Factory
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
            uint NewHouseID = TestingConstraints.BuyLotID;
            byte[] data = OnFileNotFound();
            SetHouseBlobByIDToDisk(NewHouseID, data);
            return NewHouseID;
        }

        public byte[] GetHouseBlobByID(uint HouseID) => GetDataByID(HouseID);

        /// <summary>
        /// Writes the <see cref="TSODBHouseBlob"/> to the disk at <see cref="HOUSE_DIR"/>
        /// </summary>
        /// <param name="houseID"></param>
        /// <param name="houseBlob"></param>
        public void SetHouseBlobByIDToDisk(uint houseID, byte[] houseBlobBytes) => SetDataByIDToDisk(houseID, houseBlobBytes, false);    
    }
}
