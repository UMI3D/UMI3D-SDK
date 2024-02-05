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

using System;
using umi3d.cdk.userCapture.tracking;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Wrapper for <see cref="DirectionPoseConditionDto"/>.
    /// </summary>
    public class DirectionPoseCondition : IPoseCondition
    {
        protected readonly DirectionConditionDto directionConditionDto;

        protected Transform nodeTransform;

        protected ITrackedSubskeleton trackedSkeleton;

        public DirectionPoseCondition(DirectionConditionDto dto, Transform nodeTransform, ITrackedSubskeleton trackedSkeleton)
        {
            if (nodeTransform == null)
                throw new ArgumentNullException(nameof(nodeTransform));

            this.directionConditionDto = dto ?? throw new System.ArgumentNullException(nameof(dto));
            this.nodeTransform = nodeTransform;
            this.trackedSkeleton = trackedSkeleton;
        }

        /// <inheritdoc/>
        public bool Check()
        {
            if (!trackedSkeleton.TrackedBones.TryGetValue(directionConditionDto.BoneId, out TrackedSubskeletonBone bone))
                return false;

            Vector3 conditionDirectionWorld = nodeTransform.transform.TransformDirection(directionConditionDto.Direction.Struct());

            return Vector3.Angle(conditionDirectionWorld, bone.transform.position - nodeTransform.transform.position) <= directionConditionDto.Threshold;
        }
    }
}