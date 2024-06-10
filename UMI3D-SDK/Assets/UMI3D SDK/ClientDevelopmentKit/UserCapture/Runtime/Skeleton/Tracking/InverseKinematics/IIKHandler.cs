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

namespace umi3d.cdk.userCapture.tracking.ik
{
    /// <summary>
    /// Handle Inverse Kinematics.
    /// </summary>
    public interface IIKHandler
    {
        /// <summary>
        /// Apply Inverse Kinematics logic from the controller data.
        /// </summary>
        /// <param name="layerIndex"></param>
        /// <param name="controllers"></param>
        void HandleAnimatorIK(int layerIndex, IController controller);

        /// <summary>
        /// Apply Inverse Kinematics logic from the controller data.
        /// </summary>
        /// <param name="layerIndex"></param>
        /// <param name="controllers"></param>
        void HandleAnimatorIK(int layerIndex, IEnumerable<IController> controllers);

        /// <summary>
        /// Reset all IK effects.
        /// </summary>
        /// <param name="controllers"></param>
        /// <param name="bones"></param>
        void Reset(IEnumerable<IController> controllers, IDictionary<uint, TrackedSubskeletonBone> bones);
    }
}