using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.userCapture
{
    public class BoneRotationConditionDto : PoseConditionDto
    {
        public BoneRotationConditionDto() { }

        public BoneRotationConditionDto(uint boneId, Vector4 rotation)
        {
            this.boneId = boneId;
            this.rotation = rotation;
        }

        public uint boneId { get; private set; }
        public Vector4 rotation { get; private set; }
    }
}
