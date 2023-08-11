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

namespace umi3d.common
{
    /// <summary>
    /// DTO for all nodes of the UMI3D environment tree.
    /// </summary>
    [System.Serializable]
    public class UMI3DAbstractNodeDto : AbstractEntityDto
    {
        /// <summary>
        /// Should the node run it active scripts and components, and be displayed in the scene?
        /// </summary>
        public bool active { get; set; } = true;

        /// <summary>
        /// Should the node exist only on immersive devices?
        /// </summary>
        public bool immersiveOnly { get; set; } = false;

        /// <summary>
        /// Anchor related to that node for Augmented reality.
        /// </summary>
        public UMI3DAnchorDto anchorDto { get; set; } = null;

        /// <summary>
        /// UMI3D id of the parent node.
        /// </summary>
        public ulong pid { get; set; } = 0;

        /// <summary>
        /// Should the node be immuned to modifications?
        /// </summary>
        public bool isStatic { get; set; } = false;
    }
}