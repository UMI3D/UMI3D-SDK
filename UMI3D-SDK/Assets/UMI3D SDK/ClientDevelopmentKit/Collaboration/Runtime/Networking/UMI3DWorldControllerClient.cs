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

using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.collaboration.dto.networking;
using umi3d.common.collaboration.dto.signaling;
using umi3d.common.interaction;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Handles the connection to a World Controller.
    /// </summary>
    /// Creates the <see cref="UMI3DEnvironmentClient"/>.
    public class UMI3DWorldControllerClient
    {
        private readonly MediaDto media;

        private readonly GateDto gate;
        private string globalToken;
        private UMI3DEnvironmentClient environment;
        private PrivateIdentityDto privateIdentity;

        private readonly bool isConnecting, isConnected;
        public bool IsConnected()
        {
            return isConnected;
        }

        public UMI3DWorldControllerClient(RedirectionDto redirection, string globalToken = null)
        {
            this.media = redirection.media;
            this.gate = redirection.gate;
            this.globalToken = globalToken;
            isConnecting = false;
            isConnected = false;
            privateIdentity = null;
        }

        public ulong GetUserID() { return environment?.GetUserID() ?? 0; }

        public async Task<bool> Connect(bool downloadLibraryOnly = false)
        {
            async Task<bool> Connect(ConnectionDto dto)
            {
                if (UMI3DCollaborationClientServer.Exists && !string.IsNullOrEmpty(media.url))
                {
                    UMI3DDto answerDto = await HttpClient.Connect(dto, media.url);
                    if (answerDto is PrivateIdentityDto identity)
                    {
                        globalToken = identity.globalToken;
                        privateIdentity = identity;
                        return true;
                    }
                    else if (answerDto is ConnectionFormDto form)
                    {
                        FormAnswerDto answer = await UMI3DCollaborationClientServer.Instance.Identifier.GetParameterDtos(form);
                        var _answer = new FormConnectionAnswerDto()
                        {
                            formAnswerDto = answer,
                            metadata = form.metadata,
                            globalToken = form.globalToken,
                            gate = dto.gate,
                            libraryPreloading = dto.libraryPreloading
                        };
                        return await Connect(_answer);
                    }
                }
                return false;
            }

            if (!isConnected && !isConnecting)
            {
                return await Connect(new ConnectionDto()
                {
                    globalToken = this.globalToken,
                    gate = this.gate,
                    libraryPreloading = downloadLibraryOnly
                });
            }
            else
            {
                return false;
            }
        }

        public UMI3DWorldControllerClient Redirection(RedirectionDto redirection)
        {
            return new UMI3DWorldControllerClient(
                redirection, 
                globalToken: media.url == redirection.media.url ? globalToken : null
            );

        }

        public async Task<UMI3DEnvironmentClient> ConnectToEnvironment(MultiProgress progress)
        {
            if (environment != null)
                await environment.Logout(false);

            environment = new UMI3DEnvironmentClient(privateIdentity.connectionDto, this, progress);
            if (environment.Connect())
                return environment;
            else
                return null;
        }
    }
}