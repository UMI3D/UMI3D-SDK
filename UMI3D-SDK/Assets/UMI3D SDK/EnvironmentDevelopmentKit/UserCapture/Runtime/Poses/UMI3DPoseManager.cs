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
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.edk.userCapture
{
    public class UMI3DPoseManager : Singleton<UMI3DPoseManager>
    {
        private readonly IPoseContainer poseContainerService;

        public UMI3DPoseManager() : base()
        {
            this.poseContainerService = UMI3DPoseContainer.Instance;
            Init();
        }

        public UMI3DPoseManager(IPoseContainer poseContainer) : base()
        {
            this.poseContainerService = poseContainer;
            Init();
        }

        bool posesInitialized = false;

        public Dictionary<ulong, List<PoseDto>> allPoses = new Dictionary<ulong, List<PoseDto>>();
        protected List<UMI3DPoseOverriderContainerDto> allPoseOverriderContainer = new();

        public void UpdateAlPoseOverriders(UMI3DPoseOverriderContainerDto allPoseOverriderContainer)
        {
            this.allPoseOverriderContainer.Find(a => a.id == allPoseOverriderContainer.id).poseOverriderDtos = allPoseOverriderContainer.poseOverriderDtos;
        }

        public List<UMI3DPoseOverriderContainerDto> GetOverriders()
        {
            return allPoseOverriderContainer;
        }

        public void Init()
        {
            List<UMI3DPose_so> allServerPoses = poseContainerService.GetAllServerPoses();
            if (posesInitialized == false)
            {
                posesInitialized = true;
                List<PoseDto> poses = new List<PoseDto>();
                for (int i = 0; i < allServerPoses.Count; i++)
                {
                    allServerPoses[i].SendPoseIndexationEvent(i);
                    PoseDto poseDto = allServerPoses[i].ToDTO();
                    poseDto.id = i;
                    poses.Add(poseDto);
                }

                allPoses.Add(0, poses);
            }
        }

        public void InitNewUserPoses(ulong userId, List<PoseDto> poseDtos)
        {
            allPoses.Add(userId, poseDtos);
        }
    }
}