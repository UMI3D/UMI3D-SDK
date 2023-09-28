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
using umi3d.cdk.userCapture.tracking;
using umi3d.common.userCapture.tracking;
using UnityEngine;

namespace umi3d.cdk.userCapture
{
    public interface IPersonalSkeleton : ISkeleton
    {
        IDictionary<uint, float> BonesAsyncFPS { get; }

        Transform Transform { get; }

        /// <summary>
        /// Size of the skeleton.
        /// </summary>
        Vector3 worldSize { get; }

        /// <summary>
        /// Skeleton Initialization.
        /// </summary>
        public void SelfInit();

        /// <summary>
        /// Write a tracking frame from all <see cref="IWritableSubskeleton"/>.
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        UserTrackingFrameDto GetFrame(TrackingOption option);
    }
}