using System.Collections;
using System.Collections.Generic;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.common.userCapture
{
    public class NodeRotationAnchoredBonePoseDto : BonePoseDto
    {
        public NodeRotationAnchoredBonePoseDto() { }

        public NodeRotationAnchoredBonePoseDto(uint node, uint bone, Vector3 position, Vector4 rotation) : base(bone, position, rotation)
        {
            this.node = node;
        }

        public NodeRotationAnchoredBonePoseDto(uint node, BonePoseDto bonePoseDto) : base(bonePoseDto.bone, bonePoseDto.position, bonePoseDto.rotation)
        {
            this.node = node;
        }

        public uint node { get; private set; }
    }
}

