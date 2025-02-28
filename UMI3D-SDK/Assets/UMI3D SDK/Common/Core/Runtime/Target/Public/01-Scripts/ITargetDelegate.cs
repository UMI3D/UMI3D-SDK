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
    public interface ITargetDelegate 
    {
        /// <summary>
        /// This method is called when the operating system corresponding to the user's device changed.<br/>
        /// This should only happen at compile time.
        /// </summary>
        /// <param name="operatingSystem"></param>
        void OperatingSystemHasChanged(OperatingSystem operatingSystem) { }

        /// <summary>
        /// This method is called when the platform corresponding to the user's device changed.<br/>
        /// This should only happen at compile time.
        /// </summary>
        /// <param name="platform"></param>
        void PlatformHasChanged(Platform platform) { }

        /// <summary>
        /// This method is called when the active plugins changed.<br/>
        /// This should only happen at compile time.
        /// </summary>
        /// <param name="plugins"></param>
        void ActivePluginsHaveChanged(IReadOnlyList<Plugin> plugins) { }

        /// <summary>
        /// This method is called when the active features changed.<br/>
        /// This should only happen at compile time.
        /// </summary>
        /// <param name="features"></param>
        void ActiveFeaturesHaveChanged(IReadOnlyList<Feature> features) { }

        /// <summary>
        /// This method is called when the authorized immersive types changed.<br/>
        /// This should only happen at compile time.
        /// </summary>
        /// <param name="immersiveTypes"></param>
        void AuthorizedImmersiveTypesHaveChanged(IReadOnlyList<ImmersiveType> immersiveTypes) { }

        /// <summary>
        /// This method is called when the active immersive type changed.
        /// </summary>
        /// <param name="immersiveType"></param>
        void CurrentImmersiveTypeHasChanged(ImmersiveType immersiveType) { }

        /// <summary>
        /// This method is called when the authorized controllers changed.<br/>
        /// This should only happen at compile time.
        /// </summary>
        /// <param name="controllers"></param>
        void AuthorizedControllersHaveChanged(IReadOnlyList<Controller> controllers) { }

        /// <summary>
        /// This method is called when the active controllers changed.
        /// </summary>
        /// <param name="controllers"></param>
        void CurrentControllersHaveChanged(IReadOnlyList<Controller> controllers) { }
    }
}