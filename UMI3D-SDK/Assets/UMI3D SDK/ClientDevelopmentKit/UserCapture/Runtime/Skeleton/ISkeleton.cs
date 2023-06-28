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

using umi3d.common.userCapture;
using umi3d.common.userCapture.tracking;
using UnityEngine;

namespace umi3d.cdk.userCapture
{
    public interface ISkeleton
    {
        Dictionary<uint, s_Transform> Bones { get; }

        List<ISubSkeleton> Skeletons { get; }

        UMI3DSkeletonHierarchy SkeletonHierarchy { get; }

        Transform HipsAnchor { get; }

        /// <summary>
        /// User's registered id
        /// </summary>
        public ulong UserId { get; }

        #region Data struture

        public class s_Transform
        {
            public Vector3 s_Position;
            public Quaternion s_Rotation;
        }

        public struct SavedTransform
        {
            public Transform obj;
            public Vector3 savedPosition;
            public Quaternion savedRotation;
            public Vector3 savedLocalScale;
            public Vector3 savedLossyScale;
        }

        #endregion Data struture

        public void UpdateFrame(UserTrackingFrameDto frame);

        public ISkeleton Compute();
    }
}