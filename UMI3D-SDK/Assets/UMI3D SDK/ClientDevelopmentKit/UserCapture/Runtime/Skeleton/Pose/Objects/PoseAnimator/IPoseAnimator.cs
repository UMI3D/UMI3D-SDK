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

using System;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Pose overrider handler to calculate whether the pose can be played or not depending on the set of condition of a specific container
    /// </summary>
    public interface IPoseAnimator
    {
        ulong Id { get; }

        /// <summary>
        /// If true, the processor tries to auto-check environmental pose overriders.
        /// </summary>
        bool IsWatching { get; }

        /// <summary>
        /// Pose clip associated to this animator;
        /// </summary>
        PoseClip PoseClip { get; }

        /// <summary>
        /// Sends a signal when the condition become validated
        /// </summary>
        event Action ConditionsInvalided;

        /// <summary>
        /// Sends a signal when the condition become invalid
        /// </summary>
        event Action ConditionsValidated;

        /// <summary>
        /// Start to check all overriders
        /// </summary>
        void StartWatchActivationConditions();

        /// <summary>
        /// Stops the check of fall the overriders
        /// </summary>
        void StopWatchActivationConditions();

        /// <summary>
        /// Active poses that listens to this activation mode.
        /// </summary>
        bool TryActivate();
    }
}