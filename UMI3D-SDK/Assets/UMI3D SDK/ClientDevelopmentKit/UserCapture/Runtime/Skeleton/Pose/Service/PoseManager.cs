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
using System.Collections.Generic;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace umi3d.cdk.userCapture.pose
{
    public class PoseManager : Singleton<PoseManager>, IPoseManager
    {
        [SerializeField] private List<UMI3DPoseOverriderContainerDto> poseOverriderContainerDtos = new();

        public PoseDto defaultPose;
        public PoseDto[] localPoses;

        public Dictionary<ulong, List<PoseDto>> allPoses;
        public Dictionary<ulong, PoseOverriderContainerHandlerUnit> allPoseHandlerUnits = new Dictionary<ulong, PoseOverriderContainerHandlerUnit>();

        private bool isInit = false;

        #region Dependency Injection

        private readonly ISkeletonManager skeletonManager;
        private readonly ILoadingManager loadingManager;

        public PoseManager()
        {
            skeletonManager = PersonalSkeletonManager.Instance;
            loadingManager = UMI3DEnvironmentLoader.Instance;
            InitLocalPoses();
        }

        public PoseManager(ISkeletonManager skeletonManager, ILoadingManager loadingManager)
        {
            this.skeletonManager = skeletonManager;
            this.loadingManager = loadingManager;
            InitLocalPoses();
        }

        #endregion Dependency Injection

        private void InitLocalPoses()
        {
            if (isInit == false)
            {
                isInit = true;

                List<UMI3DPose_so> clientPoses = (loadingManager.LoadingParameters as UMI3DUserCaptureLoadingParameters).clientPoses;
                localPoses = new PoseDto[clientPoses.Count];
                for (int i = 0; i < clientPoses.Count; i++)
                {
                    PoseDto poseDto = clientPoses[i].ToDTO();
                    poseDto.id = i;
                    localPoses[i] = poseDto;
                }
            }
        }

        /// <inheritdoc/>
        public void SetPoses(Dictionary<ulong, List<PoseDto>> allPoses)
        {
            this.allPoses = allPoses;
        }

        /// <inheritdoc/>
        public PoseDto GetPose(ulong key, int index)
        {
            List<PoseDto> poses = allPoses[key];
            return poses?[index];
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void SubscribeNewPoseHandlerUnit(UMI3DPoseOverriderContainerDto overrider)
        {
            PoseOverriderContainerHandlerUnit unit = new PoseOverriderContainerHandlerUnit(overrider);
            unit.OnConditionValidated += (unit, poseOverriderDto) => SetTargetPose(poseOverriderDto);
            unit.OnConditionDesactivated += (unit, poseOverriderDto) => StopTargetPose(poseOverriderDto);
            allPoseHandlerUnits.Add(overrider.relatedNodeId, unit);
        }

        /// <inheritdoc/>
        public void UnSubscribePoseHandlerUnit(UMI3DPoseOverriderContainerDto overrider)
        {
            if (allPoseHandlerUnits.TryGetValue(overrider.relatedNodeId, out PoseOverriderContainerHandlerUnit unit))
            {
                unit.OnConditionValidated -= (unit, poseOverriderDto) => SetTargetPose(poseOverriderDto);
                unit.DisableCheck();
                allPoseHandlerUnits.Remove(overrider.relatedNodeId);
            }
        }

        /// <inheritdoc/>
        public void OnHoverEnter(ulong id)
        {
            if (allPoseHandlerUnits.TryGetValue(id, out PoseOverriderContainerHandlerUnit unit))
            {
                unit.CheckHoverEnterConditions();
            }
        }

        /// <inheritdoc/>
        public void OnHoverExit(ulong id)
        {
            if (allPoseHandlerUnits.TryGetValue(id, out PoseOverriderContainerHandlerUnit unit))
            {
                unit.CheckHoverExitConditions();
            }
        }

        /// <inheritdoc/>
        public void OnTrigger(ulong id)
        {
            if (allPoseHandlerUnits.TryGetValue(id, out PoseOverriderContainerHandlerUnit unit))
            {
                unit.CheckTriggerConditions();
            }
        }

        /// <inheritdoc/>
        public void OnRelease(ulong id)
        {
            if (allPoseHandlerUnits.TryGetValue(id, out PoseOverriderContainerHandlerUnit unit))
            {
                unit.CheckReleaseConditions();
            }
        }

        /// <inheritdoc/>
        public void SetTargetPose(PoseOverriderDto poseOverriderDto, bool isSeverPose = true)
        {
            if (poseOverriderDto != null && allPoses != null)
            {
                foreach (PoseDto pose in allPoses[0])
                {
                    if (pose.id == poseOverriderDto.poseIndexinPoseManager)
                    {
                        skeletonManager.personalSkeleton.PoseSkeleton.SetPose(poseOverriderDto.composable, new List<PoseDto>() { pose }, isSeverPose);
                        return;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void StopTargetPose(PoseOverriderDto poseOverriderDto, bool isServerPose = true)
        {
            if (poseOverriderDto != null && allPoses != null)
            {
                foreach (PoseDto pose in allPoses[0])
                {
                    if (pose.id == poseOverriderDto.poseIndexinPoseManager)
                    {
                        skeletonManager.personalSkeleton.PoseSkeleton.StopPose(new List<PoseDto>() { pose }, isServerPose);
                        return;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void StopAllPoses()
        {
            skeletonManager.personalSkeleton.PoseSkeleton.StopAllPoses();
        }
    }
}