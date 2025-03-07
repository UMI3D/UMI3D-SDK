using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace umi3d.common.lbe.description
{
    public class UserGroupMrDto : UMI3DDto
    {
        public List<ulong> Members { get; set; }

        public bool EnableOcclusion { get; set; }

        public UserGroupMrDto(ulong user, bool Occlusion)
        {
            Members = new List<ulong> { user };
            EnableOcclusion = Occlusion;
        }
    }
}
