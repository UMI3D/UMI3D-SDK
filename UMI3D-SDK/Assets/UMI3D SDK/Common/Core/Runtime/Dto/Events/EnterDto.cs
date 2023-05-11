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
    /// Describes the entrance point in an environment.
    /// </summary>
    [System.Serializable]
    public class EnterDto : UMI3DDto
    {
        /// <summary>
        /// User spawn position.
        /// </summary>
        public Vector3Dto userPosition { get; set; }
        /// <summary>
        /// User spawn rotation.
        /// </summary>
        public Vector4Dto userRotation { get; set; }
        /// <summary>
        /// Has the dto alredy been used?
        /// </summary>
        public bool usedDto { get; set; }
    }
}