using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace umi3d.common.userCapture
{
    public class AnchorBonePoseDto : BonePoseDto
    {
        public AnchorBonePoseDto() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherBone">The anchor bone</param>
        /// <param name="bone">The current bone to update</param>
        /// <param name="position">The position of the bone</param>
        /// <param name="rotation">The rotation of the bone</param>
        public AnchorBonePoseDto(uint otherBone, uint bone, Vector3 position, Vector4 rotation) : base (bone, position, rotation) 
        { 
            this.otherBone = otherBone;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherBone">The anchor bone</param>
        /// <param name="bonePoseDto">The bone DTO corresponding to the current anchor bone pose dto you want to do</param>
        public AnchorBonePoseDto(uint otherBone, BonePoseDto bonePoseDto) : base (bonePoseDto.bone, bonePoseDto.position, bonePoseDto.rotation)
        {
            this.otherBone = otherBone;
        }

        /// <summary>
        /// The anchor bone
        /// </summary>
        public uint otherBone { get; private set; }
    }
}
