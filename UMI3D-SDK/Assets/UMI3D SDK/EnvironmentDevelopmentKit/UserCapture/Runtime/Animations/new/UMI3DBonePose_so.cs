using System.Collections;
using System.Collections.Generic;
using umi3d.common.userCapture;
using UnityEngine;

public class UMI3DBonePose_so : ScriptableObject
{
    public UMI3DBonePose_so(uint bone, Vector3 position, Vector4 rotation)
    {
        this.bone = bone; 
        this.position = position;
        this.rotation = rotation;
    }

    public readonly uint bone;
    public readonly Vector3 position;
    public readonly Vector4 rotation;

    public BonePoseDto ToDTO()
    {
        return new BonePoseDto(bone, position, rotation);
    }
}
