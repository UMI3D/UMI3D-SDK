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
        private const DebugScope scope = DebugScope.EDK | DebugScope.UserCapture | DebugScope.User;
        /// <summary>
        /// Lock for  <see cref="JoinDtoReception(UMI3DUser, Vector3Dto, Dictionary{uint, bool})"/>.
        /// </summary>
        static object joinLock = new object();
        static object logoutLock = new object();

        private readonly IPoseContainer poseContainerService;

        public UMI3DPoseManager()
        {
            this.poseContainerService = UMI3DPoseContainer.Instance;
            Init();
        }

        public UMI3DPoseManager(IPoseContainer poseContainer)
        {
            this.poseContainerService = poseContainer;
            Init();
        }

        bool posesInitialized = false;

        public Dictionary<ulong, List<PoseDto>> allPoses = new Dictionary<ulong, List<PoseDto>>();
        private UMI3DAsyncDictionnaryProperty<ulong, List<PoseDto>> _objectAllPoses;
        public UMI3DAsyncDictionnaryProperty<ulong, List<PoseDto>> objectAllPoses;

        protected List<UMI3DPoseOverriderContainerDto> allPoseOverriderContainer = new();

        private UMI3DAsyncListProperty<UMI3DPoseOverriderContainerDto> _objectAllPoseOverrider;
        public UMI3DAsyncListProperty<UMI3DPoseOverriderContainerDto> objectAllPoseOverrider;

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

                _objectAllPoses = new UMI3DAsyncDictionnaryProperty<ulong, List<PoseDto>>(UMI3DGlobalID.EnvironementId, UMI3DPropertyKeys.AllPoses, allPoses);
                _objectAllPoseOverrider = new UMI3DAsyncListProperty<UMI3DPoseOverriderContainerDto>(UMI3DGlobalID.EnvironementId, UMI3DPropertyKeys.AllPoseOverriderContainers, null, null);
            }

            UMI3DServer.Instance.OnUserLeave.AddListener((u) => RemovePosesOnLeftUser(u));
        }

        public async Task InitNewUserPoses(UMI3DUser user, List<PoseDto> userPoses)
        {
            Operation operation;
            lock (joinLock)
            {
                operation = objectAllPoses.Add(user.Id(), userPoses);
            }

            SendOperation(operation);
        }

        private void RemovePosesOnLeftUser(UMI3DUser user)
        {
            Operation operation;
            lock (logoutLock)
            {
                operation = objectAllPoses.Remove(user.Id());
            }

            SendOperation(operation);
        }

        private void SendOperation(Operation operation)
        {
            Transaction transaction = new Transaction()
            {
                reliable = true
            };

            transaction.Add(operation);

            transaction.Dispatch();
        }
    }
}