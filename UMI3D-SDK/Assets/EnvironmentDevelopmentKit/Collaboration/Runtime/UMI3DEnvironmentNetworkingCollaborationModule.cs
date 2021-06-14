using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.edk.collaboration
{
    public class UMI3DEnvironmentNetworkingCollaborationModule : Umi3dNetworkingHelperModule
    {

        public override bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            result = default;
            readable = false;
            return false;
        }

        public override bool Write<T>(T value, out Bytable bytable)
        {
            switch (value)
            {
                case UserDto user:
                    bytable = UMI3DNetworkingHelper.Write<ulong>(user.id)
                    + UMI3DNetworkingHelper.Write<uint>((uint)user.status)
                    + UMI3DNetworkingHelper.Write<ulong>(user.avatarId)
                    + UMI3DNetworkingHelper.Write<ulong>(user.audioSourceId)
                    + UMI3DNetworkingHelper.Write<ulong>(user.videoSourceId)
                    + UMI3DNetworkingHelper.Write<uint>(user.networkId);
                    return true;
                case UMI3DCollaborationUser user:
                    bytable = UMI3DNetworkingHelper.Write<ulong>(user.Id())
                    + UMI3DNetworkingHelper.Write<uint>((uint)user.status)
                    + UMI3DNetworkingHelper.Write<ulong>(user.Avatar == null ? 0 : user.Avatar.Id())
                    + UMI3DNetworkingHelper.Write<ulong>(user.audioPlayer?.Id() ?? 0)
                    + UMI3DNetworkingHelper.Write<ulong>(user.videoPlayer?.Id() ?? 0)
                    + UMI3DNetworkingHelper.Write<uint>(user.networkPlayer?.NetworkId ?? 0);
                    return true;
            }
            bytable = null;
            return false;
        }
    }
}