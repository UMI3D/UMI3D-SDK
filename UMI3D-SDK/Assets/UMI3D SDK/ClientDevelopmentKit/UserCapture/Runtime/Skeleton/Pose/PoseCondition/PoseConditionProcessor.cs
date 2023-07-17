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
        /// Sends a signal when the condition is validated
        /// </summary>
        public event Action<PoseOverrider> ConditionValidated;

        /// <summary>
        /// Sends a signal when the condition is deactivated
        /// </summary>
        public event Action<PoseOverrider> ConditionDeactivated;

        /// <summary>
        /// All the overriders  which ca only be considered if they an interaction occurs
        /// </summary>
        private readonly List<PoseOverrider> interactionalPoseOverriders = new();
        /// <summary>
        /// All the overriders that are considered without interaction (more performance expensive)
        /// </summary>
        private readonly List<PoseOverrider> nonInteractionalPoseOverriders = new();

        public PoseConditionProcessor(PoseOverriderContainer overriderContainer)
        {
            this.poseOverriderContainer = overriderContainer ?? throw new System.ArgumentNullException();
            interactionalPoseOverriders = poseOverriderContainer.PoseOverriders
                                                .Where(pod => pod.ActivationMode != (ushort)PoseActivationMode.NONE)
                                                .ToList();
            nonInteractionalPoseOverriders.AddRange(poseOverriderContainer.PoseOverriders.Except(interactionalPoseOverriders));

            if (nonInteractionalPoseOverriders.Count > 0)
                CheckConditionsOfAllOverriders();
        }


        private const DebugScope scope = DebugScope.CDK | DebugScope.UserCapture;

        /// <summary>
        /// If th e condition processor is enable
        /// </summary>
        private bool isProcessing;

        /// <summary>
        /// Stops the check of fall the overriders
        /// </summary>
        public void DisableCheck()
        {
            CoroutineManager.Instance.DettachCoroutine(checkRoutine);
            checkRoutine = null;
            isProcessing = false;
        }

        /// <summary>
        /// Active poses that listens to this activation mode.
        /// </summary>
        public bool TryActivate(PoseActivationMode mode)
        {
            foreach (PoseOverrider poseOverrider in interactionalPoseOverriders)
            {
                if (poseOverrider.ActivationMode == (ushort)mode && poseOverrider.CheckConditions())
                {
                    if (poseOverrider.IsActive) 
                        continue;

                    ConditionValidated?.Invoke(poseOverrider);
                    poseOverrider.IsActive = true;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Start to check all overriders
        /// return -1 if there is no pose playable,
        /// overwise returns the index of the playable pose
        /// </summary>
        public void CheckConditionsOfAllOverriders()
        {
            if (!isProcessing)
            {
                isProcessing = true;
                checkRoutine = CoroutineManager.Instance.AttachCoroutine(RegularCheckRoutine());
            }
        }

        private Coroutine checkRoutine;

        private float checkPeriod = 0.1f;

        private IEnumerator RegularCheckRoutine()
        {
            while (isProcessing)
            {
                yield return new WaitForSeconds(seconds: checkPeriod);
                for (int i = 0; i < nonInteractionalPoseOverriders.Count; i++)
                {
                    var poseOverrider = nonInteractionalPoseOverriders[i];
                    if (poseOverrider.CheckConditions() && !poseOverrider.IsActive)
                    {
                        ConditionValidated?.Invoke(poseOverrider);
                        poseOverrider.IsActive = true;
                    }
                    else if (poseOverrider.IsActive)
                    {
                        poseOverrider.IsActive = false;
                        ConditionDeactivated?.Invoke(poseOverrider);
                    }
                }
            }
            if (checkRoutine != null)
            {
                CoroutineManager.Instance.DettachCoroutine(checkRoutine);
                checkRoutine = null;
            }
            isProcessing = false;
        }
    }
}