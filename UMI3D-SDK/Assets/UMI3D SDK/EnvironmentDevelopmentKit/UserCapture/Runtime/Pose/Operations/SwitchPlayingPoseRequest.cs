/*
Copyright 2019 - 2023 Inetum

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
using umi3d.common.userCapture.pose;

namespace umi3d.edk.userCapture.pose
{
    /// <summary>
    /// An operation to tell a client to stop to play a specific pose
    /// and start to play another with a transition.
    /// </summary>
    public class SwitchPlayingPoseRequest : Operation
    {
        /// <summary>
        /// The id of the pose clip currently playing
        /// </summary>
        public ulong posePlayingId;

        /// <summary>
        /// The id of the pose clip to start playing
        /// </summary>
        public ulong poseToPlayId;

        /// <summary>
        /// The duration of the transition in seconds
        /// </summary>
        public float transitionDuration;

        public SwitchPlayingPoseRequest(ulong posePlayingId, ulong poseToPlayId, float transitionDuration = 0.25f)
        {
            this.posePlayingId = posePlayingId;
            this.poseToPlayId = poseToPlayId;
            this.transitionDuration = transitionDuration;
        }

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(GetOperationKey())
                + UMI3DSerializer.Write(posePlayingId)
                + UMI3DSerializer.Write(poseToPlayId)
                + UMI3DSerializer.Write(transitionDuration);
        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            SwitchPlayingPoseRequestDto dto = CreateDto();
            WriteProperties(dto);
            return dto;
        }

        /// <summary>
        /// Get operation related key in <see cref="UMI3DOperationKeys"/>.
        /// </summary>
        /// <returns></returns>
        protected virtual uint GetOperationKey()
        {
            return UMI3DOperationKeys.SwitchPlayingPoseRequest;
        }

        protected virtual SwitchPlayingPoseRequestDto CreateDto()
        {
            return new SwitchPlayingPoseRequestDto();
        }

        protected virtual void WriteProperties(SwitchPlayingPoseRequestDto dto)
        {
            dto.posePlayingId = posePlayingId;
            dto.poseToPlayId = poseToPlayId;
            dto.transitionDuration = transitionDuration;
        }
    }
}