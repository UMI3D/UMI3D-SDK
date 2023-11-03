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

namespace umi3d.edk.userCapture.pose
{
    /// <summary>
    /// Service that handle poses from the environment side.
    /// </summary>
    public class PoseManager : Singleton<PoseManager>, IPoseManager
    {
        #region Dependency Injection

        public PoseManager() : base()
        {
            Init();
        }

        #endregion Dependency Injection

        /// <summary>
        /// A bool to make sure the initialisation only occurs once
        /// </summary>
        private bool initialized = false;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IDictionary<ulong, IList<PoseClip>> Poses => poses;

        private readonly Dictionary<ulong, IList<PoseClip>> poses = new();

        /// <inheritdoc/>
        public IList<UMI3DPoseAnimator> PoseAnimators { get; private set; } = new List<UMI3DPoseAnimator>();

        /// <inheritdoc/>
        public PoseClip RegisterUserCustomPose(ulong userId, IUMI3DPoseData poseResource)
        {
            PoseClip poseToAdd = new(poseResource);
            poseToAdd.Id(); // register
            if (poses.ContainsKey(userId))
            {
                if (!poses[userId].Contains(poseToAdd))
                    poses[userId].Add(poseToAdd);
            }
            else
                poses.Add(userId, new List<PoseClip>() { poseToAdd });
            return poseToAdd;
        }

        /// <summary>
        /// Inits all the poses and pose overriders to make them ready for dto server-client exchanges
        /// </summary>
        private void Init()
        {
            if (!initialized)
            {
                foreach (var overrider in UnityEngine.Object.FindObjectsOfType<UMI3DPoseAnimator>())
                {
                    overrider.Id(); // register
                    if (!PoseAnimators.Contains(overrider))
                        PoseAnimators.Add(overrider);
                }

                initialized = true;
            }
        }

        public PoseClip RegisterEnvironmentPose(IUMI3DPoseData poseResource)
        {
            var pose = new PoseClip(poseResource); ;
            pose.Id(); // register

            if (poses.ContainsKey(UMI3DGlobalID.EnvironementId))
            {
                if (!poses[UMI3DGlobalID.EnvironementId].Contains(pose))
                    poses[UMI3DGlobalID.EnvironementId].Add(pose);
            }
            else
                poses.Add(UMI3DGlobalID.EnvironementId, new List<PoseClip>() { pose });

            return pose;
        }

        public void RegisterPoseOverrider(UMI3DPoseAnimator overrider)
        {
            if (overrider == null)
                throw new System.ArgumentNullException(nameof(overrider));

            PoseAnimators.Add(overrider);
        }
    }
}