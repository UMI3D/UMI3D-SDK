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
using System.Linq;

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
        protected readonly Dictionary<ulong, IPoseOverridersContainerProcessor> poseOverridersContainerProcessors = new Dictionary<ulong, IPoseOverridersContainerProcessor>();

        private readonly List<PoseOverridersContainer> containers = new();

        #region Dependency Injection

        private readonly ISkeletonManager skeletonManager;
        private readonly ILoadingManager loadingManager;
        private readonly IUMI3DClientServer clientServerService;

        public PoseManager() : this(PersonalSkeletonManager.Instance, UMI3DEnvironmentLoader.Instance, UMI3DClientServer.Instance)
        { }

        public PoseManager(ISkeletonManager skeletonManager, ILoadingManager loadingManager, IUMI3DClientServer clientServerService)
        {
            this.skeletonManager = skeletonManager;
            this.loadingManager = loadingManager;
            this.clientServerService = clientServerService;
            
            Init();
        }

        #endregion Dependency Injection

        private void Init()
        {
            Poses.Add(UMI3DGlobalID.EnvironementId, new List<SkeletonPose>());
            clientServerService.OnLeaving.AddListener(Reset);
            clientServerService.OnRedirection.AddListener(Reset);
        }

        private void Reset()
        {
            StopAllPoses();
            containers.ToArray().ForEach(x => RemovePoseOverriders(x));
            Poses.Clear();
            Poses.Add(UMI3DGlobalID.EnvironementId, new List<SkeletonPose>());
        }

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

            if (containers.Contains(container))
                return;

            if (poseOverridersContainerProcessors.ContainsKey(container.NodeId))
                return;

            containers.Add(container);

            PoseOverridersContainerProcessor unit = new (container);
            unit.ConditionsValidated += ApplyPoseOverride;
            unit.ConditionsInvalided += StopPoseOverride;
            poseOverridersContainerProcessors.Add(container.NodeId, unit);
        }

        /// <inheritdoc/>
        public void RemovePoseOverriders(PoseOverridersContainer container)
        {
            if (container == null)
                throw new System.ArgumentNullException(nameof(container));

            if (!containers.Contains(container)
                || !poseOverridersContainerProcessors.TryGetValue(container.NodeId, out IPoseOverridersContainerProcessor unit))
                return;
            
            unit.ConditionsValidated -= ApplyPoseOverride;
            unit.ConditionsInvalided -= StopPoseOverride;
            unit.StopWatchActivationConditions();
            poseOverridersContainerProcessors.Remove(container.NodeId);
            containers.Remove(container);
        }

        /// <inheritdoc/>
        public bool TryActivatePoseOverriders(ulong nodeId, PoseActivationMode poseActivationMode)
        {
            if (!poseOverridersContainerProcessors.TryGetValue(nodeId, out IPoseOverridersContainerProcessor unit))
                return false;

            return unit.TryActivate(poseActivationMode);
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