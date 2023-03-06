using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.userCapture
{
    public class PoseOverriderDto : MonoBehaviour
    {
        public PoseOverriderDto() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pose"></param>
        /// <param name="poseConditionDtos">The different condition that are needed for the overrider to get activated</param>
        /// <param name="duration"></param>
        /// <param name="interpolationable"></param>
        /// <param name="composable"></param>
        public PoseOverriderDto(PoseDto pose, PoseConditionDto[] poseConditionDtos, DurationDto duration, bool interpolationable, bool composable)
        {
            this.pose = pose;
            this.poseConditions = poseConditionDtos;
            this.duration = duration;
            this.interpolationable = interpolationable;
            this.composable = composable;
        }

        public PoseDto pose { get; private set; }
        /// <summary>
        /// The different condition that are needed for the overrider to get activated
        /// </summary>
        public PoseConditionDto[] poseConditions { get; private set; }
        public DurationDto duration { get; private set; }
        public bool interpolationable { get; private set; }
        public bool composable { get; private set; }
    }
}

