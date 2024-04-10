﻿/*
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
using umi3d.common;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using umi3d.edk.core;

namespace umi3d.edk.userCapture.pose
{
    /// <summary>
    /// Pose animation.
    /// </summary>
    public class PoseClip : AbstractLoadableEntity
    {
        /// <summary>
        /// Pose animation description.
        /// </summary>
        public readonly UMI3DAsyncProperty<IUMI3DPoseData> PoseResource;

        public PoseClip(IUMI3DPoseData poseResource)
        {
            if (poseResource == null)
                throw new System.ArgumentNullException(nameof(poseResource));

            this.PoseResource = new UMI3DAsyncProperty<IUMI3DPoseData>(Id(), UMI3DPropertyKeys.PoseClipResource, poseResource);
            IsComposable = new UMI3DAsyncProperty<bool>(Id(), UMI3DPropertyKeys.PoseClipComposable, true);
            IsInterpolable = new UMI3DAsyncProperty<bool>(Id(), UMI3DPropertyKeys.PoseClipInterpolable, true);
        }

        /// <summary>
        /// The bone that anchor the pose
        /// </summary>
        public PoseAnchorDto Anchor => PoseResource.GetValue().Anchor;

        /// <summary>
        /// All the bones that describe the pose
        /// </summary>
        public IList<BoneDto> Bones => PoseResource.GetValue().Bones;

        /// <summary>
        /// See <see cref="PoseClipDto.isComposable"/>.
        /// </summary>
        public readonly UMI3DAsyncProperty<bool> IsComposable;

        /// <summary>
        /// See <see cref="PoseClipDto.isInterpolable"/>.
        /// </summary>
        public readonly UMI3DAsyncProperty<bool> IsInterpolable;

        /// <inheritdoc/>
        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return new PoseClipDto()
            {
                id = Id(),
                pose = PoseResource.GetValue(user).ToPoseDto(),
                isInterpolable = IsInterpolable.GetValue(user),
                isComposable = IsComposable.GetValue(user),
            };
        }
    }
}