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
    /// DTO to transfer anchors. Anchors are the link between an object and a position in the real world used in AR.
    /// </summary>
    [System.Serializable]
    public class UMI3DAnchorDto : UMI3DDto
    {
        /// <summary>
        /// Anchor's offset relatively to the parent position.
        /// </summary>
        public SerializableVector3 positionOffset = null;

        /// <summary>
        /// Anchor's offset relatively to the parent rotation.
        /// </summary>
        public SerializableVector4 rotationOffset = null;

        /// <summary>
        /// Anchor's offset relatively to the parent scale.
        /// </summary>
        public SerializableVector3 scaleOffset = null;

        public UMI3DAnchorDto() : base() { }
    }
}