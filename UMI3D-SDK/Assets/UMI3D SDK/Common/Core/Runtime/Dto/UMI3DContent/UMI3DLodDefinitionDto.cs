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
    /// DTO for Level of Detail component, switching between nodes to display on constraints.
    /// </summary>
    [System.Serializable]
    public class UMI3DLodDefinitionDto : UMI3DDto
    {
        /// <summary>
        /// Nodes to display or not according to the current level of detail.
        /// </summary>
        public List<ulong> nodes;


        public float screenSize;

        /// <summary>
        /// Value between 0 and 1 that define the transition zone size.
        /// </summary>
        public float fadeTransition;
    }
}