using System.Runtime.Serialization;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.TSOTCP.Voltron.Protocol.TSO.Voltron.Struct
{
    /// <summary>
    /// This is a structure consisting of an AriesID and MasterID </para>
    /// </summary>
    [Serializable]
    public record TSOAriesIDStruct : ITSONumeralStringStruct
    {
        public const string DEFAULT_HEADER = "A ";

        private string _header = DEFAULT_HEADER;

        [TSOVoltronString(Data.Common.Serialization.Voltron.TSOVoltronValueTypes.Pascal)]
        public string AriesID { get; set; } = "";
        [TSOVoltronString(Data.Common.Serialization.Voltron.TSOVoltronValueTypes.Pascal)]
        public string MasterID { get; set; } = "";        

        public TSOAriesIDStruct() { }
        /// <summary>
        /// Creates a new <see cref="TSOAriesIDStruct"/> using the <see cref="FormatIDString(uint, string)"/> function to create an AriesID for you by an ID
        /// </summary>
        /// <param name="AriesID"></param>
        /// <param name="MasterID"></param>
        public TSOAriesIDStruct(uint AriesID, string MasterID, string FormatSpecifier = DEFAULT_HEADER) : this()
        {
            _header = FormatSpecifier;
            this.AriesID = ITSONumeralStringStruct.FormatIDString(AriesID);
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

        /// <summary>
        /// Gets a blank <see cref="TSOAriesIDStruct"/> object with blank IDs
        /// </summary>
        [IgnoreDataMember]
        [TSOVoltronIgnorable]
        internal static TSOAriesIDStruct Default => new TSOAriesIDStruct() { AriesID = "", MasterID = "" };

        string ITSONumeralStringStruct.FormatSpecifier => _header;
        string ITSONumeralStringStruct.IDString { get => AriesID; set => AriesID = value; }
        string ITSONumeralStringStruct.NameString { get => MasterID; set => MasterID = value; }
    }
}
