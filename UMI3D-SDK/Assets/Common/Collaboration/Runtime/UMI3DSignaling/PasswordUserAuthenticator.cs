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
using BeardedManStudios;
using BeardedManStudios.Forge.Networking;

namespace umi3d.common.collaboration
{
    /// <summary>
    /// Authenticates the user to the server through a password.
    /// </summary>
    internal sealed class PasswordUserAuthenticator : IUserAuthenticator
    {
        /// <summary>
        /// The password to require.
        /// </summary>
        private readonly string password;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="password">The password to require when connecting to the server.</param>
        public PasswordUserAuthenticator(string password)
        {
            this.password = password;
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
            authServerAction(ObjectMapper.BMSByte(password));
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
            string sentPassword = response.GetBasicType<string>();

            if (sentPassword == password)
                authUserAction(player);

            else
                rejectUserAction(player);
        }
    }
}