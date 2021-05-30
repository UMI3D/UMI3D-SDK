using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk.volume.volumedrawing
{
    public abstract class AbstractSelectable : MonoBehaviour, IEntity
    {
        public abstract void EnableHighlight();
        public abstract void DisableHighlight();
    }
}
