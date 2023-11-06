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

using System.Linq;
using System.Threading.Tasks;

using umi3d.common;
using umi3d.common.userCapture.pose;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Loader for <see cref="PoseAnimatorDto"/>.
    /// </summary>
    public class PoseAnimatorLoader : AbstractLoader<PoseAnimatorDto, PoseAnimator>, IEntity
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.UserCapture | DebugScope.Loading;

        private static readonly UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.7", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;

        #region Dependencies Injection

        private readonly IEnvironmentManager environmentService;
        private readonly ILoadingManager loadingService;
        private readonly ISkeletonManager skeletonService;

        public PoseAnimatorLoader() : this(environmentService: UMI3DEnvironmentLoader.Instance,
                                           loadingService: UMI3DEnvironmentLoader.Instance,
                                           skeletonService: PersonalSkeletonManager.Instance)
        {
        }

        public PoseAnimatorLoader(IEnvironmentManager environmentService,
                                  ILoadingManager loadingService,
                                  ISkeletonManager skeletonService)
        {
            this.environmentService = environmentService;
            this.loadingService = loadingService;
            this.skeletonService = skeletonService; ;
        }

        #endregion Dependencies Injection

        /// <summary>
        /// Init the IDs, inits the overriders, registers this entity to the environnement loader
        /// </summary>
        public override async Task<PoseAnimator> Load(PoseAnimatorDto dto)
        {
            if (dto == null)
                throw new System.ArgumentNullException(nameof(dto));

            IPoseCondition[] poseConditions = await Task.WhenAll(dto.poseConditions
                                                                .Select(x => LoadPoseCondition(x)));

            UMI3DEntityInstance poseClipInstance = await loadingService.WaitUntilEntityLoaded(dto.poseClipId, null);
            PoseClip poseClip = (PoseClip)poseClipInstance.Object;

            PoseAnimator poseAnimator = new(dto, poseClip, poseConditions.Where(x => x is not null).ToArray());

            if (poseAnimator.ActivationMode == (ushort)PoseAnimatorActivationMode.AUTO)
                poseAnimator.StartWatchActivationConditions();

            environmentService.RegisterEntity(dto.id, dto, poseAnimator, () => Delete(dto.id)).NotifyLoaded();

            return poseAnimator;
        }

        /// <inheritdoc/>
        public override void Delete(ulong id)
        {
            PoseAnimator animator = environmentService.GetEntityObject<PoseAnimator>(id);
            animator.Clear();
        }

        /// <summary>
        /// Instantiate a <see cref="IPoseCondition"/> from a  <see cref="AbstractPoseConditionDto"/>.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        protected virtual async Task<IPoseCondition> LoadPoseCondition(AbstractPoseConditionDto dto)
        {
            switch (dto)
            {
                case MagnitudeConditionDto magnitudeConditionDto:
                    {
                        UMI3DNodeInstance targetNodeInstance = (UMI3DNodeInstance)await loadingService.WaitUntilEntityLoaded(magnitudeConditionDto.TargetNodeId, null);
                        return new MagnitudePoseCondition(magnitudeConditionDto, targetNodeInstance.transform, skeletonService.PersonalSkeleton.TrackedSubskeleton);
                    }
                case DirectionConditionDto directionConditionDto:
                    {
                        UMI3DNodeInstance targetNodeInstance = (UMI3DNodeInstance)await loadingService.WaitUntilEntityLoaded(directionConditionDto.TargetNodeId, null);
                        return new DirectionPoseCondition(directionConditionDto, targetNodeInstance.transform, skeletonService.PersonalSkeleton.TrackedSubskeleton);
                    }
                case BoneRotationConditionDto boneRotationConditionDto:
                    {
                        return new BoneRotationPoseCondition(boneRotationConditionDto, skeletonService.PersonalSkeleton.TrackedSubskeleton);
                    }
                case ScaleConditionDto scaleConditionDto:
                    {
                        UMI3DNodeInstance targetNodeInstance = (UMI3DNodeInstance)await loadingService.WaitUntilEntityLoaded(scaleConditionDto.TargetId, null);
                        return new ScalePoseCondition(scaleConditionDto, targetNodeInstance.transform);
                    }
                case EnvironmentPoseConditionDto environmentPoseConditionDto:
                    {
                        EnvironmentPoseCondition environmentPoseCondition = new(environmentPoseConditionDto);
                        environmentService.RegisterEntity(environmentPoseConditionDto.Id, environmentPoseConditionDto, environmentPoseCondition).NotifyLoaded();
                        return environmentPoseCondition;
                    }

                default:
                    return await Task.FromResult<IPoseCondition>(null);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            return Task.FromResult(false);
        }
    }
}