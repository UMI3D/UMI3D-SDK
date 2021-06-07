using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.common;
using umi3d.common.collaboration;
using System;

namespace umi3d.cdk.collaboration
{
    public class UMI3DCollaborationNetworkingModule : Umi3dNetworkingHelperModule
    {
        public override bool Read<T>(byte[] array, ref int position, ref int length, out bool readable, out T result)
        {
            switch (true)
            {
                case true when typeof(T) == typeof(UserDto):
                    if (length < 2 * sizeof(uint) + 4 * sizeof(ulong))
                    {
                        result = default(T);
                        readable = false;
                        return true;
                    }
                    var user = new UserDto();
                    user.id = UMI3DNetworkingHelper.Read<ulong>(array, ref position, ref length);
                    user.status = (StatusType)UMI3DNetworkingHelper.Read<uint>(array, ref position, ref length);
                    user.avatarId = UMI3DNetworkingHelper.Read<ulong>(array, ref position, ref length);
                    user.audioSourceId = UMI3DNetworkingHelper.Read<ulong>(array, ref position, ref length);
                    user.videoSourceId = UMI3DNetworkingHelper.Read<ulong>(array, ref position, ref length);
                    user.networkId = UMI3DNetworkingHelper.Read<uint>(array, ref position, ref length);
                    result = (T)Convert.ChangeType(user, typeof(T));
                    readable = true;
                    return true;
                case true when typeof(T) == typeof(UMI3DAnimationDto.AnimationChainDto):
                    if (length < sizeof(ulong) + sizeof(float))
                    {
                        result = default(T);
                        readable = false;
                        return true;
                    }
                    var value = new UMI3DAnimationDto.AnimationChainDto()
                    {
                        animationId = UMI3DNetworkingHelper.Read<ulong>(array, ref position, ref length),
                        startOnProgress = UMI3DNetworkingHelper.Read<float>(array, ref position, ref length),
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