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


namespace umi3d.common.userCapture.pose
{
    /// <summary>
    /// Wrap several pose overriders around a same related UMI3D node.
    /// </summary>
    public class UMI3DPoseOverridersContainerDto : AbstractEntityDto, IEntity
    {
        /// <summary>
        /// Id the corresponding node in the scene
        /// </summary>
        public ulong relatedNodeId { get; set; }

        /// <summary>
        /// All the pose overriders of the linked container
        /// </summary>
        public PoseOverriderDto[] poseOverriderDtos { get; set; }
    }
}