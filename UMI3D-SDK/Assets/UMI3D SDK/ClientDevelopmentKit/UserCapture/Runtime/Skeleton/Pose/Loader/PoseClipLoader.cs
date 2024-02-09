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

using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Loader for <see cref="PoseClip"/>.
    /// </summary>
    public class PoseClipLoader : AbstractLoader<PoseClipDto, PoseClip>, IEntity
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.UserCapture | DebugScope.Loading;

        public override UMI3DVersion.VersionCompatibility version => _version;
        private static readonly UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.7", "*");

        #region Dependencies Injection

        private readonly IEnvironmentManager environmentService;

        public PoseClipLoader()
        {
            environmentService = UMI3DEnvironmentLoader.Instance;
        }

        public PoseClipLoader(IEnvironmentManager environmentService)
        {
            this.environmentService = environmentService;
        }

        #endregion Dependencies Injection

        /// <inheritdoc/>
        public override Task<PoseClip> Load(ulong environmentId, PoseClipDto dto)
        {
            PoseClip pose = new PoseClip(dto);

            environmentService.RegisterEntity(environmentId, dto.id, dto, pose, () => Delete(environmentId, dto.id)).NotifyLoaded();

            return Task.FromResult(pose);
        }

        /// <inheritdoc/>
        public override void Delete(ulong environmentId, ulong id)
        {
        }

        #region PropertySetters

        public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            if (value.entity.dto is not PoseClipDto dto)
                return Task.FromResult(false);

            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.PoseClipResource:
                    SetPoseProperty(value.environmentId, dto.id, UMI3DSerializer.Read<PoseDto>(value.container));
                    break;

                case UMI3DPropertyKeys.PoseClipInterpolable:
                    SetIsInterpolableProperty(value.environmentId, dto.id, UMI3DSerializer.Read<bool>(value.container));
                    break;

                case UMI3DPropertyKeys.PoseClipComposable:
                    SetIsComposableProperty(value.environmentId, dto.id, UMI3DSerializer.Read<bool>(value.container));
                    break;

                default:
                    return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }

        public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            if (value.entity.dto is not PoseClipDto dto)
                return Task.FromResult(false);

            switch (value.property.property)
            {
                case UMI3DPropertyKeys.PoseClipResource:
                    SetPoseProperty(value.environmentId, dto.id, (PoseDto)value.property.value);
                    break;

                case UMI3DPropertyKeys.PoseClipInterpolable:
                    SetIsInterpolableProperty(value.environmentId, dto.id, (bool)value.property.value);
                    break;

                case UMI3DPropertyKeys.PoseClipComposable:
                    SetIsComposableProperty(value.environmentId, dto.id, (bool)value.property.value);
                    break;

                default:
                    return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }

        protected void SetPoseProperty(ulong environmentId, ulong entityId, PoseDto pose)
        {
            if (!environmentService.TryGetEntity(environmentId, entityId, out PoseClip poseClip))
                return;

            poseClip.Dto.pose = pose;
        }

        protected void SetIsInterpolableProperty(ulong environmentId, ulong entityId, bool isInterpolable)
        {
            if (!environmentService.TryGetEntity(environmentId, entityId, out PoseClip poseClip))
                return;

            poseClip.Dto.isInterpolable = isInterpolable;
        }

        protected void SetIsComposableProperty(ulong environmentId, ulong entityId, bool isComposable)
        {
            if (!environmentService.TryGetEntity(environmentId, entityId, out PoseClip poseClip))
                return;

            poseClip.Dto.isComposable = isComposable;
        }

        #endregion PropertySetters
    }
}