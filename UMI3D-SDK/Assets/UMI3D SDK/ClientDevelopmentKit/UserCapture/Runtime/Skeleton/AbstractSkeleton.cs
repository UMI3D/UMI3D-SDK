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

using System;
using System.Collections.Generic;
using umi3d.cdk.userCapture.pose;
using umi3d.cdk.userCapture.tracking;
using umi3d.cdk.utils.extrapolation;
using umi3d.common;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.tracking;

using UnityEngine;

namespace umi3d.cdk.userCapture
{
    public abstract class AbstractSkeleton : MonoBehaviour, ISkeleton
    {
        protected const DebugScope scope = DebugScope.CDK | DebugScope.UserCapture;

        public virtual Dictionary<uint, ISkeleton.s_Transform> Bones { get; protected set; } = new();

        public virtual List<ISubSkeleton> Skeletons { get; protected set; } = new();

        public UMI3DSkeletonHierarchy SkeletonHierarchy { get; set; }

        public virtual Transform HipsAnchor { get => hipsAnchor; set => hipsAnchor = value; }

        public virtual ulong UserId { get; set; }

        protected Vector3LinearDelayedExtrapolator nodePositionExtrapolator = new();

        protected QuaternionLinearDelayedExtrapolator nodeRotationExtrapolator = new();

        public TrackedSkeleton TrackedSkeleton;
        public PoseSkeleton PoseSkeleton = null;

        [SerializeField]
        protected Transform hipsAnchor;

        public ISkeleton Compute()
        {
            if (Skeletons == null || Skeletons.Count == 0)
                return this;

            RetrieveBonesRotation(SkeletonHierarchy);

            if (!Bones.ContainsKey(BoneType.Hips))
                return this;

            foreach (uint boneType in Bones.Keys)
                alreadyComputedBonesCache[boneType] = false;

            //very naive : for now, we consider the tracked hips as the computer hips
            Bones[BoneType.Hips].s_Position = HipsAnchor != null ? HipsAnchor.position : Vector3.zero;
            Bones[BoneType.Hips].s_Rotation = HipsAnchor != null ? HipsAnchor.rotation : Quaternion.identity;

            alreadyComputedBonesCache[BoneType.Hips] = true;

            // better use normal recusive computations then.
            foreach (uint boneType in Bones.Keys)
            {
                if (!alreadyComputedBonesCache[boneType])
                    ComputeBonePosition(boneType);
            }

            return this;
        }

        /// <summary>
        /// Cache for bottom-up recursive <see cref="ComputeBonePosition(uint)"/> method.
        /// Speeding up computations.
        /// </summary>
        private Dictionary<uint, bool> alreadyComputedBonesCache = new();

        /// <summary>
        /// Compute the final position of each bone, and their parents recursively if not already computed
        /// </summary>
        /// <param name="boneType"></param>
        private void ComputeBonePosition(uint boneType)
        {
            if (!alreadyComputedBonesCache[boneType]
                && SkeletonHierarchy.HierarchyDict.TryGetValue(boneType, out var boneRelation)
                && boneRelation.boneTypeParent != BoneType.None)
            {
                if (!alreadyComputedBonesCache[boneRelation.boneTypeParent])
                    ComputeBonePosition(boneRelation.boneTypeParent);

                Bones[boneType].s_Position = Bones[boneRelation.boneTypeParent].s_Position + Bones[boneRelation.boneTypeParent].s_Rotation * boneRelation.relativePosition;
                Bones[boneType].s_Rotation = Bones[BoneType.Hips].s_Rotation * Bones[boneType].s_Rotation; // all global bones rotations should be turned the same way as the anchor
                alreadyComputedBonesCache[boneType] = true;
            }
        }

        /// <summary>
        /// Get all final bone rotation, based on subskeletons. Lastest subskeleton has lowest priority.
        /// </summary>
        /// <param name="hierarchy"></param>
        private void RetrieveBonesRotation(UMI3DSkeletonHierarchy hierarchy)
        {
            // consider all bones we should have according to the hierarchy, and set all values to identity
            foreach (var bone in hierarchy.HierarchyDict.Keys)
            {
                if (Bones.ContainsKey(bone))
                    Bones[bone].s_Rotation = Quaternion.identity;
                else
                    Bones[bone] = new ISkeleton.s_Transform() { s_Rotation = Quaternion.identity };
            }

            // for each subskeleton, in descending order (lastest has lowest priority),
            // get the relative orientation of all available bones
            for (int i = Skeletons.Count - 1; 0 <= i; i--)
            {
                var skeleton = Skeletons[i];

                List<BoneDto> bones;

                try
                {
                    bones = skeleton.GetPose()?.bones;
                }
                catch (Exception e)
                {
                    Debug.Log(skeleton?.GetType()?.Name.ToString());
                    UMI3DLogger.LogException(e, scope);
                    return;
                }

                if (bones is null) //if bones are null, sub skeleton should not have any effect. e.g. pose skeleton with no current pose.
                    continue;

                foreach (var b in bones)
                {
                    // if a bone rotation has already been registered, erase it
                    if (Bones.ContainsKey(b.boneType))
                        Bones[b.boneType].s_Rotation = b.rotation.Quaternion();
                    else
                        Bones.Add(b.boneType, new ISkeleton.s_Transform() { s_Rotation = b.rotation.Quaternion() });
                }
            }
        }

        public abstract void UpdateFrame(UserTrackingFrameDto frame);
    }
}