using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.userCapture
{
    public class DirectionConditionDto : PoseConditionDto
    {
        public DirectionConditionDto() { }

        public DirectionConditionDto(Vector3 direction)
        {
            this.direction = direction;
        }

        public Vector3 direction { get; private set; }
    }
}
