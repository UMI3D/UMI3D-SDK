using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace umi3d.common.userCapture
{
    [CreateAssetMenu()]
    [Serializable]
    public class UMI3DPoseOveridder_so : ScriptableObject
    {
        public UMI3DPose_so pose;
        /// <summary>
        /// The different condition that are needed for the overrider to get activated
        /// </summary>
        public PoseConditionDto[] poseConditions;
        public DurationDto duration;
        public bool interpolationable;
        public bool composable;

        public PoseOverriderDto ToDto(int poseIndexinPoseManager)
        {
            PoseOverriderDto dto = new PoseOverriderDto(poseIndexinPoseManager, poseConditions, duration, interpolationable, composable);
            return dto;
        }
    }
}
