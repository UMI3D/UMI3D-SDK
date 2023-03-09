using System.Collections;
using System.Collections.Generic;
using umi3d.common.userCapture;
using UnityEngine;

public class UMI3DPose_so : ScriptableObject
{
    public UMI3DPose_so(List<UMI3DBonePose_so> bonePoses, uint boneAnchor)
    {
        this.bonePoses = bonePoses;
        this.boneAnchor = boneAnchor;   
    }

    List<UMI3DBonePose_so> bonePoses = new List<UMI3DBonePose_so>();
    uint boneAnchor;

    public List<UMI3DBonePose_so> BonePoses { get => bonePoses;  }
    public uint BoneAnchor { get => boneAnchor;  }

    public PoseDto ToDTO()
    {
        List<BonePoseDto> bonePosesDtos = new List<BonePoseDto>();
        bonePoses.ForEach(bp =>
        {
            bonePosesDtos.Add(bp.ToDTO());
        });
        return new PoseDto(bonePosesDtos.ToArray(), boneAnchor);
    }
}
