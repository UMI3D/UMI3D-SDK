using inetum.unityUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.edk;
using umi3d.edk.interaction;
using umi3d.edk.userCapture;
using UnityEngine;

namespace umi3d.common.collaboration
{
    public class UMI3DPoseOverriderManager : UMI3DPoseManager
    {
        private readonly IPoseOverriderFieldContainer poseOverriderContainerService;

        public UMI3DPoseOverriderManager() : base() 
        {
            this.poseOverriderContainerService = UMI3DPoseOverrideFieldContainer.Instance as IPoseOverriderFieldContainer;
            InitEachPoseAnimationWithPoseOverriderContainer();
        }

        public UMI3DPoseOverriderManager(IPoseOverriderFieldContainer poseOverriderFieldContainer, IPoseContainer poseContainer) : base(poseContainer)
        {
            this.poseOverriderContainerService = poseOverriderFieldContainer;
        }

        private void InitEachPoseAnimationWithPoseOverriderContainer()
        {
            List<OverriderContainerField> overriderContainerFields = poseOverriderContainerService.GetAllPoseOverriders();
            for (int i = 0; i < overriderContainerFields.Count; i++)
            {
                overriderContainerFields[i].PoseOverriderContainer.Id();
                if (overriderContainerFields[i].uMI3DEvent.GetComponent<UMI3DPoseOverriderAnimation>() == null)
                {
                    overriderContainerFields[i].uMI3DEvent.gameObject.AddComponent<UMI3DPoseOverriderAnimation>()
                                                    .Init(overriderContainerFields[i].PoseOverriderContainer);
                }

                overriderContainerFields[i].SetNode();
                allPoseOverriderContainer.Add(overriderContainerFields[i].PoseOverriderContainer.ToDto());
            }
        }
    }
}

