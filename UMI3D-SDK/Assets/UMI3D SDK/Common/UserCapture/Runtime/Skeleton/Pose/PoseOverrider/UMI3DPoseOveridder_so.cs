using inetum.unityUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.userCapture.pose
{
    [CreateAssetMenu()]
    [Serializable]
    public class UMI3DPoseOveridder_so : ScriptableObject
    {
        public UMI3DPose_so pose;

        /// <summary>
        /// The different condition that are needed for the overrider to get activated
        /// </summary>
        [SerializeReference] public PoseConditionDto[] poseConditions;

        public DurationDto duration;
        public bool interpolationable;
        public bool composable;
        public bool isHoverEnter;
        public bool isHoverExit;
        public bool isRelease;
        public bool isTrigger;

        public PoseOverriderDto ToDto(int poseIndexinPoseManager)
        {
            PoseOverriderDto dto = new PoseOverriderDto(poseIndexinPoseManager, GetPoseConditionsCopy(), duration, interpolationable, composable, isHoverEnter, isHoverExit, isRelease, isTrigger);
            return dto;
        }

        public PoseConditionDto[] GetPoseConditionsCopy()
        {
            List<PoseConditionDto> copy = new List<PoseConditionDto>();
            poseConditions.ForEach(pc =>
            {
                switch (pc)
                {
                    case MagnitudeConditionDto magnitudeConditionDto:
                        copy.Add(new MagnitudeConditionDto()
                        {
                            magnitude = magnitudeConditionDto.magnitude,
                            BoneOrigine = magnitudeConditionDto.BoneOrigine,
                            targetObjectId = magnitudeConditionDto.TargetObjectId
                        });
                        break;

                    default:
                        throw new NotImplementedException("Please implemente clone for other conditions");
                }
            });
            return copy.ToArray();
        }

        public DurationDto GetDurationCopy()
        {
            DurationDto copy = new DurationDto()
            {
                max = duration.max,
                min = duration.min,
                duration = duration.duration
            };

            return copy;
        }
    }
}