/*
Copyright 2019 - 2023 Inetum

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

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

