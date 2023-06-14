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
        List<IController> controllersToDestroy = new();
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

        public virtual Vector3 GetBonePosition(uint index)
        {
            bones.TryGetValue(index, out var boneTransform);
            return boneTransform.transform.position;
        }

        public virtual Quaternion GetBoneRotation(uint index)
        {
            bones.TryGetValue(index, out var boneTransform);
            return boneTransform.transform.rotation;
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
                        SetControl(controller, BoneTypeConverter.ConvertToBoneType(controller.boneType).Value);
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
                        SetControl(controller, BoneTypeConverter.ConvertToBoneType(controller.boneType).Value);
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