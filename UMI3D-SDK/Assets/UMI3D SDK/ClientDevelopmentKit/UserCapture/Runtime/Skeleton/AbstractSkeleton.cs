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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.cdk.userCapture.animation;
using umi3d.cdk.userCapture.pose;
using umi3d.cdk.userCapture.tracking;
using umi3d.common;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using umi3d.common.userCapture.tracking;
using umi3d.common.utils;
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

        #region Fields

        /// <inheritdoc/>
        public virtual IDictionary<uint, ISkeleton.Transformation> Bones { get; protected set; } = new Dictionary<uint, ISkeleton.Transformation>();

        /// <summary>
        /// Lock for concurrent access to <see cref="Subskeletons"/> collection.
        /// </summary>
        public object SubskeletonsLock { get; } = new();

        /// <inheritdoc/>
        public virtual IReadOnlyList<ISubskeleton> Subskeletons => subskeletons;

        protected List<ISubskeleton> subskeletons = new();

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
        public ITrackedSubskeleton TrackedSubskeleton => _trackedSkeleton;
        protected ITrackedSubskeleton _trackedSkeleton;

        [SerializeField]
        protected TrackedSubskeleton trackedSkeleton;

        /// <summary>
        /// Susbskeleton for body poses.
        /// </summary>
        public IPoseSubskeleton PoseSubskeleton { get; protected set; }


        protected UserTrackingFrameDto lastFrame;
        public UserTrackingFrameDto LastFrame => lastFrame;

        /// <summary>
        /// Anchor of the skeleton hierarchy.
        /// </summary>
        [SerializeField, Tooltip("Anchor of the skeleton hierarchy.")]
        protected Transform hipsAnchor;

        #endregion Fields

        public void Init(ITrackedSubskeleton trackedSkeleton, IPoseSubskeleton poseSkeleton)
        {
            this._trackedSkeleton = trackedSkeleton;
            HipsAnchor = TrackedSubskeleton.Hips;
            PoseSubskeleton = poseSkeleton;
            subskeletons = new List<ISubskeleton> { TrackedSubskeleton };

            // init bones to prevent from early bindings arrival
            foreach (var boneType in SkeletonHierarchy.Relations.Keys)
            {
                Bones[boneType] = new ISkeleton.Transformation()
                {
                    Position = Vector3.zero,
                    Rotation = Quaternion.identity,
                    LocalRotation = Quaternion.identity,
                };
            }
            Bones[BoneType.Hips].Position = HipsAnchor != null ? HipsAnchor.position : Vector3.zero;
            Bones[BoneType.Hips].Rotation = HipsAnchor != null ? HipsAnchor.rotation : Quaternion.identity;

            StartCoroutine(InitPoseSubskeleton());
        }

        private IEnumerator InitPoseSubskeleton()
        {
            while (this.lastFrame == null)
            {
                yield return new WaitForEndOfFrame();
            }

            subskeletons.AddSorted(PoseSubskeleton);
        }

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
            Bones[BoneType.Hips].Position = HipsAnchor != null ? HipsAnchor.position : Vector3.zero;
            Bones[BoneType.Hips].Rotation = HipsAnchor != null ? HipsAnchor.rotation : Quaternion.identity;

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
                && SkeletonHierarchy.Relations.TryGetValue(boneType, out var boneRelation)
                && boneRelation.boneTypeParent != BoneType.None)
            {
                if (!alreadyComputedBonesCache[boneRelation.boneTypeParent])
                    ComputeBonePosition(boneRelation.boneTypeParent);

                Matrix4x4 m = Matrix4x4.TRS(Bones[boneRelation.boneTypeParent].Position, Bones[boneRelation.boneTypeParent].Rotation, transform.localScale * 0.5f);
                Bones[boneType].Position = m.MultiplyPoint3x4(boneRelation.relativePosition); //Bones[boneRelation.boneTypeParent].Position + Bones[boneRelation.boneTypeParent].Rotation * boneRelation.relativePosition;
                Bones[boneType].Rotation = (Bones[boneRelation.boneTypeParent].Rotation * Bones[boneType].LocalRotation).normalized;
                alreadyComputedBonesCache[boneType] = true;
            }
        }

        /// <summary>
        /// Get all final bone rotation, based on subskeletons. Lastest subskeleton has lowest priority.
        /// </summary>
        /// <param name="hierarchy"></param>
        private void RetrieveBonesRotation(UMI3DSkeletonHierarchy hierarchy)
        {
            //UnityEngine.Debug.Log($"<color=orange>Compute for {UserId}</color>");
            // consider all bones we should have according to the hierarchy, and set all values to identity
            foreach (var bone in hierarchy.Relations.Keys)
            {
                if (Bones.ContainsKey(bone))
                {
                    Bones[bone].LocalRotation = Quaternion.identity;
                }
                else
                    Bones[bone] = new ISkeleton.Transformation() { Rotation = Quaternion.identity, LocalRotation = Quaternion.identity };
            }

            // for each subskeleton, in ascending order (last has highest priority),
            // get the relative orientation of all available bones
            lock (SubskeletonsLock)
                foreach (var skeleton in Subskeletons)
                {
                    List<SubSkeletonBoneDto> bones = skeleton.GetPose(hierarchy)?.bones;

                    if (bones is null) // if bones are null, sub skeleton should not have any effect. e.g. pose skeleton with no current pose.
                        continue;

                    foreach (var b in bones.Where(c => c.boneType != BoneType.Hips))
                    {
                        // if a bone rotation has already been registered, erase it

                        if (Bones.ContainsKey(b.boneType))
                            Bones[b.boneType].LocalRotation = b.localRotation.Quaternion();
                        else
                            Bones.Add(b.boneType, new ISkeleton.Transformation() { LocalRotation = b.localRotation.Quaternion() });
                    }
                }
        }

        /// <inheritdoc/>
        public abstract void UpdateBones(UserTrackingFrameDto frame);

        public UserCameraPropertiesDto GetCameraDto()
        {
            return new UserCameraPropertiesDto()
            {
                scale = 1f,
                projectionMatrix = TrackedSubskeleton.ViewPoint.projectionMatrix.Dto(),
                boneType = BoneType.Viewpoint,
            };
        }

        class SkeletonComparer : IComparer<ISubskeleton>
        {
            public int Compare(ISubskeleton x, ISubskeleton y)
            {
                return x.Priority.CompareTo(y.Priority);
            }
        }


        public void AddSubskeleton(IAnimatedSubskeleton animatedSubskeleton)
        {
            if (animatedSubskeleton == null)
                return;

            lock (SubskeletonsLock) // loader can start parallel async tasks, required to load concurrently
            {
                UnityMainThreadDispatcherManager.Instance.Enqueue(() => { lock (SubskeletonsLock) { subskeletons.AddSorted(animatedSubskeleton); } });

                // if some animator parameters should be updated by the browsers itself, start listening to them
                if (animatedSubskeleton.SelfUpdatedAnimatorParameters.Count > 0)
                    animatedSubskeleton.StartParameterSelfUpdate(this);
            }
        }

        public void RemoveSubskeleton(IAnimatedSubskeleton animatedSubskeleton)
        {
            if (animatedSubskeleton == null)
                return;

            if (animatedSubskeleton.SelfUpdatedAnimatorParameters.Count > 0)
                animatedSubskeleton.StopParameterSelfUpdate();

            if (subskeletons.Contains(animatedSubskeleton))
                subskeletons.Remove(animatedSubskeleton);
        }
    }
}