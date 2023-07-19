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
    public class PoseConditionProcessor
    {
        /// <summary>
        /// Ref to the related pose container
        /// </summary>
        private PoseOverriderContainer poseOverriderContainer;

        /// <summary>
        /// Sends a signal when the condition become validated
        /// </summary>
        public event Action<PoseOverrider> ConditionValidated;

        /// <summary>
        /// Sends a signal when the condition become invalid
        /// </summary>
        public event Action<PoseOverrider> ConditionInvalided;

        /// <summary>
        /// All the overriders  which ca only be considered if they an interaction occurs
        /// </summary>
        private readonly List<PoseOverrider> interactionalPoseOverriders = new();

        /// <summary>
        /// All the overriders that are considered without interaction (more performance expensive)
        /// </summary>
        private readonly List<PoseOverrider> nonInteractionalPoseOverriders = new();

        private Dictionary<PoseOverrider, Coroutine> watchConditionsCoroutines = new();

        public PoseConditionProcessor(PoseOverriderContainer overriderContainer)
        {
            this.poseOverriderContainer = overriderContainer ?? throw new System.ArgumentNullException();
            interactionalPoseOverriders = poseOverriderContainer.PoseOverriders
                                                .Where(pod => pod.ActivationMode != (ushort)PoseActivationMode.NONE)
                                                .ToList();
            nonInteractionalPoseOverriders.AddRange(poseOverriderContainer.PoseOverriders.Except(interactionalPoseOverriders));

            if (nonInteractionalPoseOverriders.Count > 0)
                StartWatchNonInteractionalConditions();
        }

        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.UserCapture;

        /// <summary>
        /// If th e condition processor is enable
        /// </summary>
        private bool isProcessing;

        /// <summary>
        /// Start to check all overriders
        /// </summary>
        public void StartWatchNonInteractionalConditions()
        {
            if (!isProcessing)
            {
                isProcessing = true;
                regularActivationCheckRoutine = CoroutineManager.Instance.AttachCoroutine(RegularActivationCheckRoutine());
            }
        }

        /// <summary>
        /// Stops the check of fall the overriders
        /// </summary>
        public void StopWatchNonInteractionalConditions()
        {
            CoroutineManager.Instance.DettachCoroutine(regularActivationCheckRoutine);
            regularActivationCheckRoutine = null;
            isProcessing = false;
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
                    ConditionValidated?.Invoke(poseOverrider);
                    StartWatchConditions(poseOverrider);
                    return true;
                }
            }
            return false;
        }

        private Coroutine regularActivationCheckRoutine;

        private const float CHECK_PERIOD = 0.1f;

        private IEnumerator RegularActivationCheckRoutine()
        {
            while (isProcessing)
            {
                yield return new WaitForSeconds(seconds: CHECK_PERIOD);

                // check to enable/disable auto-watched poses (nonInteractional)
                for (int i = 0; i < nonInteractionalPoseOverriders.Count; i++)
                {
                    var poseOverrider = nonInteractionalPoseOverriders[i];
                    if (!poseOverrider.IsActive && poseOverrider.CheckConditions())
                    {
                        poseOverrider.IsActive = true;
                        ConditionValidated?.Invoke(poseOverrider);
                        StartWatchConditions(poseOverrider);
                    }
                }
            }
            if (regularActivationCheckRoutine != null)
            {
                CoroutineManager.Instance.DettachCoroutine(regularActivationCheckRoutine);
                regularActivationCheckRoutine = null;
            }
            isProcessing = false;
        }

        private void StartWatchConditions(PoseOverrider poseOverrider)
        {
            if (poseOverrider == null || !poseOverrider.IsActive)
                return;

            var coroutine = CoroutineManager.Instance.AttachCoroutine(WatchConditionsRoutine(poseOverrider));
            watchConditionsCoroutines.Add(poseOverrider, coroutine);
        }

        private void StopWatchConditions(PoseOverrider poseOverrider)
        {
            watchConditionsCoroutines.TryGetValue(poseOverrider, out Coroutine coroutine);
            if (coroutine != null)
            {
                watchConditionsCoroutines.Remove(poseOverrider);
                CoroutineManager.Instance.DettachCoroutine(coroutine);
            }
        }

        private IEnumerator WatchConditionsRoutine(PoseOverrider poseOverrider)
        {
            float startTime = Time.time;
            while (poseOverrider.IsActive && !poseOverrider.IsFinished(startTime))
            {
                yield return new WaitForSeconds(seconds: CHECK_PERIOD);
            }
            poseOverrider.IsActive = false;
            ConditionInvalided?.Invoke(poseOverrider);

            StopWatchConditions(poseOverrider);
        }
    }
}