/*
Copyright 2019 - 2021 Inetum
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
using BeardedManStudios.Forge.Networking.Unity;
using System;

namespace umi3d.common.collaboration
{
    public sealed class UMI3DAuthenticator : IUserAuthenticator
    {
        private const DebugScope scope = DebugScope.Common | DebugScope.Collaboration | DebugScope.Networking;

        public void IssueChallenge(NetWorker networker, NetworkingPlayer player, Action<NetworkingPlayer, BMSByte> issueChallengeAction, Action<NetworkingPlayer> skipAuthAction)
        {
            issueChallengeAction(player, ObjectMapper.BMSByte());
        }

        private readonly Action<Action<string>> getLocalToken;
        public Action<string, NetworkingPlayer, Action<bool>> shouldAccdeptPlayer;


        public UMI3DAuthenticator(Action<Action<string>> getLocalToken)
        {
            this.getLocalToken = getLocalToken;
        }

        public UMI3DAuthenticator()
        {

        }


        public void AcceptChallenge(NetWorker networker, BMSByte challenge, Action<BMSByte> authServerAction, Action rejectServerAction)
        {
            if (getLocalToken != null)
                MainThreadManager.Run(() =>
                {
                    getLocalToken((g) =>
                    {
                        authServerAction(ObjectMapper.BMSByte(g));
                    });
                });
            else
                rejectServerAction.Invoke();

        }

        public void VerifyResponse(NetWorker networker, NetworkingPlayer player, BMSByte response, Action<NetworkingPlayer> authUserAction, Action<NetworkingPlayer> rejectUserAction)
        {
            string basicString = response.GetBasicType<string>();

            MainThreadManager.Run(() =>
            {

                if (basicString != null)
                    AcceptPlayer(basicString, player, () => authUserAction(player), () => rejectUserAction(player));
                else
                    rejectUserAction(player);

            });
        }

        private void AcceptPlayer(string token, NetworkingPlayer player, Action authServerAction, Action rejectServerAction)
        {
            if (shouldAccdeptPlayer == null)
            {
                authServerAction();
            }
            else
            {
                shouldAccdeptPlayer(token, player, (b) =>
                {
                    if (b)
                        authServerAction();
                    else
                        rejectServerAction();
                });
            }
        }
    }
}