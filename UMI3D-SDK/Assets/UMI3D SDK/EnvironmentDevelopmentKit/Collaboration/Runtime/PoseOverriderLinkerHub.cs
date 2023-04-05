using inetum.unityUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.edk.interaction;
using umi3d.edk.userCapture;
using UnityEngine;

namespace umi3d.common.collaboration
{
    public class PoseOverriderLinkerHub : SingleBehaviour<PoseOverriderLinkerHub>
    {
        public List<OverriderContainerField> allPoseOverriders = new List<OverriderContainerField>();

        [Serializable]
        public class OverriderContainerField
        {
            [SerializeField] UMI3DPoseOverriderContainer poseOverriderContainer;
            [SerializeField] UMI3DEvent uMI3DEvent;

            public UMI3DPoseOverriderContainer PoseOverriderContainer { get => poseOverriderContainer; }
            public UMI3DEvent UMI3DEvent { get => uMI3DEvent;  }
        }
    }
}
