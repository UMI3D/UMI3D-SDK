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
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using umi3d.common.userCapture.tracking;
using UnityEngine;

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

        private List<uint> types = new List<uint>();

        public void Start()
        {
            trackedAnimator.IkCallback = new System.Action<int>((u => HandleAnimatorIK(u)));

            foreach (var bone in GetComponentsInChildren<TrackedSubskeletonBone>())
            {
                if (!bones.ContainsKey(bone.boneType))
                    bones.Add(bone.boneType, bone);
                if (bone.GetType() == typeof(TrackedSubskeletonBoneController))
                    controllers.Add(new DistantController() { boneType = bone.boneType, isActif = true, position = bone.transform.position, rotation = bone.transform.rotation, isOverrider = true });
            }
        }

        protected void Update()
        {
            for (int i = 0; i < controllers.Count; i++)
            {
                var controller = controllers[i];
                (controller as DistantController).position = bones[controller.boneType].transform.position;
                (controller as DistantController).rotation = bones[controller.boneType].transform.rotation;
            }
        }

        public PoseDto GetPose()
        {
            var dto = new PoseDto
            {
                bones = bones
                    .Select(kp => kp.Value)
                    //.Where(x => controllers.Exists(y => y.boneType.Equals(x.boneType)))
                    .Select(tb => tb.ToBoneDto()).ToList()
            };
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
            types.Clear();
            foreach (var bone in trackingFrame.trackedBones)
            {
                DistantController vc = controllers.Find(c => c.boneType == bone.boneType) as DistantController;

                if (vc == null)
                {
                    vc = new DistantController();
                    vc.boneType = bone.boneType;
                    vc.isOverrider = bone.isOverrider;
                    controllers.Add(vc);
                }

                vc.isActif = true;
                vc.position = bone.position.Struct();
                vc.rotation = bone.rotation.Quaternion();

                types.Add(bone.boneType);

                if (bones.TryGetValue(bone.boneType, out var boneTransform) && bone.boneType.Equals(BoneType.Head))
                {
                    boneTransform.transform.rotation = vc.rotation;
                }
            }
            foreach (var dc in controllers.Where(c => c is DistantController && !types.Contains(c.boneType)).ToList())
            {
                controllersToDestroy.Add(dc);
                controllers.Remove(dc);
            }
        }

        public void WriteTrackingFrame(UserTrackingFrameDto trackingFrame, TrackingOption option)
        {
            if (bones.Count == 0)
                return;
            trackingFrame.trackedBones = bones.Select(kp => kp.Value).OfType<TrackedSubskeletonBoneController>().Select(tb => tb.ToControllerDto()).Where(b => b != null).ToList();
            foreach (var asyncBone in BonesAsyncFPS)
            {
                trackingFrame.trackedBones.Add(bones.First(p => p.Value.boneType == asyncBone.Key).Value.ToControllerDto());
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

            foreach (var controller in controllers.Where(c => c.boneType != BoneType.Head))
            {
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

                    case BoneType.Viewpoint:
                        LookAt(controller);
                        break;

                    default:
                        SetControl(controller, BoneTypeConvertingExtensions.ConvertToBoneType(controller.boneType).Value);
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

                    case BoneType.Viewpoint:
                        LookAt(controller);
                        break;

                    default:
                        SetControl(controller, BoneTypeConvertingExtensions.ConvertToBoneType(controller.boneType).Value);
                        break;
                }
                controller.Destroy();
            }
            controllersToDestroy.Clear();
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
                animator.SetIKHintPositionWeight(hint, 1);
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
                animator.SetLookAtPosition(controller.position);
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