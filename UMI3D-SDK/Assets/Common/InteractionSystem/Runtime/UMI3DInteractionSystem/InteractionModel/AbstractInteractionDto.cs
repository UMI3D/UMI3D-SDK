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

namespace umi3d.common.interaction
{
    /// <summary>
    /// Abstract Class to describe an interaction
    /// </summary>
    [System.Serializable]
    public abstract class AbstractInteractionDto : AbstractEntityDto
    {
        /// <summary>
        /// Name of the interaction
        /// </summary>
        public string name = null;

        /// <summary>
        /// Description of the interaction
        /// </summary>
        public string description = null;

        /// <summary>
        /// 2D Icon that can be used to display the interaction
        /// </summary>
        public ResourceDto icon2D;

        /// <summary>
        /// 3D Icon that can be used to display the interaction
        /// </summary>
        public ResourceDto icon3D;
    }
}