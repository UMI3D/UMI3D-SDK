using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.userCapture
{
    public class UMI3DOverriderMetaClassDto : UMI3DDto, IEntity
    {
        public ulong id;
        public PoseOverriderDto[] poseOverriderDtos;
    }
}
