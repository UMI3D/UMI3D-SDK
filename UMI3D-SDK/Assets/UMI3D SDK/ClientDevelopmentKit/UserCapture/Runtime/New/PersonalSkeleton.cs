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
using umi3d.cdk.utils.extrapolation;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace umi3d.cdk.userCapture
{

    public class PersonalSkeleton : AbstractSkeleton
    {
        public TrackedSkeleton TrackedSkeleton;

        public float skeletonHighOffset = 0;

        public Vector3 worldSize => TrackedSkeleton.transform.lossyScale;

        public void Init()
        {
            subSkeletons = new List<ISubSkeleton>();
            subSkeletons[0] = TrackedSkeleton;
        }

        public UserTrackingFrameDto GetFrame(TrackingOption option) {
            var frame = new UserTrackingFrameDto()
            {
                position = PoseManager.Instance.transform.position,
                rotation = PoseManager.Instance.transform.rotation,
                skeletonHighOffset = skeletonHighOffset,
            };

            foreach (var skeleton in subSkeletons)
                skeleton.WriteTrackingFrame(frame, option);

            return frame;
        }

        public UserCameraPropertiesDto GetCameraProperty()
        {
            foreach(var skeleton in subSkeletons)
            {
                var c = skeleton.GetCameraDto();
                if(c != null)
                    return c;
            }
            return null;
        }

        public override void UpdateFrame(UserTrackingFrameDto frame)
        {
            UMI3DLogger.LogWarning("The personal ISkeleton should not receive frame", scope);
        }
    }
}