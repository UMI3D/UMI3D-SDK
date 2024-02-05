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


using UnityEngine;

namespace umi3d.common.userCapture.description
{
    /// <summary>
    /// Mapping between a UMI3D and other objects, based on links.
    /// </summary>
    [System.Serializable]
    public class SkeletonMapping
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.Common | DebugScope.UserCapture;

        /// <summary>
        /// Mapped UMI3D bone.
        /// </summary>
        public uint BoneType;

        /// <summary>
        /// Rule to compute the mapping between the bone and other objects.
        /// </summary>
        public ISkeletonMappingLink Link;

        public SkeletonMapping(uint boneType, ISkeletonMappingLink link)
        {
            this.BoneType = boneType;
            this.Link = link;
        }

        /// <summary>
        /// Get pose of the bone after computing links.
        /// </summary>
        /// <returns></returns>
        public virtual BoneDto GetPose()
        {
            if (Link == null)
                UMI3DLogger.LogWarning("Skeleton Mapping Link is null.", DEBUG_SCOPE);

           // Debug.Log((Link as GameNodeLink).transform != null);
            if (((Link as GameNodeLink)?.transform) == null)
            {
                return new BoneDto()
                {
                    boneType = BoneType,
                    rotation = Quaternion.identity.Dto()
                };
            }
            var computed = Link.Compute();
            Vector4Dto rotation = new Vector4Dto()
            {
                X = computed.rotation.x,
                Y = computed.rotation.y,
                Z = computed.rotation.z,
                W = computed.rotation.w
            };

            return new BoneDto()
            {
                boneType = BoneType,
                rotation = rotation
            };
        }
    }
}