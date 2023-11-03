/*
using UnityEngine;

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
using System.Collections.Generic;
using System.Linq;
using umi3d.edk;
using umi3d.edk.core;
using umi3d.edk.userCapture.pose;
using UnityEngine;

namespace umi3d.common.userCapture.pose
{
    /// <summary>
    /// Controls a pose animation.
    /// </summary>
    public class UMI3DPoseAnimator : AbstractLoadableComponentEntity, UMI3DLoadableEntity
    {
        [SerializeField]
        public UMI3DPose_so pose_so;

        private IUMI3DPoseData pose;

        /// <summary>
        /// Pose ressource for animation.
        /// </summary>
        public IUMI3DPoseData Pose
        {
            get
            {
                if (pose == null)
                    pose = pose_so;

                return pose;
            }
            set
            {
                pose = value;
            }
        }

        private PoseClip poseClip;
        public PoseClip PoseClip
        {
            get
            {
                if (poseClip == null)
                {
                    poseClip = Pose == null ? null : new PoseClip(Pose);
                }

                return poseClip;
            }
            set
            {
                poseClip = value;
            }
        }


        /// <summary>
        /// The different condition that are needed for the overrider to get activated
        /// </summary>
        [SerializeReference, HideInInspector]
        public AbstractPoseConditionDto[] poseConditions;

        /// <summary>
        /// Poses conditions validated by the environment.
        /// </summary>
        [HideInInspector]
        public List<UMI3DEnvironmentPoseCondition> environmentPoseConditions = new();

        public IReadOnlyList<AbstractPoseConditionDto> PoseConditions => GetPoseConditions().ToList();

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

        [Tooltip("Expected duration of the pose animation.")]
        public Duration duration;

        [Tooltip("Can the pose animation be interpolated when it is applied?")]
        [HideInInspector]
        public bool interpolable;

        [Tooltip("Can the pose animation be composed with another pose animation?")]
        public bool composable;

        [Tooltip("How the pose animator could be activated by the user.")]
        public PoseAnimatorActivationMode activationMode;

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

        public UMI3DNode relativeNode;

        [Header("- Direction condition")]
        public bool HasDirectionCondition;

        /// <summary>
        /// distance
        /// </summary>
        public Vector3 Direction;

        [Header("- Scale condition")]
        public bool HasScaleCondition;

        /// <summary>
        /// distance
        /// </summary>
        public Vector3 TargetScale;

        #endregion Pose condition access

        public virtual PoseAnimatorDto ToDto()
        {
            return new PoseAnimatorDto()
            {
                id = Id(),
                relatedNodeId = relativeNode.Id(),
                poseId = poseClip.Id(),
                poseConditions = GetPoseConditions(),
                duration = duration.ToDto(),
                isInterpolable = interpolable,
                isComposable = composable,
                activationMode = (ushort)activationMode
            };
        }

        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return ToDto();
        }

        public virtual AbstractPoseConditionDto[] GetPoseConditions()
        {
            List<AbstractPoseConditionDto> copy = new();

            copy.AddRange(environmentPoseConditions.Select(x => x.ToEntityDto() as AbstractPoseConditionDto));

            if (HasMagnitudeCondition)
            {
                copy.Add(new MagnitudeConditionDto()
                {
                    Magnitude = Magnitude,
                    BoneOrigin = BoneOrigin,
                    TargetNodeId = relativeNode.Id()
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