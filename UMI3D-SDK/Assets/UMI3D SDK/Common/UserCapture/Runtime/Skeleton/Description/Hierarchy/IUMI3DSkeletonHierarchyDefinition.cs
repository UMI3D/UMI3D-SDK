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

using System.Collections.Generic;

namespace umi3d.common.userCapture.description
{
    /// <summary>
    /// Define a UMI3D skeleton hierarchy of bones.
    /// </summary>
    public interface IUMI3DSkeletonHierarchyDefinition
    {
        /// <summary>
        /// Collection of relation between a bone and its parent.
        /// </summary>
        IList<BoneRelation> Relations { get; }

        public struct BoneRelation
        {
            /// <summary>
            /// Bone type in UMI3D standards.
            /// </summary>
            public uint boneType;

            /// <summary>
            /// Parent bone in the hierarchy.
            /// </summary>
            public uint parentBoneType;

            /// <summary>
            /// The position of the current bone type relative to its parent.
            /// </summary>
            public Vector3Dto relativePosition;
        }
    }
}