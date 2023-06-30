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
using umi3d.common.userCapture.animation;
using UnityEngine;

namespace umi3d.edk.userCapture.animation
{
    /// <summary>
    ///  A Skeleton node is a subskeleton with a Unity Animator
    /// that is packaged in a bundle. It is loaded the same way as a Mesh.
    /// </summary>
    public class UMI3DSkeletonAnimationNode : UMI3DModel
    {
        /// <summary>
        /// List of states names in the embedded animator.
        /// </summary>
        [Tooltip("List of states names in the embedded animator.")]
        public List<string> animationStates = new();

        /// <summary>
        /// ID of the user owning this node. Their skeleton is the one affected by this node.
        /// </summary>
        [EditorReadOnly]
        public ulong userId;

        /// <summary>
        /// Collection of ID of UMI3D animations associated with this node.
        /// </summary>
        [EditorReadOnly]
        public ulong[] relatedAnimationIds;

        /// <summary>
        /// Priority for application of skeleton animation.
        /// </summary>
        public uint priority;

        /// <summary>
        /// List of parameters that are updated by the browsers themselves based on skeleton movement.
        /// </summary>
        /// Available parameters are listed in <see cref="SkeletonAnimatorParameterKeys"/>.
        public uint[] animatorSelfTrackedParameters;

        /// <inheritdoc/>
        protected override UMI3DNodeDto CreateDto()
        {
            return new SkeletonAnimationNodeDto();
        }

        protected override void WriteProperties(UMI3DAbstractNodeDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var skeletonNodeDto = dto as SkeletonAnimationNodeDto;
            skeletonNodeDto.userId = userId;
            skeletonNodeDto.relatedAnimationsId = relatedAnimationIds ?? new ulong[0];
            skeletonNodeDto.priority = priority;
            skeletonNodeDto.animatorSelfTrackedParameters = animatorSelfTrackedParameters ?? new uint[0];
        }

        public override Bytable ToBytes(UMI3DUser user)
        {
            return base.ToBytes(user)
                    + UMI3DSerializer.Write(userId)
                    + UMI3DSerializer.Write(priority)
                    + UMI3DSerializer.WriteCollection(relatedAnimationIds)
                    + UMI3DSerializer.WriteCollection(animatorSelfTrackedParameters);
        }
    }
}