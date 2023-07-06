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
using System.Collections.Generic;
using umi3d.common.userCapture.pose;

namespace umi3d.edk.userCapture.pose
{
    public class UMI3DPoseManager : Singleton<UMI3DPoseManager>, IUMI3DPoseManager
    {
        private readonly IPoseContainer poseContainerService;
        private readonly IPoseOverriderFieldContainer poseOverriderContainerService;

        public UMI3DPoseManager() : base()
        {
            this.poseContainerService = UMI3DPoseContainer.Instance;
            this.poseOverriderContainerService = UMI3DPoseOverrideFieldContainer.Instance as IPoseOverriderFieldContainer;
            Init();
        }

        public UMI3DPoseManager(IPoseContainer poseContainer, IPoseOverriderFieldContainer poseOverriderFieldContainer) : base()
        {
            this.poseContainerService = poseContainer;
            this.poseOverriderContainerService = poseOverriderFieldContainer;
            Init();
        }

        private bool posesInitialized = false;

        public Dictionary<ulong, List<PoseDto>> allPoses = new Dictionary<ulong, List<PoseDto>>();
        protected List<UMI3DPoseOverriderContainerDto> allPoseOverriderContainer = new();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Dictionary<ulong, List<PoseDto>> AllPoses {get => allPoses;} 
        /// <inheritdoc/>
        public List<UMI3DPoseOverriderContainerDto> AllPoseOverriderContainer { get => allPoseOverriderContainer; }

        /// <inheritdoc/>
        public void SetNewUserPose(ulong userId, List<PoseDto> poseDtos)
        {
            allPoses.Add(userId, poseDtos);
        }

        /// <summary>
        /// Inits all the poses and pose overriders to make them ready for dto server-client exchanges
        /// </summary>
        private void Init()
        {
            if (posesInitialized == false)
            {
                posesInitialized = true;

                ServerPoseInit();
                PoseOverrider_Init();
            }
        }

        /// <summary>
        /// Take pose in scriptables object format and put them in posedto format
        /// </summary>
        private void ServerPoseInit()
        {
            List<UMI3DPose_so> allServerPoses = poseContainerService.GetAllServerPoses();
            List<PoseDto> poses = new List<PoseDto>();
            for (int i = 0; i < allServerPoses.Count; i++)
            {
                PoseDto poseDto = allServerPoses[i].ToDTO();
                poseDto.id = i;
                allServerPoses[i].poseRef = poseDto.id;
                poses.Add(poseDto);
            }

            allPoses.Add(0, poses);
        }

        /// <summary>
        /// Take pose overriders fields to generate all the needed pose overriders containers 
        /// </summary>
        private void PoseOverrider_Init()
        {
            List<OverriderContainerField> overriderContainerFields = poseOverriderContainerService.GetAllPoseOverriders();
            for (int i = 0; i < overriderContainerFields.Count; i++)
            {
                overriderContainerFields[i].PoseOverriderContainer.Id();

                if (overriderContainerFields[i].uMI3DModel.GetComponent<UMI3DPoseOverriderAnimation>() == null)
                {
                    overriderContainerFields[i].uMI3DModel.gameObject.AddComponent<UMI3DPoseOverriderAnimation>()
                                                    .Init(overriderContainerFields[i].PoseOverriderContainer);
                }

                overriderContainerFields[i].SetNode();
                allPoseOverriderContainer.Add(overriderContainerFields[i].PoseOverriderContainer.ToDto());
            }
        }
    }
}