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
                    + UMI3DNetworkingHelper.Write<int>(user.audioFrequency)
                    + UMI3DNetworkingHelper.Write<ulong>(user.videoSourceId)
                    + UMI3DNetworkingHelper.Write<uint>(user.networkId);
                    return true;
                case UMI3DCollaborationUser user:
                    bytable = UMI3DNetworkingHelper.Write<ulong>(user.Id())
                    + UMI3DNetworkingHelper.Write<uint>((uint)user.status)
                    + UMI3DNetworkingHelper.Write<ulong>(user.Avatar == null ? 0 : user.Avatar.Id())
                    + UMI3DNetworkingHelper.Write<ulong>(user.audioPlayer?.Id() ?? 0)
                    + UMI3DNetworkingHelper.Write<int>(user.audioFrequency)
                    + UMI3DNetworkingHelper.Write<ulong>(user.videoPlayer?.Id() ?? 0)
                    + UMI3DNetworkingHelper.Write<uint>(user.networkPlayer?.NetworkId ?? 0);
                    return true;
            }
            bytable = null;
            return false;
        }
    }
}