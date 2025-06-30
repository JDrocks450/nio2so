using System.Runtime.Serialization;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct
{
    /// <summary>
    /// This is the structure used in Packets to track where the packet is coming (or going)
    /// <para>It consists of an AriesID and MasterID</para>
    /// </summary>
    [Serializable]
    public class TSOAriesIDStruct
    {
        [TSOVoltronString(Data.Common.Serialization.Voltron.TSOVoltronValueTypes.Pascal)]
        public string AriesID { get; set; }
        [TSOVoltronString(Data.Common.Serialization.Voltron.TSOVoltronValueTypes.Pascal)]
        public string MasterID { get; set; }        

        public TSOAriesIDStruct() { }
        public TSOAriesIDStruct(uint AriesID, string MasterID) : this()
        {
            this.AriesID = $"??{AriesID}";
            this.MasterID = MasterID;
        }
        public TSOAriesIDStruct(string AriesID, string MasterID) : this()
        {
            this.AriesID = AriesID;
            this.MasterID = MasterID;
        }

        public override string ToString()
        {
            return $"{AriesID} {MasterID}";
        }

        /// <summary>
        /// Gets a blank <see cref="TSOAriesIDStruct"/> object with blank IDs
        /// </summary>
        [IgnoreDataMember]
        internal static TSOAriesIDStruct Default => new TSOAriesIDStruct() { AriesID = "", MasterID = "" };
    }
}
