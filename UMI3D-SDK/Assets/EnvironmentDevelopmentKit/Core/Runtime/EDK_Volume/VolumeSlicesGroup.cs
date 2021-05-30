using System.Collections.Generic;
using UnityEngine;

namespace umi3d.edk.volume.volumedrawing
{
    public class VolumeSlicesGroup : MonoBehaviour, IVolumeDescriptor
    {
        public List<VolumeSlice> volumeSlices = new List<VolumeSlice>();
    }
}