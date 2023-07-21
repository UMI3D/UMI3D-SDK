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

using umi3d.common.userCapture.pose;
using UnityEngine;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Links a collection of condition to a specific pose.
    /// </summary>
    public class PoseOverrider
    {
        private PoseOverriderDto dto;

        /// <summary>
        /// If true, the pose overrider is currently applied.
        /// </summary>
        public bool IsActive { get; set; }

        public PoseOverrider(PoseOverriderDto dto, IPoseCondition[] poseConditions)
        {
            this.dto = dto;
            this.PoseConditions = poseConditions ?? new IPoseCondition[0];
        }

        /// <summary>
        /// The is a server pose, so this is its index in the list of poses of the user 0
        /// </summary>
        public int PoseIndexInPoseManager => dto.poseIndexInPoseManager;

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
        /// State if the pose require user's cursor interaction to be applied.
        /// </summary>
        public bool IsInteractional => ActivationMode != (ushort)PoseActivationMode.NONE;

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

        /// <summary>
        /// Check if a pose overriders should be ended.
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public bool IsFinished(float startTime)
        {
            float currentDuration = Time.time - startTime;

            if (Duration.min.HasValue && currentDuration < Duration.min)
                return false;

            if (CheckConditions())
            {
                if (Duration.max.HasValue && currentDuration > Duration.max)
                    return true;

                if (!Duration.max.HasValue && Duration.duration == 0) // infinite mode
                    return false;

                if (!Duration.max.HasValue && currentDuration > Duration.duration)
                    return true;

                return false;
            }
            
            return true;
        }
    }
}