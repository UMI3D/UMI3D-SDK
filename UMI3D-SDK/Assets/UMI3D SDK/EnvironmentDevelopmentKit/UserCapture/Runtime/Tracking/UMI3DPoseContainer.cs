using inetum.unityUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common.userCapture;
using umi3d.edk;
using UnityEngine;

public class UMI3DPoseContainer : SingleBehaviour<UMI3DPoseContainer>, IPoseContainer
{
    [SerializeField] List<UMI3DPose_so> allServerPoses = new List<UMI3DPose_so>();
    public List<UMI3DPose_so> GetAllServerPoses() { return allServerPoses; }
}

public interface IPoseContainer
{
    public List<UMI3DPose_so> GetAllServerPoses();
}
