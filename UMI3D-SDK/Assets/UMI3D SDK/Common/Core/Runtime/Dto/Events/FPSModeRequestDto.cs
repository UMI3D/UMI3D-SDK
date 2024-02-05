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

namespace umi3d.common
{
    /// <summary>
    /// Navigation mode for fps navigation
    /// </summary>
    public class FPSModeRequestDto : FlyingModeRequestDto
    {
        /// <summary>
        /// maximum jumpd heigth
        /// </summary>
        public float jumpHeigth { get; set; }

        /// <summary>
        /// Maximum distance to which a user can teleport it self with a navigation fishing road
        /// </summary>
        public float fishingRoadMaxDistance { get; set; }
    }
}