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
        /// <summary>
        /// Creates a new <see cref="TSOAriesIDStruct"/> using the <see cref="FormatIDString(uint, string)"/> function to create an AriesID for you by an ID
        /// </summary>
        /// <param name="AriesID"></param>
        /// <param name="MasterID"></param>
        public TSOAriesIDStruct(uint AriesID, string MasterID) : this()
        {
            this.AriesID = FormatIDString(AriesID);
            this.MasterID = MasterID;
        }
        /// <summary>
        /// Creates a new <see cref="TSOAriesIDStruct"/>
        /// </summary>
        /// <param name="AriesID"></param>
        /// <param name="MasterID"></param>
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
        /// <summary>
        /// Formats a string into the ID found in a <see cref="TSOAriesIDStruct"/>
        /// <para/>Example: <c>A 1338</c>        
        /// </summary>
        /// <param name="ID">Example: <c>A 1338</c>  </param>
        /// <param name="Header"></param>
        /// <returns></returns>
        public static string FormatIDString(uint ID, string Header = "A ") => Header + ID;
    }
}
