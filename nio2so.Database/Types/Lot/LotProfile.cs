using nio2so.DataService.Common.Tokens;

namespace nio2so.DataService.Common.Types.Lot
{
    /// <summary>
    /// Contains basic information about a lot
    /// </summary>
    [Serializable]
    public class LotProfile
    {
        /// <summary>
        /// Default parameterless constructor
        /// </summary>
        public LotProfile()
        {
        }
        /// <summary>
        /// Creates a new <see cref="LotProfile"/> with the supplied parameter list
        /// </summary>
        /// <param name="houseID"></param>
        /// <param name="position"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public LotProfile(HouseIDToken houseID, AvatarIDToken Owner, LotPosition position, string name, string description)
        {
            HouseID = houseID;
            Position = position;
            Name = name;
            Description = description;
            OwnerAvatar = Owner;
        }
        /// <summary>
        /// The ID of the HouseBlob this lot's layout is located at
        /// </summary>
        public HouseIDToken HouseID { get; set; }
        /// <summary>
        /// The position in the MapView this lot belongs at
        /// </summary>
        public LotPosition Position { get; set; } = new();
        /// <summary>
        /// The name of this House
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// The description of this lot
        /// </summary>
        public string Description { get; set; } = "";
        /// <summary>
        /// The avatar that owns this lot
        /// </summary>
        public AvatarIDToken OwnerAvatar { get; set; }
    }
}
