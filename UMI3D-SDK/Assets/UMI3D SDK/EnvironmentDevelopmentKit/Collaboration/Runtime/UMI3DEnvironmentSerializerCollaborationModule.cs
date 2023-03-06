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
using umi3d.common.collaboration;

namespace umi3d.edk.collaboration
{
    /// <summary>
    /// Helper for serialization of classes from the Collaboration module.
    /// </summary>
    public class UMI3DEnvironmentSerializerCollaborationModule : UMI3DSerializerModule
    {
        /// <inheritdoc/>
        public override bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            switch (true)
            {
                case true when typeof(T) == typeof(RegisterIdentityDto):
                    if (container.length < (2 * sizeof(uint)) + (4 * sizeof(ulong)))
                    {
                        result = default(T);
                        readable = false;
                        return true;
                    }
                    var identity = new RegisterIdentityDto
                    {
                        userId = UMI3DSerializer.Read<ulong>(container),
                        login = UMI3DSerializer.Read<string>(container),
                        localToken = UMI3DSerializer.Read<string>(container),
                        key = UMI3DSerializer.Read<string>(container),
                        metaData = UMI3DSerializer.ReadArray<byte>(container)
                    };
                    result = (T)(object)identity;
                    readable = true;
                    return true;
            }

            result = default(T);
            readable = false;
            return false;
        }

        /// <inheritdoc/>
        public override bool Write<T>(T value, out Bytable bytable)
        {
            switch (value)
            {
                case UserDto user:
                    bytable = UMI3DSerializer.Write<ulong>(user.id)
                    + UMI3DSerializer.Write<uint>((uint)user.status)
                    //+ UMI3DSerializer.Write<ulong>(user.avatarId)
                    + UMI3DSerializer.Write<ulong>(user.audioSourceId)
                    + UMI3DSerializer.Write<int>(user.audioFrequency)
                    + UMI3DSerializer.Write<ulong>(user.videoSourceId)
                    + UMI3DSerializer.Write<uint>(user.networkId)

                    + UMI3DSerializer.Write(user.language)

                    + UMI3DSerializer.Write(user.microphoneStatus)
                    + UMI3DSerializer.Write(user.avatarStatus)
                    + UMI3DSerializer.Write(user.attentionRequired)

                    + UMI3DSerializer.Write(user.audioServerUrl)
                    + UMI3DSerializer.Write(user.audioChannel)
                    + UMI3DSerializer.Write(user.audioLogin)
                    + UMI3DSerializer.Write(user.audioUseMumble)

                     + UMI3DSerializer.Write<string>(user.login);
                    return true;
                case UMI3DCollaborationUser user:
                    bytable = UMI3DSerializer.Write<ulong>(user.Id())
                    + UMI3DSerializer.Write<uint>((uint)user.status)
                    //+ UMI3DSerializer.Write<ulong>(user.Avatar == null ? 0 : user.Avatar.Id())
                    + UMI3DSerializer.Write<ulong>(user.audioPlayer?.Id() ?? 0)
                    + UMI3DSerializer.Write<int>(user.audioFrequency.GetValue())
                    + UMI3DSerializer.Write<ulong>(user.videoPlayer?.Id() ?? 0)
                    + UMI3DSerializer.Write<uint>(user.networkPlayer?.NetworkId ?? 0)

                    + UMI3DSerializer.Write(user.language)

                    + UMI3DSerializer.Write(user.microphoneStatus.GetValue())
                    + UMI3DSerializer.Write(user.avatarStatus.GetValue())
                    + UMI3DSerializer.Write(user.attentionRequired.GetValue())

                    + UMI3DSerializer.Write(user.audioServerUrl.GetValue())
                    + UMI3DSerializer.Write(user.audioChannel.GetValue())
                    + UMI3DSerializer.Write(user.audioLogin.GetValue())
                    + UMI3DSerializer.Write(user.audioUseMumble.GetValue())

                    + UMI3DSerializer.Write<string>(string.IsNullOrEmpty(user.displayName) ? (string.IsNullOrEmpty(user.login) ? user.Id().ToString() : user.login) : user.displayName);
                    return true;
                case RegisterIdentityDto identity:
                    bytable = UMI3DSerializer.Write(identity.userId)
                        + UMI3DSerializer.Write(identity.login)
                        + UMI3DSerializer.Write(identity.localToken)
                        + UMI3DSerializer.Write(identity.key)
                        + UMI3DSerializer.Write(identity.metaData);
                    return true;

            }
            bytable = null;
            return false;
        }
    }
}