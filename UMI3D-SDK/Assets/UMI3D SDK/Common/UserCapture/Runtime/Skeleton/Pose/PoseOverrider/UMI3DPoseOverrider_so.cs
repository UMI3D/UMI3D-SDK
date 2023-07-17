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
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.userCapture.pose
{
    /// <summary>
    /// A pose overrider description as a scriptable object
    /// </summary>
    [Serializable]
    public class UMI3DPoseOverrider_so : ScriptableObject
    {
        public UMI3DPose_so pose;

        /// <summary>
        /// The different condition that are needed for the overrider to get activated
        /// </summary>
        [SerializeReference] public PoseConditionDto[] poseConditions;

        [System.Serializable]
        public struct Duration
        {
            public ulong duration;

            public bool hasMin;

            public ulong min;

            public bool hasMax;

            public ulong max;

            public DurationDto ToDto()
            {
                return new()
                {
                    max = hasMax ? max : null,
                    min = hasMin ? min : null,
                    duration = duration
                };
            }
        }

        public Duration duration;
        public bool interpolable;
        public bool composable;

        public PoseActivationMode activationMode;

        public PoseOverriderDto ToDto(int poseIndexInPoseManager)
        {
            return new PoseOverriderDto()
            {
                poseIndexInPoseManager = poseIndexInPoseManager,
                poseConditions = GetPoseConditionsCopy(),
                duration = duration.ToDto(),
                isInterpolable = interpolable,
                isComposable = composable,
                activationMode = (ushort)activationMode
            };
        }

        public PoseConditionDto[] GetPoseConditionsCopy() //? why ?
        {
            List<PoseConditionDto> copy = new List<PoseConditionDto>();
            poseConditions.ForEach(pc =>
            {
                switch (pc)
                {
                    case MagnitudeConditionDto magnitudeConditionDto:
                        copy.Add(new MagnitudeConditionDto()
                        {
                            Magnitude = magnitudeConditionDto.Magnitude,
                            BoneOrigin = magnitudeConditionDto.BoneOrigin,
                            TargetNodeId = magnitudeConditionDto.TargetNodeId
                        });
                        break;

                    default:
                        throw new NotImplementedException("Please implement clone for other conditions");
                }
            });
            return copy.ToArray();
        }
    }
}