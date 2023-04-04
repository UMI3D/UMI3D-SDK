using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common.userCapture;
using umi3d.edk.interaction;
using UnityEngine;

namespace umi3d.common.collaboration
{
    public class PoseOverriderLinker : MonoBehaviour
    {
        [SerializeField] List<UMI3DPoseOveridder_so> hoverEnterOverriders;
        [SerializeField] List<UMI3DPoseOveridder_so> hoverExitOverriders;
        [SerializeField] List<UMI3DPoseOveridder_so> triggerOverriders;
        [SerializeField] List<UMI3DPoseOveridder_so> realeaseOverriders;

        private void Awake()
        {
            SetUpUmi3dEvent();
            SetUpUmi3dInteraction();
        }

        private void SetUpUmi3dEvent()
        {
            UMI3DEvent umi3dEvent = GetComponent<UMI3DEvent>();
            if (umi3dEvent != null )
            {
                umi3dEvent.SetHoverEnterPose(GetHoverEnterPoseDtos());
                umi3dEvent.SetHoverExitPose(GetHoverExitPoseDtos());
                umi3dEvent.SetTriggerExitPose(GetTriggerPoseDtos());
                umi3dEvent.SetReleaseExitPose(GetReleasePoseDtos());
            }
        }

        private void SetUpUmi3dInteraction()
        {
            throw new NotImplementedException();
        }

        public List<PoseOverriderDto> GetHoverEnterPoseDtos()
        {
            List<PoseOverriderDto> dtos = new();

            for (int i = 0; i < hoverEnterOverriders.Count; i++)
            {
                dtos.Add(hoverEnterOverriders[i].ToDto(-1));
            }

            return dtos;
        }

        public List<PoseOverriderDto> GetHoverExitPoseDtos()
        {
            List<PoseOverriderDto> dtos = new();

            for (int i = 0; i < hoverExitOverriders.Count; i++)
            {
                dtos.Add(hoverExitOverriders[i].ToDto(-1));
            }

            return dtos;
        }

        public List<PoseOverriderDto> GetTriggerPoseDtos()
        {
            List<PoseOverriderDto> dtos = new();

            for (int i = 0; i < triggerOverriders.Count; i++)
            {
                dtos.Add(triggerOverriders[i].ToDto(-1));
            }

            return dtos;
        }

        public List<PoseOverriderDto> GetReleasePoseDtos()
        {
            List<PoseOverriderDto> dtos = new();

            for (int i = 0; i < realeaseOverriders.Count; i++)
            {
                dtos.Add(realeaseOverriders[i].ToDto(-1));
            }

            return dtos;
        }
    }
}

