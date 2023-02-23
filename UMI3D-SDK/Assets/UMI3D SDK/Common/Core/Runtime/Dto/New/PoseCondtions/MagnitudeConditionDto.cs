using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.userCapture
{
    public class MagnitudeConditionDto : PoseConditionDto
    {
        public MagnitudeConditionDto() { }

        public MagnitudeConditionDto(float magnitude)
        {
            this.magnitude = magnitude;
        }

        public float magnitude { get; private set; }
    }
}

