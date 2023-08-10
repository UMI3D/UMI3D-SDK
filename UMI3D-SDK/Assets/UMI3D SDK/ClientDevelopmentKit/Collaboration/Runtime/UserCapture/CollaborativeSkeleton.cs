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
using umi3d.cdk.utils.extrapolation;
using umi3d.common.userCapture.tracking;

namespace umi3d.cdk.collaboration.userCapture
{
    /// <summary>
    /// Skeleton of other users than the browser's main user.
    /// </summary>
    public class CollaborativeSkeleton : AbstractSkeleton
    {
        protected Vector3LinearDelayedExtrapolator posExtrapolator = new();
        protected QuaternionLinearDelayedExtrapolator rotExtrapolator = new();

        private void Update()
        {
            transform.position = posExtrapolator.Extrapolate();
            transform.rotation = rotExtrapolator.Extrapolate();
        }

        public override void UpdateBones(UserTrackingFrameDto frame)
        {
            foreach (ISubskeleton skeleton in Subskeletons)
            {
                if (skeleton is IWritableSubskeleton writableSubskeleton)
                    writableSubskeleton.UpdateBones(frame);
            }

            posExtrapolator.AddMeasure(frame.position.Struct());
            rotExtrapolator.AddMeasure(frame.rotation.Quaternion());
        }
    }
}