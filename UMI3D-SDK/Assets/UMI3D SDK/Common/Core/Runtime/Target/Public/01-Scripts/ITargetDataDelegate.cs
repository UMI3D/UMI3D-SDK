/*
Copyright 2019 - 2025 Inetum

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

namespace umi3d.common.core.target
{
    public interface ITargetDataDelegate 
    {
        /// <summary>
        /// Return the operating system.
        /// </summary>
        /// <returns></returns>
        OperatingSystem GetOperatingSystem();

        /// <summary>
        /// Return the platform corresponding to this device.
        /// </summary>
        /// <returns></returns>
        Platform GetPlatform();

        /// <summary>
        /// Return the active plugins.
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<Plugin> GetActivePlugins();
        /// <summary>
        /// Return the active features.
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<Feature> GetActiveFeatures();

        /// <summary>
        /// Return the list of authorized immersive type for the device.
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<ImmersiveType> GetAuthorizedImmersiveTypes();
        /// <summary>
        /// Return the current immersive type.
        /// </summary>
        /// <returns></returns>
        ImmersiveType GetCurrentImmersiveType();

        /// <summary>
        /// Return the list of authorized controllers for the device.
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<Controller> GetAuthorizedControllers();
        /// <summary>
        /// Return the list of the current active controllers.
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<Controller> GetCurrentControllers();

        /// <summary>
        /// Try to set the current controllers used by the user on this device.<br/>
        /// If one of those controllers are not an authorized controller then this method log an error message and return false.
        /// </summary>
        /// <param name="controllers"></param>
        /// <returns></returns>
        bool TrySetCurrentControllers(IEnumerable<Controller> controllers);

        /// <summary>
        /// Try to set the current immersive type.<br/>
        /// If this immersive type is not authorized then this method log an error message and return false.
        /// </summary>
        /// <param name="controllers"></param>
        /// <returns></returns>
        bool TrySetCurrentImmersiveType(ImmersiveType immersiveType);
    }
}