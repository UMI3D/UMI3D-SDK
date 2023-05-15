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
using System.Linq;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.userCapture
{
    public class PersonalSkeleton : AbstractSkeleton
    {
        public Dictionary<uint, float> BonesAsyncFPS { get; protected set; } = new();

        public Vector3 worldSize => TrackedSkeleton.transform.lossyScale;

        protected void Start()
        {
            Skeletons = new List<ISubSkeleton>
            {
                TrackedSkeleton, poseSkeleton
            };
        }

        public UserTrackingFrameDto GetFrame(TrackingOption option)
        {
            var frame = new UserTrackingFrameDto()
            {
                position = transform.position.Dto(),
                rotation = transform.rotation.Dto(),
            };

            foreach (ISubWritableSkeleton skeleton in Skeletons.OfType<ISubWritableSkeleton>())
                skeleton.WriteTrackingFrame(frame, option);

            return frame;
        }

        public UserCameraPropertiesDto GetCameraProperty()
        {
            //The first skeleton is the TrackedSkeleton
            foreach (var skeleton in Skeletons)
            {
                var c = skeleton.GetCameraDto();
                if (c != null)
                    return c;
            }
            return null;
        }

        public override void UpdateFrame(UserTrackingFrameDto frame)
        {
            if (Skeletons != null)
                foreach (ISubWritableSkeleton skeleton in Skeletons.OfType<ISubWritableSkeleton>())
                    skeleton.UpdateFrame(frame);
        }


        public void FixedUpdate()
        {
            // TODO: Delay this computation after the form
            this.Compute();
        }
    }
}