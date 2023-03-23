using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.userCapture
{
    public class UMI3DPoseOverriderContainerDto : UMI3DDto, IEntity
    {
        public ulong id;
        public PoseOverriderDto[] poseOverriderDtos;
    }
}
