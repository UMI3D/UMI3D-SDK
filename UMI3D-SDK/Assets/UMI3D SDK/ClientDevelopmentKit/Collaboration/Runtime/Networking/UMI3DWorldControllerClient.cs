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

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    public class UMI3DWorldControllerClient
    {
        MediaDto media;
        public string name => media?.name;
        GateDto gate;
        string globalToken;
        PrivateIdentityDto privateIdentity;
        public PublicIdentityDto PublicIdentity => new PublicIdentityDto() 
        { 
            login = privateIdentity.login, 
            userId = privateIdentity.userId 
        };

        public IdentityDto Identity => new IdentityDto()
        {
            login = privateIdentity.login,
            userId = privateIdentity.userId,
            headerToken = privateIdentity.headerToken,
            localToken = privateIdentity.localToken,
            key = privateIdentity.key
        };

        bool isConnecting, isConnected;
        public bool IsConnected() => isConnected;



        public UMI3DWorldControllerClient(MediaDto media)
        {
            this.media = media;
            isConnecting = false;
            isConnected = false;
            privateIdentity = null;
        }

        public UMI3DWorldControllerClient(MediaDto media, GateDto gate) : this(media)
        {
            this.gate = gate;
        }

        public UMI3DWorldControllerClient(RedirectionDto redirection) : this(redirection.media, redirection.gate)
        {
        }

        public UMI3DWorldControllerClient(RedirectionDto redirection, string globalToken) : this(redirection)
        {
            this.globalToken = globalToken;
        }

        public async Task<bool> Connect(bool downloadLibraryOnly = false)
        {
            if(!isConnected && !isConnecting)
                return await Connect(new ConnectionDto()
                {
                    GlobalToken = this.globalToken,
                    gate = this.gate,
                    LibraryPreloading = downloadLibraryOnly
                });
            return false;
        }

        async Task<bool> Connect(ConnectionDto dto)
        {
            if (UMI3DCollaborationClientServer.Exists)
            {
                var answerDto = await HttpClient.Connect(dto, media.url);
                if (answerDto is PrivateIdentityDto identity)
                {
                    Connected(identity);
                    return true;
                }
                else if (answerDto is ConnectionFormDto form)
                {
                    FormAnswerDto answer = await GetFormAnswer(form);
                    var _answer = new FormConnectionAnswerDto()
                    {
                        FormAnswerDto = answer,
                        GlobalToken = form.temporaryUserId,
                        gate = dto.gate,
                        LibraryPreloading = dto.LibraryPreloading
                    };
                    return await Connect(_answer);
                }
            }
            return false;
        }

        void Connected(PrivateIdentityDto identity)
        {
            globalToken = identity.GlobalToken;
            privateIdentity = identity;
        }

        async Task<FormAnswerDto> GetFormAnswer(ConnectionFormDto form)
        {
            return await UMI3DCollaborationClientServer.Instance.Identifier.GetParameterDtos(form);
        }

        public UMI3DWorldControllerClient Redirection(RedirectionDto redirection)
        {
            if (media.url == redirection.media.url)
                return new UMI3DWorldControllerClient(redirection, globalToken);
            else
                return new UMI3DWorldControllerClient(redirection);
        }

        public UMI3DEnvironmentClient ConnectToEnvironment()
        {
           var env = new UMI3DEnvironmentClient(privateIdentity.connectionDto, this);
            if (env.Connect())
                return env;
            else
                return null;
        }

        public void Logout()
        {

        }

        public void Clear()
        {
            Logout();
        }

    }
}