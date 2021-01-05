/*
Copyright 2019 Gfi Informatique
Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
    http://www.apache.org/licenses/LICENSE-2.0
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections.Generic;
using BeardedManStudios;
using BeardedManStudios.Forge.Networking;

namespace umi3d.common.collaboration
{
    /// <summary>
    /// Authenticates the user to the server through a password.
    /// Auth provided as a "login:password" string.
    /// </summary>
    internal sealed class BasicUserAuthenticator : IUserAuthenticator
    {
        /// <summary>
        /// The user password.
        /// </summary>
        private readonly string password;

        /// <summary>
        /// The required pairs.
        /// </summary>
        private readonly string login;

        /// <summary>
        /// The user login.
        /// </summary>
        private Dictionary<string, string> authDB;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="login">The login to require when connecting to the server.</param>
        /// <param name="password">The password to require when connecting to the server.</param>
        public BasicUserAuthenticator(string login, string password)
        {
            this.login = login;
            this.password = password;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="authDB">The accepted login/password pairs.</param>
        public BasicUserAuthenticator(Dictionary<string, string> authDB = null)
        {
            this.authDB = authDB;
        }

        /// <summary>
        /// Remove access to a user
        /// </summary>
        /// <param name="login">The user's login</param>
        public void RemoveLogin(string login)
        {
            if (authDB != null)
                authDB.Remove(login);
        }

        /// <summary>
        /// Update the password for a user.
        /// Creates an access for the user if it is a new login.
        /// </summary>
        /// <param name="login"></param>
        /// <param name="Password"></param>
        public void SetPasswordFor(string login, string Password)
        {
            if (authDB == null)
                authDB = new Dictionary<string, string>();
            if (authDB.ContainsKey(login))
                authDB[login] = password;
            else
                authDB.Add(login, password);
        }

        /// <inheritdoc/>
        public void IssueChallenge(
            NetWorker networker,
            NetworkingPlayer player,
            Action<NetworkingPlayer, BMSByte> issueChallengeAction,
            Action<NetworkingPlayer> skipAuthAction
        )
        {
            issueChallengeAction(player, new BMSByte());
        }

        /// <inheritdoc/>
        public void AcceptChallenge(
            NetWorker networker,
            BMSByte challenge,
            Action<BMSByte> authServerAction,
            Action rejectServerAction
        )
        {
            authServerAction(ObjectMapper.BMSByte(login + ":" + password));
        }

        /// <inheritdoc/>
        public void VerifyResponse(
            NetWorker networker,
            NetworkingPlayer player,
            BMSByte response,
            Action<NetworkingPlayer> authUserAction,
            Action<NetworkingPlayer> rejectUserAction
        )
        {
            string basicString = response.GetBasicType<string>();
            string[] splitted = basicString.Split(':');
            if (authDB == null || splitted.Length != 2)
                rejectUserAction(player);
            else if (authDB.ContainsKey(splitted[0]) && authDB[splitted[0]] == splitted[1])
                authUserAction(player);
            else
                rejectUserAction(player);
        }
    }
}