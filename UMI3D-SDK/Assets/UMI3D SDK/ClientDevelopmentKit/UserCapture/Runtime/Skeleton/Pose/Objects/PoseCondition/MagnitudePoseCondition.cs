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

using System;
using umi3d.cdk.userCapture.tracking;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Wrapper for <see cref="MagnitudeConditionDto"/>.
    /// </summary>
    public class MagnitudePoseCondition : IPoseCondition
    {
        protected readonly MagnitudeConditionDto magnitudeConditionDto;

        protected Transform nodeTransform;

        protected ITrackedSubskeleton trackedSkeleton;

        public MagnitudePoseCondition(MagnitudeConditionDto dto, Transform nodeTransform, ITrackedSubskeleton trackedSkeleton)
        {
            if (nodeTransform == null)
                throw new ArgumentNullException(nameof(nodeTransform));

            this.magnitudeConditionDto = dto ?? throw new ArgumentNullException(nameof(dto));
            this.nodeTransform = nodeTransform;
            this.trackedSkeleton = trackedSkeleton ?? throw new ArgumentNullException(nameof(trackedSkeleton));
        }

        /// <inheritdoc/>
        public bool Check()
        {
            Vector3 targetPosition = nodeTransform.position;

            Vector3 bonePosition = Vector3.zero;
            if (trackedSkeleton.TrackedBones.TryGetValue(magnitudeConditionDto.BoneOrigin, out TrackedSubskeletonBone bone))
                bonePosition = bone.transform.position;

            if (magnitudeConditionDto.IgnoreHeight)
            {
                targetPosition = Vector3.ProjectOnPlane(targetPosition, Vector3.up);
                bonePosition = Vector3.ProjectOnPlane(bonePosition, Vector3.up);
            }

            return Vector3.Distance(targetPosition, bonePosition) < magnitudeConditionDto.Magnitude;
        }
    }
}