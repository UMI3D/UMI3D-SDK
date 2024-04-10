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
    /// Request to activate a pose animator.
    /// </summary>
    public class CheckPoseAnimatorConditionsRequest : Operation
    {
        /// <summary>
        /// UMI3D id of the pose animator to try to activate.
        /// </summary>
        public ulong poseAnimatorId;

        /// <summary>
        /// If true, the request will try to only activate the pose animator. <br/>
        /// If false, the request will try to only deactivate the pose animator. <br/>
        /// If null, the request ask for a check and activation/deactivation without knowing.
        /// </summary>
        public bool ShouldActivate = true;

        public CheckPoseAnimatorConditionsRequest(ulong poseAnimatorId)
        {
            this.poseAnimatorId = poseAnimatorId;
        }

        /// <inheritdoc/>
        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(GetOperationKey())
                + UMI3DSerializer.Write(poseAnimatorId)
                + UMI3DSerializer.Write(ShouldActivate);
        }

        /// <inheritdoc/>
        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            CheckPoseAnimatorConditionsRequestDto dto = CreateDto();
            WriteProperties(dto, user.Id());
            return dto;
        }

        /// <summary>
        /// Get operation related key in <see cref="UMI3DOperationKeys"/>.
        /// </summary>
        /// <returns></returns>
        protected virtual uint GetOperationKey()
        {
            return UMI3DOperationKeys.CheckPoseAnimatorConditionsRequest;
        }

        protected virtual CheckPoseAnimatorConditionsRequestDto CreateDto()
        {
            return new CheckPoseAnimatorConditionsRequestDto();
        }

        protected virtual void WriteProperties(CheckPoseAnimatorConditionsRequestDto dto, ulong userID)
        {
            dto.PoseAnimatorId = poseAnimatorId;
            dto.ShouldActivate = ShouldActivate;
        }
    }
}