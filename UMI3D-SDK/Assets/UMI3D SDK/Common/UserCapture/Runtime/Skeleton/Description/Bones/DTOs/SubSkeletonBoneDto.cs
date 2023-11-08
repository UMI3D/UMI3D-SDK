/*
Copyright 2019 - 2021 Inetum

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

namespace umi3d.common.userCapture.description
{
    public class SubSkeletonBoneDto : UMI3DDto
    {
        /// <summary>
        /// Defines the type of the bone.
        /// </summary>
        public uint boneType { get; set; }

        /// <summary>
        /// Rotation of the bone in world space
        /// </summary>
        public Vector4Dto localRotation { get; set; }

        /// <summary>
        /// Rotation of the bone in world space
        /// </summary>
        public Vector4Dto rotation { get; set; }
    }
}