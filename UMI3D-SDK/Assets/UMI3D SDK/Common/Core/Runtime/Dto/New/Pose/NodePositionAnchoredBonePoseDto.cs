using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.userCapture
{
    public class NodePositionAnchoredBonePoseDto : BonePoseDto
    {
        public NodePositionAnchoredBonePoseDto() { }

        public NodePositionAnchoredBonePoseDto(uint node, uint bone, Vector3 position, Vector4 rotation) : base(bone, position, rotation)
        {
            this.node = node;
        }

        public NodePositionAnchoredBonePoseDto(uint node, BonePoseDto bonePoseDto) : base (bonePoseDto.bone, bonePoseDto.position, bonePoseDto.rotation)
        {
            this.node = node;
        }

        public uint node { get; private set; }
    }
}
