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

using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.userCapture.pose;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Loader for <see cref="UMI3DPoseOverridersContainerDto"/>.
    /// </summary>
    public class UMI3DPoseOverriderContainerLoader : AbstractLoader, IEntity
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.UserCapture | DebugScope.Loading;

        private UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;

        /// <summary>
        /// Init the IDs, inits the overriders, registers this entity to the environnement loader
        /// </summary>
        public void Load(UMI3DPoseOverridersContainerDto poseOverriderContainerDto)
        {
            var container = LoadContainer(poseOverriderContainerDto);
            UMI3DEnvironmentLoader.Instance.RegisterEntity(poseOverriderContainerDto.id, poseOverriderContainerDto, container).NotifyLoaded();
        }

        /// <summary>
        /// Instantiate a <see cref="PoseOverridersContainer"/> from a  <see cref="UMI3DPoseOverridersContainerDto"/>.
        /// </summary>
        /// <param name="poseOverriderContainerDto"></param>
        /// <returns></returns>
        public PoseOverridersContainer LoadContainer(UMI3DPoseOverridersContainerDto poseOverriderContainerDto)
        {
            var poseOverriders = poseOverriderContainerDto.poseOverriderDtos.Select(x => LoadPoseOverrider(x)).ToArray();
            PoseOverridersContainer container = new PoseOverridersContainer(poseOverriderContainerDto, poseOverriders);
            PoseManager.Instance.AddPoseOverriders(container);
            return container;
        }

        /// <summary>
        /// Instantiate a <see cref="PoseOverrider"/> from a  <see cref="PoseOverriderDto"/>.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public PoseOverrider LoadPoseOverrider(PoseOverriderDto dto)
        {
            return new PoseOverrider(dto,
                                     dto.poseConditions
                                        .Select(x => LoadPoseCondition(x))
                                        .Where(x => x is not null)
                                        .ToArray());
        }

        /// <summary>
        /// Instantiate a <see cref="IPoseCondition"/> from a  <see cref="AbstractPoseConditionDto"/>.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        private IPoseCondition LoadPoseCondition(AbstractPoseConditionDto dto)
        {
            switch (dto)
            {
                case MagnitudeConditionDto magnitudeConditionDto:
                    {
                        UMI3DNodeInstance targetNodeInstance = UMI3DEnvironmentLoader.Instance.GetNodeInstance(magnitudeConditionDto.TargetNodeId);
                        return new MagnitudePoseCondition(magnitudeConditionDto, targetNodeInstance.transform, PersonalSkeletonManager.Instance.PersonalSkeleton.TrackedSubskeleton);
                    }
                case BoneRotationConditionDto boneRotationConditionDto:
                    {
                        return new BoneRotationPoseCondition(boneRotationConditionDto, PersonalSkeletonManager.Instance.PersonalSkeleton.TrackedSubskeleton);
                    }
                case ScaleConditionDto scaleConditionDto:
                    {
                        UMI3DNodeInstance targetNodeInstance = UMI3DEnvironmentLoader.Instance.GetNodeInstance(scaleConditionDto.TargetId);
                        return new ScalePoseCondition(scaleConditionDto, targetNodeInstance.transform);
                    }

                default:
                    return null;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is UMI3DPoseOverridersContainerDto;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            switch (value.dto)
            {
                case UMI3DPoseOverridersContainerDto overriderContainerDto:
                    Load(overriderContainerDto);
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

                    Load(value.entity.dto as UMI3DPoseOverridersContainerDto);
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
                    Load(new UMI3DPoseOverridersContainerDto() { id = id, poseOverriderDtos = dtos });
                    return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}