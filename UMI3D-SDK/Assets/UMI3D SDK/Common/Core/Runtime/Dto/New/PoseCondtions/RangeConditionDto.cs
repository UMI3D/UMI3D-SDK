using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.userCapture
{
    public class RangeConditionDto : PoseConditionDto
    {
        public RangeConditionDto() { } 

        public RangeConditionDto(PoseConditionDto conditionA, PoseConditionDto conditionB)
        {
            this.conditionA = conditionA;
            this.conditionB = conditionB;
        }

        public PoseConditionDto conditionA { get; private set; }
        public PoseConditionDto conditionB { get; private set; }
    }
}
