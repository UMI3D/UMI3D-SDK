using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.userCapture
{
    public class UserScaleConditinoDto : PoseConditionDto
    {
        public UserScaleConditinoDto() { }

        public UserScaleConditinoDto(Vector3 scale)
        {
            this.scale = scale;
        }

        public Vector3 scale;
    }
}
