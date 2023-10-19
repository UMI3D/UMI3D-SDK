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

using inetum.unityUtils;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using umi3d.common;
using umi3d.common.userCapture.pose;

using UnityEngine;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Pose overrider handler to calculate whether the pose can be played or not depending on the set of condition of a specific container
    /// </summary>
    public class PoseOverridersContainerProcessor : IPoseOverridersContainerProcessor
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.UserCapture;

        /// <summary>
        /// Ref to the related pose container
        /// </summary>
        private PoseOverridersContainer poseOverriderContainer;

        /// <summary>
        /// Sends a signal when the condition become validated
        /// </summary>
        public event Action<PoseOverrider> ConditionsValidated;

        /// <summary>
        /// Sends a signal when the condition become invalid
        /// </summary>
        public event Action<PoseOverrider> ConditionsInvalided;

        /// <summary>
        /// If true, the processor tries to auto-check environmental pose overriders.
        /// </summary>
        public bool IsWatching => isWatchingActivation;

        private bool isWatchingActivation;

        /// <summary>
        /// All the overriders  which ca only be considered if they an interaction occurs
        /// </summary>
        private readonly List<PoseOverrider> interactionalPoseOverriders = new();

        /// <summary>
        /// All the overriders that are considered without interaction (more performance expensive)
        /// </summary>
        private readonly List<PoseOverrider> nonInteractionalPoseOverriders = new();

        /// <summary>
        /// Coroutines that keep track of pose overriders end.
        /// </summary>
        private readonly Dictionary<PoseOverrider, Coroutine> watchEndOfConditionsCoroutines = new();

        #region Dependency Injection

        private readonly ICoroutineService coroutineService;

        public PoseOverridersContainerProcessor(PoseOverridersContainer overriderContainer, ICoroutineService coroutineService)
        {
            this.poseOverriderContainer = overriderContainer ?? throw new System.ArgumentNullException();
            interactionalPoseOverriders = poseOverriderContainer.PoseOverriders
                                                .Where(pod => pod.ActivationMode != (ushort)PoseActivationMode.NONE)
                                                .ToList();

            this.coroutineService = coroutineService ?? CoroutineManager.Instance;

            nonInteractionalPoseOverriders.AddRange(poseOverriderContainer.PoseOverriders.Except(interactionalPoseOverriders));

            if (nonInteractionalPoseOverriders.Count > 0)
                StartWatchActivationConditions();
        }

        public PoseOverridersContainerProcessor(PoseOverridersContainer overriderContainer)
            : this(overriderContainer, null)
        {
        }

        #endregion Dependency Injection

        /// <summary>
        /// Start to check all overriders
        /// </summary>
        public void StartWatchActivationConditions()
        {
            if (IsWatching)
                return;

            isWatchingActivation = true;
            regularActivationCheckRoutine = coroutineService.AttachCoroutine(RegularActivationCheckRoutine());
        }

        /// <summary>
        /// Stops the check of fall the overriders
        /// </summary>
        public void StopWatchActivationConditions()
        {
            if (!IsWatching)
                return;

            coroutineService.DetachCoroutine(regularActivationCheckRoutine);
            regularActivationCheckRoutine = null;
            isWatchingActivation = false;
        }

        /// <summary>
        /// Active poses that listens to this activation mode.
        /// </summary>
        public bool TryActivate(PoseActivationMode mode)
        {
            foreach (PoseOverrider poseOverrider in interactionalPoseOverriders)
            {
                if (poseOverrider.IsActive)
                    continue;

                if (poseOverrider.ActivationMode == (ushort)mode && poseOverrider.CheckConditions())
                {
                    poseOverrider.IsActive = true;
                    ConditionsValidated?.Invoke(poseOverrider);
                    StartWatchEndOfConditions(poseOverrider);
                    return true;
                }
            }
            return false;
        }

        private Coroutine regularActivationCheckRoutine;

        /// <summary>
        /// Period of time waited between two auto-check of conditions, in seconds.
        /// </summary>
        private const float CHECK_PERIOD = 0.1f;

        /// <summary>
        /// Auto-check regularly if conditions for environmental pose overriders are met and activate pose overriders if so.
        /// </summary>
        /// <returns></returns>
        private IEnumerator RegularActivationCheckRoutine()
        {
            while (IsWatching)
            {
                yield return new WaitForSeconds(seconds: CHECK_PERIOD);

                // check to enable/disable auto-watched poses (nonInteractional)
                for (int i = 0; i < nonInteractionalPoseOverriders.Count; i++)
                {
                    var poseOverrider = nonInteractionalPoseOverriders[i];
                    if (!poseOverrider.IsActive && poseOverrider.CheckConditions())
                    {
                        poseOverrider.IsActive = true;
                        ConditionsValidated?.Invoke(poseOverrider);
                        StartWatchEndOfConditions(poseOverrider);
                    }
                }
            }
            StopWatchActivationConditions();
        }

        /// <summary>
        /// Start to watch for a pose overrider end.
        /// </summary>
        /// <param name="poseOverrider"></param>
        private void StartWatchEndOfConditions(PoseOverrider poseOverrider)
        {
            if (poseOverrider == null || !poseOverrider.IsActive)
                return;

            var coroutine = coroutineService.AttachCoroutine(WatchConditionsRoutine(poseOverrider));
            watchEndOfConditionsCoroutines.Add(poseOverrider, coroutine);
        }

        /// <summary>
        /// Stop to watch for a pose overrider end.
        /// </summary>
        /// <param name="poseOverrider"></param>
        private void StopWatchEndOfConditions(PoseOverrider poseOverrider)
        {
            watchEndOfConditionsCoroutines.TryGetValue(poseOverrider, out Coroutine coroutine);
            if (coroutine != null)
            {
                watchEndOfConditionsCoroutines.Remove(poseOverrider);
                coroutineService.DetachCoroutine(coroutine);
            }
        }

        /// <summary>
        /// Watch over a pose overrider ends.
        /// </summary>
        /// <param name="poseOverrider"></param>
        /// <returns></returns>
        private IEnumerator WatchConditionsRoutine(PoseOverrider poseOverrider)
        {
            float startTime = Time.time;
            while (poseOverrider.IsActive && !IsProcessorFinished(poseOverrider, startTime))
            {
                yield return new WaitForSeconds(seconds: CHECK_PERIOD);
            }
            poseOverrider.IsActive = false;
            ConditionsInvalided?.Invoke(poseOverrider);

            StopWatchEndOfConditions(poseOverrider);
        }

        /// <summary>
        /// Check if a pose overriders should be ended.
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        private bool IsProcessorFinished(PoseOverrider poseOverrider, float startTime)
        {
            float currentDuration = Time.time - startTime;

            var duration = poseOverrider.Duration;
            if (duration.min.HasValue && currentDuration < duration.min)
                return false;

            if (poseOverrider.CheckConditions())
            {
                if (duration.max.HasValue && currentDuration > duration.max)
                    return true;

                if (!duration.max.HasValue && duration.duration == 0) // infinite mode
                    return false;

                if (!duration.max.HasValue && currentDuration > duration.duration)
                    return true;

                return false;
            }

            return true;
        }
    }
}