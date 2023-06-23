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
            PoseOverriderDto dto = new PoseOverriderDto(poseIndexinPoseManager, poseConditions, duration, interpolationable, composable, isHoverEnter, isHoverExit, isRelease, isTrigger);
            return dto;
        }
    }
}
