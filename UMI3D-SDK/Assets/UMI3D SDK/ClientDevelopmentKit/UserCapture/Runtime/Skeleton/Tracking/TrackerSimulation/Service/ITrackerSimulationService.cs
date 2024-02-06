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

using System.Collections.Generic;

namespace umi3d.cdk.userCapture.tracking
{
    /// <summary>
    /// Manages tracker simulation for all skeletons.
    /// </summary>
    public interface ITrackerSimulationService
    {
        /// <summary>
        /// Current tracker simulmator for each skeleton.
        /// </summary>
        IReadOnlyDictionary<ISkeleton, ITrackerSimulator> TrackerSimulators { get; }

        /// <summary>
        /// Get or create a tracker simulator for a skeleton.
        /// </summary>
        /// <param name="skeleton"></param>
        /// <returns></returns>
        ITrackerSimulator GetTrackerSimulator(ISkeleton skeleton);

        /// <summary>
        /// Remove a tracker simulator for a skeleton.
        /// </summary>
        /// <param name="skeleton"></param>
        void RemoveTrackerSimulator(ISkeleton skeleton);
    }
}