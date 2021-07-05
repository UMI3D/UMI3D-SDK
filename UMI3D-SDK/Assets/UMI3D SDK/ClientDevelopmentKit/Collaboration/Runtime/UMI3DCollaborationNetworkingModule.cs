using System;
using umi3d.common;
using umi3d.common.collaboration;

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
                    result = (T)Convert.ChangeType(user, typeof(T));
                    readable = true;
                    return true;
                case true when typeof(T) == typeof(UMI3DAnimationDto.AnimationChainDto):
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
                    result = (T)Convert.ChangeType(value, typeof(T));
                    readable = true;
                    return true;
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