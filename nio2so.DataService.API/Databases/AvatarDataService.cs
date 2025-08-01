﻿using nio2so.Data.Common.Testing;
using nio2so.DataService.API.Controllers;
using nio2so.DataService.API.Databases.Libraries;
using nio2so.DataService.Common.Tokens;
using nio2so.DataService.Common.Types;
using nio2so.DataService.Common.Types.Avatar;
using nio2so.DataService.Common.Types.Search;

namespace nio2so.DataService.API.Databases
{
    /// <summary>
    /// Serves Avatar information:
    /// <list type="bullet">Profile information of the Avatar shown in SAS</list>
    /// </summary>
    internal class AvatarDataService : DataServiceBase, ISearchable<uint>
    {
        const string charblob_folder = @"charblob";
        const string AvatarLibName = "AVATARS";

        private JSONDictionaryLibrary<uint, AvatarInfo> AvatarsLibrary => GetLibrary<JSONDictionaryLibrary<uint, AvatarInfo>>(AvatarLibName);

        internal AvatarDataService() : base() { }

        protected override void AddLibraries()
        {
            string fPath = ServerSettings.Current.AvatarInfoFile;

            //avatar database
            Libraries.Add(AvatarLibName, 
                new JSONDictionaryLibrary<uint, AvatarInfo>(fPath, EnsureDefaultValues));
            //**folder of charblobs, database
            Libraries.Add("BLOBS",
                new FileObjectLibrary(Path.Combine(Path.GetDirectoryName(fPath), charblob_folder), "avatar", "charblob", CharBlobNotFound));
            //**avatar creation index** taken from The Sims 2 with its Sim Creation Index idea
            Libraries.Add("AVATAR CREATION INDEX", new JSONCreationIndex(ServerSettings.Current.AvatarCreationIndexFile));

            base.AddLibraries();
        }

        async Task EnsureDefaultValues()
        {
            ServerSettings settings = ServerSettings.Current;
            // not needed
            AvatarsLibrary.Add(1337, new AvatarInfo(settings.StaticAccounts[0], 1337)
            {
                AvatarCharacter = new()
                {
                    AvatarName = "bisquick",
                    AvatarDescription = "default value",
                    Funds = TestingConstraints.StartingFunds,
                    MyLotID = TestingConstraints.MyHouseID
                },
                RelationshipInfo = new()
                {
                    Outgoing = new() { { 161, new(1337,161,-1,-1)} }
                }
            });
            AvatarsLibrary.Add(161, new AvatarInfo(settings.StaticAccounts[1], 161)
            {
                AvatarCharacter = new()
                {
                    AvatarName = "FriendlyBuddy",
                    AvatarDescription = "default value",
                    Funds = TestingConstraints.StartingFunds,
                    MyLotID = 0
                },
                RelationshipInfo = new()
                {
                    Outgoing = new() { { 1337, new(161, 1337, 1, 1) } }
                }
            });
        }       

        Task<byte[]> CharBlobNotFound() => File.ReadAllBytesAsync(ServerSettings.Current.DefaultCharblobPath);

        /// <summary>
        /// Returns the <see cref="AvatarProfile"/> for the given <paramref name="AvatarID"/>. The profile has no personal details and requires no validation **public info**
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public AvatarProfile GetProfileByAvatarID(AvatarIDToken AvatarID)
        {
            void correctErrors(AvatarProfile Profile)
            {
                if (Profile.AvatarID != AvatarID)
                {
                    if (Profile.AvatarID != 0) // TODO: implement handler for this? this could theoretically only happen by human intervention.. in which perhaps it is intentional?
                                               // this would be useful for moving avatar from one shard to another.
                        throw new InvalidDataException($"The returned AvatarProfile for {AvatarID} belongs to {Profile.AvatarID}...?");
                }
            }
            var DataFile = GetLibrary<JSONDictionaryLibrary<uint, AvatarInfo>>(AvatarLibName);
            if (!DataFile.ContainsKey(AvatarID))
                throw new KeyNotFoundException(nameof(AvatarID));
            AvatarProfile profile = DataFile[AvatarID].Profile;
            correctErrors(profile);
            return profile;
        }
        /// <summary>
        /// Returns the name of the Avatar with the given ID
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        public string GetNameByID(AvatarIDToken AvatarID) => GetProfileByAvatarID(AvatarID).Name;

        /// <summary>
        /// Returns the <see cref="AvatarInfo.AvatarRelationshipInfo"/> for the given <paramref name="AvatarID"/>. Relationships are public info, the friendship web exists. **public info**
        /// <para/>This is how the <paramref name="AvatarID"/> feels about other sims.
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public IEnumerable<AvatarRelationship> GetRelationshipsByAvatarID(AvatarIDToken AvatarID)
        {
            if (!AvatarsLibrary.ContainsKey(AvatarID))
                throw new KeyNotFoundException(nameof(AvatarID));
            var relationships = AvatarsLibrary[AvatarID].RelationshipInfo;            
            return relationships.Outgoing.Values;
        }
        /// <summary>
        /// Returns the reverse <see cref="AvatarInfo.AvatarRelationshipInfo"/> for the given <paramref name="AvatarID"/>. Relationships are public info, the friendship web exists. **public info**
        /// <para/>This is how other sims feel about the <paramref name="AvatarID"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public IEnumerable<AvatarRelationship> GetReverseRelationshipsByAvatarID(AvatarIDToken AvatarID)
        {
            foreach(var avatarFile in AvatarsLibrary)
            {
                if (!avatarFile.Value.RelationshipInfo.Outgoing.TryGetValue(AvatarID, out var Relationship))
                    continue;
                yield return Relationship;
            }
        }

