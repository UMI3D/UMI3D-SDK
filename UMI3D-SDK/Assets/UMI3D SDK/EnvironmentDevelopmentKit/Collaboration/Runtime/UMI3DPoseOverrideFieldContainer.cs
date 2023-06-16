using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.edk.interaction;
using umi3d.edk;
using UnityEngine;
using umi3d.edk.userCapture;

public class UMI3DPoseOverrideFieldContainer : UMI3DPoseContainer, IPoseOverriderFieldContainer
{
    [SerializeField] private List<OverriderContainerField> allPoseOverriders = new List<OverriderContainerField>();
    public List<OverriderContainerField> GetAllPoseOverriders()
    {
        return allPoseOverriders;
    }
}

public interface IPoseOverriderFieldContainer
{
    public List<OverriderContainerField> GetAllPoseOverriders();
}

[Serializable]
public class OverriderContainerField
{
    [SerializeField] UMI3DPoseOverriderContainer poseOverriderContainer;
    public UMI3DPoseOverriderContainer PoseOverriderContainer { get => poseOverriderContainer; }

    [SerializeField] UMI3DEvent _uMI3DEvent;
    public UMI3DEvent uMI3DEvent { get => _uMI3DEvent; }

    public void SetNode()
    {
        PoseOverriderContainer.nodeID = uMI3DEvent.GetComponent<UMI3DModel>().Id();
        PoseOverriderContainer.eventID = uMI3DEvent.Id();
    }
}
