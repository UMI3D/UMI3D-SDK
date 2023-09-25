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
using umi3d.common;
using System.Linq;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using umi3d.common.userCapture.tracking;
using UnityEngine;
using umi3d.cdk.utils.extrapolation;

namespace umi3d.cdk.userCapture.tracking
{
    public class TrackedSubskeleton : MonoBehaviour, ITrackedSubskeleton
    {
        public IDictionary<uint, float> BonesAsyncFPS { get; set; } = new Dictionary<uint, float>();

        public List<IController> controllers = new List<IController>();
        private List<IController> controllersToDestroy = new();

        [SerializeField]
        public Camera viewpoint;
        public Camera ViewPoint => viewpoint;

        [SerializeField]
        public Transform hips;
        public Transform Hips => hips;

        [SerializeField]
        private Animator animator;
        public TrackedAnimator trackedAnimator;

        [SerializeField]
        public Dictionary<uint, TrackedSubskeletonBone> bones = new();
        public IReadOnlyDictionary<uint, TrackedSubskeletonBone> TrackedBones => bones;

        public int Priority => GetPriority();

        private int GetPriority()
        {
            AbstractSkeleton skeleton = this.transform.parent.GetComponent<AbstractSkeleton>();
            UserTrackingFrameDto frame;

            if (skeleton is PersonalSkeleton)
                frame = (skeleton as PersonalSkeleton).GetFrame(new TrackingOption());
            else
                frame = skeleton.LastFrame;

            if (frame != null && frame.trackedBones.Exists(c => (c.boneType == BoneType.RightHand || c.boneType == BoneType.LeftHand)))
                return 101;

            return 0;
        }

        private List<uint> receivedTypes = new List<uint>();
        private Dictionary<uint, (Vector3LinearDelayedExtrapolator PositionExtrapolator, QuaternionLinearDelayedExtrapolator RotationExtrapolator, IController Controller)> extrapolators = new();

        public void Start()
        {
            if (trackedAnimator == null)
            {
                UMI3DLogger.LogWarning("TrackedAnimator was null for TrackedSubskeleton. Generating a new one", DebugScope.CDK);
                trackedAnimator = gameObject.AddComponent<TrackedAnimator>();
            }
            trackedAnimator.IkCallback += u => HandleAnimatorIK(u);

            foreach (var bone in GetComponentsInChildren<TrackedSubskeletonBone>())
            {
                if (!bones.ContainsKey(bone.boneType))
                    bones.Add(bone.boneType, bone);
            }

            foreach (var tracker in GetComponentsInChildren<Tracker>())
            {
                controllers.Add(tracker.distantController);
            }
        }

        private void Update()
        {
            foreach (var extrapolator in extrapolators.Values)
                if (extrapolator.Controller is DistantController vc)
                {
                    vc.position = extrapolator.PositionExtrapolator.Extrapolate();
                    vc.rotation = extrapolator.RotationExtrapolator.Extrapolate();
                }
        }


        public SubSkeletonPoseDto GetPose(UMI3DSkeletonHierarchy hierarchy)
        {
            var dto = new SubSkeletonPoseDto() { bones = new(bones.Count) };

            foreach (var bone in bones.Values.Where(x => x.positionComputed || controllers.Any(y => y.boneType == x.boneType)))
            {
                //.Where(x => controllers.Exists(y => y.boneType.Equals(x.boneType)))
                dto.bones.Add(bone.ToBoneDto());
            }
            return dto;
        }

        public UserTrackingBoneDto GetController(uint boneType)
        {
            return new UserTrackingBoneDto()
            {
                bone = bones[boneType].ToControllerDto()
            };
        }

