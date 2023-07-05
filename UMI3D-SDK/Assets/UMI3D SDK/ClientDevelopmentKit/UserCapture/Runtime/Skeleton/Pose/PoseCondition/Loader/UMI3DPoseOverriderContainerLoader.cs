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

using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.userCapture.pose;

namespace umi3d.cdk.userCapture.pose
{
    public class UMI3DPoseOverriderContainerLoader : AbstractLoader, IEntity
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Interaction;

        /// <summary>
        /// When the condtions of a pose are satisfied,
        /// returns the right pose overrider
        /// </summary>
        public List<PoseOverriderDto> poseOverriderDtos = new List<PoseOverriderDto>();

        private UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;

        private ulong overriderID;

        /// <summary>
        /// Init the IDs, inits the overriders, registers this entity to the environnement loader
        /// </summary>
        /// <param name="poseOverriderContainerDto"></param>
        private void InitDefinition(UMI3DPoseOverriderContainerDto poseOverriderContainerDto)
        {
            overriderID = poseOverriderContainerDto.id;
            poseOverriderDtos.AddRange(poseOverriderContainerDto.poseOverriderDtos);
            UMI3DEnvironmentLoader.Instance.RegisterEntity(this.overriderID, poseOverriderContainerDto, this).NotifyLoaded();
            StartNewPoseOverriderContainerUnit(poseOverriderContainerDto);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is UMI3DPoseOverriderContainerDto;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            switch (value.dto)
            {
                case UMI3DPoseOverriderContainerDto overriderContainerDto:
                    InitDefinition(overriderContainerDto);
                    break;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            switch (value.property.property)
            {
                case UMI3DPropertyKeys.ActivePoseOverrider:

                    InitDefinition(value.entity.dto as UMI3DPoseOverriderContainerDto);
                    return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.ActivePoseOverrider:
                    ulong id = UMI3DSerializer.Read<ulong>(value.container);
                    PoseOverriderDto[] dtos = UMI3DSerializer.ReadArray<PoseOverriderDto>(value.container);
                    InitDefinition(new UMI3DPoseOverriderContainerDto() { id = id, poseOverriderDtos = dtos });
                    return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        private void StartNewPoseOverriderContainerUnit(UMI3DPoseOverriderContainerDto uMI3DPoseOverriderContainerDto)
        {
            PoseManager.Instance.SubscribeNewPoseHandlerUnit(uMI3DPoseOverriderContainerDto);
        }

        private void StopPoseOverriderContainerUnit(UMI3DPoseOverriderContainerDto uMI3DPoseOverriderContainerDto)
        {
            PoseManager.Instance.UnSubscribePoseHandlerUnit(uMI3DPoseOverriderContainerDto);
        }
    }
}