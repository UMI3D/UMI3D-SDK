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
using System.Linq;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.userCapture
{
    public class PoseManager : SingleBehaviour<PoseManager>
    {
        [SerializeField] List<UMI3DPose_so> clientPoses = new List<UMI3DPose_so>();
        [SerializeField] List<UMI3DPoseOverriderContainerDto> poseOverriderContainerDtos = new();


        public PoseDto defaultPose;
        public PoseDto[] localPoses;

        public Dictionary<ulong, List<PoseDto>> allPoses;

        private ISkeletonManager skeletonManager;

        private void Start()
        {
            localPoses = new PoseDto[clientPoses.Count];
            for (int i = 0; i< clientPoses.Count; i++)
            {
                PoseDto poseDto = clientPoses[i].ToDTO();
                poseDto.id = i;
                localPoses[i] = poseDto;
            }

            skeletonManager = PersonalSkeletonManager.Instance;
        }

        public void SetPoses(Dictionary<ulong, List<PoseDto>> allPoses)
        {
            this.allPoses = allPoses;
        }

        public PoseDto GetPose(ulong key, int index)
        {
            List<PoseDto> poses = allPoses[key];
            return poses?[index];
        }


        public void SetPosesOverriders(List<UMI3DPoseOverriderContainerDto> allPoseOverriderContainer)
        {
            poseOverriderContainerDtos = allPoseOverriderContainer;

            for (int i = 0; i < allPoseOverriderContainer.Count; i++)
            {
                PoseOverriderContainerHandlerUnit handlerUnit = new PoseOverriderContainerHandlerUnit();
                handlerUnit.SetPoseOverriderContainer(allPoseOverriderContainer[i]);
                handlerUnit.CheckCondtionOfAllOverriders();
                handlerUnit.onConditionValidated += (pose) =>
                {
                    ApplyTargetPoseToPersonalSkeleton_PoseSkeleton(pose);
                };
            }
        }

        Coroutine poseOverriderContainerHandlerCoroutine = null;

        internal void HandlePoseOverriderContainerHandlerUnitCheckCorroutine(IEnumerator enumerator)
        {
            poseOverriderContainerHandlerCoroutine = StartCoroutine(enumerator);
        }

        internal void DisablePoseOverriderContainerHandlerUnitCheckCorroutine()
        {
            if (poseOverriderContainerHandlerCoroutine != null)
            {
                StopCoroutine(poseOverriderContainerHandlerCoroutine);
            }
        }

        internal void ApplyTargetPoseToPersonalSkeleton_PoseSkeleton(PoseOverriderDto poseOverriderDto)
        {
            if (poseOverriderDto != null && allPoses != null)
            {
                foreach (PoseDto pose in allPoses[0])
                {
                    if (pose.id == poseOverriderDto.poseIndexinPoseManager)
                    {

                        skeletonManager.personalSkeleton.poseSkeleton.SetPose(true, new List<PoseDto>() { pose }, true);
                        return;
                    }
                }
            }
        }
    }
}