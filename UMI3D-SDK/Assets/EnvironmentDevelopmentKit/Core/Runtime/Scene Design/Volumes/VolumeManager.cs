using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using umi3d.common;
using umi3d.edk.volume.volumedrawing;

namespace umi3d.edk.volume
{
    public class VolumeManager : Singleton<VolumeManager>
    {
        public Dictionary<string, IVolume> volumes = new Dictionary<string, IVolume>(); 
    }
}