        /// <summary>
        /// Returns the <see cref="AvatarInfo.AvatarBookmarkInfo"/> for the given <paramref name="AvatarID"/>. Bookmarks are public info, like a follower list on social media **public info**
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public Common.Types.AvatarInfo.AvatarBookmarkInfo GetBookmarksByAvatarID(AvatarIDToken AvatarID)
        {
            void correctErrors(Common.Types.AvatarInfo.AvatarBookmarkInfo Bookmark)
            {
                // TODO
            }
            if (!AvatarsLibrary.ContainsKey(AvatarID))
                throw new KeyNotFoundException(nameof(AvatarID));
            var bookmark = AvatarsLibrary[AvatarID].BookmarkInfo;
            correctErrors(bookmark);
            return bookmark;
        }

        /// <summary>
        /// Updates the list of bookmarks attached to this <paramref name="AvatarID"/> to be <paramref name="Avatars"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <param name="Avatars"></param>
        /// <returns></returns>
        public bool SetBookmarksByAvatarID(AvatarIDToken AvatarID, IEnumerable<AvatarIDToken> Avatars)
        {
            if (!AvatarsLibrary.ContainsKey(AvatarID))
                return false;            
            AvatarsLibrary[AvatarID].BookmarkInfo.BookmarkAvatars = new(Avatars);
            Save();
            return true;
        }
        /// <summary>
        /// Returns the <see cref="TSODBChar"/> for the given <paramref name="AvatarID"/>. This is vital to the social aspect of the game. **public info**
        /// <para/>This will reevaluate fields on the <see cref="TSODBChar"/> and save the updated values to the disk.
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public TSODBChar GetCharacterByAvatarID(AvatarIDToken AvatarID)
        {
            void Reevaluate(TSODBChar CharData)
            {
                //**get updated lot membership value
                CharData.MyLotID = APIDataServices.LotDataService.GetRoommateHouseIDByAvatarID(AvatarID) ?? 0;

                // TODO
                if (AvatarID == 161)
                {
                    CharData.Unknown1 = 0;
                    CharData.Unknown3 = 0;
                    CharData.Unknown4 = 0;
                    CharData.Unknown6 = 0;
                }
            }
            if (!AvatarsLibrary.ContainsKey(AvatarID))
                throw new KeyNotFoundException(nameof(AvatarID));
            var file = AvatarsLibrary[AvatarID].AvatarCharacter;
            Reevaluate(file);
            Save();
            return file;
        }

        /// <summary>
        /// Updates the <see cref="TSODBChar"/> attached to this <paramref name="AvatarID"/> to be <paramref name="CharacterFile"/>
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <param name="Avatars"></param>
        /// <returns></returns>
        public bool SetCharacterByAvatarID(AvatarIDToken AvatarID, TSODBChar CharacterFile)
        {
            if (!AvatarsLibrary.ContainsKey(AvatarID))
                return false;
            AvatarsLibrary[AvatarID].AvatarCharacter = CharacterFile;
            Save();
            return true;
        }

        /// <summary>
        /// Returns the <see cref="TSODBCharBlob"/> for the given <see cref="AvatarIDToken"/> in binary
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public Task<byte[]> GetCharBlobByAvatarID(AvatarIDToken AvatarID) => GetLibrary<FileObjectLibrary>("BLOBS").GetDataByID(AvatarID);
        /// <summary>
        /// Sets the <see cref="TSODBCharBlob"/> for the given <see cref="AvatarIDToken"/> in binary
        /// </summary>
        /// <param name="AvatarID"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public Task SetCharBlobByAvatarID(AvatarIDToken AvatarID, byte[] CharBlobStream, bool overwrite = true) =>
            GetLibrary<FileObjectLibrary>("BLOBS").SetDataByIDToDisk(AvatarID,CharBlobStream,overwrite);

        /// <summary>
        /// Creates a new <see cref="AvatarInfo"/> file with a new unique <see cref="AvatarIDToken"/>
        /// </summary>
        /// <param name="user"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public AvatarIDToken CreateNewAvatar(UserToken user, string method)
        {
            AvatarInfo newInfo;
            uint AvatarID = 0;

            JSONCreationIndex lib = GetLibrary<JSONCreationIndex>("AVATAR CREATION INDEX");
            do
            {
                AvatarID = lib.GetNextID(AvatarsLibrary);
            }
            while (!AvatarsLibrary.TryAdd(AvatarID, newInfo = new AvatarInfo(user, AvatarID)
            {                
                CreatedUsing = method
            }));
            newInfo.AvatarCharacter.Funds = TestingConstraints.StartingFunds;
            Save();
            //add the avatar to this account
            if (!APIDataServices.UserDataService.AddAvatarToAccount(user, AvatarID, out _))
                throw new InvalidOperationException("Tried to link an avatar to an account that is full.");
            return AvatarID;
        }

        public bool DebitCreditTransaction(int AccountChange)
        {
            return true;
        }

        public IDictionary<uint,string> SearchExact(string QueryString) => AvatarsLibrary.SearchExact(QueryString);
        public IDictionary<uint, string> SearchBroad(string QueryString, int MaxResults) => AvatarsLibrary.SearchBroad(QueryString, MaxResults);        
    }
}
