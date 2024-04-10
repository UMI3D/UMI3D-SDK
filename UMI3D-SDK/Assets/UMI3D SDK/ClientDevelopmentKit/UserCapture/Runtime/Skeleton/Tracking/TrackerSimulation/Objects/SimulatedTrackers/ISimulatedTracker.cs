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

using UnityEngine;

namespace umi3d.cdk.userCapture.tracking
{
    /// <summary>
    /// Tracker that does not physically exist.
    /// </summary>
    public interface ISimulatedTracker
    {
        /// <summary>
        /// Bone type associated with the tracker.
        /// </summary>
        uint BoneType { get; }

        /// <summary>
        /// Controller corresponding to the tracker.
        /// </summary>
        IController Controller { get; }

        /// <summary>
        /// Gameobject reference of the tracker in the scene.
        /// </summary>
        GameObject GameObject { get; }
    }
}