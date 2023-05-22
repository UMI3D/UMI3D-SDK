using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common
{
    public class ApplyPoseDto : AbstractOperationDto
    {
        public ulong userID { get; set; }
        public ulong poseKey { get; set; }
        public int indexInList { get; set; }

        public bool stopPose { get; set; } = false;
    }
}
