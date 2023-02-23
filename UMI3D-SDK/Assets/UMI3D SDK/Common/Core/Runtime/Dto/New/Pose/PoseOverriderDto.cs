using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.userCapture
{
    public class PoseOverriderDto : MonoBehaviour
    {
        public PoseOverriderDto() { }   

        public PoseDto pose { get; private set; }
        //public PoseCondition
        public DurationDto duration { get; private set; }
        public bool interpolationable { get; private set; }
        public bool composable { get; private set; }
    }
}

