using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.userCapture
{
    /// <summary>
    /// In multy binding the priority state which binding should be match the most.
    /// </summary>
    public class MultyBindingDto : BindingDataDto
    {
        public MultyBindingDto() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Bindings">List Of all the simple bindings to apply</param>
        /// <param name="priority">level of priority of this binding [impact the order in which it is applied]</param>
        /// <param name="partialFit"> State if the binding can be applied partialy or not. A partial fit can happen in MultyBinding when it's not the binding with the highest priority.</param>
        public MultyBindingDto(BindingDataDto[] Bindings,
                        int priority, bool partialFit) : base(priority, partialFit)
        {
            this.Bindings = Bindings;
        }

        public MultyBindingDto(BindingDataDto[] Bindings, BindingDataDto bindingDataDto)
            : base(bindingDataDto.priority, bindingDataDto.partialFit)
        {
            this.Bindings = Bindings;
        }

        /// <summary>
        /// List Of all the simple bindings to apply
        /// </summary>
        public BindingDataDto[] Bindings { get; private set; }
    }
}
