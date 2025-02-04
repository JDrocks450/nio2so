﻿using nio2so.Data.Common.Testing;

namespace nio2so.TSOHTTPS.Protocol.Data
{
    /// <summary>
    /// Regulates the creation, modification and deletion of a <see cref="UserAccount"/>
    /// </summary>
    internal class UserAccountRegulator : TSOHTTPRegulator<string, UserAccount>
    {
        protected override string FileName => TestingConstraints.UserAccountDictionaryFileName;

        public static UserAccount DefaultUser { get; } = new(
                TestingConstraints.LoginUsername,
                TestingConstraints.LoginPassword
            );

        public UserAccountRegulator()
        { 
            //Add the default user
            TryAdd(DefaultUser.UserName, DefaultUser);
        }
    }
}
