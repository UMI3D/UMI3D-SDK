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

using umi3d.cdk.interaction;
using umi3d.cdk.userCapture.pose;
using umi3d.common.collaboration.userCapture.pose.dto;

namespace umi3d.cdk.collaboration.userCapture.pose
{
    /// <summary>
    /// Pose condition that is true when an interactable is projected.
    /// </summary>
    public class ProjectedPoseCondition : IPoseCondition
    {
        private ProjectedPoseConditionDto dto;

        protected ulong InteractableId => dto.interactableId;

        private bool isProjected;

        public ProjectedPoseCondition(ProjectedPoseConditionDto dto, Interactable interactable)
        {
            this.dto = dto;
            interactable.onProject.AddListener(() => isProjected = true);
            interactable.onRelease.AddListener(() => isProjected = false);
        }

        /// <inheritdoc/>
        public bool Check()
        {
            return isProjected;
        }
    }
}