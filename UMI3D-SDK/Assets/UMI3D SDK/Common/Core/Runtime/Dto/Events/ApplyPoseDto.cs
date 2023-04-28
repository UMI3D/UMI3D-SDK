using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common
{
    public class ApplyPoseDto : AbstractOperationDto
    {
        public ulong userID;
        public ulong poseKey;
        public int indexInList;

        public bool stopPose = false;

        public ApplyPoseDto() : base() { }
    }
}
