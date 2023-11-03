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

using System.Threading.Tasks;

using umi3d.cdk.interaction;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.pose;
using umi3d.common.collaboration.userCapture.pose.dto;
using umi3d.common.userCapture.pose;

namespace umi3d.cdk.collaboration.userCapture.pose
{
    /// <summary>
    /// Loader for <see cref="PoseAnimator"/> in a collaboration context.
    /// </summary>
    public class UMI3DCollaborationPoseAnimatorLoader : PoseAnimatorLoader
    {
        private readonly ILoadingManager loadingService;

        public UMI3DCollaborationPoseAnimatorLoader()
            : this(environmentService: UMI3DEnvironmentLoader.Instance,
                    loadingService: UMI3DEnvironmentLoader.Instance,
                    skeletonService: PersonalSkeletonManager.Instance)
        {
        }

        public UMI3DCollaborationPoseAnimatorLoader(IEnvironmentManager environmentService,
                                                    ILoadingManager loadingService,
                                                    ISkeletonManager skeletonService)
            : base(environmentService, loadingService, skeletonService)
        {
            this.loadingService = loadingService;
        }

        protected override async Task<IPoseCondition> LoadPoseCondition(AbstractPoseConditionDto dto)
        {
            switch (dto)
            {
                case ProjectedPoseConditionDto projectedPoseConditionDto:
                    {
                        UMI3DEntityInstance entityInstance = await loadingService.WaitUntilEntityLoaded(projectedPoseConditionDto.interactableId, null);
                        Interactable interactable = (Interactable)entityInstance.Object;

                        return new ProjectedPoseCondition(projectedPoseConditionDto, interactable);
                    }
                default:
                    {
                        return await base.LoadPoseCondition(dto);
                    }
            }
        }
    }
}