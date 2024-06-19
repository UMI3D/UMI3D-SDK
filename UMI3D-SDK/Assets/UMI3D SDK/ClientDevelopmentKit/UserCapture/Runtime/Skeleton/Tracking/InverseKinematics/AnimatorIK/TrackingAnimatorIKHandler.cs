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

namespace umi3d.cdk.userCapture.tracking.ik
{
    /// <summary>
    /// Ik logic for tracking subskeleton with computed bones marking.
    /// </summary>
    public class TrackingAnimatorIKHandler : AnimatorIKHandler
    {
        public TrackingAnimatorIKHandler(Animator animator) : base(animator)
        {
        }

        public readonly IReadOnlyDictionary<uint, uint[]> BonesToMarkComputed = new Dictionary<uint, uint[]>()
        {
            { BoneType.LeftKnee, new uint[] { BoneType.LeftHip } },
            { BoneType.RightKnee, new uint[] { BoneType.RightHip } },

            { BoneType.LeftForearm, new uint[] { BoneType.LeftUpperArm } },
            { BoneType.RightForearm, new uint[] { BoneType.RightUpperArm } },

            { BoneType.LeftAnkle, new uint[] { BoneType.LeftKnee, BoneType.LeftHip } },
            { BoneType.RightAnkle, new uint[] { BoneType.RightKnee, BoneType.RightHip } },

            { BoneType.LeftHand, new uint[] { BoneType.LeftForearm, BoneType.LeftUpperArm } },
            { BoneType.RightHand, new uint[] { BoneType.RightForearm, BoneType.RightUpperArm } },

            { BoneType.Head, new uint[] { } },
            { BoneType.Viewpoint, new uint[] { BoneType.Head } },

            { BoneType.Hips, new uint[] { } },
        };

        /// <summary>
        /// Called by OnAnimatorIK in TrackedAnimator, set all tracked bones as computed and positions IK hints.
        /// </summary>
        /// <param name="layerIndex"></param>
        public void HandleAnimatorIK(int layerIndex, IEnumerable<IController> controllers, IDictionary<uint, TrackedSubskeletonBone> bones)
        {
            HandleAnimatorIK(layerIndex, controllers);

            bones.Values.ForEach(b => b.positionComputed = false);

            void SetComputed(params uint[] bonesToSet)
            {
                foreach (var boneType in bonesToSet)
                    if (bones.TryGetValue(boneType, out var bone))
                        bones[boneType].positionComputed = true;
            }

            foreach (var controller in controllers)
            {
                if (BonesToMarkComputed.TryGetValue(controller.boneType, out uint[] toMarkComputed))
                {
                    SetComputed(controller.boneType);
                    SetComputed(toMarkComputed);

                    if (controller.boneType is BoneType.Viewpoint)
                        bones[controller.boneType].transform.rotation = controller.rotation;
                }
                else
                {
                    var boneTypeUnity = BoneTypeConvertingExtensions.ConvertToBoneType(controller.boneType);
                    if (boneTypeUnity.HasValue)
                    {
                        SetComputed(controller.boneType);
                        SetControl(controller, boneTypeUnity.Value);
                    }
                }
            }
        }
    }
}