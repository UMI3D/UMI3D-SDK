using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.common.collaboration
{
    public class BindingDataDto : UMI3DDto
    {
        public BindingDataDto() { }

        public BindingDataDto(int priority, bool partialFit)
        {
            this.priority = priority;
            this.partialFit = partialFit;
        }

        /// <summary>
        /// level of priority of this binding [impact the order in which it is applied]
        /// </summary>
        public int priority { get; private set; }

        /// <summary>
        /// State if the binding can be applied partialy or not. A partial fit can happen in MultyBinding when it's not the binding with the highest priority.
        /// </summary>
        public bool partialFit { get; private set; }
    }
}
