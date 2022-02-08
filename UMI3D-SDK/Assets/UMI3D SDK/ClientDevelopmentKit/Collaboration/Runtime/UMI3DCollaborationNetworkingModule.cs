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
using umi3d.common.collaboration;
using umi3d.common.interaction;

namespace umi3d.cdk.collaboration
{
    public class UMI3DCollaborationNetworkingModule : Umi3dNetworkingHelperModule
    {
        public override bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            switch (true)
            {
                case true when typeof(T) == typeof(UserDto):
                    if (container.length < 2 * sizeof(uint) + 4 * sizeof(ulong))
                    {
                        result = default(T);
                        readable = false;
                        return true;
                    }
                    var user = new UserDto
                    {
                        id = UMI3DNetworkingHelper.Read<ulong>(container),
                        status = (StatusType)UMI3DNetworkingHelper.Read<uint>(container),
                        avatarId = UMI3DNetworkingHelper.Read<ulong>(container),
                        audioSourceId = UMI3DNetworkingHelper.Read<ulong>(container),
                        audioFrequency = UMI3DNetworkingHelper.Read<int>(container),
                        videoSourceId = UMI3DNetworkingHelper.Read<ulong>(container),
                        networkId = UMI3DNetworkingHelper.Read<uint>(container)
                    };
                    result = (T)(object)user;
                    readable = true;
                    return true;
                case true when typeof(T) == typeof(UMI3DAnimationDto.AnimationChainDto):
                    {
                        if (container.length < sizeof(ulong) + sizeof(float))
                        {
                            result = default(T);
                            readable = false;
                            return true;
                        }
                        var value = new UMI3DAnimationDto.AnimationChainDto()
                        {
                            animationId = UMI3DNetworkingHelper.Read<ulong>(container),
                            startOnProgress = UMI3DNetworkingHelper.Read<float>(container),
                        };
                        result = (T)(object)value;
                        readable = true;
                        return true;
                    }
                case true when typeof(T) == typeof(UMI3DNodeAnimation.OperationChain):
                    {
                        float at = UMI3DNetworkingHelper.Read<float>(container);
                        var op = new ByteContainer(container);
                        var value = new UMI3DNodeAnimation.OperationChain(op, at);
                        result = (T)(object)value;
                        readable = true;
                        return true;
                    }
                case true when typeof(T) == typeof(AbstractInteractionDto):
                    {
                        AbstractInteractionDto value = UMI3DAbstractToolLoader.ReadAbstractInteractionDto(container, out readable);
                        result = (T)(object)value;
                        return true;
                    }
                case true when typeof(T) == typeof(DofGroupOptionDto):
                    {
                        var value = new DofGroupOptionDto
                        {
                            name = UMI3DNetworkingHelper.Read<string>(container),
                            separations = UMI3DNetworkingHelper.ReadList<DofGroupDto>(container)
                        };
                        result = (T)(object)value;
                        readable = true;
                        return true;
                    }
                case true when typeof(T) == typeof(DofGroupDto):
                    {
                        var value = new DofGroupDto
                        {
                            name = UMI3DNetworkingHelper.Read<string>(container),
                            dofs = (DofGroupEnum)UMI3DNetworkingHelper.Read<int>(container)
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

        public override bool Write<T>(T value, out Bytable bytable)
        {
            bytable = null;
            return false;
        }
    }
}