using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.common.userCapture
{
    public class PoseDto : UMI3DDto
    {
        public PoseDto() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bonePoseDtos">all the bone pose that are composing the current pose</param>
        /// <param name="boneAnchor"></param>
        public PoseDto(BonePoseDto[] bones, uint boneAnchor)
        {
            this.bones = bones;
            this.boneAnchor = boneAnchor;
        }

        /// <summary>
        /// all the bone pose that are composing the current pose
        /// </summary>
        public BonePoseDto[] bones { get; private set; }
        public void SetBonePoseDtoArray(BonePoseDto[] bones)
        {
            this.bones = bones;
        }

        public uint boneAnchor { get; private set; }
    }
}
