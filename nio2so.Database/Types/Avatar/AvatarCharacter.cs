using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.DataService.Common.Types.Avatar
{
    /// <summary>
    /// Basic information about an avatar, such as Name, Description and Money
    /// </summary>
    public record TSODBChar
    {
        /// <summary>
        /// The name of the Avatar
        /// </summary>
        [TSOVoltronString(Data.Common.Serialization.Voltron.TSOVoltronValueTypes.Length_Prefixed_Byte)]
        public string AvatarName { get; set; } = "Empty";
        /// <summary>
        /// The description set by the owner of this avatar
        /// </summary>
        [TSOVoltronString(Data.Common.Serialization.Voltron.TSOVoltronValueTypes.Length_Prefixed_Byte)]
        public string AvatarDescription { get; set; } = "Empty description...";
        public uint Unknown1 { get; set; } = 10;
        /// <summary>
        /// The Lot this Avatar is a roommate/owner of
        /// </summary>
        public uint MyLotID { get; set; } = 0;
        public uint Unknown3 { get; set; } = 30;
        public uint Unknown4 { get; set; } = 40;
        /// <summary>
        /// The amount of money this avatar has
        /// </summary>
        public int Funds { get; set; } = 0;
        public uint Unknown6 { get; set; } = 60;

        public TSODBChar() : base() { }
    }
}
