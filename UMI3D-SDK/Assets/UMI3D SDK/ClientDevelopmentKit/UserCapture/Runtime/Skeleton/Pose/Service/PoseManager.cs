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

using inetum.unityUtils;
using System.Linq;

using umi3d.common;
using umi3d.common.userCapture.description;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Manager that handles poses.
    /// </summary>
    public class PoseManager : Singleton<PoseManager>, IPoseManager
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.UserCapture;

        #region Dependency Injection

        private readonly ISkeletonManager skeletonManager;
        private readonly IEnvironmentManager environmentManager;
        private readonly IUMI3DClientServer clientServerService;

        public PoseManager() : this(PersonalSkeletonManager.Instance, UMI3DEnvironmentLoader.Instance, UMI3DClientServer.Instance)
        { }

        public PoseManager(ISkeletonManager skeletonManager, IEnvironmentManager environmentManager, IUMI3DClientServer clientServerService)
        {
            this.skeletonManager = skeletonManager;
            this.environmentManager = environmentManager;
            this.clientServerService = clientServerService;

            Init();
        }

        #endregion Dependency Injection

        private void Init()
        {
            clientServerService.OnLeaving.AddListener(Reset);
            clientServerService.OnRedirection.AddListener(Reset);
        }

        private void Reset()
        {
            StopAllPoses();
        }

        /// <inheritdoc/>
        public bool TryActivatePoseAnimator(ulong environmentId, ulong poseAnimatorId)
        {
            if (!environmentManager.TryGetEntity(environmentId, poseAnimatorId, out IPoseAnimator poseAnimator))
            {
                UMI3DLogger.LogWarning($"Unable to try to activate pose animator {environmentId} {poseAnimatorId}. Entity {poseAnimatorId} not found.", DEBUG_SCOPE);
                return false;
            }

            return poseAnimator.TryActivate();
        }

        public bool TryDeactivatePoseAnimator(ulong environmentId, ulong poseAnimatorId)
        {
            if (!environmentManager.TryGetEntity(environmentId, poseAnimatorId, out IPoseAnimator poseAnimator))
            {
                UMI3DLogger.LogWarning($"Unable to try to deactivate pose animator {environmentId} {poseAnimatorId}. Entity {poseAnimatorId} not found.", DEBUG_SCOPE);
                return false;
            }

            return poseAnimator.TryDeactivate();
        }

        /// <inheritdoc/>
        public void PlayPoseClip(PoseClip poseClip, PoseAnchorDto anchorToForce = null, ISubskeletonDescriptionInterpolationPlayer.PlayingParameters parameters = null)
        {
            if (poseClip == null)
                throw new System.ArgumentNullException(nameof(poseClip));

            skeletonManager.PersonalSkeleton.PoseSubskeleton.StartPose(poseClip, parameters: parameters, anchorToForce: anchorToForce);
        }

        /// <inheritdoc/>
        public void StopPoseClip(PoseClip poseClip)
        {
            if (poseClip == null)
                throw new System.ArgumentNullException(nameof(poseClip));

            if (skeletonManager?.PersonalSkeleton?.PoseSubskeleton == null)
                return; // skeleton was deleted in the meanwhile

            if (!skeletonManager.PersonalSkeleton.PoseSubskeleton.AppliedPoses.Contains(poseClip))
                return;

            skeletonManager.PersonalSkeleton.PoseSubskeleton.StopPose(poseClip);
        }

        /// <inheritdoc/>
        public void StopAllPoses()
        {
            skeletonManager.PersonalSkeleton.PoseSubskeleton.StopAllPoses();
        }

        /// <inheritdoc/>
        public void ChangeEnvironmentPoseCondition(ulong environmentId, ulong poseConditionId, bool shouldBeValidated)
        {
            EnvironmentPoseCondition condition = environmentManager.GetEntityObject<EnvironmentPoseCondition>(environmentId, poseConditionId);

            if (shouldBeValidated)
                condition.Validate();
            else
                condition.Invalidate();
        }
    }
}