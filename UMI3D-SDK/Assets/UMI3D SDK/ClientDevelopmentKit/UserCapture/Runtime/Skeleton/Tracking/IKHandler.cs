/*
Copyright 2019 - 2024 Inetum

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
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using UnityEngine;

namespace umi3d.cdk.userCapture.tracking
{
    public class IKHandler
    {
        private readonly Animator animator;

        public IKHandler(Animator animator)
        {
            this.animator = animator != null ? animator : throw new System.ArgumentNullException(nameof(animator));
        }

        /// <summary>
        /// Called by OnAnimatorIK in TrackedAnimator, set all tracked bones as computed and positions IK hints.
        /// </summary>
        /// <param name="layerIndex"></param>
        public void HandleAnimatorIK(int layerIndex, IEnumerable<IController> controllers, IDictionary<uint, TrackedSubskeletonBone> bones)
        {
            bones.Values.ForEach(b => b.positionComputed = false);

            void SetComputed(params uint[] bonesToSet)
            {
                foreach (var boneType in bonesToSet)
                    if (bones.TryGetValue(boneType, out var bone))
                        bones[boneType].positionComputed = true;
            }

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
                        bones[controller.boneType].transform.rotation = controller.rotation;
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

        public void Reset(IEnumerable<IController> controllers, IDictionary<uint, TrackedSubskeletonBone> bones)
        {
            //clean controllers to destroy;
            foreach (var controller in controllers)
            {
                controller.isActive = false;
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
                        bones[controller.boneType].transform.rotation = controller.rotation;
                        break;

                    default:
                        SetControl(controller, BoneTypeConvertingExtensions.ConvertToBoneType(controller.boneType).Value);
                        break;
                }
            }
        }


        private void SetControl(IController controller, HumanBodyBones goal)
        {
            if (controller.isActive)
                animator.SetBoneLocalRotation(goal, controller.rotation);
        }

        private void SetGoal(IController controller, AvatarIKGoal goal)
        {
            if (controller.isActive)
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
            if (controller.isActive)
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
            if (controller.isActive)
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

    }
}
