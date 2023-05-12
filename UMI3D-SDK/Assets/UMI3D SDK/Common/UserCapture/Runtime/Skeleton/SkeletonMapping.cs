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

namespace umi3d.common.userCapture
{
    [System.Serializable]
    public class SkeletonMapping
    {
        [inetum.unityUtils.ConstEnum(typeof(BoneType), typeof(uint))]
        public uint BoneType;
        public SkeletonMappingLink Link;

        public SkeletonMapping(uint boneType, SkeletonMappingLink link)
        {
            this.BoneType = boneType;
            this.Link = link;
        }

        public BoneDto GetPose()
        {
            var computed = Link.Compute();
            Quaternion rotation = new Quaternion(computed.rotation.x,
                                                computed.rotation.y,
                                                computed.rotation.z,
                                                computed.rotation.w
            );

            return new BoneDto() {
                boneType = boneType,
                rotation = rotation.Dto()
        }
    }
}