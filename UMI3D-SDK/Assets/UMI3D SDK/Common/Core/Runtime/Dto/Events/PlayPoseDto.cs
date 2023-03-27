using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common
{
    public class PlayPoseDto : AbstractOperationDto
    {
        public ulong userID;
        public ulong poseKey;
        public int indexInList;

        public bool stopPose = false;

        public PlayPoseDto() : base() { }
    }
}
