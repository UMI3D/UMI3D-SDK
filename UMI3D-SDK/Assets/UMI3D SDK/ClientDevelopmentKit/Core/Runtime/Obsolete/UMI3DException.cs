using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.cdk
{
    [Obsolete("Use umi3d.common.Umi3dException")]
    public class Umi3dException : umi3d.common.Umi3dException
    {
        public Umi3dException(string message) : base(message)
        {
        }
    }
}