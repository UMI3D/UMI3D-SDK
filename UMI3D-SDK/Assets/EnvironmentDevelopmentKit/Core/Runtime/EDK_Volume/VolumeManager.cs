using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.common;
using umi3d.edk.volume.volumedrawing;

namespace umi3d.edk.volume
{
    public class VolumeManager : Singleton<VolumeManager>
    {
        public List<IVolumeDescriptor> volumes = new List<IVolumeDescriptor>();

    }
}