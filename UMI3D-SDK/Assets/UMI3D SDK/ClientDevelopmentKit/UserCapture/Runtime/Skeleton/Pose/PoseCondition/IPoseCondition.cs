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

using umi3d.cdk.userCapture.tracking;
using umi3d.common;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace umi3d.cdk.userCapture.pose
{
    public interface IPoseCondition
    {
        public bool Check();
    }

    public class MagnitudePoseCondition : IPoseCondition
    {
        protected MagnitudeConditionDto MagnitudeConditionDto;

        protected Transform nodeTransform;

        protected TrackedSkeleton trackedSkeleton;

        public MagnitudePoseCondition(MagnitudeConditionDto dto, Transform nodeTransform, TrackedSkeleton trackedSkeleton)
        {
            this.MagnitudeConditionDto = dto;
            this.nodeTransform = nodeTransform;
            this.trackedSkeleton = trackedSkeleton;
        }

        public bool Check()
        {
            Vector3 targetPosition = nodeTransform.position;

            Vector3 bonePosition = Vector3.zero;
            if (trackedSkeleton.bones.TryGetValue(MagnitudeConditionDto.BoneOrigin, out TrackedSkeletonBone bone))
                bonePosition = bone.transform.position;

            return Vector3.Distance(targetPosition, bonePosition) < MagnitudeConditionDto.Magnitude;
        }
    }

    public class BoneRotationPoseCondition : IPoseCondition
    {
        protected BoneRotationConditionDto boneRotationConditionDto;

        protected TrackedSkeleton trackedSkeleton;

        public BoneRotationPoseCondition(BoneRotationConditionDto dto, TrackedSkeleton trackedSkeleton)
        {
            this.boneRotationConditionDto = dto;
            this.trackedSkeleton = trackedSkeleton;
        }

        public bool Check()
        {
            Quaternion boneRotation;
            if (trackedSkeleton.bones.TryGetValue(boneRotationConditionDto.BoneId, out TrackedSkeletonBone bone))
                boneRotation = bone.transform.rotation;
            else
                return false;

            return Quaternion.Angle(boneRotation, boneRotationConditionDto.Rotation.Quaternion()) < boneRotationConditionDto.AcceptanceRange;
        }
    }

    public class ScalePoseCondition : IPoseCondition
    {
        protected ScaleConditionDto scaleConditionDto;

        protected Transform nodeTransform;

        public ScalePoseCondition(ScaleConditionDto dto, Transform nodeTransform)
        {
            this.scaleConditionDto = dto;
            this.nodeTransform = nodeTransform;
        }

        public bool Check()
        {
            Vector3 targetScale = nodeTransform.localScale;
            Vector3 wantedScale = scaleConditionDto.Scale.Struct();

            return targetScale.sqrMagnitude <= wantedScale.sqrMagnitude;
        }
    }
}