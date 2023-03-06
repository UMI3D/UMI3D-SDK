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
    /// DTO describing an nimation using an animator to animate objects.
    /// </summary>
    [System.Serializable]
    public class UMI3DAnimatorAnimationDto : UMI3DAbstractAnimationDto
    {
        /// <summary>
        /// Node where the animator can be found on.
        /// </summary>
        public ulong nodeId = 0;

        /// <summary>
        /// Animation state's name in the animator controller.
        /// </summary>
        public string stateName = "";

        /// <summary>
        /// Animator parameters.
        /// </summary>
        public Dictionary<string, object> parameters = new Dictionary<string, object>();
    }
}
