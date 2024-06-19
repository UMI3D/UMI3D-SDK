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
using System.Linq;
using umi3d.cdk.userCapture.tracking.ik;
using umi3d.cdk.utils.extrapolation;
using umi3d.common;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.tracking;
using UnityEngine;

namespace umi3d.cdk.userCapture.tracking
{
    public class TrackedSubskeleton : MonoBehaviour, ITrackedSubskeleton
    {
        #region Dependencies Injection

        private ILoadingManager loadingService;
        private ISkeleton skeleton;

        private void Awake()
        {
            this.loadingService = UMI3DEnvironmentLoader.Instance;
            skeleton = this.transform.parent.GetComponent<AbstractSkeleton>();
        }

        #endregion Dependencies Injection

        public IDictionary<uint, float> BonesAsyncFPS { get; set; } = new Dictionary<uint, float>();

        public IReadOnlyDictionary<uint, IController> Controllers => controllers;
        private readonly Dictionary<uint, IController> controllers = new();

        private readonly Queue<IController> controllersToClean = new();
        private readonly Queue<IController> controllersToDestroy = new();

        private readonly List<uint> receivedTypes = new List<uint>();
        private readonly Dictionary<uint, (Vector3LinearDelayedExtrapolator PositionExtrapolator, QuaternionLinearDelayedExtrapolator RotationExtrapolator, IController Controller)> extrapolators = new();

        private TrackingAnimatorIKHandler ikHandler;

        [SerializeField]
        public Camera viewpoint;

        public Camera ViewPoint => viewpoint;

        [SerializeField]
        public Transform hips;

        public Transform Hips => hips;

        /// <summary>
        /// Reference of IK Animator
        /// </summary>
        [SerializeField]
        private Animator animator;

        /// <summary>
        /// Animator wrapper.
        /// </summary>
        [SerializeField]
        protected TrackedAnimator trackedAnimator;
        public TrackedAnimator TrackedAnimator => trackedAnimator;

        [SerializeField]
        public Dictionary<uint, TrackedSubskeletonBone> bones = new();

        public IReadOnlyDictionary<uint, TrackedSubskeletonBone> TrackedBones => bones;

        public int Priority => GetPriority();

        public ulong EnvironmentId { get; set; }

        private int GetPriority()
        {
            //AbstractSkeleton skeleton = this.transform.parent.GetComponent<AbstractSkeleton>();
            //UserTrackingFrameDto frame = skeleton is PersonalSkeleton personalSkeleton ? personalSkeleton.GetFrame(new TrackingOption()) : skeleton.LastFrame;

            //if (frame != null && frame.trackedBones.Exists(c => (c.boneType == BoneType.RightHand || c.boneType == BoneType.LeftHand)))
            //    return 101;

            return 0;
        }

        #region Lifecycle

        public void Start()
        {
            if (trackedAnimator == null && !TryGetComponent(out trackedAnimator))
            {
                UMI3DLogger.LogWarning("TrackedAnimator not found for TrackedSubskeleton. Generating a new one", DebugScope.CDK);
                if (animator == null)
                {
                    UMI3DLogger.LogWarning("Animator for IK not found. Generating a default one", DebugScope.CDK);
                    animator = gameObject.AddComponent<Animator>();
                    animator.Rebind();
                }
                trackedAnimator = gameObject.AddComponent<TrackedAnimator>();
            }

            ikHandler = new TrackingAnimatorIKHandler(animator);
            trackedAnimator.IkCallback += ApplyIK;

            foreach (var bone in GetComponentsInChildren<TrackedSubskeletonBone>())
            {
                if (!bones.ContainsKey(bone.boneType))
                    bones.Add(bone.boneType, bone);
            }

            foreach (var tracker in GetComponentsInChildren<Tracker>())
            {
                controllers.Add(tracker.BoneType, tracker);
            }
        }

