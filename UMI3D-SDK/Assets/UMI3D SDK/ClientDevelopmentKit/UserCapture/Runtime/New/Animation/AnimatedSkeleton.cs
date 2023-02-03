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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.common.userCapture;

namespace umi3d.cdk.userCapture
{

    public class AnimatedSkeleton : ISubSkeleton
    {
        public SkeletonMapper mapper;

        public UserCameraPropertiesDto GetCameraDto()
        {
            return null;
        }

        public virtual PoseDto GetPose()
        {
            if (!mapper.animations.Select(id => UMI3DAnimatorAnimation.Get(id)).Any(a => a?.IsPlayin() ?? false))
                return null;
            return mapper.GetPose();
        }

        public void Update(UserTrackingFrameDto trackingFrame)
        {
            throw new System.NotImplementedException();
        }

        public void WriteTrackingFrame(UserTrackingFrameDto trackingFrame, TrackingOption option)
        {
            throw new System.NotImplementedException();
        }
    }
}