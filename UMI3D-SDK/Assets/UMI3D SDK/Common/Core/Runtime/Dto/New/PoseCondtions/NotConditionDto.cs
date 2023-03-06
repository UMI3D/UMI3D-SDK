using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.userCapture
{
    public class NotConditionDto : PoseConditionDto
    {
        public NotConditionDto() { }

        public NotConditionDto(PoseConditionDto[] conditions)
        {
            this.conditions = conditions;
        }


        public PoseConditionDto[] conditions { get; private set; }
    }
}
