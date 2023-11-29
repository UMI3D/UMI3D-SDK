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
using System.Linq;
using BeardedManStudios.Forge.Networking;

namespace umi3d.edk.collaboration.tracking
{
    public class UMI3DTrackingRelay : UMI3DToUserRelay<List<UserTrackingFrameDto>>
    {

        public UMI3DTrackingRelay(IForgeServer server) : base(server)
        {
            dataChannel = DataChannelTypes.Tracking;
        }

        /// <inheritdoc/>
        protected override byte[] GetMessage(List<List<UserTrackingFrameDto>> frames)
        {
            var _frames = frames.SelectMany(x => x).ToList();
            if (UMI3DEnvironment.Instance.useDto)
                return (new UMI3DDtoListDto<UserTrackingFrameDto>() { values = _frames }).ToBson();
            else
                return UMI3DSerializer.WriteCollection(_frames).ToBytes();

        }


        public void SetFrame(NetworkingPlayer source, UserTrackingFrameDto frame)
        {
            SetFrame(source, new List<UserTrackingFrameDto>() { frame });
        }
    }
}