        /// <summary>
        /// Inverse kinematics are applied on the subskeleton based on the position of each controller.
        /// </summary>
        /// <param name="layer"></param>
        private void ApplyIK(int layer)
        {
            // clean controllers
            ikHandler.Reset(controllersToClean, bones);
            controllersToClean.Clear();

            // destroy deleted controllers
            foreach (var controller in controllersToDestroy)
                controller.Destroy();
            controllersToDestroy.Clear();

            // apply actual IK
            ikHandler.HandleAnimatorIK(layer, controllers.Values, bones);
        }

        private void Update()
        {
            if (skeleton is IPersonalSkeleton)
                return; // no use of extrapolator for own skeleton, it also introduce a race conflict with Updates from Trackers, because extrapolators never receive tracking frame to be updated from.

            UpdateControllersPosition();
        }

        /// <summary>
        /// Controllers positions are extrapolated during Update based on the extrapolator informations from tracking frames.
        /// It applies to collaborative tracked subskeletons only.
        /// </summary>
        private void UpdateControllersPosition()
        {
            foreach (var extrapolator in extrapolators.Values)
                if (extrapolator.Controller is VirtualController vc)
                {
                    vc.position = extrapolator.PositionExtrapolator.Extrapolate();
                    vc.rotation = extrapolator.RotationExtrapolator.Extrapolate();
                }
        }

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

            foreach (var controller in controllers.Values)
            {
                if (controller != null)
                    controller.Destroy();
            }

            bones.Clear();
            controllers.Clear();
            BonesAsyncFPS.Clear();

            controllersToClean.Clear();
            controllersToDestroy.Clear();
            receivedTypes.Clear();
            extrapolators.Clear();

            if (trackedAnimator != null)
                trackedAnimator.IkCallback -= ApplyIK;
        }

        protected virtual void OnDestroy()
        {
            Clean();
        }

        #endregion Destruction

        #endregion Lifecycle

        /// <inheritdoc/>
        public SubSkeletonPoseDto GetPose(UMI3DSkeletonHierarchy hierarchy)
        {
            var dto = new SubSkeletonPoseDto() { bones = new(bones.Count) };

            // get the boneDto of each bone that is relevant for IK or is a controller
            foreach (var bone in bones.Values.Where(x => x.positionComputed || controllers.ContainsKey(x.boneType)))
            {
                //.Where(x => controllers.Exists(y => y.boneType.Equals(x.boneType)))
                dto.bones.Add(bone.ToBoneDto());
            }
            return dto;
        }

        #region Tracking Frame

        /// <inheritdoc/>
        public void UpdateBones(UserTrackingFrameDto trackingFrame)
        {
            receivedTypes.Clear();
            foreach (var bone in trackingFrame.trackedBones)
            {
                if (!controllers.TryGetValue(bone.boneType, out IController controller)
                    || controller is not VirtualController vc) // controllers from tracking frames should be handled as distant controllers
                {
                    // create controller from tracking frame
                    vc = new VirtualController
                    {
                        boneType = bone.boneType,
                        isOverrider = bone.isOverrider,
                        position = bone.position.Struct(),
                        rotation = bone.rotation.Quaternion()
                    };
                    ReplaceController(vc);
                }

                vc.isActive = true;
                if (extrapolators.ContainsKey(bone.boneType))
                {
                    extrapolators[bone.boneType].PositionExtrapolator.AddMeasure(bone.position.Struct());
                    extrapolators[bone.boneType].RotationExtrapolator.AddMeasure(bone.rotation.Quaternion());
                }
                else
                {
                    vc.position = bone.position.Struct();
                    vc.rotation = bone.rotation.Quaternion();
                }

                receivedTypes.Add(bone.boneType);
            }

            Queue<VirtualController> controllersToRemove = new();
            foreach (var c in controllers.Values)
            {
                if (c is VirtualController dc && !receivedTypes.Contains(c.boneType))
                {
                    extrapolators.Remove(c.boneType);
                    controllersToRemove.Enqueue(dc);
                }
            }
            foreach (var dc in controllersToRemove)
            {
                DeleteController(dc);
            }
        }

