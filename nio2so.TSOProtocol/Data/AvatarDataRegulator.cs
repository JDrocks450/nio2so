using nio2so.Data.Common.Testing;

namespace nio2so.TSOHTTPS.Protocol.Data
{
    internal class AvatarDataRegulator : TSOHTTPRegulator<uint, AvatarData>
    {
        protected override string FileName => TestingConstraints.AvatarDataDictionaryFileName;

        public AvatarData? GetByID(uint AvatarID)
        {
            if (!TryGetValue(AvatarID, out AvatarData Data))
                // No data found for avatar
                ; // disabled for now while this is being worked on          
            //**This should really be delegated to a separate server that handles DB requests but scale right now
            //doesn't justify it, so use the disk on the host PC
            if (!System.IO.File.Exists(@$"E:\packets\avatar\avatar{AvatarID}.charblob"))
                return null;
            return Data;
        }
    }
}
