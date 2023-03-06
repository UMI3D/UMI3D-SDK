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
using umi3d.common.userCapture;

namespace umi3d.cdk.userCapture
{
    public class PoseSkeleton : ISubSkeleton
    {
        public PoseDto defaultPose => PoseManager.Instance.defaultPose;
        public PoseDto[] defaultPoses => PoseManager.Instance.defaultPoses;
        public PoseDto[] serverPoses;

        public PoseDto GetPose()
        {
            throw new System.NotImplementedException();
        }

        public void UpdateFrame(UserTrackingFrameDto trackingFrame)
        {
            throw new System.NotImplementedException();
        }

        public void WriteTrackingFrame(UserTrackingFrameDto trackingFrame, TrackingOption option)
        {
            throw new System.NotImplementedException();
        }

        public UserCameraPropertiesDto GetCameraDto()
        {
            return null;
        }
    }

}