using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.userCapture
{
    public class DurationDto : UMI3DDto
    {
        public DurationDto() { }

        public DurationDto(ulong duration, ulong? min, ulong? max)
        {
            this.duration = duration;
            this.min = min; 
            this.max = max;
        }

        public ulong duration { get; private set; }
        public ulong? min { get; private set; } 
        public ulong? max { get; private set;}
    }
}

