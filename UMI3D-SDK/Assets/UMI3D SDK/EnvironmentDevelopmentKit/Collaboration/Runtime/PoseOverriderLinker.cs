using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common.userCapture;
using umi3d.edk.interaction;
using umi3d.edk.userCapture;
using UnityEngine;

namespace umi3d.common.collaboration
{
    public class PoseOverriderLinker : MonoBehaviour
    {
        [SerializeField] List<UMI3DPoseOveridder_so> hoverEnterOverriders;
        [SerializeField] List<UMI3DPoseOveridder_so> hoverExitOverriders;
        [SerializeField] List<UMI3DPoseOveridder_so> triggerOverriders;
        [SerializeField] List<UMI3DPoseOveridder_so> releaseOverriders;

        UMI3DPoseOverriderContainer hoverEnterContainer = null;
        UMI3DPoseOverriderContainer hoverExitContainer = null;
        UMI3DPoseOverriderContainer triggerContainer = null;
        UMI3DPoseOverriderContainer releaseContainer = null;

        private void Awake()
        {
            Init();
            SetUpUmi3dEvent();
            SetUpUmi3dInteractable();
        }

        private void Init()
        {
            hoverEnterContainer = new UMI3DPoseOverriderContainer(hoverEnterOverriders);
            hoverExitContainer = new UMI3DPoseOverriderContainer(hoverExitOverriders);
            triggerContainer = new UMI3DPoseOverriderContainer(triggerOverriders);
            releaseContainer = new UMI3DPoseOverriderContainer(releaseOverriders);
        }

        UMI3DEvent umi3dEvent = null;
        UMI3DInteractable umi3dInteractable = null;

        private void SetUpUmi3dEvent()
        {
            umi3dEvent = GetComponent<UMI3DEvent>();
            umi3dEvent.onHasRegistered += (id) =>
            {
                UpdatePoseOverriderEventContainers(id);
            };
        }

        private void SetUpUmi3dInteractable()
        {
            umi3dInteractable = GetComponent<UMI3DInteractable>();
            umi3dInteractable.onHasRegistered += (id) =>
            {
                UpdatePoseOverriderInteractableContainers(id);
            };
        }

        private void UpdatePoseOverriderEventContainers(ulong id)
        {
            triggerContainer.SetEventID(id);
            releaseContainer.SetEventID(id);
        }

        private void UpdatePoseOverriderInteractableContainers(ulong id)
        {
            hoverEnterContainer.SetEventID(id);
            hoverExitContainer.SetEventID(id);
        }
    }
}

