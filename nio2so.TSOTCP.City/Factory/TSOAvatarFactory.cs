using nio2so.Formats.DB;
using nio2so.TSOTCP.City.TSO.Voltron;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nio2so.TSOTCP.City.Factory
{
    /// <summary>
    /// Creates <see cref="TSODBCharBlob"/> and <see cref="TSODBChar"/> data types and saves them to disk
    /// </summary>
    [TSOFactory]
    internal class TSOAvatarFactory : TSOFactoryBase
    {
        protected override string MY_DIR => TSOVoltronConst.AvatarDataDirectory;
        protected override string MY_ITEMNAME => "avatar";
        protected override string MY_EXT => ".charblob";
        const string CharExt = ".char";

        public TSOAvatarFactory() : base()
        {

        }

        protected override byte[] OnFileNotFound() => File.ReadAllBytes(Path.Combine(TSOVoltronConst.WorkspaceDirectory,
            "const", "default_charblob.charblob"));

        public TSODBCharBlob GetCharBlobByID(uint AvatarID)
        {
            TSODBCharBlob charblob = new(GetDataByID(AvatarID));
            charblob.EnsureNoErrors();
            return charblob;
        }
        public void SetCharBlobByIDToDisk(uint AvatarID, TSODBCharBlob CharBlob) => SetDataByIDToDisk(AvatarID, CharBlob.BlobData);
        public TSODBChar GetCharByID(uint AvatarID) => new(GetDataByID(AvatarID, CharExt));        
        public void SetCharByIDToDisk(uint AvatarID, TSODBChar CharBlob) => SetDataByIDToDisk(AvatarID, CharBlob.BlobData, true, CharExt);
    }
}
