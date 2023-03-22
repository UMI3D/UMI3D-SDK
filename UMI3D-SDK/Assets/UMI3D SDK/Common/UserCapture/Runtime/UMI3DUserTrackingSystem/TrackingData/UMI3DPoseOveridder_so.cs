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

        public UMI3DPose_so pose { get; private set; }
        /// <summary>
        /// The different condition that are needed for the overrider to get activated
        /// </summary>
        public PoseConditionDto[] poseConditions { get; private set; }
        public DurationDto duration { get; private set; }
        public bool interpolationable { get; private set; }
        public bool composable { get; private set; }

        public PoseOverriderDto ToDto(int poseIndexinPoseManager)
        {
            PoseOverriderDto dto = new PoseOverriderDto(poseIndexinPoseManager, poseConditions, duration, interpolationable, composable);
            return dto;
        }
    }
}
