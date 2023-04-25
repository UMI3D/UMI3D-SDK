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
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Abstract class for user navigation methods in scene.
    /// </summary>
    public abstract class AbstractNavigation : MonoBehaviour
    {
        /// <summary>
        /// Disable this navigation system.
        /// </summary>
        public abstract void Disable();

        /// <summary>
        /// Activate this navigation system.
        /// </summary>
        public abstract void Activate();

        /// <summary>
        /// Apply navigation request from server.
        /// </summary>
        /// <param name="data"></param>
        /// <seealso cref="Teleport(TeleportDto)"/>
        public abstract void Navigate(NavigateDto data);

        /// <summary>
        /// Apply teleport request from server.
        /// </summary>
        /// <param name="data"></param>
        /// <seealso cref="Navigate(NavigateDto)"/>
        public abstract void Teleport(TeleportDto data);

        /// <summary>
        /// Apply FrameRequestDto request from server.
        /// </summary>
        /// <param name="data"></param>
        public abstract void UpdateFrame(FrameRequestDto data);
    }
}