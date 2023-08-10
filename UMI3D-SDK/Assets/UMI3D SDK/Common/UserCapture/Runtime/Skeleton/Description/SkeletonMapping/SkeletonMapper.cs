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

using System.Linq;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace umi3d.common.userCapture.description
{
    /// <summary>
    /// Mapper between any skeleton hiearchy and the UMI3D one.
    /// </summary>
    public class SkeletonMapper : MonoBehaviour, ISkeletonMapper
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.Common | DebugScope.UserCapture;

        public BonePoseDto BoneAnchor;
        public SkeletonMapping[] Mappings = new SkeletonMapping[0];

        /// <summary>
        /// Get pose of the bone using mappings.
        /// </summary>
        /// <returns></returns>
        public virtual PoseDto GetPose()
        {
            if (BoneAnchor == null)
                UMI3DLogger.LogWarning("BoneAnchor is null.", DEBUG_SCOPE);

            return new PoseDto()
            {
                boneAnchor = BoneAnchor,
                bones = Mappings.Select(m => m?.GetPose()).Where(b => b != null).ToList()
            };
        }
    }
}