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

using umi3d.cdk.interaction;
using umi3d.common;
using umi3d.common.collaboration.dto.signaling;
using umi3d.common.interaction;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Helper to serialize classes of the Collaboration module.
    /// </summary>
    public class UMI3DCollaborationSerializerModule : UMI3DSerializerModule
    {
        public bool? IsCountable<T>()
        {
            return true;
        }

        /// <inheritdoc/>
        public bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            switch (true)
            {
                case true when typeof(T) == typeof(UserDto):
                    if (container.length < (2 * sizeof(uint)) + (4 * sizeof(ulong)) + sizeof(int))
                    {
                        result = default(T);
                        readable = false;
                        return true;
                    }
                    var user = new UserDto
                    {
                        id = UMI3DSerializer.Read<ulong>(container),
                        status = (StatusType)UMI3DSerializer.Read<uint>(container),
                        //avatarId = UMI3DSerializer.Read<ulong>(container),
                        audioSourceId = UMI3DSerializer.Read<ulong>(container),
                        audioFrequency = UMI3DSerializer.Read<int>(container),
                        videoSourceId = UMI3DSerializer.Read<ulong>(container),
                        networkId = UMI3DSerializer.Read<uint>(container),

                        language = UMI3DSerializer.Read<string>(container),

                        microphoneStatus = UMI3DSerializer.Read<bool>(container),
                        avatarStatus = UMI3DSerializer.Read<bool>(container),
                        attentionRequired = UMI3DSerializer.Read<bool>(container),

                        audioServerUrl = UMI3DSerializer.Read<string>(container),
                        audioChannel = UMI3DSerializer.Read<string>(container),
                        audioLogin = UMI3DSerializer.Read<string>(container),
                        audioUseMumble = UMI3DSerializer.Read<bool>(container),

                        login = UMI3DSerializer.Read<string>(container),
                        userSize = UMI3DSerializer.Read<Vector3Dto>(container),

                    };
                    result = (T)(object)user;
                    readable = true;
                    return true;
                case true when typeof(T) == typeof(AnimationChainDto):
                    {
                        if (container.length < sizeof(ulong) + sizeof(float))
                        {
                            result = default(T);
                            readable = false;
                            return true;
                        }
                        var value = new AnimationChainDto()
                        {
                            animationId = UMI3DSerializer.Read<ulong>(container),
                            startOnProgress = UMI3DSerializer.Read<float>(container),
                        };
                        result = (T)(object)value;
                        readable = true;
                        return true;
                    }
                case true when typeof(T) == typeof(UMI3DNodeAnimation.OperationChain):
                    {
                        float at = UMI3DSerializer.Read<float>(container);
                        var op = new ByteContainer(container);
                        var value = new UMI3DNodeAnimation.OperationChain(op, at);
                        result = (T)(object)value;
                        readable = true;
                        return true;
                    }
                case true when typeof(T) == typeof(AbstractInteractionDto):
                    {
                        AbstractInteractionDto value = UMI3DInteractionLoader.ReadAbstractInteractionDto(container, out readable);
                        result = (T)(object)value;
                        return true;
                    }
                case true when typeof(T) == typeof(DofGroupOptionDto):
                    {
                        var value = new DofGroupOptionDto
                        {
                            name = UMI3DSerializer.Read<string>(container),
                            separations = UMI3DSerializer.ReadList<DofGroupDto>(container)
                        };
                        result = (T)(object)value;
                        readable = true;
                        return true;
                    }
                case true when typeof(T) == typeof(DofGroupDto):
                    {
                        var value = new DofGroupDto
                        {
                            name = UMI3DSerializer.Read<string>(container),
                            dofs = (DofGroupEnum)UMI3DSerializer.Read<int>(container)
                        };
                        result = (T)(object)value;
                        readable = true;
                        return true;
                    }
            }

            result = default(T);
            readable = false;
            return false;
        }

        /// <inheritdoc/>
        public bool Write<T>(T value, out Bytable bytable, params object[] parameters)
        {
            bytable = null;
            return false;
        }
    }
}