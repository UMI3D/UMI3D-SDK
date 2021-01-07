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
using System.Diagnostics.Eventing.Reader;
using UnityEngine;

namespace umi3d.common.collaboration
{
    public sealed class UMI3DAuthenticator : IUserAuthenticator
    {
        const string sepparator = ":";
        private readonly Func<string> getPin;
        private readonly Func<(string, string)> getLoginPassword;
        private readonly Func<string, bool> getAuthorized;
        private readonly string pin;
        private readonly AuthenticationType authenticationType;

        public Action<string> LoginSet;

        public UMI3DAuthenticator(Func<string> getPin, Func<(string, string)> getLoginPassword)
        {
            this.getPin = getPin;
            this.getLoginPassword = getLoginPassword;
            authenticationType = AuthenticationType.None;
        }

        public UMI3DAuthenticator(string pin)
        {
            this.pin = pin;
            authenticationType = AuthenticationType.Pin;
        }

        public UMI3DAuthenticator(Func<string,bool> getAuthorized)
        {
            this.getAuthorized = getAuthorized;
            authenticationType = AuthenticationType.Basic;
        }

        public UMI3DAuthenticator()
        {
            authenticationType = AuthenticationType.None;
        }

        public void AcceptChallenge(NetWorker networker, BMSByte challenge, Action<BMSByte> authServerAction, Action rejectServerAction)
        {
            AuthenticationType type = challenge.GetBasicType<AuthenticationType>();
            switch (type)
            {
                case AuthenticationType.None:
                    authServerAction(new BMSByte());
                    break;
                case AuthenticationType.Basic:
                    authServerAction(ObjectMapper.BMSByte(getAuthLoginPassword()));
                    break;
                case AuthenticationType.Pin:
                    authServerAction(ObjectMapper.BMSByte(getAuthPin()));
                    break;
                default:
                    Debug.LogWarning($"Unknow AuthenticationType [{type}]");
                    authServerAction(new BMSByte());
                    break;
            }
        }

        public void IssueChallenge(NetWorker networker, NetworkingPlayer player, Action<NetworkingPlayer, BMSByte> issueChallengeAction, Action<NetworkingPlayer> skipAuthAction)
        {
            issueChallengeAction(player, ObjectMapper.BMSByte(authenticationType));
        }

        public void VerifyResponse(NetWorker networker, NetworkingPlayer player, BMSByte response, Action<NetworkingPlayer> authUserAction, Action<NetworkingPlayer> rejectUserAction)
        {
            string basicString = response.GetBasicType<string>();
            switch (authenticationType)
            {
                case AuthenticationType.None:
                    authUserAction(player);
                    break;
                case AuthenticationType.Basic:
                    if (getAuthorized(basicString))
                        authUserAction(player);
                    else
                        rejectUserAction(player);
                    break;
                case AuthenticationType.Pin:
                    if (basicString == pin)
                        authUserAction(player);
                    else
                        rejectUserAction(player);
                    break;
                default:
                    Debug.LogWarning($"Unknow AuthenticationType [{authenticationType}]");
                    rejectUserAction(player);
                    break;
            }
        }

        string getAuthLoginPassword()
        {
            if (getLoginPassword == null)
                return "";
            (string login, string password) = getLoginPassword();
            LoginSet?.Invoke(login);
            return login + sepparator + password;
        }
        string getAuthPin()
        {
            return getPin?.Invoke();
        }


    }
}