using umi3d.common;
using UnityEngine.Events;

namespace umi3d.edk.volume
{
    public interface IVolume : IVolumeDescriptor
    {
        UMI3DUserEvent GetUserEnter();
        UMI3DUserEvent GetUserExit();
    }
}


