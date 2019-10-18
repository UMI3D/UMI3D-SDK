using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common
{
    [System.Serializable]
    public class AvatarPartDto : ModelDto
    {
        public string UserId;

        public AvatarPartDto() : base()
        {
            colliderType = ColliderType.None;
        }
    }
}