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

using System.Collections.Generic;
using umi3d.common.collaboration;
using umi3d.common;
using umi3d.common.userCapture.tracking;

namespace umi3d.edk.collaboration.tracking
{
    public class UMI3DTrackingRelay : UMI3DToUserRelay<UserTrackingFrameDto>
    {
        public UMI3DTrackingRelay(IForgeServer server) : base(server)
        {
            dataChannel = DataChannelTypes.Tracking;
        }

        /// <inheritdoc/>
        protected override byte[] GetMessage(List<UserTrackingFrameDto> frames)
        {
            if (UMI3DEnvironment.Instance.useDto)
                return (new UMI3DDtoListDto<UserTrackingFrameDto>() { values = frames }).ToBson();
            else
                return UMI3DSerializer.WriteCollection(frames).ToBytes();

        }
    }
}