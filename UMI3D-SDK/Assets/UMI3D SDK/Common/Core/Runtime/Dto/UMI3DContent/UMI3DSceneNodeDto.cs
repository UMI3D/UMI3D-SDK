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
    /// DTO describing a scene as a node.
    /// </summary>
    [System.Serializable]
    public class UMI3DSceneNodeDto : UMI3DAbstractNodeDto
    {
        /// <summary>
        /// Scene reference point's position.
        /// </summary>
        public SerializableVector3 position;

        /// <summary>
        /// Scene reference rotation.
        /// </summary>
        public SerializableVector4 rotation;

        /// <summary>
        /// Scene reference scale.
        /// </summary>
        public SerializableVector3 scale;

        /// <summary>
        /// Animations packeaged with the scene.
        /// </summary>
        public List<UMI3DAbstractAnimationDto> animations = new List<UMI3DAbstractAnimationDto>();

        /// <summary>
        /// All UMI3D entities that are not on the scene graph.
        /// </summary>
        public List<IEntity> otherEntities;

        /// <summary>
        /// Libraries required in the scene by their named ID.
        /// </summary>
        public List<string> LibrariesId;
    }
}
