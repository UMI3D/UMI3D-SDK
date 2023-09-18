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
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.userCapture.pose;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Manager that handles poses.
    /// </summary>
    public class PoseManager : Singleton<PoseManager>, IPoseManager
    {
        /// <summary>
        /// All the local pose of the client
        /// </summary>
        public PoseDto[] localPoses = new PoseDto[0];

        /// <summary>
        /// All the poses in the experience key: userID, value : list of the poses of the related user
        /// </summary>
        public IDictionary<ulong, IList<SkeletonPose>> Poses { get; set; } = new Dictionary<ulong, IList<SkeletonPose>>();

        /// <summary>
        /// All pose condition processors indexed by related node id.
        /// </summary>
        protected Dictionary<ulong, IPoseOverridersContainerProcessor> poseOverridersContainerProcessors = new Dictionary<ulong, IPoseOverridersContainerProcessor>();

        #region Dependency Injection

        private readonly ISkeletonManager skeletonManager;
        private readonly ILoadingManager loadingManager;

        public PoseManager()
        {
            skeletonManager = PersonalSkeletonManager.Instance;
            loadingManager = UMI3DEnvironmentLoader.Instance;
            Poses.Add(UMI3DGlobalID.EnvironementId, new List<SkeletonPose>());
        }

        public PoseManager(ISkeletonManager skeletonManager, ILoadingManager loadingManager)
        {
            this.skeletonManager = skeletonManager;
            this.loadingManager = loadingManager;
            Poses.Add(UMI3DGlobalID.EnvironementId, new List<SkeletonPose>());
        }

        #endregion Dependency Injection

        public void InitLocalPoses()
        {
            var clientPoses = (loadingManager.AbstractLoadingParameters as IUMI3DUserCaptureLoadingParameters).ClientPoses;
            localPoses = new PoseDto[clientPoses.Count];
            for (int i = 0; i < clientPoses.Count; i++)
            {
                PoseDto poseDto = clientPoses[i].ToDto();
                poseDto.index = i;
                localPoses[i] = poseDto;
            }
        }

        /// <inheritdoc/>
        public void AddPoseOverriders(PoseOverridersContainer container)
        {
            if (container == null)
                throw new System.ArgumentNullException(nameof(container));

            if (poseOverridersContainerProcessors.ContainsKey(container.NodeId))
                return;

            PoseOverridersContainerProcessor unit = new PoseOverridersContainerProcessor(container);
            unit.ConditionsValidated += ApplyPoseOverride;
            unit.ConditionsInvalided += StopPoseOverride;
            poseOverridersContainerProcessors.Add(container.NodeId, unit);
        }

        /// <inheritdoc/>
        public void RemovePoseOverriders(PoseOverridersContainer container)
        {
            if (container == null)
                throw new System.ArgumentNullException(nameof(container));

            if (poseOverridersContainerProcessors.TryGetValue(container.NodeId, out IPoseOverridersContainerProcessor unit))
            {
                unit.ConditionsValidated -= ApplyPoseOverride;
                unit.StopWatchNonInteractionalConditions();
                poseOverridersContainerProcessors.Remove(container.NodeId);
            }
        }

        /// <inheritdoc/>
        public void TryActivatePoseOverriders(ulong nodeId, PoseActivationMode poseActivationMode)
        {
            if (poseOverridersContainerProcessors.TryGetValue(nodeId, out IPoseOverridersContainerProcessor unit))
            {
                unit.TryActivate(poseActivationMode);
            }
        }

        /// <inheritdoc/>
        public void ApplyPoseOverride(PoseOverrider poseOverrider)
        {
            if (poseOverrider == null)
                throw new System.ArgumentNullException(nameof(poseOverrider));

            foreach (SkeletonPose pose in Poses[UMI3DGlobalID.EnvironementId])
            {
                if (pose.Index == poseOverrider.PoseIndexInPoseManager)
                {
                    skeletonManager.PersonalSkeleton.PoseSubskeleton.StartPose(pose);
                    return;
                }
            }
        }

        /// <inheritdoc/>
        public void StopPoseOverride(PoseOverrider poseOverrider)
        {
            if (poseOverrider == null)
                throw new System.ArgumentNullException(nameof(poseOverrider));

            foreach (SkeletonPose pose in Poses[UMI3DGlobalID.EnvironementId])
            {
                if (pose.Index == poseOverrider.PoseIndexInPoseManager)
                {
                    skeletonManager.PersonalSkeleton.PoseSubskeleton.StopPose(pose);
                    return;
                }
            }
        }

        /// <inheritdoc/>
        public void StopAllPoses()
        {
            skeletonManager.PersonalSkeleton.PoseSubskeleton.StopAllPoses();
        }
    }
}