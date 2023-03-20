using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common
{
    public class PlayPoseDto : AbstractOperationDto
    {
        public ulong userID;
        public int indexInList; 

        public PlayPoseDto() : base() { }
    }
}
