using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.common.userCapture;
using System;
using umi3d.common.lbe.description;


namespace umi3d.common.lbe
{
    public class UserGuardianRequestDto : AbstractBrowserRequestDto
    {
        //public bool isAdmin { get; set; }

        //public uint lbeGroupId { get; set; } = 1; // to reset after demo

        public List<ARAnchorDto> aRAnchors { get; set; }

        //public bool isImmersive { get; set; }
    }
}

