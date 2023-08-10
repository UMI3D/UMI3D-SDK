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
using umi3d.common.userCapture.tracking;
using UnityEngine;

namespace umi3d.cdk.userCapture.tracking
{
    public interface ITrackedSubskeleton : IWritableSubskeleton
    {
        IDictionary<uint, float> BonesAsyncFPS { get; set; }
        Camera ViewPoint { get; }
        Transform Hips { get; }
        IReadOnlyDictionary<uint, TrackedSubskeletonBone> TrackedBones { get; }

        UserTrackingBoneDto GetController(uint boneType);
    }
}