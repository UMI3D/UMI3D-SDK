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
    public class PoseManager : Singleton<PoseManager>
    {
        [SerializeField] List<UMI3DPoseOverriderContainerDto> poseOverriderContainerDtos = new();

        public PoseDto defaultPose;
        public PoseDto[] localPoses;

        public Dictionary<ulong, List<PoseDto>> allPoses;
        public Dictionary<ulong, PoseOverriderContainerHandlerUnit> allPoseHandlerUnits = new Dictionary<ulong, PoseOverriderContainerHandlerUnit>(); 

        private readonly ISkeletonManager skeletonManager;
        private readonly UMI3DEnvironmentLoader environmentLoader;

        bool isInit = false;

        public PoseManager()
        {
            skeletonManager = PersonalSkeletonManager.Instance;
            environmentLoader = UMI3DEnvironmentLoader.Instance;
            InitLocalPoses();
        }

        public PoseManager(ISkeletonManager skeletonManager, UMI3DEnvironmentLoader environmentLoader)
        {
            this.skeletonManager = skeletonManager;
            this.environmentLoader = environmentLoader;
            InitLocalPoses();
        }

        private void InitLocalPoses()
        {
            if (isInit == false)
            {
                isInit= true;

                List<UMI3DPose_so> clientPoses = (environmentLoader.LoadingParameters as UMI3DUserCaptureLoadingParameters).clientPoses;
                localPoses = new PoseDto[clientPoses.Count];
                for (int i = 0; i < clientPoses.Count; i++)
                {
                    PoseDto poseDto = clientPoses[i].ToDTO();
                    poseDto.id = i;
                    localPoses[i] = poseDto;
                }
            }
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

        /// <summary>
        /// Inits all the pose overrider containers
        /// </summary>
        /// <param name="allPoseOverriderContainer"></param>
        public void SetPosesOverriders(List<UMI3DPoseOverriderContainerDto> allPoseOverriderContainer)
        {
            poseOverriderContainerDtos = allPoseOverriderContainer;

            for (int i = 0; i < allPoseOverriderContainer.Count; i++)
            {
                PoseOverriderContainerHandlerUnit handlerUnit = new PoseOverriderContainerHandlerUnit(allPoseOverriderContainer[i]);
                handlerUnit.CheckCondtionOfAllOverriders();
                handlerUnit.OnConditionValidated += (unit, pose) =>
                {
                    SetTargetPose(pose);
                };
                handlerUnit.OnConditionDesactivated += (unit, pose) =>
                {
                    StopTargetPose(pose);
                };
                allPoseHandlerUnits.Add(allPoseOverriderContainer[i].relatedNodeId, handlerUnit);
            }
        }

        /// <summary>
        /// Allows to add a pose handler unit at runtime
        /// </summary>
        /// <param name="overrider"></param>
        /// <param name="unit"></param>
        public void SubscribeNewPoseHandlerUnit(UMI3DPoseOverriderContainerDto overrider)
        {
            PoseOverriderContainerHandlerUnit unit = new PoseOverriderContainerHandlerUnit(overrider);
            unit.OnConditionValidated += (unit, poseOverriderDto) => SetTargetPose(poseOverriderDto);
            unit.OnConditionDesactivated += (unit, poseOverriderDto) => StopTargetPose(poseOverriderDto);
            allPoseHandlerUnits.Add(overrider.relatedNodeId, unit);
        }

        /// <summary>
        /// Allows to remove a pose handler unit at runtime
        /// </summary>
        /// <param name="overrider"></param>
        public void UnSubscribePoseHandlerUnit(UMI3DPoseOverriderContainerDto overrider)
        {
            if (allPoseHandlerUnits.TryGetValue(overrider.relatedNodeId, out PoseOverriderContainerHandlerUnit unit))
            {
                unit.OnConditionValidated -= (unit, poseOverriderDto) => SetTargetPose(poseOverriderDto);
                unit.DisableCheck();
                allPoseHandlerUnits.Remove(overrider.relatedNodeId);
            }
        }

        /// <summary>
        /// Activated if the Hover Enter is triggered
        /// </summary>
        public void OnHoverEnter(ulong id)
        {
            if (allPoseHandlerUnits.TryGetValue(id, out PoseOverriderContainerHandlerUnit unit))
            {
                unit.OnHoverEnter();
            }
        }

        /// <summary>
        /// Activated if the Hover Exit is triggered
        /// </summary>
        public void OnHoverExit(ulong id)
        {
            if (allPoseHandlerUnits.TryGetValue(id, out PoseOverriderContainerHandlerUnit unit))
            {
                unit.OnHoverExit();
            }
        }

        /// <summary>
        /// Activated if the Trigger is triggered
        /// </summary>
        public void OnTrigger(ulong id)
        {
            if (allPoseHandlerUnits.TryGetValue(id, out PoseOverriderContainerHandlerUnit unit))
            {
                unit.OnTrigger();
            }
        }

        /// <summary>
        /// Activated if the Release Enter is triggered
        /// </summary>
        public void OnRelease(ulong id)
        {
            if (allPoseHandlerUnits.TryGetValue(id, out PoseOverriderContainerHandlerUnit unit))
            {
                unit.OnRelease();
            }
        }

        /// <summary>
        /// Sets the related pose to the overrider Dto, in the poseSkeleton
        /// </summary>
        /// <param name="poseOverriderDto"></param>
        public void SetTargetPose(PoseOverriderDto poseOverriderDto, bool isSeverPose = true)
        {
            if (poseOverriderDto != null && allPoses != null)
            {
                foreach (PoseDto pose in allPoses[0])
                {
                    if (pose.id == poseOverriderDto.poseIndexinPoseManager)
                    {
                        skeletonManager.personalSkeleton.poseSkeleton.SetPose(poseOverriderDto.composable, new List<PoseDto>() { pose }, isSeverPose);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Stops the related pose to the overriderDto, in the poseSkeleton
        /// </summary>
        /// <param name="poseOverriderDto"></param>
        public void StopTargetPose(PoseOverriderDto poseOverriderDto, bool isServerPose = true)
        {
            if (poseOverriderDto != null && allPoses != null)
            {
                foreach (PoseDto pose in allPoses[0])
                {
                    if (pose.id == poseOverriderDto.poseIndexinPoseManager)
                    {
                        skeletonManager.personalSkeleton.poseSkeleton.StopPose(new List<PoseDto>() { pose }, isServerPose);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Stops all poses
        /// </summary>
        /// <param name="poseOverriderDto"></param>
        public void StopAllPoses()
        {
            skeletonManager.personalSkeleton.poseSkeleton.StopAllPoses();
        }
    }
}