        public void UpdateBones(UserTrackingFrameDto trackingFrame)
        {
            receivedTypes.Clear();
            foreach (var bone in trackingFrame.trackedBones)
            {
                if (controllers.Find(c => c.boneType == bone.boneType) is not DistantController vc)
                {
                    vc = new DistantController
                    {
                        boneType = bone.boneType,
                        isOverrider = bone.isOverrider,
                        position = bone.position.Struct(),
                        rotation = bone.rotation.Quaternion()
                    };
                    controllers.Add(vc);
                    extrapolators[bone.boneType] = (new(), new(), vc);
                }

                vc.isActif = true;
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

            Queue<DistantController> controllersToRemove = new();
            foreach (var c in controllers)
            {
                if (c is DistantController dc && !receivedTypes.Contains(c.boneType))
                {
                    extrapolators.Remove(c.boneType);
                    controllersToRemove.Enqueue(dc);
                    controllersToDestroy.Add(dc);
                }
            }
            foreach (var dc in controllersToRemove)
                controllers.Remove(dc);
        }

        public void WriteTrackingFrame(UserTrackingFrameDto trackingFrame, TrackingOption option)
        {
            if (bones.Count == 0)
                return;

            trackingFrame.trackedBones = new(bones.Count);

            foreach (var controller in controllers)
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

        #region Ik

        /// <summary>
        /// Called by OnAnimatorIK in TrackedAnimator
        /// </summary>
        /// <param name="layerIndex"></param>
        private void HandleAnimatorIK(int layerIndex)
        {
            CleanToDestroy();

            bones.ForEach(b => b.Value.positionComputed = false);

            foreach (var controller in controllers)
            {

                switch (controller.boneType)
                {
                    case BoneType.LeftKnee:
                        SetComputed(controller.boneType, BoneType.LeftHip);
                        SetHint(controller, AvatarIKHint.LeftKnee);
                        break;

                    case BoneType.RightKnee:
                        SetComputed(controller.boneType, BoneType.RightHip);
                        SetHint(controller, AvatarIKHint.RightKnee);
                        break;

                    case BoneType.LeftForearm:
                        SetComputed(controller.boneType, BoneType.LeftUpperArm);
                        SetHint(controller, AvatarIKHint.LeftElbow);
                        break;

                    case BoneType.RightForearm:
                        SetComputed(controller.boneType, BoneType.RightUpperArm);
                        SetHint(controller, AvatarIKHint.RightElbow);
                        break;

                    case BoneType.LeftAnkle:
                        SetComputed(controller.boneType, BoneType.LeftKnee, BoneType.LeftHip);
                        SetGoal(controller, AvatarIKGoal.LeftFoot);
                        break;

                    case BoneType.RightAnkle:
                        SetComputed(controller.boneType, BoneType.RightKnee, BoneType.RightHip);
                        SetGoal(controller, AvatarIKGoal.RightFoot);
                        break;

                    case BoneType.LeftHand:
                        SetComputed(controller.boneType, BoneType.LeftForearm, BoneType.LeftUpperArm);
                        SetGoal(controller, AvatarIKGoal.LeftHand);
                        break;

                    case BoneType.RightHand:
                        SetComputed(controller.boneType, BoneType.RightForearm, BoneType.RightUpperArm);
                        SetGoal(controller, AvatarIKGoal.RightHand);
                        break;

                    case BoneType.Head:
                        SetComputed(controller.boneType);
                        LookAt(controller);
                        break;

                    case BoneType.Viewpoint:
                        SetComputed(controller.boneType);
                        this.bones[controller.boneType].transform.rotation = controller.rotation;
                        break;

                    default:
                        var boneTypeUnity = BoneTypeConvertingExtensions.ConvertToBoneType(controller.boneType);
                        if (boneTypeUnity.HasValue)
                        {
                            SetComputed(controller.boneType);
                            SetControl(controller, boneTypeUnity.Value);
                        }
                        break;
                }
            }
        }

        private void CleanToDestroy()
        {
            //clean controllers to destroy;
            foreach (var controller in controllersToDestroy)
            {
                controller.isActif = false;
                switch (controller.boneType)
                {
                    case BoneType.LeftKnee:
                        SetHint(controller, AvatarIKHint.LeftKnee);
                        break;

                    case BoneType.RightKnee:
                        SetHint(controller, AvatarIKHint.RightKnee);
                        break;

                    case BoneType.LeftForearm:
                        SetHint(controller, AvatarIKHint.LeftElbow);
                        break;

                    case BoneType.RightForearm:
                        SetHint(controller, AvatarIKHint.RightElbow);
                        break;

                    case BoneType.LeftAnkle:
                        SetGoal(controller, AvatarIKGoal.LeftFoot);
                        break;

                    case BoneType.RightAnkle:
                        SetGoal(controller, AvatarIKGoal.RightFoot);
                        break;

                    case BoneType.LeftHand:
                        SetGoal(controller, AvatarIKGoal.LeftHand);
                        break;

                    case BoneType.RightHand:
                        SetGoal(controller, AvatarIKGoal.RightHand);
                        break;

                    case BoneType.Head:
                        LookAt(controller);
                        break;

                    case BoneType.Viewpoint:
                        this.bones[controller.boneType].transform.rotation = controller.rotation;
                        break;

                    default:
                        SetControl(controller, BoneTypeConvertingExtensions.ConvertToBoneType(controller.boneType).Value);
                        break;
                }
                controller.Destroy();
            }
            controllersToDestroy.Clear();
        }


        void SetComputed(params uint[] bones)
        {
            foreach (var boneType in bones)
                if (this.bones.ContainsKey(boneType))
                    this.bones[boneType].positionComputed = true;
        }

        private void SetControl(IController controller, HumanBodyBones goal)
        {
            if (controller.isActif)
                animator.SetBoneLocalRotation(goal, controller.rotation);
        }

        private void SetGoal(IController controller, AvatarIKGoal goal)
        {
            if (controller.isActif)
            {
                animator.SetIKPosition(goal, controller.position);

                switch (controller.boneType)
                {
                    case BoneType.RightHand:
                        animator.SetIKRotation(goal, controller.rotation * Quaternion.Euler(0, 90, 0));
                        break;
                    case BoneType.LeftHand:
                        animator.SetIKRotation(goal, controller.rotation * Quaternion.Euler(0, -90, 0));
                        break;
                    default:
                        animator.SetIKRotation(goal, controller.rotation);
                        break;
                }

                animator.SetIKPositionWeight(goal, 1);
                animator.SetIKRotationWeight(goal, 1);
            }
            else
            {
                animator.SetIKPositionWeight(goal, 0);
                animator.SetIKRotationWeight(goal, 0);
            }
        }

        private void SetHint(IController controller, AvatarIKHint hint)
        {
            if (controller.isActif)
            {
                animator.SetIKHintPosition(hint, controller.position);
                animator.SetIKHintPositionWeight(hint, 0.6f);
            }
            else
            {
                animator.SetIKHintPositionWeight(hint, 0);
            }
        }

        private void LookAt(IController controller)
        {
            if (controller.isActif)
            {
                var pos = controller.boneType == BoneType.Head ? controller.position + controller.rotation * Vector3.forward : controller.position;
                animator.SetLookAtPosition(pos);
                animator.SetLookAtWeight(1);
            }
            else
            {
                animator.SetLookAtWeight(0);
            }
        }

        #endregion Ik
    }
}