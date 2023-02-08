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
using umi3d.cdk.userCapture;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    public class CollaborativeSkeleton : ISkeleton
    {

        public UMI3DUser User;

        public Dictionary<uint, Transform> Bones { get; set; }
        public List<ISubSkeleton> Skeletons { get; set; } = new();

        public void UpdateFrame(UserTrackingFrameDto frame)
        {
            if (Skeletons != null)
                foreach (var skeleton in Skeletons)
                    skeleton.Update(frame);
        }

    }
}