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

namespace umi3d.edk.userCapture.pose
{
    /// <summary>
    /// Service that handle poses from the environment side.
    /// </summary>
    public class PoseManager : Singleton<PoseManager>, IPoseManager
    {
        #region Dependency Injection

        private readonly IUMI3DServer umi3dServerService;

        public PoseManager() : this(umi3dServerService: UMI3DServer.Instance)
        {
        }

        public PoseManager(IUMI3DServer umi3dServerService) : base()
        {
            this.umi3dServerService = umi3dServerService;

            Init();
        }

        #endregion Dependency Injection

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IDictionary<ulong, IList<PoseClip>> PoseClipsByUser => poses;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IReadOnlyList<PoseClip> PoseClips => poses.Values.SelectMany(x => x).ToList();

        private readonly Dictionary<ulong, IList<PoseClip>> poses = new();

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

        /// <inheritdoc/>
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

        #region Lifecycle

        /// <summary>
        /// Inits all the poses and pose overriders to make them ready for dto server-client exchanges
        /// </summary>
        private void Init()
        {
            umi3dServerService.OnUserActive.AddListener(DispatchPoseClips);
            umi3dServerService.OnUserMissing.AddListener(CleanPoseClips);
            umi3dServerService.OnUserLeave.AddListener(CleanPoseClips);
        }

        /// <summary>
        /// Send pose clip to a new user.
        /// </summary>
        /// <param name="user"></param>
        private void DispatchPoseClips(UMI3DUser user)
        {
            if (poses.Count == 0)
                return;

            Transaction t = new() { reliable = true };
            foreach (var poseClip in PoseClips)
            {
                t.AddIfNotNull(poseClip.GetLoadEntity(new() { user }));
            }
            t.Dispatch();
        }

        /// <summary>
        /// Delete pose clips of old user.
        /// </summary>
        /// <param name="user"></param>
        private void CleanPoseClips(UMI3DUser user)
        {
            if (!PoseClipsByUser.ContainsKey(user.Id()))
                return;
            PoseClipsByUser.Remove(user.Id());
        }

        #endregion Lifecycle
    }
}