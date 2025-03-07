using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.lbe.description
{
    public class ARAnchorDto : UMI3DDto
    {
        /// <summary>
        /// Position anchor.
        /// </summary>
        public Vector3Dto position { get; set; }

        /// <summary>
        /// Rotation anchor.
        /// </summary>
        public Vector4Dto rotation { get; set; }
    }
}
