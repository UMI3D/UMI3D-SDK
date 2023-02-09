using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.collaboration
{
    public class BindingDto : UMI3DDto
    {
        public BindingDto() { }

        public BindingDto(ulong objectId, bool active, BindingDataDto data)
        {
            this.bindingId = objectId;
            this.active = active;
            this.data = data;
        }

        /// <summary>
        /// An identifier defined by the designer.
        /// </summary>
        public ulong bindingId { get; private set; }
        public bool active { get; private set; }
        public BindingDataDto data { get; private set; }
    }
}
