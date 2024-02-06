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

using System.Threading.Tasks;

using umi3d.common.userCapture.description;

namespace umi3d.cdk.userCapture.tracking
{
    /// <summary>
    /// Manages tracker simulation for a skeleton.
    /// </summary>
    public interface ITrackerSimulator
    {
        /// <summary>
        /// Start anchoring through a tracker simulation.
        /// </summary>
        /// <param name="poseAnchor"></param>
        /// <returns></returns>
        Task StartTrackerSimulation(PoseAnchorDto poseAnchor);

        /// <summary>
        /// Stop anchoring through a tracker simulation.
        /// </summary>
        /// <param name="poseAnchor"></param>
        /// <returns></returns
        void StopTrackerSimulation(PoseAnchorDto poseAnchor);
    }
}