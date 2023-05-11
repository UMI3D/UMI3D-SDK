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

namespace umi3d.common
{
    /// <summary>
    /// DTO to describe a node in the glTF scene graph.
    /// </summary>
    [System.Serializable]
    public class GlTFNodeDto : UMI3DDto, IEntity
    {
        /// <summary>
        /// Description labelling the node.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Position of the node on the 3 axes.
        /// </summary>
        /// The position is given in a left-hand coordinate system, just like Unity does.
        public Vector3Dto position { get; set; }

        /// <summary>
        /// Rotation of the node as a quaternion.
        /// </summary>
        public Vector4Dto rotation { get; set; }

        /// <summary>
        /// Scale of the node.
        /// </summary>
        /// The scale is given in a left-hand coordinate system, just like Unity does.
        public Vector3Dto scale { get; set; }

        /// <summary>
        /// List of the node's children UMI3D id.
        /// </summary>
        public List<int> children { get; set; } = null;

        /// <summary>
        /// glTF extensions available for that node.
        /// </summary>
        public GlTFNodeExtensions extensions { get; set; } = new GlTFNodeExtensions();
    }
}
