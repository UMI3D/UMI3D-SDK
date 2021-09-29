using System;
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
                    var user = new UserDto();
                    user.id = UMI3DNetworkingHelper.Read<ulong>(container);
                    user.status = (StatusType)UMI3DNetworkingHelper.Read<uint>(container);
                    user.avatarId = UMI3DNetworkingHelper.Read<ulong>(container);
                    user.audioSourceId = UMI3DNetworkingHelper.Read<ulong>(container);
                    user.videoSourceId = UMI3DNetworkingHelper.Read<ulong>(container);
                    user.networkId = UMI3DNetworkingHelper.Read<uint>(container);
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
                        var at = UMI3DNetworkingHelper.Read<float>(container);
                        var op = new ByteContainer(container);
                        var value = new UMI3DNodeAnimation.OperationChain(op, at);
                        result = (T)(object)value;
                        readable = true;
                        return true;
                    }
                case true when typeof(T) == typeof(AbstractInteractionDto):
                    {
                        var value = UMI3DAbstractToolLoader.ReadAbstractInteractionDto(container, out readable);
                        result = (T)(object)value;
                        return true;
                    }
                case true when typeof(T) == typeof(DofGroupOptionDto):
                    {
                        var value = new DofGroupOptionDto();
                        value.name = UMI3DNetworkingHelper.Read<string>(container);
                        value.separations = UMI3DNetworkingHelper.ReadList<DofGroupDto>(container);
                        result = (T)(object)value;
                        readable = true;
                        return true;
                    }
                case true when typeof(T) == typeof(DofGroupDto):
                    {
                        var value = new DofGroupDto();
                        value.name = UMI3DNetworkingHelper.Read<string>(container);
                        value.dofs = (DofGroupEnum)UMI3DNetworkingHelper.Read<int>(container);
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