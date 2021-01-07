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

using BeardedManStudios;
using BeardedManStudios.Forge.Networking;
using System;

namespace umi3d.common.collaboration
{
    public class PinAuthenticator : IUserAuthenticator
    {
        public string pin;

        public PinAuthenticator(string pin)
        {
            this.pin = pin;
        }

        public void AcceptChallenge(NetWorker networker, BMSByte challenge, Action<BMSByte> authServerAction, Action rejectServerAction)
        {
            authServerAction(ObjectMapper.BMSByte(pin));
        }

        public void IssueChallenge(NetWorker networker, NetworkingPlayer player, Action<NetworkingPlayer, BMSByte> issueChallengeAction, Action<NetworkingPlayer> skipAuthAction)
        {
            issueChallengeAction(player, new BMSByte());
        }

        public void VerifyResponse(NetWorker networker, NetworkingPlayer player, BMSByte response, Action<NetworkingPlayer> authUserAction, Action<NetworkingPlayer> rejectUserAction)
        {
            string sentPassword = response.GetBasicType<string>();

            if (sentPassword == pin)
                authUserAction(player);
            else
                rejectUserAction(player);
        }
    }
}