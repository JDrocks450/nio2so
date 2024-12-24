using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.City.TSO.Voltron.Struct
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
        public TSOAriesIDStruct(string ariesID, string masterID) : this()
        {
            AriesID = ariesID;
            MasterID = masterID;
        }
    }
}
