using nio2so.DataService.Common.Tokens;

namespace nio2so.DataService.Common.Types
{
    public interface ICopyable
    {
        /// <summary>
        /// <see cref="ICopyable"/> implements a simple <b>Public Property</b> copy method using reflection
        /// </summary>
        /// <param name="Other"></param>
        /// <exception cref="InvalidOperationException"></exception>
        void Copy(ICopyable Other)
        {
            Type otherType = Other.GetType();
            Type myType = GetType();
            if (myType != otherType) throw new InvalidOperationException($"Attempted to copy {otherType.Name} to {myType.Name}");
            foreach (var property in Other.GetType().GetProperties())
                if (property.CanWrite && property.CanRead) property.SetValue(this, property.GetValue(Other)); // set onto this what value is on Other
        }
    }

    [Serializable]
    public class UserInfo : ICopyable
    {
        public UserInfo()
        {

        }

        /// <summary>
        /// Copy constructor that guarantees <see cref="UserAccountToken"/> will be <paramref name="UserToken"/>
        /// </summary>
        /// <param name="UserToken"></param>
        /// <param name="Other"></param>
        public UserInfo(UserToken UserToken, UserInfo Other) : this()
        {
            Copy(Other);
            UserAccountToken = UserToken;
        }
        /// <summary>
        /// Creates a new <see cref="UserInfo"/> with <paramref name="avatars"/>
        /// </summary>
        /// <param name="UserToken"></param>
        /// <param name="avatars"></param>
        public UserInfo(UserToken UserToken, params uint[] avatars) : this()
        {
            UserAccountToken = UserToken;
            Avatars = [..avatars];
        }

        /// <summary>
        /// The ID of this User's profile        
        /// </summary>
        public UserToken UserAccountToken { get; set; }
        /// <summary>
        /// The IDs of avatars that belong to this User        
        /// </summary>
        public AvatarIDToken[] Avatars
        {
            get => _avatars; set
            {
                _avatars = value;
                Array.Resize(ref _avatars, 3);
            }
        }
        AvatarIDToken[] _avatars = new AvatarIDToken[3];

        /// <summary>
        /// <inheritdoc cref="ICopyable.Copy(ICopyable)"/>
        /// </summary>
        /// <param name="Other"></param>
        public void Copy(UserInfo Other) => ((ICopyable)this).Copy(Other);
    }
}
