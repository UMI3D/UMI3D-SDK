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

using UnityEngine;

namespace umi3d.cdk.userCapture.pose
{
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
        /// All pose condition processors
        /// </summary>
        public Dictionary<ulong, PoseConditionProcessor> conditionProcessors = new Dictionary<ulong, PoseConditionProcessor>();

        #region Dependency Injection

        private readonly ISkeletonManager skeletonManager;
        private readonly ILoadingManager loadingManager;

        public PoseManager()
        {
            skeletonManager = PersonalSkeletonManager.Instance;
            loadingManager = UMI3DEnvironmentLoader.Instance;
        }

        public PoseManager(ISkeletonManager skeletonManager, ILoadingManager loadingManager)
        {
            this.skeletonManager = skeletonManager;
            this.loadingManager = loadingManager;
        }

        #endregion Dependency Injection

        public void InitLocalPoses()
        {
            List<UMI3DPose_so> clientPoses = (loadingManager.AbstractLoadingParameters as IUMI3DUserCaptureLoadingParameters).ClientPoses;
            localPoses = new PoseDto[clientPoses.Count];
            for (int i = 0; i < clientPoses.Count; i++)
            {
                PoseDto poseDto = clientPoses[i].ToDto();
                poseDto.index = i;
                localPoses[i] = poseDto;
            }
        }

        /// <inheritdoc/>
        public void SubscribePoseConditionProcessor(PoseOverriderContainer overrider)
        {
            if (conditionProcessors.ContainsKey(overrider.NodeId))
                return;

            PoseConditionProcessor unit = new PoseConditionProcessor(overrider);
            unit.ConditionValidated += ApplyPoseOverride;
            unit.ConditionDeactivated += StopPoseOverride;
            conditionProcessors.Add(overrider.NodeId, unit);
        }

        /// <inheritdoc/>
        public void UnsubscribePoseConditionProcessor(PoseOverriderContainer overrider)
        {
            if (conditionProcessors.TryGetValue(overrider.NodeId, out PoseConditionProcessor unit))
            {
                unit.ConditionValidated -= ApplyPoseOverride;
                unit.DisableCheck();
                conditionProcessors.Remove(overrider.NodeId);
            }
        }

        /// <inheritdoc/>
        public void TryActivatePose(ulong id, PoseActivationMode poseActivationMode)
        {
            if (conditionProcessors.TryGetValue(id, out PoseConditionProcessor unit))
            {
                Debug.Log("Activation de pose");
                unit.TryActivate(poseActivationMode);
            }
        }

        /// <inheritdoc/>
        public void ApplyPoseOverride(PoseOverrider poseOverrider)
        {
            Debug.Log("Set target pose");
            if (poseOverrider != null)
            {
                foreach (SkeletonPose pose in Poses[UMI3DGlobalID.EnvironementId])
                {
                    Debug.Log("Start de pose");
                    if (pose.Index == poseOverrider.PoseIndexInPoseManager)
                    {
                        skeletonManager.PersonalSkeleton.PoseSubskeleton.StartPose(pose);
                        return;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void StopPoseOverride(PoseOverrider poseOverrider)
        {
            if (poseOverrider != null)
            {
                foreach (SkeletonPose pose in Poses[UMI3DGlobalID.EnvironementId])
                {
                    if (pose.Index == poseOverrider.PoseIndexInPoseManager)
                    {
                        skeletonManager.PersonalSkeleton.PoseSubskeleton.StopPose(pose);
                        return;
                    }
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