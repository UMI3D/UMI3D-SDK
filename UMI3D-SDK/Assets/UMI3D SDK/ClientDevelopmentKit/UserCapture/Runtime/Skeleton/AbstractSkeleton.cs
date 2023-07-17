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

using System.Collections.Generic;
using umi3d.cdk.userCapture.pose;
using umi3d.cdk.userCapture.tracking;
using umi3d.common;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.tracking;

using UnityEngine;

namespace umi3d.cdk.userCapture
{
    /// <summary>
    /// Representation of a human user, controlled by a player. <br/>
    /// Can either be the skeleton of the browser's user or skeleton of other players.
    /// </summary>
    public abstract class AbstractSkeleton : MonoBehaviour, ISkeleton
    {
        protected const DebugScope scope = DebugScope.CDK | DebugScope.UserCapture;

        /// <inheritdoc/>
        public virtual Dictionary<uint, ISkeleton.s_Transform> Bones { get; protected set; } = new();

        /// <inheritdoc/>
        public virtual List<ISubskeleton> Subskeletons { get; protected set; } = new();

        /// <inheritdoc/>
        public UMI3DSkeletonHierarchy SkeletonHierarchy 
        { 
            get
            {
                return _skeletonHierarchy;
            }
            set
            {
                if (value == null)
                    throw new System.ArgumentNullException("Cannot set a null Hierarchy.");
                _skeletonHierarchy = value;
            }
        }
        private UMI3DSkeletonHierarchy _skeletonHierarchy;

        /// <inheritdoc/>
        public virtual Transform HipsAnchor { get => hipsAnchor; set => hipsAnchor = value; }

        /// <inheritdoc/>
        public virtual ulong UserId { get; set; }

        /// <summary>
        /// Subskeleton updated from tracked controllers.
        /// </summary>
        public TrackedSkeleton TrackedSubskeleton 
        { 
            get
            {
                return trackedSkeleton;
            }
            protected set
            {
                trackedSkeleton = value;
            }
        }
        [SerializeField]
        private TrackedSkeleton trackedSkeleton;

        /// <summary>
        /// Susbskeleton for body poses.
        /// </summary>
        public PoseSkeleton PoseSubskeleton { get; protected set; }

        /// <summary>
        /// Anchor of the skeleton hierarchy.
        /// </summary>
        [SerializeField, Tooltip("Anchor of the skeleton hierarchy.")]
        protected Transform hipsAnchor;

        /// <inheritdoc/>
        public ISkeleton Compute()
        {
            if (Subskeletons == null || Subskeletons.Count == 0)
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
        /// Containing id of the bones set by the TrackedSkeleton in <see cref="RetrieveBonesRotation(UMI3DSkeletonHierarchy)"/> method.
        /// Preventing from the application of the Hips rotation to these bones in <see cref="ComputeBonePosition(uint)"/> method.
        /// </summary>
        private List<uint> bonesSetByTrackedSkeleton = new();

        /// <summary>
        /// Compute the final position of each bone, and their parents recursively if not already computed
        /// </summary>
        /// <param name="boneType"></param>
        private void ComputeBonePosition(uint boneType)
        {
            if (!alreadyComputedBonesCache[boneType]
                && SkeletonHierarchy.Relations.TryGetValue(boneType, out var boneRelation)
                && boneRelation.boneTypeParent != BoneType.None)
            {
                if (!alreadyComputedBonesCache[boneRelation.boneTypeParent])
                    ComputeBonePosition(boneRelation.boneTypeParent);

                Bones[boneType].s_Position = Bones[boneRelation.boneTypeParent].s_Position + Bones[boneRelation.boneTypeParent].s_Rotation * boneRelation.relativePosition;

                if (!bonesSetByTrackedSkeleton.Contains(boneType))
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
            foreach (var bone in hierarchy.Relations.Keys)
            {
                if (Bones.ContainsKey(bone))
                    Bones[bone].s_Rotation = Quaternion.identity;
                else
                    Bones[bone] = new ISkeleton.s_Transform() { s_Rotation = Quaternion.identity };
            }

            bonesSetByTrackedSkeleton.Clear();

            // for each subskeleton, in descending order (lastest has lowest priority),
            // get the relative orientation of all available bones
            for (int i = Subskeletons.Count - 1; 0 <= i; i--)
            {
                var skeleton = Subskeletons[i];

                List<BoneDto> bones;

                bones = skeleton.GetPose()?.bones;

                if (bones is null) // if bones are null, sub skeleton should not have any effect. e.g. pose skeleton with no current pose.
                    continue;

                foreach (var b in bones)
                {
                    // if a bone rotation has already been registered, erase it
                    if (Bones.ContainsKey(b.boneType))
                        Bones[b.boneType].s_Rotation = b.rotation.Quaternion();
                    else
                        Bones.Add(b.boneType, new ISkeleton.s_Transform() { s_Rotation = b.rotation.Quaternion() });
                }

                if (i == 0) //the TrackedSkeleton is the first SubSkeleton
                {
                    foreach (var b in bones)
                    {
                        bonesSetByTrackedSkeleton.Add(b.boneType);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public abstract void UpdateFrame(UserTrackingFrameDto frame);
    }
}