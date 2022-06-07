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

namespace umi3d.common.collaboration
{
    /// <summary>
    /// Abstract class to describe an operation
    /// </summary>
    [Serializable]
    public class UserDto : AbstractEntityDto
    {
        public StatusType status;
        public ulong avatarId;
        public ulong audioSourceId;
        public int audioFrequency;
        public ulong videoSourceId;
        public uint networkId;
        public string login;

        public UserDto(UserDto source)
        {
            this.status = source.status;
            this.avatarId = source.avatarId;
            this.audioSourceId = source.audioSourceId;
            this.audioFrequency = source.audioFrequency;
            this.videoSourceId = source.videoSourceId;
            this.networkId = source.networkId;
            this.id = source.id;
            this.login = source.login;
        }

        public UserDto() { }
    }


}