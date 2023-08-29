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
    /// Subskeleton that receive poses on its bones.
    /// </summary>
    public class PoseSubskeleton : IPoseSubskeleton
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

        /// <inheritdoc/>
        public IReadOnlyList<SkeletonPose> AppliedPoses => appliedPoses;
        protected List<SkeletonPose> appliedPoses = new();

        public int Priority => 100;

        /// <inheritdoc/>
        public void StartPose(IEnumerable<SkeletonPose> posesToAdd, bool isOverriding = false)
        {
            if (posesToAdd == null)
                return;

            if (isOverriding)
                StopAllPoses();

            appliedPoses.AddRange(posesToAdd);
        }

        /// <inheritdoc/>
        public void StartPose(SkeletonPose poseToAdd, bool isOverriding = false)
        {
            if (poseToAdd == null)
                return;

            if (isOverriding)
                appliedPoses.Clear();

            appliedPoses.Add(poseToAdd);
        }

        /// <inheritdoc/>
        public void StopPose(IEnumerable<SkeletonPose> posesToStop)
        {
            if (posesToStop == null)
                return;
            posesToStop.ForEach(pts =>
            {
                appliedPoses.Remove(pts);
            });
        }

        /// <inheritdoc/>
        public void StopPose(SkeletonPose poseToStop)
        {
            if (poseToStop == null)
                return;
            appliedPoses.Remove(poseToStop);
        }

        /// <inheritdoc/>
        public void StopPose(IEnumerable<int> posesToStopIds)
        {
            posesToStopIds.ForEach(poseId =>
            {
                appliedPoses.Remove(appliedPoses.Find(x => x.Index == poseId));
            });
        }

        /// <inheritdoc/>
        public void StopPose(int poseToStopId)
        {
            appliedPoses.Remove(appliedPoses.Find(x => x.Index == poseToStopId));
        }

        /// <inheritdoc/>
        public void StopAllPoses()
        {
            appliedPoses.Clear();
        }

        /// <inheritdoc/>
        public PoseDto GetPose()
        {
            PoseDto poseDto = new PoseDto() { bones = new List<BoneDto>() };
            foreach (var pose in appliedPoses)
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

        /// <inheritdoc/>
        public void UpdateBones(UserTrackingFrameDto trackingFrame)
        {
            // add new poses
            foreach (var poseIndex in trackingFrame.customPosesIndexes)
            {
                if (!poseManagerService.Poses.TryGetValue(trackingFrame.userId, out IList<SkeletonPose> userPoses))
                    continue;

                var pose = userPoses[poseIndex];

                if (!appliedPoses.Contains(pose))
                    StartPose(pose);
            }

            foreach (var poseIndex in trackingFrame.environmentPosesIndexes)
            {
                if (!poseManagerService.Poses.TryGetValue(UMI3DGlobalID.EnvironementId, out IList<SkeletonPose> userPoses))
                    continue;

                var pose = userPoses[poseIndex];

                if (!appliedPoses.Contains(pose))
                    StartPose(pose);
            }

            // remove not activated poses
            int nbObjToRemove = appliedPoses.Count - (trackingFrame.customPosesIndexes.Count + trackingFrame.environmentPosesIndexes.Count);
            if (nbObjToRemove > 0)
            {
                Queue<SkeletonPose> posesToRemove = new Queue<SkeletonPose>(nbObjToRemove);
                appliedPoses.ForEach(pose =>
                {
                    if (!trackingFrame.customPosesIndexes.Contains(pose.Index) && !trackingFrame.environmentPosesIndexes.Contains(pose.Index))
                        posesToRemove.Enqueue(pose);
                });
                foreach (SkeletonPose pose in posesToRemove)
                    StopPose(pose);
            }
        }

        /// <inheritdoc/>
        public void WriteTrackingFrame(UserTrackingFrameDto trackingFrame, TrackingOption option)
        {
            trackingFrame.customPosesIndexes ??= new();
            trackingFrame.environmentPosesIndexes ??= new();

            appliedPoses.ForEach((pose) =>
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