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
using System;
using System.Collections.Generic;
using System.Linq;
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
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.UserCapture;

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

        public int Priority => PRIORITY;

        private const int PRIORITY = 100;

        /// <inheritdoc/>
        public void StartPose(IEnumerable<SkeletonPose> posesToAdd, bool isOverriding = false)
        {
            if (posesToAdd == null)
                throw new ArgumentNullException(nameof(posesToAdd), $"Cannot start pose.");

            if (isOverriding)
                StopAllPoses();

            appliedPoses.AddRange(posesToAdd);
        }

        /// <inheritdoc/>
        public void StartPose(SkeletonPose poseToAdd, bool isOverriding = false)
        {
            if (poseToAdd == null)
                throw new ArgumentNullException(nameof(poseToAdd), $"Cannot start pose.");

            if (isOverriding)
                StopAllPoses();

            appliedPoses.Add(poseToAdd);
        }

        /// <inheritdoc/>
        public void StopPose(IEnumerable<SkeletonPose> posesToStop)
        {
            if (posesToStop == null)
                return;

            posesToStop.ForEach(StopPose);
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
            StopPose(posesToStopIds.Select(poseId => appliedPoses.Find(x => x.Index == poseId)));
        }

        /// <inheritdoc/>
        public void StopPose(int poseToStopId)
        {
            StopPose(appliedPoses.Find(x => x.Index == poseToStopId));
        }

        /// <inheritdoc/>
        public void StopAllPoses()
        {
            appliedPoses.Clear();
        }

        /// <inheritdoc/>
        public SubSkeletonPoseDto GetPose(UMI3DSkeletonHierarchy hierarchy)
        {
            if (hierarchy == null)
                throw new ArgumentNullException(nameof(hierarchy));

            SubSkeletonPoseDto poseDto = new SubSkeletonPoseDto() { bones = new List<SubSkeletonBoneDto>() };
            foreach (var pose in appliedPoses)
            {
                foreach (var bone in pose.Bones)
                {
                    int indexOf = poseDto.bones.FindIndex(a => a.boneType == bone.boneType);

                    SubSkeletonBoneDto bonePose = GetBonePose(hierarchy, bone, pose).subBone;
                    if (indexOf != -1)
                        poseDto.bones[indexOf] = bonePose;
                    else
                        poseDto.bones.Add(bonePose);
                }
            }

            computedMap.Clear();
            return poseDto;
        }


        private Dictionary<uint, (BoneDto bone, SubSkeletonBoneDto subBone)> computedMap = new();

        /// <summary>
        /// Recursively compute local rotation for a bone.
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <param name="boneDto"></param>
        /// <param name="pose"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private (BoneDto bone, SubSkeletonBoneDto subBone) GetBonePose(UMI3DSkeletonHierarchy hierarchy, BoneDto boneDto, SkeletonPose pose)
        {
            if (boneDto == null)
                throw new ArgumentNullException(nameof(boneDto));

            if (computedMap.ContainsKey(boneDto.boneType))
                return computedMap[boneDto.boneType];

            if (!hierarchy.Relations.ContainsKey(boneDto.boneType))
                throw new ArgumentException($"Bone ({boneDto.boneType}, {BoneTypeHelper.GetBoneName(boneDto.boneType)}) not defined in hierarchy.", nameof(boneDto));

            var relation = hierarchy.Relations[boneDto.boneType];

            var parentBone = pose.Bones.Find(b => b.boneType == relation.boneTypeParent);

            SubSkeletonBoneDto subBone = new() { boneType= boneDto.boneType };
            if (parentBone == default) // bone has no parent
            {
                subBone.localRotation = boneDto.rotation;
            }
            else // bone has a parent and thus its rotation depends on it
            {
                var parent = GetBonePose(hierarchy, parentBone, pose);
                subBone.localRotation = (UnityEngine.Quaternion.Inverse(parent.bone.rotation.Quaternion()) * boneDto.rotation.Quaternion()).Dto();
            }

            computedMap[boneDto.boneType] = new()
            {
                bone = boneDto,
                subBone = subBone
            };

            return computedMap[boneDto.boneType];
        }

        /// <inheritdoc/>
        public void UpdateBones(UserTrackingFrameDto trackingFrame)
        {
            if (trackingFrame is null)
                throw new ArgumentNullException(nameof(trackingFrame));

            StartUserCustomPoses(trackingFrame);

            StartEnvironmentPoses(trackingFrame);

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
                StopPose(posesToRemove);
            }
        }

        private void StartUserCustomPoses(UserTrackingFrameDto trackingFrame)
        {
            if (trackingFrame.customPosesIndexes.Count > 0)
            {
                if (poseManagerService.Poses.TryGetValue(trackingFrame.userId, out IList<SkeletonPose> userPoses))
                {
                    foreach (var poseIndex in trackingFrame.customPosesIndexes)
                    {
                        if (poseIndex <= 0 || poseIndex >= userPoses.Count)
                        {
                            UMI3DLogger.LogWarning($"Cannot apply custom pose of user {trackingFrame.userId}. Invalid pose index {poseIndex}.", DEBUG_SCOPE);
                            continue;
                        }

                        var pose = userPoses[poseIndex];

                        if (!appliedPoses.Contains(pose))
                            StartPose(pose);
                    }
                }
                else
                {
                    UMI3DLogger.LogWarning($"Cannot apply custom pose of user {trackingFrame.userId}. User's custom poses not found.", DEBUG_SCOPE);
                }
            }
        }

        private void StartEnvironmentPoses(UserTrackingFrameDto trackingFrame)
        {
            foreach (var poseIndex in trackingFrame.environmentPosesIndexes)
            {
                if (!poseManagerService.Poses.TryGetValue(UMI3DGlobalID.EnvironementId, out IList<SkeletonPose> userPoses))
                {
                    UMI3DLogger.LogWarning($"Cannot apply environment pose for user {trackingFrame.userId}. Environment poses not found.", DEBUG_SCOPE);
                    continue;
                }
                if (poseIndex <= 0 || poseIndex >= userPoses.Count)
                {
                    UMI3DLogger.LogWarning($"Cannot apply environment pose for user {trackingFrame.userId}. Invalid pose index {poseIndex}.", DEBUG_SCOPE);
                    continue;
                }

                var pose = userPoses[poseIndex];

                if (!appliedPoses.Contains(pose))
                    StartPose(pose);
            }
        }

        /// <inheritdoc/>
        public void WriteTrackingFrame(UserTrackingFrameDto trackingFrame, TrackingOption option)
        {
            if (trackingFrame == null)
                throw new ArgumentNullException(nameof(trackingFrame));

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