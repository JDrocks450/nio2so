using System.Runtime.Serialization;
using static nio2so.Data.Common.Serialization.Voltron.TSOVoltronSerializationAttributes;

namespace nio2so.DataService.Common.Types.Avatar
{
    public record AvatarRelationship
    {
        public AvatarRelationship()
        {
        }

        public AvatarRelationship(uint sender, uint receiver, int sTR, int lTR)
        {
            Sender = sender;
            Receiver = receiver;
            STR = sTR;
            LTR = lTR;
        }

        public uint Sender { get; set; } = 0; // avatarID [1]
        public uint Receiver { get; set; } = 0; // avatarID [2]
        /// <summary>
        /// Short term relationship <para/>
        /// Cannot be zero.
        /// </summary>
        public int STR { get; set; } = 1;
        /// <summary>
        /// Long term relationship <para/>
        /// Cannot be zero.
        /// </summary>
        public int LTR { get; set; } = 1;

        [IgnoreDataMember]
        [TSOVoltronIgnorable]
        public static AvatarRelationship DefaultResponse => new AvatarRelationship()
        { // HOW THEY FEEL
            Sender = 1337, // [incoming]
            Receiver = 161, // [incoming]
            STR = -1,
            LTR = -1,
        };
        [IgnoreDataMember]
        [TSOVoltronIgnorable]
        public static AvatarRelationship DefaultReversedResponse => new AvatarRelationship()
        { // HOW THEY FEEL
            Sender = 161, // [incoming]
            Receiver = 1337, // [incoming]
            STR = -1,
            LTR = -1,
        };
    }
}