        /// <inheritdoc/>
        public void WriteTrackingFrame(UserTrackingFrameDto trackingFrame, TrackingOption option)
        {
            if (bones.Count == 0)
                return;

            trackingFrame.trackedBones = new(bones.Count);

            foreach (var controller in controllers.Values)
            {
                trackingFrame.trackedBones.Add(controller.ToControllerDto());
            }

            // get dto for each bone that is async
            foreach (var asyncBone in BonesAsyncFPS)
            {
                foreach (var bone in bones.Values)
                {
                    if (bone.boneType == asyncBone.Key)
                    {
                        trackingFrame.trackedBones.Add(bone.ToControllerDto());
                        break;
                    }
                }
            }
        }

        #endregion Tracking Frame

        #region Controllers Management

        public UserTrackingBoneDto GetController(uint boneType)
        {
            return new UserTrackingBoneDto()
            {
                bone = bones[boneType].ToControllerDto()
            };
        }

        /// <summary>
        /// Called by OnAnimatorIK in TrackedAnimator, set all tracked bones as computed and positions IK hints.
        /// </summary>
        private readonly Dictionary<uint, Stack<IController>> savedControllers = new();

        public void RemoveController(uint boneType)
        {
            if (!controllers.TryGetValue(boneType, out IController controller))
                return;

            DeleteController(controller);

            if (savedControllers.TryGetValue(boneType, out Stack<IController> savedControllerStack)
                && savedControllerStack.TryPop(out IController savedController))
            {
                savedController.isActive = true;
                ReplaceController(savedController);
            }
            else
            {
                extrapolators.Remove(boneType);
            }
        }

        /// <summary>
        /// Clean and destroy a controller.
        /// </summary>
        /// <param name="controller"></param>
        private void DeleteController(IController controller)
        {
            controllersToClean.Enqueue(controller);
            controllersToDestroy.Enqueue(controller);
            controllers.Remove(controller.boneType);
        }

        /// <summary>
        /// Create or update an extrapolator for a bone type with the controller as a source.
        /// </summary>
        /// <param name="controller"></param>
        private void AttachExtrapolator(IController controller)
        {
            if (extrapolators.ContainsKey(controller.boneType))
            {
                var extrapolatorRegister = extrapolators[controller.boneType];
                extrapolators[controller.boneType] = (extrapolatorRegister.PositionExtrapolator, extrapolatorRegister.RotationExtrapolator, controller);
            }
            else
            {
                extrapolators.Add(controller.boneType, (new(), new(), controller));
            }
        }

        /// <summary>
        /// Remove the controller and replace it by another one.
        /// </summary>
        /// <param name="newController"></param>
        /// <param name="saveOldController">If true the removed controlled is saved and will be used when the replacing controller will be deleted.</param>
        public void ReplaceController(IController newController, bool saveOldController = false)
        {
            if (newController == null)
                return;

            if (newController.boneType is BoneType.None) // cannot put a controller on none bonetype
            {
                UMI3DLogger.LogWarning($"Impossible to add controller. None bone type cannot receive a controller.", DebugScope.CDK | DebugScope.UserCapture);
                return;
            }

            if (saveOldController && controllers.TryGetValue(newController.boneType, out IController oldController))
            {
                oldController.isActive = false;
                if (!savedControllers.TryGetValue(oldController.boneType, out Stack<IController> savedControllerStack))
                {
                    savedControllerStack = new();
                    savedControllers.Add(oldController.boneType, savedControllerStack);
                }
                savedControllerStack.Push(oldController);
            }

            controllers[newController.boneType] = newController;
            AttachExtrapolator(newController);
            newController.isActive = true;
        }

        #endregion Controllers Management
    }
}