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

using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.pose;
using umi3d.cdk.userCapture.tracking;
using umi3d.cdk.utils.extrapolation;
using umi3d.common.userCapture.tracking;

namespace umi3d.cdk.collaboration.userCapture
{
    public class CollaborativeSkeleton : AbstractSkeleton
    {
        protected Vector3LinearDelayedExtrapolator posExtrapolator = new();
        protected QuaternionLinearDelayedExtrapolator rotExtrapolator = new();

        private void Update()
        {
            transform.position = posExtrapolator.Extrapolate();
            transform.rotation = rotExtrapolator.Extrapolate();
        }

        public override void UpdateFrame(UserTrackingFrameDto frame)
        {
            if (Skeletons != null)
            {
                foreach (ISubSkeleton skeleton in Skeletons)
                {
                    if (skeleton is ISubWritableSkeleton writableSubskeleton)
                        writableSubskeleton.UpdateFrame(frame);
                }
            }

            posExtrapolator.AddMeasure(frame.position.Struct());
            rotExtrapolator.AddMeasure(frame.rotation.Quaternion());
        }

        public void SetSubSkeletons()
        {
            TrackedSkeleton = Instantiate((UMI3DEnvironmentLoader.Parameters as UMI3DCollabLoadingParameters).CollabTrackedSkeleton, this.transform).GetComponent<TrackedSkeleton>();
            HipsAnchor = TrackedSkeleton.Hips;
            PoseSkeleton = new PoseSkeleton();
            Skeletons.Add(TrackedSkeleton);
            Skeletons.Add(PoseSkeleton);
        }
    }
}