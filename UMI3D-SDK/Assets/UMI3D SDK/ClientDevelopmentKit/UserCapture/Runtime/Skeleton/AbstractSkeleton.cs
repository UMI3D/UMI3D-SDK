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

using umi3d.cdk.userCapture.animation;
using umi3d.cdk.userCapture.pose;
using umi3d.cdk.userCapture.description;
using umi3d.cdk.userCapture.tracking;
using umi3d.cdk.userCapture.tracking.ik;
using umi3d.common;
using umi3d.common.core;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
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

        protected Dictionary<uint, UnityTransformation> bones = new();

        /// <inheritdoc/>
        public virtual IReadOnlyDictionary<uint, UnityTransformation> Bones
        {
            get => bones;
            protected set => bones = value is not Dictionary<uint, UnityTransformation> castValue ? bones : castValue;
        }

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

        public ulong EnvironmentId { get; set; }

        /// <summary>
        /// Anchor of the skeleton hierarchy.
        /// </summary>
        [SerializeField, Tooltip("Anchor of the skeleton hierarchy.")]
        protected Transform hipsAnchor;

        private const float SKELETON_STANDARD_SIZE = 1.8f;
        private const float SKELETON_STANDARD_SIZE_INVERSE = 1f / SKELETON_STANDARD_SIZE;

        protected IIKHandler postProcessIKHandler;

        private const uint ROOT_BONE = BoneType.Hips;
        protected GameObject finalSkeletonGameObject;

        #endregion Fields

        #region DI

        protected IUnityMainThreadDispatcher UnityMainThreadDispatcherService { get; private set; }

        public void Init(ITrackedSubskeleton trackedSkeleton, IPoseSubskeleton poseSkeleton, IUnityMainThreadDispatcher mainThreadDispatcher)
        {
            UnityMainThreadDispatcherService = mainThreadDispatcher;
            Init(trackedSkeleton, poseSkeleton);
        }

        #endregion DI

        #region Lifecycle

        #region Initialisation

        public void Init(ITrackedSubskeleton trackedSubskeleton, IPoseSubskeleton poseSubskeleton)
        {
            UnityMainThreadDispatcherService ??= UnityMainThreadDispatcherManager.Instance;
            this._trackedSkeleton = trackedSubskeleton;
            HipsAnchor = TrackedSubskeleton.Hips;
            PoseSubskeleton = poseSubskeleton;
            subskeletons = new List<ISubskeleton> { TrackedSubskeleton };

            // init final skeleton game objects
            finalSkeletonGameObject = new GameObject($"Final Skeleton - user {UserId}");

            // either personal skeleton container or collab skeleton scene
            // (in order not to be influenced by displacement of this, because of tracked action)
            // final skeleton is GET logic, subskeletons is SET
            finalSkeletonGameObject.transform.SetParent(this.transform.parent);
            SkeletonHierarchy.Apply(CreateSkeletonBoneGameObject, startBone: BoneType.Hips);

            Destroyed += () =>
            {
                if (finalSkeletonGameObject != null) // clean final skeleton
                    UnityEngine.Object.Destroy(finalSkeletonGameObject);
            };

            bones[ROOT_BONE].Transform.SetParent(finalSkeletonGameObject.transform);
            bones[ROOT_BONE].Position = HipsAnchor != null ? HipsAnchor.position : Vector3.zero;
            bones[ROOT_BONE].Rotation = HipsAnchor != null ? HipsAnchor.rotation : Quaternion.identity;

            // setup IK post processor
            SimpleIKHandler ikHandler = finalSkeletonGameObject.AddComponent<SimpleIKHandler>();
            ikHandler.Init(this);
            postProcessIKHandler = ikHandler;

            // wait to add PoseSubskeleton until we received at least one tracking frame
            initPoseSubskeletonCoroutine = StartCoroutine(InitPoseSubskeleton());

            Destroyed += () =>
            {
                if (initPoseSubskeletonCoroutine != null) // stop waiting if initalization unfinished
                    StopCoroutine(initPoseSubskeletonCoroutine);
            };
        }

        private Coroutine initPoseSubskeletonCoroutine;

        private IEnumerator InitPoseSubskeleton()
        {
            while (this.lastFrame == null)
            {
                yield return new WaitForEndOfFrame();
            }

            subskeletons.AddSorted(PoseSubskeleton);
            initPoseSubskeletonCoroutine = null;
        }

        /// <summary>
        /// Create a Unity hierarchy of gameobjects for final skeleton's bones.
        /// </summary>
        /// <param name="bone"></param>
        private void CreateSkeletonBoneGameObject(uint bone)
        {
            GameObject boneGo = new(BoneTypeHelper.GetBoneName(bone));

            if (SkeletonHierarchy.Relations[bone].boneTypeParent is not BoneType.None
                && bones.TryGetValue(SkeletonHierarchy.Relations[bone].boneTypeParent, out var parentTransformation))
            {
                boneGo.transform.SetParent(parentTransformation.Transform);
            }

            bones[bone] = new(boneGo.transform)
            {
                Position = Vector3.zero,
                Rotation = Quaternion.identity,
                LocalRotation = Quaternion.identity // local after global to ensure local are identity at start
            };
        }

        #endregion Initialisation

        #region Destruction

        /// <summary>
        /// Event raised when object is destroyed.
        /// </summary>
        public event System.Action Destroyed;

        /// <summary>
        /// To call for cleaning before destroying object.
        /// </summary>
        protected virtual void Clean()
        {
            Destroyed?.Invoke();

            bones.Clear();
            subskeletons.Clear();
        }

        protected virtual void OnDestroy()
        {
            Clean();
        }

        #endregion Destruction

        #endregion Lifecycle

        #region Compute

        /// <inheritdoc/>
        public ISkeleton Compute()
        {
            PreComputed?.Invoke();

            AssembleSkeleton();

            RawComputed?.Invoke();

            PostProcessSkeleton();

            Computed?.Invoke();

            return this;
        }

        /// <summary>
        /// Called after just before each skeleton computation.
        /// </summary>
        public event System.Action PreComputed;

        /// <summary>
        /// Called after after each computation and before post-procession.
        /// </summary>
        public event System.Action RawComputed;

        /// <summary>
        /// Called after each post procession of the final skeleton.
        /// </summary>
        public event System.Action Computed;

        /// <summary>
        /// Cache for bottom-up recursive <see cref="ComputeBoneWorldTransform(uint)"/> method.
        /// Speeding up computations.
        /// </summary>
        private Dictionary<uint, bool> alreadyComputedbonesCache = new();

        private Vector3 hipsDisplacement = Vector3.zero;

        /// <summary>
        /// Assemble skeleton from subskeletons.
        /// </summary>
        private void AssembleSkeleton()
        {
            if (Subskeletons == null || Subskeletons.Count == 0)
                return;

            RetrieveBonesRotation(SkeletonHierarchy);

            if (!bones.ContainsKey(ROOT_BONE))
                return;

            if (TrackedSubskeleton.Controllers.TryGetValue(ROOT_BONE, out IController hipsController))
            {
                bones[ROOT_BONE].Position = hipsController.position;
                bones[ROOT_BONE].Rotation = hipsController.rotation;
            }
            else
            {
                //very naive : for now, we consider the tracked hips as the computer hips
                bones[ROOT_BONE].Position = HipsAnchor != null ? HipsAnchor.position + hipsDisplacement : Vector3.zero; // add displacement to have the movement of Hips from animations
                bones[ROOT_BONE].Rotation = HipsAnchor != null ? HipsAnchor.rotation * bones[ROOT_BONE].LocalRotation : Quaternion.identity;
            }

            // better use normal recursive computations then.
            foreach (uint boneType in SkeletonHierarchy.OrderedBones) // order is garantied by hierarchy
            {
                if (boneType == ROOT_BONE) 
                    continue;

                ComputeBoneWorldTransform(boneType);
            }
        }

        /// <summary>
        /// Compute the final position and rotation of each bone, and their parents recursively if not already computed
        /// </summary>
        /// <param name="boneType"></param>
        private void ComputeBoneWorldTransform(uint boneType)
        {
            if (SkeletonHierarchy.Relations.TryGetValue(boneType, out var boneRelation)
                && boneRelation.boneTypeParent != BoneType.None)
            {
                UnityTransformation parentransformation = bones[boneRelation.boneTypeParent];
                Matrix4x4 m = Matrix4x4.TRS(parentransformation.Position, parentransformation.Rotation, transform.localScale * SKELETON_STANDARD_SIZE_INVERSE);

                UnityTransformation transformation = bones[boneType];
                transformation.Position = m.MultiplyPoint3x4(boneRelation.relativePosition);
                transformation.Rotation = (parentransformation.Rotation * transformation.LocalRotation).normalized;
            }
        }

        private readonly Vector3 ExtractXZVector = Vector3.forward + Vector3.right;

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
                if (bones.TryGetValue(bone, out var boneTransformation))
                    boneTransformation.LocalRotation = Quaternion.identity;
            }

            // for each subskeleton, in ascending order (last has highest priority),
            // get the relative orientation of all available bones
            lock (SubskeletonsLock)
                foreach (var skeleton in Subskeletons)
                {
                    SubSkeletonPoseDto subskeletonPose = skeleton.GetPose(hierarchy);

                    if (subskeletonPose is null) // if bones are null, sub skeleton should not have any effect. e.g. pose skeleton with no current pose.
                        continue;

                    if (subskeletonPose.boneAnchor?.bone == ROOT_BONE) //hips displacement is used when subskeleton moves the hips as its anchor, e.g. emotes
                        hipsDisplacement = Vector3.Scale(HipsAnchor.rotation * subskeletonPose.boneAnchor.position.Struct(), ExtractXZVector);

                    foreach (SubSkeletonBoneDto subskeletonBone in subskeletonPose.bones)
                    {
                        // if a bone rotation can receive the pose
                        if (bones.TryGetValue(subskeletonBone.boneType, out var boneTransformation))
                            boneTransformation.LocalRotation = subskeletonBone.localRotation.Quaternion();
                    }
                }
        }

        /// <summary>
        /// Apply post process operations on skeleton.
        /// </summary>
        private void PostProcessSkeleton()
        {
            // IK performed by IK handler
            CorrectIK();

            // muscle restrictions
            SkeletonHierarchy.Apply(MuscleRestrict);
        }

        /// <summary>
        /// Correct IK after subskeleton merge.
        /// </summary>
        /// <param name="layer"></param>
        protected virtual void CorrectIK()
        {
            // fix rotations
            foreach (IController controller in TrackedSubskeleton.Controllers.Values)
            {
                if (!controller.isActive || !controller.isOverrider) // do not overwrite normal controllers
                    continue;

                postProcessIKHandler.HandleAnimatorIK(0, controller); // pb : what should be overriden ?
                bones[controller.boneType].Rotation = controller.rotation;
            }
        }

        /// <summary>
        /// Restrict a bone local rotation based on an associated muscle.
        /// </summary>
        /// <param name="bone"></param>
        private void MuscleRestrict(uint bone)
        {
            if (!SkeletonHierarchy.Muscles.TryGetValue(bone, out var muscle)) // muscle not defined
                return;

            if (!SkeletonHierarchy.Relations.ContainsKey(bone)) // bone not defined in hierarchy
                return;

            if (!musclesRestrictors.TryGetValue(bone, out MuscleRestrictor muscleRestrictor))
            {
                musclesRestrictors[bone] = new MuscleRestrictor(muscle);
                muscleRestrictor = musclesRestrictors[bone];
            }

            bones[bone].LocalRotation = muscleRestrictor.Restrict(bones[bone].LocalRotation);
        }

        private readonly Dictionary<uint, MuscleRestrictor> musclesRestrictors = new();

        #endregion Compute

        #region Tracking

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

        #endregion Tracking

        #region Subskeletons Management

        public void AddSubskeleton(IAnimatedSubskeleton animatedSubskeleton)
        {
            if (animatedSubskeleton == null)
                return;

            lock (SubskeletonsLock) // loader can start parallel async tasks, required to load concurrently
            {
                UnityMainThreadDispatcherService.Enqueue(() => { lock (SubskeletonsLock) { subskeletons.AddSorted(animatedSubskeleton); } });

                // if some animator parameters should be updated by the browsers itself, start listening to them
                if (animatedSubskeleton.SelfUpdatedAnimatorParameters.Count > 0)
                    animatedSubskeleton.StartParameterSelfUpdate(this);
            }
        }

        private class SubskeletonComparer : IComparer<ISubskeleton>
        {
            public int Compare(ISubskeleton x, ISubskeleton y)
            {
                return x.Priority.CompareTo(y.Priority);
            }
        }

        public void RemoveSubskeleton(IAnimatedSubskeleton animatedSubskeleton)
        {
            if (animatedSubskeleton == null)
                return;

            if (animatedSubskeleton.SelfUpdatedAnimatorParameters.Count > 0)
                animatedSubskeleton.StopParameterSelfUpdate();

            if (subskeletons.Contains(animatedSubskeleton))
            {
                lock (SubskeletonsLock)
                {
                    UnityMainThreadDispatcherService.Enqueue(() => { lock (SubskeletonsLock) { subskeletons.Remove(animatedSubskeleton); } });
                }
            }
        }

        #endregion Subskeletons Management
    }
}