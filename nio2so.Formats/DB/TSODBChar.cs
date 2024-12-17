using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.Formats.DB
{
    /// <summary>
    /// Basic information about an avatar, such as Name and Description and Money
    /// </summary>
    public class TSODBChar
    {
        [TSOVoltronString(Data.Common.Serialization.Voltron.TSOVoltronValueTypes.Length_Prefixed_Byte)] 
        public string AvatarName { get; set; }
        [TSOVoltronString(Data.Common.Serialization.Voltron.TSOVoltronValueTypes.Length_Prefixed_Byte)]
        public string AvatarDescription { get; set; }
        public uint Unknown1 { get; set; } = 10;
        public uint Unknown2 { get; set; } = 20;
        public uint Unknown3 { get; set; } = 30;
        public uint Unknown4 { get; set; } = 40;
        public uint Funds { get; set; }
        public uint Unknown6 { get; set; } = 60;

        public TSODBChar() : base() { }
    }
}
