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

using umi3d.common;
using umi3d.common.collaboration.dto.signaling;
using umi3d.edk.core;

namespace umi3d.edk.collaboration
{
    /// <summary>
    /// Entity to link a mumble user not link to a umi3d user to an audio source.
    /// </summary>
    public class AudioBinding : AbstractLoadableEntity
    {
        readonly public string audioLogin;
        readonly public string login;
        readonly public UMI3DAudioPlayer audioPlayer;

        public AudioBinding(string audioLogin, string login, UMI3DAudioPlayer audioPlayer)
        {
            if (audioLogin is null)
                throw new System.ArgumentNullException("audioLogin");
            if (login is null)
                throw new System.ArgumentNullException("login");
            if (audioPlayer is null)
                throw new System.ArgumentNullException("audioPlayer");

            this.audioLogin = audioLogin;
            this.login = login;
            this.audioPlayer = audioPlayer;
        }

        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return new AudioBindingDto()
            {
                id = Id(),

                audioLogin = audioLogin,
                login = login,
                audioPlayerId = audioPlayer.Id()
            };
        }
    }
}