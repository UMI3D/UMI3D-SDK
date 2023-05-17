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
using System.Runtime.CompilerServices;
using System.Threading;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.userCapture
{
    public class TrackedSkeleton : MonoBehaviour, ISubWritableSkeleton
    {
        public List<IController> controllers = new List<IController>();
        List<IController> controllersToDestroy;
        public Camera viewpoint;
        public TrackedAnimator trackedAnimator;
        Animator animator;
        public Dictionary<uint, TrackedSkeletonBone> bones = new Dictionary<uint, TrackedSkeletonBone>();
        List<uint> types = new List<uint>();

        private ISkeletonManager skeletonManager;

        public void Start()
        {
             foreach (var bone in GetComponentsInChildren<TrackedSkeletonBone>())
            {
                if (!bones.ContainsKey(bone.boneType))
                    bones.Add(bone.boneType, bone);
                if (bone.GetType() == typeof(TrackedSkeletonBoneController))
                    controllers.Add(new DistantController() { boneType = bone.boneType, isActif = true, position = bone.transform.position, rotation = bone.transform.rotation, isOverrider = true });
            }

            skeletonManager = PersonalSkeletonManager.Instance;
        }

        public UserCameraPropertiesDto GetCameraDto()
        {
            return new UserCameraPropertiesDto()
            {
                scale = 1f,
                projectionMatrix = viewpoint.projectionMatrix.Dto(),
                boneType = BoneType.Viewpoint,
            };
        }

        public PoseDto GetPose()
        {
            var dto = new PoseDto();
            dto?.SetBonePoseDtoArray(bones
                .Select(kp => kp.Value)
                .Where(x=> x is IController)
                .Select(tb => tb.ToBoneDto()).ToList());
            return dto;
        }

        public UserTrackingBoneDto GetBone(uint boneType)
        {
            return new UserTrackingBoneDto()
            {
                bone = bones[boneType].ToControllerDto()
            };
        }

        public void UpdateFrame(UserTrackingFrameDto trackingFrame)
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
            }
            foreach (var dc in controllers.Where(c => c is DistantController && !types.Contains(c.boneType)).ToList())
            {
                controllersToDestroy.Add(dc);
                controllers.Remove(dc);
            }
        }

        /// <summary>
        /// Unity Update method, just here because Tthe other update method is generating an error.
        /// </summary>
        public void Update()
        { }

        public void WriteTrackingFrame(UserTrackingFrameDto trackingFrame, TrackingOption option)
        {
            trackingFrame.trackedBones = bones.Select(kp => kp.Value).OfType<TrackedSkeletonBoneController>().Select(tb => tb.ToControllerDto()).Where(b => b != null).ToList();
            foreach (var asyncBone in skeletonManager.personalSkeleton.BonesAsyncFPS)
            {
                trackingFrame.trackedBones.Add(bones.First(b => b.Value.boneType.Equals(asyncBone.Key)).Value.ToControllerDto());
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

            foreach (var controller in controllers)
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
                        SetControl(controller, BoneToHumanBodyBones(controller.boneType));
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
                        SetControl(controller, BoneToHumanBodyBones(controller.boneType));
                        break;
                }
                controller.Destroy();
            }
            controllersToDestroy.Clear();
        }

        private HumanBodyBones BoneToHumanBodyBones(uint boneType)
        {
            switch (boneType)
            {
                case BoneType.Hips:
                    return HumanBodyBones.Hips;
                case BoneType.LeftHip:
                    return HumanBodyBones.LeftUpperLeg;
                case BoneType.RightHip:
                    return HumanBodyBones.RightUpperLeg;
                case BoneType.LeftKnee:
                    return HumanBodyBones.LeftLowerLeg;
                case BoneType.RightKnee:
                    return HumanBodyBones.RightLowerLeg;
                case BoneType.LeftAnkle:
                    return HumanBodyBones.LeftFoot;
                case BoneType.RightAnkle:
                    return HumanBodyBones.RightFoot;
                case BoneType.Spine:
                    return HumanBodyBones.Spine;
                case BoneType.Chest:
                    return HumanBodyBones.Chest;
                case BoneType.UpperChest:
                    return HumanBodyBones.UpperChest;
                case BoneType.Neck:
                    return HumanBodyBones.Neck;
                case BoneType.Head:
                    return HumanBodyBones.Head;
                case BoneType.LeftShoulder:
                    return HumanBodyBones.LeftShoulder;
                case BoneType.RightShoulder:
                    return HumanBodyBones.RightShoulder;
                case BoneType.LeftUpperArm:
                    return HumanBodyBones.LeftUpperArm;
                case BoneType.RightUpperArm:
                    return HumanBodyBones.RightUpperArm;
                case BoneType.LeftForearm:
                    return HumanBodyBones.LeftLowerArm;
                case BoneType.RightForearm:
                    return HumanBodyBones.RightLowerArm;
                case BoneType.LeftHand:
                    return HumanBodyBones.LeftHand;
                case BoneType.RightHand:
                    return HumanBodyBones.RightHand;
                case BoneType.LeftToeBase:
                    return HumanBodyBones.LeftToes;
                case BoneType.RightToeBase:
                    return HumanBodyBones.RightToes;
                case BoneType.LeftEye:
                    return HumanBodyBones.LeftEye;
                case BoneType.RightEye:
                    return HumanBodyBones.RightEye;
                case BoneType.Jaw:
                    return HumanBodyBones.Jaw;
                case BoneType.LeftThumbProximal:
                    return HumanBodyBones.LeftThumbProximal;
                case BoneType.LeftThumbIntermediate:
                    return HumanBodyBones.LeftThumbIntermediate;
                case BoneType.LeftThumbDistal:
                    return HumanBodyBones.LeftThumbDistal;
                case BoneType.LeftIndexProximal:
                    return HumanBodyBones.LeftIndexProximal;
                case BoneType.LeftIndexIntermediate:
                    return HumanBodyBones.LeftIndexIntermediate;
                case BoneType.LeftIndexDistal:
                    return HumanBodyBones.LeftIndexDistal;
                case BoneType.LeftMiddleProximal:
                    return HumanBodyBones.LeftMiddleProximal;
                case BoneType.LeftMiddleIntermediate:
                    return HumanBodyBones.LeftMiddleIntermediate;
                case BoneType.LeftMiddleDistal:
                    return HumanBodyBones.LeftMiddleDistal;
                case BoneType.LeftRingProximal:
                    return HumanBodyBones.LeftRingProximal;
                case BoneType.LeftRingIntermediate:
                    return HumanBodyBones.LeftRingIntermediate;
                case BoneType.LeftRingDistal:
                    return HumanBodyBones.LeftRingDistal;
                case BoneType.LeftLittleProximal:
                    return HumanBodyBones.LeftLittleProximal;
                case BoneType.LeftLittleIntermediate:
                    return HumanBodyBones.LeftLittleIntermediate;
                case BoneType.LeftLittleDistal:
                    return HumanBodyBones.LeftLittleDistal;
                case BoneType.RightThumbProximal:
                    return HumanBodyBones.RightThumbProximal;
                case BoneType.RightThumbIntermediate:
                    return HumanBodyBones.RightThumbIntermediate;
                case BoneType.RightThumbDistal:
                    return HumanBodyBones.RightThumbDistal;
                case BoneType.RightIndexProximal:
                    return HumanBodyBones.RightIndexProximal;
                case BoneType.RightIndexIntermediate:
                    return HumanBodyBones.RightIndexIntermediate;
                case BoneType.RightIndexDistal:
                    return HumanBodyBones.RightIndexDistal;
                case BoneType.RightMiddleProximal:
                    return HumanBodyBones.RightMiddleProximal;
                case BoneType.RightMiddleIntermediate:
                    return HumanBodyBones.RightMiddleIntermediate;
                case BoneType.RightMiddleDistal:
                    return HumanBodyBones.RightMiddleDistal;
                case BoneType.RightRingProximal:
                    return HumanBodyBones.RightRingProximal;
                case BoneType.RightRingIntermediate:
                    return HumanBodyBones.RightRingIntermediate;
                case BoneType.RightRingDistal:
                    return HumanBodyBones.RightRingDistal;
                case BoneType.RightLittleProximal:
                    return HumanBodyBones.RightLittleProximal;
                case BoneType.RightLittleIntermediate:
                    return HumanBodyBones.RightLittleIntermediate;
                case BoneType.RightLittleDistal:
                    return HumanBodyBones.RightLittleDistal;
                case BoneType.LastBone:
                    return HumanBodyBones.LastBone;
            }


            return HumanBodyBones.Hips;
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
                animator.SetIKRotation(goal, controller.rotation);
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
        #endregion

    }

}