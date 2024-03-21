/*
Copyright 2019 - 2024 Inetum

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

namespace umi3d.cdk.navigation
{
    /// <summary>
    /// Navigation Delegate Interface.
    /// </summary>
    public interface INavigationDelegate
    {
        /// <summary>
        /// Disable this navigation system.
        /// </summary>
        void Disable();

        /// <summary>
        /// Activate this navigation system.
        /// </summary>
        void Activate();

        /// <summary>
        /// Apply navigation request from server.
        /// </summary>
        /// <param name="data"></param>
        /// <seealso cref="Teleport(TeleportDto)"/>
        void Navigate(ulong environmentId, NavigateDto data);

        /// <summary>
        /// Apply teleport request from server.
        /// </summary>
        /// <param name="data"></param>
        /// <seealso cref="Navigate(NavigateDto)"/>
        void Teleport(ulong environmentId, TeleportDto data);

        /// <summary>
        /// Apply FrameRequestDto request from server.
        /// </summary>
        /// <param name="data"></param>
        void UpdateFrame(ulong environmentId, FrameRequestDto data);

        /// <summary>
        /// Get data on current movements of the user.
        /// </summary>
        /// <returns></returns>
        NavigationData GetNavigationData();
    }
}