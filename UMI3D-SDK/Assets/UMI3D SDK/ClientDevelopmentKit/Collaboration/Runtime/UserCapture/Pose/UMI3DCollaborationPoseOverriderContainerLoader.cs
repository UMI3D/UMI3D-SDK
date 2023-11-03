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
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.pose;
using umi3d.common.collaboration.userCapture.pose.dto;
using umi3d.common.userCapture.pose;

namespace umi3d.cdk.collaboration.userCapture.pose
{
    public class UMI3DCollaborationPoseOverriderContainerLoader : PoseAnimatorLoader
    {
        private readonly IEnvironmentManager environmentService;

        public UMI3DCollaborationPoseOverriderContainerLoader() : base()
        {
            this.environmentService = UMI3DEnvironmentLoader.Instance;
        }

        public UMI3DCollaborationPoseOverriderContainerLoader(IEnvironmentManager environmentService,
                                                              ISkeletonManager skeletonService,
                                                              IPoseManager poseService) : base(environmentService, skeletonService, poseService)
        {
            this.environmentService = environmentService;
        }

        protected override IPoseCondition LoadPoseCondition(AbstractPoseConditionDto dto)
        {
            switch (dto)
            {
                case ProjectedPoseConditionDto projectedPoseConditionDto:
                    {
                        Interactable interactable = environmentService.GetEntityObject<Interactable>(projectedPoseConditionDto.interactableId);
                        return new ProjectedPoseCondition(projectedPoseConditionDto, interactable);
                    }
                default:
                    {
                        return base.LoadPoseCondition(dto);
                    }
            }
        }
    }
}