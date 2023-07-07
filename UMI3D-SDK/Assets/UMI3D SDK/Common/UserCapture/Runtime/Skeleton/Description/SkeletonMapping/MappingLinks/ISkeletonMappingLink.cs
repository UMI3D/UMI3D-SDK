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
    /// Relation between a set f skeleton information and a position. Links are used to map an unknown skeleton to the UMI3D standard.
    /// </summary>
    public interface ISkeletonMappingLink
    {
        /// <summary>
        /// Compute the link to retrieve a position and rotation.
        /// </summary>
        /// <returns></returns>
        public (Vector3 position, Quaternion rotation) Compute();
    }
}