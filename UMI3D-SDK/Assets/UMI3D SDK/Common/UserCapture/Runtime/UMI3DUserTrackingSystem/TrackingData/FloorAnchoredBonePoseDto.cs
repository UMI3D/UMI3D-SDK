using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.userCapture
{
    public class FloorAnchoredBonePoseDto : BonePoseDto
    {
        public FloorAnchoredBonePoseDto() { }

        public FloorAnchoredBonePoseDto(BonePoseDto bonePoseDto) : base(bonePoseDto.bone, bonePoseDto.position, bonePoseDto.rotation)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bone">The current bone to update</param>
        /// <param name="position">The position of the bone</param>
        /// <param name="rotation">The rotation of the bone</param>
        public FloorAnchoredBonePoseDto(uint bone, Vector3 position, Vector4 rotation)
             : base(bone, position, rotation)
        {

        }
    }
}

