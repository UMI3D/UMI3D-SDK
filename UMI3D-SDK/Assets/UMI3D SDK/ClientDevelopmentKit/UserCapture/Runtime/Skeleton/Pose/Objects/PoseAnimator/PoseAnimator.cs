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
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Links a collection of condition to a specific pose.
    /// </summary>
    public class PoseAnimator : IPoseAnimator
    {
        private PoseAnimatorDto dto;

        /// <summary>
        /// If true, the animator is applying its pose override.
        /// </summary>
        public bool IsApplied { get; set; }

        /// <summary>
        /// UMI3D id of the pose animator.
        /// </summary>
        public ulong Id => dto.id;

        /// <summary>
        /// Pose clip associated to this animator;
        /// </summary>
        public PoseClip PoseClip => poseClip;
        private PoseClip poseClip;

        public ulong RelativeNodeId => dto.relatedNodeId;

        /// <summary>
        /// The different condition that are needed for the overrider to get activated
        /// </summary>
        public IPoseCondition[] PoseConditions { get; private set; }

        /// <summary>
        /// How long the pose should last [Not Implemented]
        /// </summary>
        public DurationDto Duration => dto.duration;

        /// <summary>
        /// If the pose can be interpolated
        /// </summary>
        public bool IsInterpolable => dto.isInterpolable;

        /// <summary>
        /// If the pose can be added to  other poses
        /// </summary>
        public bool IsComposable => dto.isComposable;

        /// <summary>
        /// How the pose is activated.
        /// </summary>
        public ushort ActivationMode => dto.activationMode;

        /// <summary>
        /// Sends a signal when the condition become validated
        /// </summary>
        public event Action ConditionsValidated;

        /// <summary>
        /// Sends a signal when the condition become invalid
        /// </summary>
        public event Action ConditionsInvalided;

        /// <summary>
        /// If true, the animator tries to auto-check if conditions are met..
        /// </summary>
        public bool IsWatching => isWatchingActivation;

        private bool isWatchingActivation;

        /// <summary>
        /// Coroutine that keep track of pose animator end.
        /// </summary>
        private Coroutine watchEndOfConditionsCoroutine;

        #region Dependency Injection

        private readonly ICoroutineService coroutineService;
        private readonly IPoseManager poseService;

        public PoseAnimator(PoseAnimatorDto dto, PoseClip poseClip, IPoseCondition[] poseConditions) : this(dto,
                                                                                         poseClip,
                                                                                         poseConditions,
                                                                                         poseService: PoseManager.Instance,
                                                                                         coroutineService: CoroutineManager.Instance)
        {
        }

        public PoseAnimator(PoseAnimatorDto poseAnimatorDto, PoseClip poseClip, IPoseCondition[] poseConditions, IPoseManager poseService, ICoroutineService coroutineService)
        {
            this.dto = poseAnimatorDto ?? throw new System.ArgumentNullException(nameof(poseAnimatorDto));
            this.PoseConditions = poseConditions ?? new IPoseCondition[0];
            this.poseClip = poseClip;

            this.coroutineService = coroutineService;
            this.poseService = poseService;
        }

        #endregion Dependency Injection

        /// <summary>
        /// Check if pose conditions are met.
        /// </summary>
        /// <returns></returns>
        public bool CheckConditions()
        {
            for (int i = 0; i < PoseConditions.Length; i++)
            {
                if (!PoseConditions[i].Check())
                    return false;
            }
            return true;
        }

        public void Clear()
        {
            StopWatchActivationConditions();
        }

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
        public virtual bool TryActivate()
        {
            if (IsApplied)
                return false;

            if (ActivationMode == (ushort)PoseAnimatorActivationMode.ON_REQUEST && CheckConditions())
            {
                Apply();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Apply the pose on the pose subskeleton.
        /// </summary>
        private void Apply()
        {
            IsApplied = true;
            poseService.PlayPoseClip(poseClip);
            ConditionsValidated?.Invoke();
            StartWatchEndOfConditions();
        }

        /// <summary>
        /// Stop the application of the pose on the pose subskeleton.
        /// </summary>
        private void EndApply()
        {
            IsApplied = false;
            poseService.StopPoseClip(poseClip);
            ConditionsInvalided?.Invoke();
            StopWatchEndOfConditions();
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
                if (!IsApplied && CheckConditions())
                    Apply();
            }
            StopWatchActivationConditions();
        }

        /// <summary>
        /// Start to watch for a pose overrider end.
        /// </summary>
        /// <param name="poseOverrider"></param>
        private void StartWatchEndOfConditions()
        {
            if (!IsApplied)
                return;

            watchEndOfConditionsCoroutine = coroutineService.AttachCoroutine(WatchConditionsRoutine());
        }

        /// <summary>
        /// Stop to watch for a pose overrider end.
        /// </summary>
        /// <param name="poseOverrider"></param>
        private void StopWatchEndOfConditions()
        {
            if (watchEndOfConditionsCoroutine == null)
                return;

            coroutineService.DetachCoroutine(watchEndOfConditionsCoroutine);
            watchEndOfConditionsCoroutine = null;
        }

        /// <summary>
        /// Watch over a pose overrider ends.
        /// </summary>
        /// <param name="poseOverrider"></param>
        /// <returns></returns>
        private IEnumerator WatchConditionsRoutine()
        {
            float startTime = Time.time;
            while (IsApplied && !IsFinished(startTime))
            {
                yield return new WaitForSeconds(seconds: CHECK_PERIOD);
            }
            EndApply();
        }

        /// <summary>
        /// Check if a pose overriders should be ended.
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        private bool IsFinished(float startTime)
        {
            float currentDuration = Time.time - startTime;

            var duration = Duration;

            // pose can be applied for a minimum duration
            if (duration.min.HasValue && currentDuration < duration.min)
                return false;

            // finish if pose conditions are not valid anymore
            if (!CheckConditions())
                return true;

            // pose can be stopped a max duration
            if (duration.max.HasValue && currentDuration > duration.max)
                return true;

            // if no max and condition validated and duration is set, duration is max
            if (!duration.max.HasValue && duration.duration != default && currentDuration > duration.duration)
                return true;

            return false;
        }
    }
}