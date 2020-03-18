/*
Copyright 2019 Gfi Informatique

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
    /// Dto for hover event request from browser.
    /// </summary>
    public class HoveredDto : UMI3DDto
    {
        /// <summary>
        /// Id of the abstract object 3D hovered.
        /// </summary>
        /// <see cref="EmptyObject3DDto"/>
        public string abstractObject3DId;

        /// <summary>
        /// Hover state.
        /// </summary>
        public bool State;

        /// <summary>
        /// Avatar bone used for hover.
        /// </summary>
        public string boneId;

        /// <summary>
        /// Hovered point position in the object's local frame.
        /// </summary>
        public SerializableVector3 Position;

        /// <summary>
        /// Normal to the object's surface at the hovered point in the object's local frame.
        /// </summary>
        public SerializableVector3 Normal;

        public HoveredDto() : base() { }
    }
}