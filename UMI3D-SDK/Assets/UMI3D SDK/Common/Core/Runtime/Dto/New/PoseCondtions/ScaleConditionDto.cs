using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.userCapture
{
    public class ScaleConditionDto : PoseConditionDto
    {
        public ScaleConditionDto() { }  

        public ScaleConditionDto(Vector3 scale)
        {
            this.scale = scale;
        }

        public Vector3 scale { get; private set; }  
    }
}
