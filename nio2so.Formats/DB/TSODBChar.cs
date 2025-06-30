using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Formats.DB
{
    /// <summary>
    /// Basic information about an avatar, such as Name, Description and Money
    /// </summary>
    public class TSODBChar
    {
        /// <summary>
        /// The name of the Avatar
        /// </summary>
        [TSOVoltronString(Data.Common.Serialization.Voltron.TSOVoltronValueTypes.Length_Prefixed_Byte)] 
        public string AvatarName { get; set; }
        /// <summary>
        /// The description set by the owner of this avatar
        /// </summary>
        [TSOVoltronString(Data.Common.Serialization.Voltron.TSOVoltronValueTypes.Length_Prefixed_Byte)]
        public string AvatarDescription { get; set; }
        public uint Unknown1 { get; set; } = 10;
        public uint Unknown2 { get; set; } = 20;
        public uint Unknown3 { get; set; } = 30;
        public uint Unknown4 { get; set; } = 40;
        /// <summary>
        /// The amount of money this avatar has
        /// </summary>
        public uint Funds { get; set; }
        public uint Unknown6 { get; set; } = 60;

        public TSODBChar() : base() { }
    }
}
