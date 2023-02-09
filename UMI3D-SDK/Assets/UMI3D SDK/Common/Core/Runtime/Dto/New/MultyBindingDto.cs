using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.collaboration
{
    public class MultyBindingDto : BindingDataDto
    {
        public MultyBindingDto() { }

        public MultyBindingDto(BindingDataDto[] Bindings,
                        int priority, bool partialFit) : base(priority, partialFit)
        {
            this.Bindings = Bindings;
        }

        public BindingDataDto[] Bindings { get; private set; }
    }
}
