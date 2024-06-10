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

using System.Collections.Generic;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using UnityEngine;

namespace umi3d.cdk.userCapture.tracking.ik
{
    /// <summary>
    /// Compute IK through an animator.
    /// </summary>
    public class AnimatorIKHandler : IIKHandler
    {
        protected readonly Animator animator;

        /// <summary>
        /// Animator controlling the IK.
        /// </summary>
        public Animator Animator => animator;

        public AnimatorIKHandler(Animator animator)
        {
            this.animator = animator != null ? animator : throw new System.ArgumentNullException(nameof(animator));
        }

        public readonly IReadOnlyDictionary<uint, AvatarIKHint> IKHints = new Dictionary<uint, AvatarIKHint>()
        {
            { BoneType.LeftKnee, AvatarIKHint.LeftKnee},
            { BoneType.RightKnee, AvatarIKHint.RightKnee },

            { BoneType.LeftForearm, AvatarIKHint.LeftElbow},
            { BoneType.RightForearm, AvatarIKHint.RightElbow },
        };

        public readonly IReadOnlyDictionary<uint, AvatarIKGoal> IKGoals = new Dictionary<uint, AvatarIKGoal>()
        {
            { BoneType.LeftAnkle, AvatarIKGoal.LeftFoot },
            { BoneType.RightAnkle, AvatarIKGoal.RightFoot },

            { BoneType.LeftHand, AvatarIKGoal.LeftHand },
            { BoneType.RightHand, AvatarIKGoal.RightHand },
        };

        /// <summary>
        /// <inheritdoc/> <br/>
        /// Called by OnAnimatorIK in TrackedAnimator, set all tracked bones as computed and positions IK hints.
        /// </summary>
        /// <param name="layerIndex"></param>
        public void HandleAnimatorIK(int layerIndex, IEnumerable<IController> controllers)
        {
            foreach (var controller in controllers)
            {
                HandleAnimatorIK(0, controller);
            }
        }

        /// <summary>
        /// <inheritdoc/> <br/>
        /// Called by OnAnimatorIK in TrackedAnimator, set all tracked bones as computed and positions IK hints.
        /// </summary>
        /// <param name="layerIndex"></param>
        public void HandleAnimatorIK(int layerIndex, IController controller)
        {
            if (IKHints.TryGetValue(controller.boneType, out AvatarIKHint avatarIKHint))
                SetHint(controller, avatarIKHint);
            else if (IKGoals.TryGetValue(controller.boneType, out AvatarIKGoal avatarIKGoal))
                SetGoal(controller, avatarIKGoal);
            else if (controller.boneType is BoneType.Head)
                LookAt(controller);
            else if (controller.boneType is not BoneType.Hips) // hips should not be moved by any IK controller.
            {
                HumanBodyBones? boneTypeUnity = BoneTypeConvertingExtensions.ConvertToBoneType(controller.boneType);
                if (boneTypeUnity.HasValue)
                    SetControl(controller, boneTypeUnity.Value);
            }
        }

        /// <inheritdoc/>
        public void Reset(IEnumerable<IController> controllers, IDictionary<uint, TrackedSubskeletonBone> bones)
        {
            //clean controllers to destroy;
            foreach (var controller in controllers)
            {
                controller.isActive = false;
            }
            HandleAnimatorIK(0, controllers);
        }

        protected void SetControl(IController controller, HumanBodyBones goal)
        {
            if (controller.isActive)
                animator.SetBoneLocalRotation(goal, controller.rotation);
        }

        protected void SetGoal(IController controller, AvatarIKGoal goal)
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

        protected void SetHint(IController controller, AvatarIKHint hint)
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

        protected void LookAt(IController controller)
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