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
using UnityEngine;

namespace umi3d.common.collaboration
{
    public sealed class UMI3DAuthenticator : IUserAuthenticator
    {
        private const string sepparator = ":";
        private readonly Action<Action<string>> getPin;
        private readonly Action<Action<(string, string)>> getLoginPassword;
        private readonly Action<Action<IdentityDto>> getIdentity;
        private readonly Action<string, Action<bool>> getAuthorized;
        public Action<IdentityDto, NetworkingPlayer, Action<bool>> shouldAccdeptPlayer;
        private readonly string pin;
        private readonly AuthenticationType authenticationType;

        public Action<string> LoginSet;

        public UMI3DAuthenticator(Action<Action<string>> getPin, Action<Action<(string, string)>> getLoginPassword, Action<Action<IdentityDto>> getIdentity)
        {
            this.getIdentity = getIdentity;
            this.getPin = getPin;
            this.getLoginPassword = getLoginPassword;
            authenticationType = AuthenticationType.None;
        }

        public UMI3DAuthenticator(string pin)
        {
            this.pin = pin;
            authenticationType = AuthenticationType.Pin;
        }

        public UMI3DAuthenticator(Action<string, Action<bool>> getAuthorized)
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
                    MainThreadManager.Run(() =>
                    {
                        sendAuthServerAction("", authServerAction);
                    });
                    break;
                case AuthenticationType.Basic:
                    MainThreadManager.Run(() =>
                    {
                        getAuthLoginPassword((auth) => sendAuthServerAction(auth, authServerAction));
                    });
                    break;
                case AuthenticationType.Pin:
                    MainThreadManager.Run(() =>
                    {
                        getAuthPin((auth) => sendAuthServerAction(auth, authServerAction));
                    });
                    break;
                default:
                    Debug.LogWarning($"Unknow AuthenticationType [{type}]");
                    rejectServerAction();
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
            var identity = UMI3DDto.FromBson(response.GetByteArray(response.StartIndex())) as IdentityDto;

            MainThreadManager.Run(() =>
            {
                switch (authenticationType)
                {
                    case AuthenticationType.None:
                        AcceptPlayer(identity, player, () => authUserAction(player), () => rejectUserAction(player));
                        break;

                    case AuthenticationType.Basic:

                        getAuthorized(basicString, (answer) =>
                            {
                                if (answer)
                                    AcceptPlayer(identity, player, () => authUserAction(player), () => rejectUserAction(player));
                                else
                                    rejectUserAction(player);
                            });
                        break;

                    case AuthenticationType.Pin:
                        if (basicString == pin)
                            AcceptPlayer(identity, player, () => authUserAction(player), () => rejectUserAction(player));
                        else
                            rejectUserAction(player);
                        break;
                    default:
                        Debug.LogWarning($"Unknow AuthenticationType [{authenticationType}]");
                        rejectUserAction(player);
                        break;
                }
            });
        }

        private void AcceptPlayer(IdentityDto identity, NetworkingPlayer player, Action authServerAction, Action rejectServerAction)
        {
            if (shouldAccdeptPlayer == null)
            {
                authServerAction();
            }
            else
            {
                shouldAccdeptPlayer(identity, player, (b) =>
                {
                    if (b)
                        authServerAction();
                    else
                        rejectServerAction();
                });

            }
        }

        private void sendAuthServerAction(string auth, Action<BMSByte> authServerAction)
        {
            getIdentity((id) =>
            {
                authServerAction(ObjectMapper.BMSByte(auth, id.ToBson()));
            });
        }

        private void getAuthLoginPassword(Action<string> callback)
        {
            if (getLoginPassword == null)
            {
                callback?.Invoke("");
            }
            else
            {
                getLoginPassword.Invoke((k) =>
                {
                    (string login, string password) = k;
                    LoginSet?.Invoke(login);
                    callback.Invoke(login + sepparator + password);
                });
            }
        }

        private void getAuthPin(Action<string> callback)
        {
            if (getPin == null)
            {
                callback?.Invoke("");
            }
            else
            {
                getPin.Invoke((pin) =>
                {
                    callback.Invoke(pin);
                });
            }
        }


    }
}