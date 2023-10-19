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
    /// <summary>
    /// Service that handle poses from the environment side.
    /// </summary>
    public class PoseManager : Singleton<PoseManager>, IPoseManager
    {
        #region Dependency Injection

        public PoseManager() : base()
        {
            Init();
        }

        public PoseManager(IPosesRegister poseContainer, IPoseOverridersRegister poseOverriderFieldContainer) : base()
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
        public IList<UMI3DPoseOverridersContainerDto> PoseOverriderContainers { get; private set; } = new List<UMI3DPoseOverridersContainerDto>();

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


                // more overriders from monos in scene
                var overridersMonos = UnityEngine.Object.FindObjectsOfType<UMI3DPoseOverrider>()
                                                                                    .GroupBy(x=>x.relativeNode.Id())
                                                                                    .ToDictionary(g=>g.Key, 
                                                                                                  g=>g.Cast<IUMI3DPoseOverriderData>());


                foreach (var (nodeId, overriders) in overridersMonos)
                {
                    var container = new UMI3DPoseOverriderContainer(nodeId, overriders);
                    container.Init(nodeId);

                    PoseOverriderContainers.Add((UMI3DPoseOverridersContainerDto)container.ToEntityDto());
                }

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
                overrider.Init();
                overrider.PoseOverriderContainer.Id();
                if (PoseOverriderContainers.Select(x => x.id).Contains(overrider.PoseOverriderContainer.Id()))
                    continue;

                PoseOverriderContainers.Add((UMI3DPoseOverridersContainerDto)overrider.PoseOverriderContainer.ToEntityDto());
            }
        }
    }
}