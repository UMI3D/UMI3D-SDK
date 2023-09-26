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
using System.Linq;
using UnityEngine;

namespace umi3d.common.userCapture.pose
{
    /// <summary>
    /// A pose overrider description as a scriptable object
    /// </summary>
    [Serializable]
    [CreateAssetMenu(menuName = "UMI3D/Pose/Overrider")]
    public class UMI3DPoseOverrider_so : ScriptableObject, IUMI3DPoseOverriderData
    {
        [SerializeField]
        public UMI3DPose_so pose;

        public IUMI3DPoseData Pose => pose;

        /// <summary>
        /// The different condition that are needed for the overrider to get activated
        /// </summary>
        [SerializeReference, HideInInspector]
        public AbstractPoseConditionDto[] poseConditions;
        public IReadOnlyList<AbstractPoseConditionDto> PoseConditions => GetPoseConditionsCopy().ToList();

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

        [Tooltip("Expected duration of the pose.")]
        public Duration duration;

        [Tooltip("Can the pose be interpolated when it is applied?")]
        [HideInInspector]
        public bool interpolable;

        [Tooltip("Can the pose be composed with another pose?")]
        public bool composable;

        [Tooltip("How the pose could be activated by the user.")]
        public PoseActivationMode activationMode;

        // HACK: Workaround not to fix pose setter
        #region Pose condition access
        [Space(10)]
        [Header("Pose conditions")]

        [Header("- Magnitude condition")]
        public bool HasMagnitudeCondition;
        /// <summary>
        /// distance
        /// </summary>
        public float Magnitude;

        /// <summary>
        /// bone id 
        /// </summary>
        [ConstEnum(typeof(BoneType), typeof(uint))]
        public uint BoneOrigin;


        [Header("- Direction condition")]
        public bool HasDirectionCondition;
        /// <summary>
        /// distance
        /// </summary>
        public Vector3 Direction;

        [Header("- Direction condition")]
        public bool HasScaleCondition;

        /// <summary>
        /// distance
        /// </summary>
        public Vector3 TargetScale;
        #endregion

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

        public AbstractPoseConditionDto[] GetPoseConditionsCopy()
        {
            List<AbstractPoseConditionDto> copy = new();

            if (HasMagnitudeCondition)
            {
                copy.Add(new MagnitudeConditionDto()
                {
                    Magnitude = Magnitude,
                    BoneOrigin = BoneOrigin
                    // UNDONE : Add related TargetNodeId = 
                });
            }
            if (HasDirectionCondition)
            {
                copy.Add(new DirectionConditionDto()
                {
                    Direction = Direction.Dto()
                });
            }
            if (HasScaleCondition)
            {
                copy.Add(new ScaleConditionDto()
                {
                    Scale = TargetScale.Dto()
                });
            }
            return copy.ToArray();
        }
    }
}