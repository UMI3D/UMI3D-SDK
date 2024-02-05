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

using System.Collections.Generic;
using umi3d.cdk.userCapture.pose;
using umi3d.cdk.userCapture.tracking;
using umi3d.common.userCapture.tracking;
using UnityEngine;

namespace umi3d.cdk.userCapture
{
    /// <summary>
    /// Skeleton of the browser's main user.
    /// </summary>
    public class PersonalSkeleton : AbstractSkeleton, IPersonalSkeleton
    {
        public IDictionary<uint, float> BonesAsyncFPS => TrackedSubskeleton.BonesAsyncFPS;

        public Transform Transform => transform;

        /// <summary>
        /// Size of the skeleton.
        /// </summary>
        public Vector3 worldSize => TrackedSubskeleton.Hips.lossyScale;

        protected void Start()
        {
            PoseSubskeleton = new PoseSubskeleton();

            //Init(trackedSkeleton, PoseSubskeleton);
        }

        public void SelfInit()
        {
            Init(trackedSkeleton, PoseSubskeleton);
        }

        /// <inheritdoc/>
        public UserTrackingFrameDto GetFrame(TrackingOption option)
        {
            var frame = new UserTrackingFrameDto()
            {
                position = transform.position.Dto(),
                rotation = transform.rotation.Dto(),
            };

            lock (SubskeletonsLock)
                foreach (ISubskeleton skeleton in Subskeletons)
                {
                    if (skeleton is IWritableSubskeleton writableSkeleton)
                        writableSkeleton.WriteTrackingFrame(frame, option);
                }

            lastFrame = frame;
            return frame;
        }

        /// <inheritdoc/>
        public override void UpdateBones(UserTrackingFrameDto frame)
        {
            lock (SubskeletonsLock)
            {
                foreach (ISubskeleton skeleton in Subskeletons)
                {
                    if (skeleton is IWritableSubskeleton writableSkeleton)
                        writableSkeleton.UpdateBones(frame);
                }
            }

            lastFrame = frame;
        }
    }
}