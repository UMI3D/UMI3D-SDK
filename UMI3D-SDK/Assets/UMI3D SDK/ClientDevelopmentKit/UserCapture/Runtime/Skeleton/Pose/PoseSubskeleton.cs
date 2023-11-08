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
using umi3d.common.userCapture;
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

        private readonly IEnvironmentManager environmentManagerService;

        public PoseSubskeleton() : this(environmentManagerService: UMI3DEnvironmentLoader.Instance)
        {
        }

        public PoseSubskeleton(IEnvironmentManager environmentManagerService)
        {
            this.environmentManagerService = environmentManagerService;
        }

        #endregion Dependency Injection

        /// <inheritdoc/>
        public IReadOnlyList<PoseClip> AppliedPoses => appliedPoses;

        protected List<PoseClip> appliedPoses = new();

        public int Priority => PRIORITY;

        private const int PRIORITY = 100;

        /// <inheritdoc/>
        public void StartPose(IEnumerable<PoseClip> posesToAdd, bool isOverriding = false)
        {
            if (posesToAdd == null)
                throw new ArgumentNullException(nameof(posesToAdd), $"Cannot start poses.");

            if (isOverriding)
                StopAllPoses();

            appliedPoses.AddRange(posesToAdd);
        }

        /// <inheritdoc/>
        public void StartPose(PoseClip poseToAdd, bool isOverriding = false)
        {
            if (poseToAdd == null)
                throw new ArgumentNullException(nameof(poseToAdd), $"Cannot start pose.");

            if (isOverriding)
                StopAllPoses();

            appliedPoses.Add(poseToAdd);
        }

        /// <inheritdoc/>
        public void StopPose(IEnumerable<PoseClip> posesToStop)
        {
            if (posesToStop == null)
                return;

            posesToStop.ForEach(StopPose);
        }

        /// <inheritdoc/>
        public void StopPose(PoseClip poseToStop)
        {
            if (poseToStop == null)
                return;

            appliedPoses.Remove(poseToStop);
        }

        /// <inheritdoc/>
        public void StopPose(IEnumerable<ulong> posesToStopIds)
        {
            StopPose(posesToStopIds.Select(poseId => appliedPoses.Find(x => x.Id == poseId)));
        }

        /// <inheritdoc/>
        public void StopPose(ulong poseToStopId)
        {
            StopPose(appliedPoses.Find(x => x.Id == poseToStopId));
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
        private (BoneDto bone, SubSkeletonBoneDto subBone) GetBonePose(UMI3DSkeletonHierarchy hierarchy, BoneDto boneDto, PoseClip pose)
        {
            if (boneDto == null)
                throw new ArgumentNullException(nameof(boneDto));

            uint boneType = boneDto.boneType;

            if (computedMap.ContainsKey(boneType))
                return computedMap[boneType];

            if (!hierarchy.Relations.ContainsKey(boneType))
                throw new ArgumentException($"Bone ({boneType}, \"{BoneTypeHelper.GetBoneName(boneType)}\") not defined in hierarchy.", nameof(boneDto));

            var relation = hierarchy.Relations[boneType];

            var parentBone = pose.Bones.Find(b => b.boneType == relation.boneTypeParent);

            SubSkeletonBoneDto subBone = new() { boneType = boneType };
            if (parentBone == default || parentBone.boneType == BoneType.None) // bone has no parent
            {
                subBone.localRotation = boneDto.rotation;
            }
            else // bone has a parent and thus its rotation depends on it
            {
                var parent = GetBonePose(hierarchy, parentBone, pose);
                subBone.localRotation = (UnityEngine.Quaternion.Inverse(parent.bone.rotation.Quaternion()) * boneDto.rotation.Quaternion()).Dto();
            }

            computedMap[boneType] = new()
            {
                bone = boneDto,
                subBone = subBone
            };

            return computedMap[boneType];
        }

        /// <inheritdoc/>
        public void UpdateBones(UserTrackingFrameDto trackingFrame)
        {
            if (trackingFrame is null)
                throw new ArgumentNullException(nameof(trackingFrame));

            StartPoses(trackingFrame);

            // remove not activated poses
            int nbObjToRemove = appliedPoses.Count - trackingFrame.poses.Count;
            if (nbObjToRemove > 0)
            {
                Queue<PoseClip> posesToRemove = new Queue<PoseClip>(nbObjToRemove);
                appliedPoses.ForEach(pose =>
                {
                    if (!trackingFrame.poses.Contains(pose.Id))
                        posesToRemove.Enqueue(pose);
                });
                StopPose(posesToRemove);
            }
        }

        private void StartPoses(UserTrackingFrameDto trackingFrame)
        {
            foreach (ulong poseId in trackingFrame.poses)
            {
                //at load, could receive tracking frame without having the pose
                UMI3DEntityInstance poseClipEntityInstance = environmentManagerService.TryGetEntityInstance(poseId);
                PoseClip poseClip = poseClipEntityInstance?.Object as PoseClip;

                if (poseClipEntityInstance is not null && !appliedPoses.Contains(poseClip))
                    StartPose(poseClip);
            }
        }

        /// <inheritdoc/>
        public void WriteTrackingFrame(UserTrackingFrameDto trackingFrame, TrackingOption option)
        {
            if (trackingFrame == null)
                throw new ArgumentNullException(nameof(trackingFrame));

            trackingFrame.poses ??= new(appliedPoses.Count);

            appliedPoses.ForEach((pose) =>
            {
                trackingFrame.poses.Add(pose.Id);
            });
        }
    }
}