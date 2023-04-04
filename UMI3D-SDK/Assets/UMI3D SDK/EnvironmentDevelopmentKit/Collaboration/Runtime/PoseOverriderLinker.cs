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
            SetUpUmi3dEvent();
            SetUpUmi3dInteractable();
        }

        UMI3DEvent umi3dEvent = null;
        UMI3DInteractable umi3dInteractable = null;

        private void SetUpUmi3dEvent()
        {
            umi3dEvent = GetComponent<UMI3DEvent>();
        }

        private void SetUpUmi3dInteractable()
        {
            umi3dInteractable = GetComponent<UMI3DInteractable>();
        }

        public void InitPoseOverriderHoverEnterContainer()
        {
            hoverEnterContainer = new UMI3DPoseOverriderContainer(hoverEnterOverriders);
            umi3dInteractable.SetHoverEnterPose(hoverEnterContainer.Id());
        }

        public void InitPoseOverriderHoverExitContainer()
        {
            hoverExitContainer = new UMI3DPoseOverriderContainer(hoverExitOverriders);
            umi3dInteractable.SetHoverExitPose(hoverExitContainer.Id());
        }

        public void InitPoseOverriderTriggerContainer()
        {
            triggerContainer = new UMI3DPoseOverriderContainer(triggerOverriders);
            umi3dEvent.SetTriggerExitPose(triggerContainer.Id());
        }

        public void InitPoseOverriderReleaseContainer()
        {
            releaseContainer = new UMI3DPoseOverriderContainer(releaseOverriders);
            umi3dEvent.SetReleaseExitPose(releaseContainer.Id());
        }
    }
}

