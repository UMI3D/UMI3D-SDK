using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.userCapture.pose
{
    public class NodeAnchoredBonePoseDto : BonePoseDto
    {
        public NodeAnchoredBonePoseDto() { }

        public NodeAnchoredBonePoseDto(uint node, uint bone, Vector3Dto position, Vector4Dto rotation) : base(bone, position, rotation)
        {
            this.node = node;
        }

        public NodeAnchoredBonePoseDto(uint node, BonePoseDto bonePoseDto) : base(bonePoseDto.Bone, bonePoseDto.Position, bonePoseDto.Rotation)
        {
            this.node = node;
        }

        public uint node { get; set; }
    }
}
