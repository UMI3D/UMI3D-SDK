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

using inetum.unityUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.collaboration.dto.networking;
using umi3d.common.collaboration.dto.signaling;
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk.collaboration
{
    public class UMI3DWorldControllerClient
    {
        private readonly MediaDto media;
        public string name => media?.name;

        private readonly GateDto gate;
        private string globalToken;
        private byte[] metaData;
        private UMI3DEnvironmentClient environment;
        private PrivateIdentityDto privateIdentity;
        public readonly UMI3DDistantEnvironmentNode node;

        /// <summary>
        /// Called to create a new Public Identity for this client.
        /// </summary>
        public PublicIdentityDto PublicIdentity => new PublicIdentityDto()
        {
            userId = privateIdentity.userId,
            login = privateIdentity.login,
            displayName = privateIdentity.displayName

        };

        /// <summary>
        /// Called to create a new Identity for this client.
        /// </summary>
        public IdentityDto Identity => new IdentityDto()
        {
            userId = privateIdentity.userId,
            login = privateIdentity.login,
            displayName = privateIdentity.displayName,
            guid = privateIdentity.guid,
            headerToken = privateIdentity.headerToken,
            localToken = privateIdentity.localToken,
            key = privateIdentity.key
        };

        private bool isConnecting, isConnected;
        public bool IsConnected()
        {
            return isConnected;
        }

        public UMI3DWorldControllerClient(MediaDto media, UMI3DDistantEnvironmentNode node)
        {
            this.node = node;
            this.media = media;
            isConnecting = false;
            isConnected = false;
            privateIdentity = null;
            this.globalToken = node.token;
            this.metaData = node.metaData;
        }

        public UMI3DWorldControllerClient(MediaDto media, GateDto gate, UMI3DDistantEnvironmentNode node) : this(media, node)
        {
            this.gate = gate;
        }

        public UMI3DWorldControllerClient(RedirectionDto redirection, UMI3DDistantEnvironmentNode node) : this(redirection.media, redirection.gate,node)
        {
        }

        public UMI3DWorldControllerClient(RedirectionDto redirection, string globalToken, UMI3DDistantEnvironmentNode node, byte[] metaData = null) : this(redirection, node)
        {
            this.globalToken = globalToken;
        }

        public ulong GetUserID() { return environment?.GetUserID() ?? 0; }

        public async Task<bool> Connect(bool downloadLibraryOnly = false)
        {
            try
            {
                if (!isConnected && !isConnecting)
                    return await Connect(new ConnectionDto()
                    {
                        globalToken = this.globalToken,
                        gate = this.gate,
                        libraryPreloading = downloadLibraryOnly,
                        isServer = true,
                        metadata = metaData
                    });
                return false;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log(ex.ToString());
                UnityEngine.Debug.LogException(ex);
                isConnecting = false;
                return false;
            }
        }


        private async Task<bool> Connect(ConnectionDto dto)
        {
            isConnecting = true;
            var res = await _Connect(dto);
            isConnecting = false;
            return res;

        }


        private async Task<bool> _Connect(ConnectionDto dto)
        {
            if (!string.IsNullOrEmpty(media.url))
            {
                UMI3DDto answerDto = await HttpClient.Connect(dto, media.url);
                if (answerDto is PrivateIdentityDto identity)
                {
                    Connected(identity);
                    isConnecting = false;
                    return true;
                }
                else if (answerDto is ConnectionFormDto form)
                {
                    FormAnswerDto answer = await GetFormAnswer(form);
                    var _answer = new FormConnectionAnswerDto()
                    {
                        formAnswerDto = answer,
                        metadata = form.metadata,
                        globalToken = form.globalToken,
                        gate = dto.gate,
                        libraryPreloading = dto.libraryPreloading,
                        isServer = true,
                    };
                    return await _Connect(_answer);
                }
            }
            return false;
        }

        private void Connected(PrivateIdentityDto identity)
        {
            globalToken = identity.globalToken;
            privateIdentity = identity;
            isConnected = true;
        }

        private async Task<FormAnswerDto> GetFormAnswer(ConnectionFormDto form)
        {
            return await Task.FromResult(new FormAnswerDto()
            {
                id = form.id,
                toolId = 0,
                boneType = 0,
                hoveredObjectId = 0,
                answers = form.fields.Select(a => new ParameterSettingRequestDto() { toolId = 0, id = a.id, boneType = 0, hoveredObjectId = 0, parameter = a.GetValue() }).ToList()
            });
        }

        public UMI3DWorldControllerClient Redirection(RedirectionDto redirection)
        {
            if (media.url == redirection.media.url)
                return new UMI3DWorldControllerClient(redirection, globalToken, node);
            else
                return new UMI3DWorldControllerClient(redirection, node);
        }

        public async Task<UMI3DEnvironmentClient> ConnectToEnvironment()
        {
            if (environment != null)
                await environment.Logout(false);

            environment = new UMI3DEnvironmentClient(privateIdentity.connectionDto, this);
            if (environment.Connect())
                return environment;
            else
                return null;
        }

        /// <summary>
        /// Logout from the World Controller server.
        /// </summary>
        public void Logout()
        {

        }

        /// <summary>
        /// Logout from the World Controller server and clear infos.
        /// </summary>
        public void Clear()
        {
            Logout();
        }
    }
}