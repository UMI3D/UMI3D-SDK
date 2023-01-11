/*
Copyright 2019 - 2021 Inetum

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

using System.ComponentModel;
using System.Threading.Tasks;
using umi3d.cdk.userCapture;
using umi3d.common;
using umi3d.common.userCapture;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader for <see cref="UMI3DHandPoseDto"/>.
    /// </summary>
    public class UMI3DHandPoseLoader : AbstractLoader
    {
        UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;
        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is UMI3DHandPoseDto;
        }

        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            var dto = value.dto as UMI3DHandPoseDto;
            UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, null).NotifyLoaded();
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            var dto = value.entity.dto as UMI3DHandPoseDto;
            if (dto == null) return false;
            switch (value.property.property)
            {
                case UMI3DPropertyKeys.ActiveHandPose:
                    dto.IsActive = (bool)value.property.value;
                    UMI3DClientUserTracking.Instance.handPoseEvent.Invoke(dto);
                    break;
                default:
                    return false;
            }
            return true;
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            var dto = value.entity.dto as UMI3DHandPoseDto;
            if (dto == null) return false;
            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.ActiveHandPose:
                    dto.IsActive = UMI3DSerializer.Read<bool>(value.container);
                    UMI3DClientUserTracking.Instance.handPoseEvent.Invoke(dto);
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Load an hand pose
        /// </summary>
        /// <param name="value"></param>
        /// <param name="propertyKey"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public  override async Task<bool> ReadUMI3DProperty(ReadUMI3DPropertyData data)
        {
            switch (data.propertyKey)
            {
                case UMI3DPropertyKeys.ActiveHandPose:
                    data.result = UMI3DSerializer.Read<bool>(data.container);
                    break;

                default:
                    return false;
            }
            return true;
        }
    }
}