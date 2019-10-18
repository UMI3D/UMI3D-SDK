using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    class CVEAvatarPart : CVEModel
    {
        public string UserId;

        protected override void initDefinition()
        {
            base.initDefinition();
        }

        public override ModelDto CreateDto()
        {
            return new AvatarPartDto() as ModelDto;
        }

        public override ModelDto ToDto(UMI3DUser user)
        {
            AvatarPartDto dto = (AvatarPartDto)base.ToDto(user);
            dto.colliderType = ColliderType.None;
            dto.UserId = UserId;
            return dto;
        }
    }
}
