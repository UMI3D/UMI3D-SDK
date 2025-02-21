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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.cdk.collaboration;
using umi3d.cdk.collaboration.emotes;
using umi3d.common;
using umi3d.common.collaboration.dto.signaling;
using static umi3d.cdk.collaboration.UMI3DUser;

namespace umi3d.cdk
{
    public class AudioBindingLoader : AbstractLoader
    {
        public class AudioUser : IAudioUser
        {
            public AudioBindingDto dto;

            public AudioUser(AudioBindingDto dto, UMI3DAudioPlayer audioplayer)
            {
                this.dto = dto;
                this.audioplayer = audioplayer;
            }

            public ulong id => dto.id;
            public string audioLogin => dto.audioLogin;

            public string login => dto.login;

            public UMI3DAudioPlayer audioplayer { get;}

            public ulong audioPlayerId => dto.audioPlayerId;
        }

        public override UMI3DVersion.VersionCompatibility version => new UMI3DVersion.VersionCompatibility("2.9.b.250217", "*");

        #region DependencyInjection

        private readonly IEnvironmentManager environmentManager;

        public AudioBindingLoader() : this(environmentManager: UMI3DCollaborationEnvironmentLoader.Instance)
        {
        }

        public AudioBindingLoader(IEnvironmentManager environmentManager)
        {
            this.environmentManager = environmentManager;
        }

        #endregion DependencyInjection

        static public List<AudioUser> users = new();

        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is AudioBindingDto;
        }

        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {

            if (value.dto is not AudioBindingDto audioBindingDto)
                throw (new common.Umi3dException("dto should be an  UMI3DAbstractNodeDto"));

            var audioEntity = await UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(value.environmentId, audioBindingDto.audioPlayerId, value.tokens);

            var user = new AudioUser(audioBindingDto, UMI3DAudioPlayer.Get(value.environmentId, audioBindingDto.audioPlayerId));

            environmentManager.RegisterEntity(value.environmentId, audioBindingDto.id, audioBindingDto, user, Delete(user)).NotifyLoaded();
            users.Add(user);
            OnNewUser?.Invoke(user);
        }

        public Action Delete(AudioUser user)
        {
            return () =>
            {
                users.Remove(user);
                OnRemoveUser?.Invoke(user);
            };
        }

        public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            throw new NotImplementedException();
        }

        public static event Action<AudioUser> OnNewUser;
        public static event Action<AudioUser> OnRemoveUser;
    }
}