using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace umi3d.common.userCapture
{
    public class BonePoseDto : UMI3DDto
    {
        public BonePoseDto() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bone">The current bone to update</param>
        /// <param name="position">The position of the bone</param>
        /// <param name="rotation">The rotation of the bone</param>
        public BonePoseDto(uint bone, Vector3 position, Vector4 rotation)
        {
            this.bone = bone;
            this.position = position;
            this.rotation = rotation;
        }

        public BonePoseDto(uint bone, Vector4 rotation)
        {
            this.bone = bone;
            this.rotation = rotation;
        }

        /// <summary>
        /// The current bone to update
        /// </summary>
        public uint bone { get; set; }
        /// <summary>
        /// The position of the bone
        /// </summary>
        public Vector3 position { get; set; }
        /// <summary>
        /// The rotation of the bone
        /// </summary>
        public Vector4 rotation { get; set; }
    }
}
