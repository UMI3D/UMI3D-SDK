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
using System.Linq;
using umi3d.common;
using umi3d.common.userCapture.pose;

namespace umi3d.edk.userCapture.pose
{
    public class UMI3DPoseManager : Singleton<UMI3DPoseManager>, IUMI3DPoseManager
    {
        #region Dependency Injection

        public UMI3DPoseManager() : base()
        {
            Init();
        }

        public UMI3DPoseManager(IPosesRegister poseContainer, IPoseOverridersRegister poseOverriderFieldContainer) : base()
        {
            Init();
        }

        #endregion Dependency Injection

        /// <summary>
        /// A bool to make sure the initialisation only occurs once
        /// </summary>
        private bool posesInitialized = false;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IDictionary<ulong, IList<PoseDto>> Poses => poses;

        private readonly Dictionary<ulong, IList<PoseDto>> poses = new();

        /// <inheritdoc/>
        public IList<UMI3DPoseOverriderContainerDto> PoseOverriderContainers { get; private set; } = new List<UMI3DPoseOverriderContainerDto>();

        /// <inheritdoc/>
        public void RegisterUserCustomPose(ulong userId, IEnumerable<PoseDto> poseDtos)
        {
            Poses.Add(userId, poseDtos.ToList());
        }

        /// <summary>
        /// Inits all the poses and pose overriders to make them ready for dto server-client exchanges
        /// </summary>
        private void Init()
        {
            if (posesInitialized == false)
            {
                var registers = UnityEngine.Object.FindObjectsOfType<UMI3DPosesRegister>();

                foreach (var register in registers)
                    Register(register);

                posesInitialized = true;
            }
        }

        private void Register(UMI3DPosesRegister register)
        {
            RegisterEnvironmentPoses(register);
            RegisterPoseOverriders(register);
        }

        private int lastAttributedIndex = 0;

        /// <summary>
        /// Take pose in scriptables object format and put them in posedto format
        /// </summary>
        public void RegisterEnvironmentPoses(IPosesRegister register)
        {
            foreach (var pose in register.EnvironmentPoses)
            {
                PoseDto poseDto = pose.ToDto();
                poseDto.index = lastAttributedIndex++;
                pose.Index = poseDto.index;

                if (poses.ContainsKey(UMI3DGlobalID.EnvironementId))
                {
                    if (!poses[UMI3DGlobalID.EnvironementId].Contains(poseDto))
                        poses[UMI3DGlobalID.EnvironementId].Add(poseDto);
                }
                else
                    poses.Add(UMI3DGlobalID.EnvironementId, new List<PoseDto>() { poseDto });
            }
        }

        /// <summary>
        /// Take pose overriders fields to generate all the needed pose overriders containers
        /// </summary>
        public void RegisterPoseOverriders(IPoseOverridersRegister register)
        {
            foreach (var overrider in register.PoseOverriderFields)
            {
                overrider.PoseOverriderContainer.Id();
                if (PoseOverriderContainers.Select(x => x.id).Contains(overrider.PoseOverriderContainer.Id()))
                    continue;

                if (!overrider.Model.TryGetComponent<UMI3DPoseOverriderAnimation>(out _))
                {
                    overrider.Model.gameObject.AddComponent<UMI3DPoseOverriderAnimation>()
                                                    .Init(overrider.PoseOverriderContainer);
                }

                overrider.Init();

                PoseOverriderContainers.Add((UMI3DPoseOverriderContainerDto)overrider.PoseOverriderContainer.ToEntityDto());
            }
        }
    }
}