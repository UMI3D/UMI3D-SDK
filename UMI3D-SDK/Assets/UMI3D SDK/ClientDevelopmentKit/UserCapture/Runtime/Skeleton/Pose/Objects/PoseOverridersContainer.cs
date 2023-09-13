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

using umi3d.common.userCapture.pose;

namespace umi3d.cdk.userCapture.pose
{
    public class PoseOverridersContainer
    {
        private UMI3DPoseOverridersContainerDto dto;

        public PoseOverrider[] PoseOverriders { get; private set; }

        /// <summary>
        /// UMI3D ID.
        /// </summary>
        public ulong Id => dto.id;

        /// <summary>
        /// See <see cref="UMI3DPoseOverridersContainerDto.relatedNodeId"/>.
        /// </summary>
        public ulong NodeId => dto.relatedNodeId;

        public PoseOverridersContainer(UMI3DPoseOverridersContainerDto dto, PoseOverrider[] poseOverriders)
        {
            this.dto = dto;
            PoseOverriders = poseOverriders ?? new PoseOverrider[0];
        }
    }
}