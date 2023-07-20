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
using umi3d.cdk.userCapture.tracking;
using umi3d.common;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using umi3d.common.userCapture.tracking;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Subskeleton that handles body poses.
    /// </summary>
    public class PoseSubskeleton : IWritableSubskeleton
    {
        #region Dependency Injection

        private readonly IPoseManager poseManagerService;

        public PoseSubskeleton()
        {
            poseManagerService = PoseManager.Instance;
        }

        public PoseSubskeleton(IPoseManager poseManager)
        {
            this.poseManagerService = poseManager;
        }

        #endregion Dependency Injection

        public IReadOnlyList<SkeletonPose> ActivatedPoses => activatedPoses;
        protected List<SkeletonPose> activatedPoses = new();

        /// <summary>
        /// Set a pose for the calculation of the next tracking frame
        /// </summary>
        /// <param name="isOverriding"></param>
        /// <param name="posesToAdd"></param>
        /// <param name="isServerPose"></param>
        public void StartPose(IEnumerable<SkeletonPose> posesToAdd, bool isOverriding = false)
        {
            if (posesToAdd == null)
                return;

            if (isOverriding)
                activatedPoses.Clear();

            activatedPoses.AddRange(posesToAdd);
        }

        /// <summary>
        /// Set a pose for the calculation of the next tracking frame
        /// </summary>
        /// <param name="isOverriding"></param>
        /// <param name="poseToAdd"></param>
        /// <param name="isServerPose"></param>
        public void StartPose(SkeletonPose poseToAdd, bool isOverriding = false)
        {
            if (poseToAdd == null)
                return;

            if (isOverriding)
                activatedPoses.Clear();

            activatedPoses.Add(poseToAdd);
        }

        /// <summary>
        /// Stops a specific set of poses
        /// </summary>
        /// <param name="posesToStop"></param>
        /// <param name="isServerPose"></param>
        public void StopPose(IEnumerable<SkeletonPose> posesToStop)
        {
            if (posesToStop == null)
                return;
            posesToStop.ForEach(pts =>
            {
                activatedPoses.Remove(pts);
            });
        }

        /// <summary>
        /// Stops a specific set of poses
        /// </summary>
        /// <param name="posesToStop"></param>
        /// <param name="isServerPose"></param>
        public void StopPose(SkeletonPose poseToStop)
        {
            if (poseToStop == null)
                return;
            activatedPoses.Remove(poseToStop);
        }

        /// <summary>
        /// Stops a specific set of poses
        /// </summary>
        /// <param name="posesToStop"></param>
        /// <param name="isServerPose"></param>
        public void StopPose(IEnumerable<int> posesToStopIds)
        {
            posesToStopIds.ForEach(poseId =>
            {
                activatedPoses.Remove(activatedPoses.Find(x => x.Index == poseId));
            });
        }

        /// <summary>
        /// Stops a specific set of poses
        /// </summary>
        /// <param name="posesToStop"></param>
        /// <param name="isServerPose"></param>
        public void StopPose(int poseToStopId)
        {
            activatedPoses.Remove(activatedPoses.Find(x => x.Index == poseToStopId));
        }

        /// <summary>
        /// stops all the poses s
        /// </summary>
        public void StopAllPoses()
        {
            activatedPoses.Clear();
        }

        /// <summary>
        ///last in has priority,,, server poses have priority
        /// </summary>
        /// <returns></returns>
        public PoseDto GetPose()
        {
            PoseDto poseDto = new PoseDto() { bones = new List<BoneDto>() };
            foreach (var pose in activatedPoses)
            {
                foreach (var bone in pose.Bones)
                {
                    int indexOf = poseDto.bones.FindIndex(a => a.boneType == bone.boneType);
                    if (indexOf != -1)
                    {
                        poseDto.bones[indexOf] = bone;
                    }
                    else
                    {
                        poseDto.bones.Add(bone);
                    }
                }
            }

            return poseDto;
        }

        /// <summary>
        /// Updates the state of the pose manager using the tracking frame
        /// </summary>
        /// <param name="trackingFrame"></param>
        public void UpdateBones(UserTrackingFrameDto trackingFrame)
        {
            // add new poses
            foreach (var poseIndex in trackingFrame.customPosesIndexes)
            {
                if (!poseManagerService.Poses.TryGetValue(trackingFrame.userId, out IList<SkeletonPose> userPoses))
                    continue;

                var pose = userPoses[poseIndex];

                if (!activatedPoses.Contains(pose))
                    StartPose(pose);
            }

            foreach (var poseIndex in trackingFrame.environmentPosesIndexes)
            {
                if (!poseManagerService.Poses.TryGetValue(UMI3DGlobalID.EnvironementId, out IList<SkeletonPose> userPoses))
                    continue;

                var pose = userPoses[poseIndex];

                if (!activatedPoses.Contains(pose))
                    StartPose(pose);
            }

            // remove not activated poses
            int nbObjToRemove = activatedPoses.Count - (trackingFrame.customPosesIndexes.Count + trackingFrame.environmentPosesIndexes.Count);
            if (nbObjToRemove > 0)
            {
                Queue<SkeletonPose> posesToRemove = new Queue<SkeletonPose>(nbObjToRemove);
                activatedPoses.ForEach(pose =>
                {
                    if (!trackingFrame.customPosesIndexes.Contains(pose.Index) && !trackingFrame.environmentPosesIndexes.Contains(pose.Index))
                        posesToRemove.Enqueue(pose);
                });
                foreach (SkeletonPose pose in posesToRemove)
                    StopPose(pose);
            }
        }

        /// <summary>
        /// Add the poses to the tracking frame
        /// </summary>
        /// <param name="trackingFrame"></param>
        /// <param name="option"></param>
        public void WriteTrackingFrame(UserTrackingFrameDto trackingFrame, TrackingOption option)
        {
            trackingFrame.customPosesIndexes ??= new();
            trackingFrame.environmentPosesIndexes ??= new();

            activatedPoses.ForEach((pose) =>
            {
                if (pose.IsCustom)
                {
                    trackingFrame.customPosesIndexes.Add(pose.Index);
                }
                else
                {
                    trackingFrame.environmentPosesIndexes.Add(pose.Index);
                }
            });
        }
    }
}