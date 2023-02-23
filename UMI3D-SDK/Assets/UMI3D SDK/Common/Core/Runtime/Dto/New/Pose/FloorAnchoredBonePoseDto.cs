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
    }
}

